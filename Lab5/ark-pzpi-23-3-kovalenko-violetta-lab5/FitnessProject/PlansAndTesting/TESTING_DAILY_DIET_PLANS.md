# Руководство по тестированию генерации дневного плана питания

## Подготовка к тестированию

### Шаг 1: Проверить, что есть пользователь и профиль

1. Открой Swagger (`/swagger`) и найдите блок `Users` и `UserProfiles`.
2. Убедись, что есть хотя бы один пользователь:
   - **GET** `/api/users`
3. Убедись, что для этого пользователя есть `UserProfile`:
   - **GET** `/api/userprofiles`

**Важно**:  
Для генерации плана у пользователя в `UserProfile` должны быть заполнены:
- `CurrentWeightKg`
- `HeightCm`
- `Sex`
- `ActivityLevel`
- (желательно) `BirthDate` — иначе возраст посчитать нельзя.

### Шаг 2: Проверить, что есть рецепты и продукты

1. В Swagger:
   - **GET** `/api/products` — список продуктов
   - **GET** `/api/recipes` — список рецептов
2. Рецепты должны иметь:
   - адекватные калории/БЖУ (`RecipeCaloriesPerPortion`, `RecipeProteinPerPortion`, `RecipeFatPerPortion`, `RecipeCarbsPerPortion`)
   - привязанные продукты (`RecipeProducts`)

Без рецептов генерация плана либо вернёт пустое меню, либо не сможет подобрать блюда.

### Шаг 3: Подготовка данных для конкретных сценариев (если сидер выключен)

Если ты не используешь стандартный сидер `DbInitializer` или хочешь вручную воспроизвести данные для конкретного кейса, можно подготовить их через API.

#### 3.1. Пользователь с диабетом + веганство

**Цель:** проверить, что при диабете и `DietaryRestriction = Vegan` система исключает:
- продукты с сахаром / высоким ГИ,
- животные продукты (мясо, рыба, молочка, яйца),
- рецепты на их основе.

1. Создай пользователя:

```json
POST /api/users
{
  "email": "diabetic.veg@example.com",
  "password": "password123",
  "locale": "uk-UA",
  "role": 0
}
```

2. Найди его `userId` через `GET /api/users`.

3. Создай профиль:

```json
POST /api/userprofiles
{
  "userId":21,
  "firstName": "Oleh",
  "lastName": "Diabetic",
  "sex": 0,
  "heightCm": 178.0,
  "currentWeightKg": 82.0,
  "activityLevel": 0,
  "goalType": 0,
  "preferredUnits": 0,
  "birthDate": "1988-01-05T00:00:00",
  "medicalConditions": "{\"Allergens\":[\"Sugar\"],\"MedicalConditions\":[\"Diabetes\",\"Hypertension\"],\"DietaryRestriction\":\"Vegan\"}"
}
```

4. Убедись, что в продуктах есть как минимум:
   - сладкие/высокий ГИ (`Brown Rice`, `Banana`, `Peanut Butter`),
   - веганские белковые (`Tofu`, `Lentils`, `Chickpeas`),
   - овощи/фрукты (`Broccoli`, `Spinach`, `Apple`, `Blueberries`).

Если чего-то не хватает — добавь через:

```json
POST /api/products
{
  "productName": "Tofu",
  "caloriesPer100g": 76,
  "proteinPer100g": 8,
  "fatPer100g": 4.8,
  "carbsPer100g": 1.9,
  "restriction": "soy,plant_protein",
  "allergens": null,
  "unit": "g"
}
```

5. Создай хотя бы один веганский рецепт (если не используешь сидер):

```json
POST /api/recipes
{
  "recipeName": "Vegan Lentil Stew",
  "recipeInstructions": "Simmer lentils with vegetables and spices.",
  "recipeCaloriesPerPortion": 320,
  "recipeFatPerPortion": 5,
  "recipeCarbsPerPortion": 45,
  "recipeProteinPerPortion": 18,
  "recipeProductsGrams": 300
}
```

и добавь продукты к рецепту через `POST /api/recipeproducts`.

После этого в `POST /api/dailydietplans/generate` используй `userId` этого пользователя и смотри, что в меню нет мяса/рыбы/молочки/яиц и “сахарных” продуктов.

#### 3.2. Пользователь с целиакией (GlutenFree)

**Цель:** проверить, что при `CeliacDisease` и `DietaryRestriction = GlutenFree` система исключает продукты и рецепты с глютеном.

1. Создай пользователя и профиль:

```json
POST /api/userprofiles
{
  "userId": 5,
  "firstName": "Anna",
  "lastName": "GlutenFree",
  "sex": 1,
  "heightCm": 170.0,
  "currentWeightKg": 65.0,
  "activityLevel": 1,
  "goalType": 2,
  "preferredUnits": 0,
  "birthDate": "1995-04-12T00:00:00",
  "medicalConditions": "{\"Allergens\":[\"Gluten\"],\"MedicalConditions\":[\"CeliacDisease\"],\"DietaryRestriction\":\"GlutenFree\"}"
}
```

2. Убедись, что в продуктах есть:
   - `Whole Wheat Bread` / `Oatmeal` / `Brown Rice` с `restriction`/`allergens`, содержащими `gluten`/`wheat`,
   - `Gluten-Free Bread` без аллергенов и с `restriction = "gluten_free"`.

3. Создай 1–2 рецепта с глютеном и 1–2 без него и проверь:
   - что в сгенерированном плане для этого пользователя **нет** рецептов с продуктами, где `Allergens` содержит `Gluten` или `restriction` включает `gluten`, `wheat` и т.п.

#### 3.3. Пользователь без ограничений

**Цель:** убедиться, что при пустых `MedicalConditions` и отсутствии `DietaryRestriction` система использует весь пул рецептов без фильтрации.

1. Создай профиль с `MedicalConditions = null` или `"None"` и без `GoalType` (или `WeightMaintenance`).
2. Сгенерируй план и сравни:
   - для этого пользователя будут доступны все типы блюд,
   - для пользователей из сценариев 3.1 и 3.2 — набор рецептов будет уже.

---

## Эндпоинт 1: Генерация нового плана

### 1.1. Успешная генерация плана

**Эндпоинт**: `POST /api/dailydietplans/generate`  
**Описание**: Генерирует новый дневной план для пользователя на указанную дату.

**Пример запроса (минимальный)**:

```json
{
  "userId": 1
}
```

**Что происходит**:
- Если `date` не передан, используется сегодняшняя дата (`DateTime.Today`).
- Сервис `MealPlanGeneratorService`:
  - считает BMR и TDEE по `UserProfile`
  - считает целевые калории и БЖУ (через `CalorieCalculator` и `MacroNutrientsCalculator`)
  - распределяет калории по приёмам пищи
  - подбирает рецепты с учётом:
    - медицинских ограничений (`MedicalRestrictionsParser`, `ProductFilterHelper`)
    - недавних рецептов (не повторяет за последние 7 дней)
  - рассчитывает порции (`PortionCalculator`) и создаёт:
    - `DailyDietPlan`
    - `Meals`
    - `MealRecipes` с `PortionsMetadata`

**Ожидаемый ответ**: `201 Created` с `DailyDietPlanResponseDto`, например:

```json
{
  "dailyDietPlanId": 3,
  "userId": 1,
  "templateDietPlanId": null,
  "dailyDietPlanName": "Meal Plan for 2025-12-15",
  "dailyPlanDescription": null,
  "dailyPlanCalories": 2300.0,
  "dailyPlanFat": 70.0,
  "dailyPlanCarbs": 260.0,
  "dailyPlanProtein": 140.0,
  "dailyPlanNumberOfMeals": 4,
  "dailyPlanStatus": 0,
  "dailyPlanCreatedAt": "2025-12-15T14:30:00Z"
}
```

### 1.2. Генерация на конкретную дату

**Пример запроса**:

```json
{
  "userId": 1,
  "date": "2025-12-20T00:00:00",
  "templateDietPlanId": null
}
```

**Ожидаемый результат**:
- Создаётся новый план с именем вида `"Meal Plan for 2025-12-20"`.
- Статус: `Planned`.

### 1.3. Ошибки

#### 1.3.1. Не существует `UserProfile` для `userId`

**Пример запроса**:

```json
{
  "userId": 9999
}
```

**Ожидаемый ответ**: `400 Bad Request`  
С текстом ошибки вида:

```json
{
  "error": "UserProfile for UserId 9999 not found"
}
```

#### 1.3.2. Не валидный `userId`

**Пример запроса**:

```json
{
  "userId": 0
}
```

**Ожидаемый результат**:
- Скорее всего `400 Bad Request` или ошибка валидации, если ты добавишь DataAnnotations в модель.

---

## Эндпоинт 2: Получение меню на день

### 2.1. Успешное получение меню

**Эндпоинт**: `GET /api/dailydietplans/{id}/meals`  
**Описание**: Возвращает план + список приёмов пищи с полной детализацией.

Сначала сгенерируй план (см. шаг 1), возьми `dailyDietPlanId` из ответа.

**Пример запроса**:

`GET /api/dailydietplans/3/meals`

**Ожидаемый ответ**: `200 OK` с `DailyDietPlanMealsDto`, например:

```json
{
  "dailyDietPlanId": 3,
  "dailyDietPlanName": "Meal Plan for 2025-12-15",
  "dailyPlanCreatedAt": "2025-12-15T14:30:00Z",
  "meals": [
    {
      "mealId": 10,
      "dailyDietPlanId": 3,
      "mealTime": 0,
      "mealOrder": 1,
      "mealTargetCalories": 600.0,
      "mealTargetFat": 20.0,
      "mealTargetCarbs": 70.0,
      "mealTargetProtein": 35.0,
      "dailyDietPlan": {
        "dailyDietPlanId": 3,
        "userId": 1,
        "templateDietPlanId": null,
        "dailyDietPlanName": "Meal Plan for 2025-12-15",
        "dailyPlanDescription": null,
        "dailyPlanCalories": 2300.0,
        "dailyPlanFat": 70.0,
        "dailyPlanCarbs": 260.0,
        "dailyPlanProtein": 140.0,
        "dailyPlanNumberOfMeals": 4,
        "dailyPlanStatus": 0,
        "dailyPlanCreatedAt": "2025-12-15T14:30:00Z"
      },
      "mealRecipes": [
        {
          "mealId": 10,
          "recipeId": 2
        }
      ],
      "recommendations": []
    }
  ]
}
```

**Что важно проверить**:
- `dailyDietPlanId`, `dailyDietPlanName`, `dailyPlanCreatedAt` совпадают с планом из шага 1.
- Количество `meals` соответствует `DailyPlanNumberOfMeals`.
- Внутри каждого `Meal`:
  - корректный `MealTime` (завтрак/обед/ужин/перекус),
  - `MealTargetCalories` и БЖУ выглядят реалистично.

### 2.2. План не найден

**Пример запроса**:

`GET /api/dailydietplans/9999/meals`

**Ожидаемый ответ**: `404 Not Found`

---

## Эндпоинт 3: Перегенерация плана

### 3.1. Успешная перегенерация

**Эндпоинт**: `POST /api/dailydietplans/{id}/regenerate`  
**Описание**: Создаёт **новый** план на ту же дату и для того же пользователя, что и исходный.

**Шаги**:
1. Сгенерируй первый план через `POST /api/dailydietplans/generate`, запомни `dailyDietPlanId` (например, `3`).
2. Вызови:

`POST /api/dailydietplans/3/regenerate`

**Ожидаемый ответ**: `201 Created` с новым `DailyDietPlanResponseDto`, например:

```json
{
  "dailyDietPlanId": 4,
  "userId": 1,
  "templateDietPlanId": null,
  "dailyDietPlanName": "Meal Plan for 2025-12-15",
  "dailyPlanDescription": null,
  "dailyPlanCalories": 2300.0,
  "dailyPlanFat": 70.0,
  "dailyPlanCarbs": 260.0,
  "dailyPlanProtein": 140.0,
  "dailyPlanNumberOfMeals": 4,
  "dailyPlanStatus": 0,
  "dailyPlanCreatedAt": "2025-12-15T14:45:00Z"
}
```

**Что проверить**:
- `dailyDietPlanId` изменился (создан новый план, старый не затирается).
- `userId` и `templateDietPlanId` совпадают с исходным планом.
- Дата в имени плана и логике — та же (используется `DailyPlanCreatedAt.Date` исходного плана).
- Можно дополнительно вызвать:
  - `GET /api/dailydietplans/4/meals` и сравнить меню с предыдущим планом — рецепты **могут отличаться**, т.к. логика генерации учитывает недавние рецепты.

### 3.2. Перегенерация несуществующего плана

**Пример запроса**:

`POST /api/dailydietplans/9999/regenerate`

**Ожидаемый ответ**: `404 Not Found`

---

## Рекомендуемая последовательность ручного теста (через Swagger)

1. **Проверить наличие пользователя и профиля**:
   - `GET /api/users`
   - `GET /api/userprofiles`
2. **Проверить наличие продуктов и рецептов**:
   - `GET /api/products`
   - `GET /api/recipes`
3. **Сгенерировать план**:
   - `POST /api/dailydietplans/generate`
   - В теле: `{ "userId": 1 }`
   - Убедиться, что вернулся `201` и корректный `DailyDietPlanResponseDto`.
4. **Посмотреть меню на день**:
   - `GET /api/dailydietplans/{id}/meals`
   - Проверить список `Meals` и `MealRecipes`.
5. **Перегенерировать план**:
   - `POST /api/dailydietplans/{id}/regenerate`
   - Убедиться, что создан новый план с новым Id.
6. **Негативные кейсы**:
   - Вызовы `GET /api/dailydietplans/9999/meals` и `POST /api/dailydietplans/9999/regenerate` → `404`.

После прохождения этих шагов можно считать, что функционал **генерации дневного плана** и **API-эндпоинты** работают корректно в типичных и основных ошибочных сценариях.


