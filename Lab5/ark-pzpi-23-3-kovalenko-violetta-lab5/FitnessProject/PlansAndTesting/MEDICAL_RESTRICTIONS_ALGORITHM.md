# Алгоритм учета медицинских ограничений

## Общая схема работы

Система учитывает медицинские ограничения пользователя на всех этапах работы с продуктами питания, особенно при генерации персонального плана питания.

---

## 1. Хранение данных

### 1.1. Структура данных в UserProfile

В таблице `UserProfiles` поле `medical_conditions` хранится как строка (TEXT/NVARCHAR).

**Формат 1: JSON (рекомендуемый)**
```json
{
  "Allergens": ["Eggs", "Fish", "Milk"],
  "MedicalConditions": ["Diabetes", "Hypertension"],
  "DietaryRestriction": "Vegetarian"
}
```

**Формат 2: Простой (через запятую)**
```
Eggs, Fish, Diabetes, Vegetarian
```

### 1.2. Структурированное представление

После парсинга данные преобразуются в `UserMedicalRestrictionsDto`:

```csharp
public class UserMedicalRestrictionsDto
{
    public List<string> Allergens { get; set; }           // Список аллергенов
    public List<string> MedicalConditions { get; set; }   // Медицинские состояния
    public DietaryRestrictionType? DietaryRestriction { get; set; }  // Диетическое ограничение
}
```

---

## 2. Парсинг данных

### 2.1. Процесс парсинга

**Метод:** `MedicalRestrictionsParser.ParseMedicalConditions(string? medicalConditions)`

**Алгоритм:**
1. Проверка на пустоту → возврат пустого DTO
2. Попытка десериализации JSON:
   - Успех → возврат структурированного DTO
   - Ошибка → переход к простому парсингу
3. Простой парсинг (через запятую):
   - Разделение строки по запятым
   - Для каждой части:
     - Попытка распознать как `DietaryRestrictionType` enum
     - Если не enum → добавление в `MedicalConditions`

**Пример:**
```
Вход: "Eggs, Fish, Diabetes, Vegetarian"
Выход:
  - Allergens: ["Eggs", "Fish"]
  - MedicalConditions: ["Diabetes"]
  - DietaryRestriction: Vegetarian
```

---

## 3. Применение фильтров к продуктам

### 3.1. Основной метод фильтрации

**Метод:** `ProductFilterHelper.FilterProductsByRestrictions(IEnumerable<Product> products, UserMedicalRestrictionsDto restrictions)`

**Последовательность применения фильтров (каскадно):**

```
Все продукты
    ↓
[Фильтр 1: Аллергены]
    ↓
[Фильтр 2: Диетические ограничения]
    ↓
[Фильтр 3: Медицинские состояния]
    ↓
Разрешенные продукты
```

### 3.2. Фильтр по аллергенам

**Метод:** `FilterProductsByAllergens(IEnumerable<Product> products, List<string> userAllergens)`

**Алгоритм:**
1. Для каждого продукта проверяется поле `Allergens`
2. Если `Allergens` пустое → продукт проходит
3. Если `Allergens` содержит любой из аллергенов пользователя → продукт исключается
4. Проверка без учета регистра (case-insensitive)

**Пример:**
```
Пользователь: аллергия на "Eggs", "Fish"
Продукт 1: Allergens = "Eggs" → ИСКЛЮЧЕН
Продукт 2: Allergens = "Milk" → ПРОШЕЛ
Продукт 3: Allergens = null → ПРОШЕЛ
Продукт 4: Allergens = "Fish, Gluten" → ИСКЛЮЧЕН
```

### 3.3. Фильтр по диетическим ограничениям

**Метод:** `FilterProductsByDietaryRestrictions(IEnumerable<Product> products, DietaryRestrictionType restriction)`

**Типы ограничений и их логика:**

#### Vegetarian (Вегетарианство)
- **Исключаются:** мясо (beef, pork, chicken, turkey, lamb, veal)
- **Проверка:** по `ProductName` и `Restriction`
- **Ключевые слова:** "meat", "beef", "pork", "chicken", "turkey", "lamb", "veal", "мясо", "говядина", "свинина", "курица"

#### Vegan (Веганство)
- **Исключаются:** все продукты животного происхождения
- **Проверка:** по `ProductName`, `Restriction`, `Allergens`
- **Ключевые слова:** "meat", "fish", "egg", "milk", "cheese", "butter", "honey", "мясо", "рыба", "яйцо", "молоко", "сыр", "мед"
- **Дополнительно:** исключаются продукты с аллергенами "Eggs", "Milk", "Fish"

#### Pescatarian (Пескетарианство)
- **Исключаются:** мясо (но рыба разрешена)
- **Логика:** аналогично вегетарианству

#### GlutenFree (Без глютена)
- **Исключаются:** продукты с глютеном
- **Ключевые слова:** "wheat", "gluten", "barley", "rye", "пшеница", "глютен", "ячмень", "рожь"
- **Дополнительно:** исключаются продукты с аллергеном "Gluten"

#### LactoseFree (Без лактозы)
- **Исключаются:** молочные продукты
- **Ключевые слова:** "milk", "cheese", "yogurt", "butter", "cream", "молоко", "сыр", "йогурт", "сливки"
- **Дополнительно:** исключаются продукты с аллергеном "Milk"

#### Halal (Халяль)
- **Исключаются:** свинина, алкоголь
- **Ключевые слова:** "pork", "alcohol", "свинина", "алкоголь"

#### Kosher (Кошер)
- **Исключаются:** свинина, морепродукты
- **Ключевые слова:** "pork", "shellfish", "свинина", "морепродукты"

**Пример (Vegetarian):**
```
Продукт 1: ProductName = "Chicken Breast" → ИСКЛЮЧЕН
Продукт 2: ProductName = "Broccoli" → ПРОШЕЛ
Продукт 3: Restriction = "Contains meat" → ИСКЛЮЧЕН
Продукт 4: ProductName = "Salmon" → ПРОШЕЛ (рыба разрешена)
```

### 3.4. Фильтр по медицинским состояниям

**Метод:** `FilterProductsByMedicalConditions(IEnumerable<Product> products, List<string> medicalConditions)`

**Алгоритм:**
1. Для каждого медицинского состояния ищется маппинг в `MedicalRestrictionsMapper`
2. Получается список ключевых слов для исключения
3. Для каждого продукта проверяется:
   - `ProductName` + `Restriction` на наличие ключевых слов
   - Если найдено совпадение → продукт исключается

**Маппинг медицинских состояний:**

| Медицинское состояние | Исключаемые ключевые слова |
|----------------------|---------------------------|
| Diabetes | "high_gi", "sugar", "honey", "syrup" |
| Hypertension | "high_sodium", "salt" |
| KidneyDisease | "high_protein", "high_sodium" |
| CeliacDisease | "gluten", "wheat" |
| LactoseIntolerance | "milk", "lactose", "dairy" |

**Пример (Diabetes):**
```
Пользователь: MedicalConditions = ["Diabetes"]
Продукт 1: ProductName = "Honey" → ИСКЛЮЧЕН (содержит "honey")
Продукт 2: ProductName = "Brown Rice" → ПРОШЕЛ
Продукт 3: Restriction = "High sugar content" → ИСКЛЮЧЕН (содержит "sugar")
```

---

## 4. Интеграция в генерацию плана питания

### 4.1. Последовательность операций

```
1. Получение UserProfile
   ↓
2. Парсинг MedicalConditions → UserMedicalRestrictionsDto
   ↓
3. Получение всех продуктов из БД
   ↓
4. Применение фильтров → разрешенные продукты
   ↓
5. Генерация плана питания только из разрешенных продуктов
   ↓
6. Расчет калорий и БЖУ с учетом ограничений
```

### 4.2. Пример полного цикла

**Входные данные:**
- UserId = 1
- MedicalConditions = `{"Allergens": ["Eggs"], "MedicalConditions": ["Diabetes"], "DietaryRestriction": "Vegetarian"}`

**Шаг 1: Парсинг**
```csharp
var restrictions = MedicalRestrictionsParser.ParseMedicalConditions(userProfile.MedicalConditions);
// restrictions.Allergens = ["Eggs"]
// restrictions.MedicalConditions = ["Diabetes"]
// restrictions.DietaryRestriction = Vegetarian
```

**Шаг 2: Получение продуктов**
```csharp
var allProducts = await _productRepository.GetAllAsync();
// 100 продуктов в БД
```

**Шаг 3: Применение фильтров**
```csharp
var allowedProducts = ProductFilterHelper.FilterProductsByRestrictions(allProducts, restrictions);
// После фильтра по аллергенам: 95 продуктов (исключены продукты с "Eggs")
// После фильтра по вегетарианству: 80 продуктов (исключено мясо)
// После фильтра по диабету: 75 продуктов (исключены продукты с сахаром)
// Итого: 75 разрешенных продуктов
```

**Шаг 4: Генерация плана**
```csharp
var mealPlan = await GenerateMealPlanAsync(userId, date, macroTargets, allowedProducts);
// План генерируется только из 75 разрешенных продуктов
```

---

## 5. Особенности реализации

### 5.1. Производительность

- **Ленивая оценка (Lazy Evaluation):** LINQ использует `IEnumerable`, поэтому фильтры применяются только при итерации
- **Каскадная фильтрация:** каждый фильтр уменьшает количество продуктов для следующего фильтра
- **Кэширование:** парсинг `MedicalConditions` можно кэшировать на уровне сессии пользователя

### 5.2. Расширяемость

- **Добавление новых диетических ограничений:** добавить новый case в `FilterProductsByDietaryRestrictions`
- **Добавление медицинских состояний:** добавить запись в `MedicalRestrictionsMapper.MedicalConditionToProducts`
- **Новые аллергены:** автоматически поддерживаются через проверку строки

### 5.3. Обработка ошибок

- **Некорректный JSON:** fallback на простой парсинг
- **Неизвестное медицинское состояние:** игнорируется (не влияет на фильтрацию)
- **Пустые данные:** возвращаются все продукты (нет ограничений)

---

## 6. Примеры использования

### Пример 1: Пользователь с аллергией на яйца

```csharp
// Входные данные
var medicalConditions = "{\"Allergens\": [\"Eggs\"]}";

// Парсинг
var restrictions = MedicalRestrictionsParser.ParseMedicalConditions(medicalConditions);

// Фильтрация
var products = await _productRepository.GetAllAsync();
var filtered = ProductFilterHelper.FilterProductsByRestrictions(products, restrictions);

// Результат: все продукты кроме тех, где Allergens содержит "Eggs"
```

### Пример 2: Веган с диабетом

```csharp
// Входные данные
var medicalConditions = "{\"MedicalConditions\": [\"Diabetes\"], \"DietaryRestriction\": \"Vegan\"}";

// Парсинг
var restrictions = MedicalRestrictionsParser.ParseMedicalConditions(medicalConditions);

// Фильтрация
var products = await _productRepository.GetAllAsync();
var filtered = ProductFilterHelper.FilterProductsByRestrictions(products, restrictions);

// Результат: 
// - Исключены все продукты животного происхождения (Vegan)
// - Исключены продукты с сахаром/медом (Diabetes)
```

### Пример 3: Без ограничений

```csharp
// Входные данные
var medicalConditions = null;

// Парсинг
var restrictions = MedicalRestrictionsParser.ParseMedicalConditions(medicalConditions);
// restrictions = пустой DTO

// Фильтрация
var products = await _productRepository.GetAllAsync();
var filtered = ProductFilterHelper.FilterProductsByRestrictions(products, restrictions);

// Результат: все продукты (фильтры не применяются)
```

---

## 7. Визуальная схема

```
┌─────────────────────────────────────────────────────────────┐
│                    UserProfile.MedicalConditions            │
│              (строка: JSON или через запятую)               │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│         MedicalRestrictionsParser.ParseMedicalConditions()  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Попытка JSON десериализации                         │  │
│  │  ↓ (ошибка)                                          │  │
│  │  Простой парсинг через запятую                       │  │
│  └──────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              UserMedicalRestrictionsDto                     │
│  • Allergens: List<string>                                  │
│  • MedicalConditions: List<string>                          │
│  • DietaryRestriction: DietaryRestrictionType?              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│         ProductFilterHelper.FilterProductsByRestrictions()  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  [1] Фильтр по аллергенам                            │  │
│  │      FilterProductsByAllergens()                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                            ↓                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  [2] Фильтр по диетическим ограничениям              │  │
│  │      FilterProductsByDietaryRestrictions()           │  │
│  └──────────────────────────────────────────────────────┘  │
│                            ↓                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  [3] Фильтр по медицинским состояниям                │  │
│  │      FilterProductsByMedicalConditions()             │  │
│  │      → MedicalRestrictionsMapper.ShouldExcludeProduct│  │
│  └──────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              Разрешенные продукты                           │
│         (готовы для генерации плана питания)                │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. Рекомендации по использованию

1. **При создании/обновлении UserProfile:**
   - Используйте JSON формат для `MedicalConditions` (более структурированный)
   - Валидируйте аллергены и медицинские состояния перед сохранением

2. **При генерации плана питания:**
   - Всегда парсите `MedicalConditions` перед фильтрацией
   - Кэшируйте результат парсинга на время генерации плана
   - Логируйте количество исключенных продуктов для отладки

3. **При добавлении новых продуктов:**
   - Заполняйте поле `Allergens` для продуктов с известными аллергенами
   - Используйте поле `Restriction` для дополнительной информации (например, "High sugar content")

4. **Расширение системы:**
   - Для новых медицинских состояний добавьте маппинг в `MedicalRestrictionsMapper`
   - Для новых диетических ограничений добавьте новый enum и метод фильтрации

