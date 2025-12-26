# Розробка функцій адміністрування серверної частини програмної системи

## Загальна архітектура

Система адміністрування побудована на основі ролей користувачів та механізму автентифікації через Basic Authentication. Реалізовано два рівні доступу: **Admin** (контент-менеджер) та **SuperAdmin** (системний адміністратор), кожен з яких має чітко визначені права доступу до функцій системи.

## 1. Система автентифікації та авторизації

### 1.1. Basic Authentication Handler

**Розташування:** `FitnessProject/BLL/Services/BasicAuthenticationHandler.cs`

**Призначення:** Реалізує механізм Basic Authentication для захисту адміністративних ендпоінтів.

**Алгоритм роботи:**

1. **Перевірка заголовка Authorization:**
   - Система перевіряє наявність заголовка `Authorization` у HTTP-запиті
   - Якщо заголовок відсутній, повертається `AuthenticateResult.NoResult()`

2. **Парсинг credentials:**
   - Заголовок парситься як `Basic <base64_encoded_credentials>`
   - Декодується base64 рядок у формат `email:password`
   - Перевіряється коректність формату (наявність двокрапки)

3. **Валідація користувача:**
   - За email знаходиться користувач у базі даних через `IUserRepository.GetByEmailAsync(email)`
   - Якщо користувач не знайдений, повертається помилка автентифікації

4. **Перевірка пароля:**
   - Пароль хешується за допомогою SHA256
   - Генеруються два формати хешу: Base64 та Hex (для сумісності з різними версіями БД)
   - Хеш порівнюється зі збереженим у базі даних (`user.PasswordHash`)
   - Підтримуються обидва формати для забезпечення міграційної сумісності

5. **Формування claims:**
   - Створюються claims для ідентифікації користувача:
     - `ClaimTypes.NameIdentifier` — ID користувача
     - `ClaimTypes.Name` — email користувача
     - `ClaimTypes.Role` — роль користувача (User, Admin, SuperAdmin)
     - `locale` — локаль користувача (uk-UA або en-US)

6. **Нормалізація локалі:**
   - Локаль нормалізується до стандартних форматів:
     - `uk`/`ua` → `uk-UA`
     - `en` → `en-US`
     - За замовчуванням → `uk-UA`

**Формула хешування пароля:**
```
hash = SHA256(UTF8(password))
base64Hash = Base64Encode(hash)
hexHash = HexEncode(hash).ToLower()
```

**Логування:**
- Детальне логування всіх етапів автентифікації для діагностики
- Логування помилок пароля з маскованими значеннями для безпеки

### 1.2. Налаштування автентифікації в Program.cs

**Розташування:** `FitnessProject/Program.cs`

**Конфігурація:**

1. **Реєстрація схеми Basic Authentication:**
   ```csharp
   builder.Services.AddAuthentication(options =>
   {
       options.DefaultAuthenticateScheme = "Basic";
       options.DefaultChallengeScheme = "Basic";
   }).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
   ```

2. **Реєстрація авторизації:**
   ```csharp
   builder.Services.AddAuthorization();
   ```

3. **Middleware pipeline:**
   - `app.UseRequestLocalization()` — встановлення локалі
   - `app.UseAuthentication()` — обробка автентифікації
   - `app.UseAuthorization()` — перевірка прав доступу

4. **Swagger конфігурація:**
   - Додано підтримку Basic Authentication у Swagger UI
   - Користувач може ввести credentials безпосередньо в Swagger

### 1.3. Ролі користувачів

**Розташування:** `FitnessProject/Enums/UserRole.cs`

**Визначені ролі:**
- `User` — звичайний користувач (не має доступу до адмін-панелі)
- `Admin` — адміністратор контенту (читання + обмежений доступ)
- `SuperAdmin` — системний адміністратор (повний доступ)

## 2. Функції Admin (контент-менеджер)

Admin має доступ **тільки для читання** до більшості даних системи та повний доступ до перегляду інформації.

### 2.1. Управління продуктами (тільки читання)

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminProductsController.cs`

**Ендпоінти:**
- `GET /api/admin/products` — отримання списку всіх продуктів
- `GET /api/admin/products/{id}` — отримання продукту за ID
- `GET /api/admin/products/{id}/details` — отримання детальної інформації про продукт

**Авторизація:** `[Authorize(Roles = "Admin,SuperAdmin")]` на рівні класу

**Функціональність:**
- Перегляд всіх продуктів у системі
- Перегляд детальної інформації про продукт (назва, калорійність, макронутрієнти, теги, алергени)
- Локалізовані повідомлення про помилки через `IStringLocalizer<SharedResources>`

### 2.2. Управління рецептами (тільки читання)

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminRecipesController.cs`

**Ендпоінти:**
- `GET /api/admin/recipes` — отримання списку всіх рецептів
- `GET /api/admin/recipes/{id}` — отримання рецепту за ID
- `GET /api/admin/recipes/{id}/details` — отримання детальної інформації про рецепт (з продуктами)

**Авторизація:** `[Authorize(Roles = "Admin,SuperAdmin")]` на рівні класу

**Функціональність:**
- Перегляд всіх рецептів у системі
- Перегляд детальної інформації про рецепт (назва, опис, продукти, порції, макронутрієнти)
- Локалізовані повідомлення про помилки

### 2.3. Перегляд користувачів

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminUsersController.cs`

**Ендпоінти:**
- `GET /api/admin/users` — отримання списку всіх користувачів
- `GET /api/admin/users/{id}` — отримання користувача за ID
- `GET /api/admin/users/{id}/details` — отримання детальної інформації про користувача (з профілем)

**Авторизація:** `[Authorize(Roles = "Admin,SuperAdmin")]` на рівні класу

**Функціональність:**
- Перегляд списку користувачів системи
- Перегляд детальної інформації про користувача (email, роль, профіль, обмеження)

### 2.4. Перегляд пристроїв

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminDevicesController.cs`

**Ендпоінти:**
- `GET /api/admin/devices` — отримання списку всіх пристроїв
- `GET /api/admin/devices/{id}` — отримання пристрою за ID

**Авторизація:** `[Authorize(Roles = "Admin,SuperAdmin")]` на рівні класу

**Функціональність:**
- Перегляд всіх підключених пристроїв
- Перегляд інформації про пристрій (тип, модель, користувач)

### 2.5. Перегляд планів дієт (тільки читання)

**Контролер:** `FitnessProject/Controllers/DailyDietPlansController.cs`

**Ендпоінти (доступні Admin):**
- `GET /api/dailydietplans` — отримання списку всіх планів
- `GET /api/dailydietplans/{id}` — отримання плану за ID
- `GET /api/dailydietplans/{id}/details` — отримання детального плану
- `GET /api/dailydietplans/{id}/meals` — отримання прийомів їжі для плану

**Авторизація:** `[Authorize(Roles = "Admin,SuperAdmin")]` на рівні класу

**Функціональність:**
- Перегляд всіх згенерованих планів дієт
- Перегляд детальної інформації про план (калорії, макронутрієнти, прийоми їжі, рецепти)

### 2.6. Системна інформація

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminOverviewController.cs`

**Ендпоінт:**
- `GET /api/admin/overview` — отримання загальної інформації про систему

**Авторизація:** `[Authorize(Roles = "Admin,SuperAdmin")]` на рівні класу

**Повертає інформацію:**
- `BuildVersion` — версія збірки (з AssemblyInformationalVersion або Version)
- `Commit` — хеш коміту (з конфігурації `BUILD_COMMIT`)
- `Database` — назва бази даних (парситься зі строки підключення)
- `Connection` — маскована строка підключення (без паролів)
- `LastMigration` — остання застосована міграція EF Core
- `Uptime` — час роботи сервера у форматі `HH:mm:ss`
- `StartedAtUtc` — час запуску сервера (UTC)

**Алгоритм отримання версії:**
1. Спробувати отримати `AssemblyInformationalVersion`
2. Якщо відсутній, використати `Assembly.Version`
3. Якщо обидва відсутні, повернути "n/a"

**Маскування строки підключення:**
- Видаляються всі параметри, що починаються з `Password` або `Pwd`
- Залишаються тільки безпечні параметри (Server, Database, TrustedConnection тощо)

## 3. Функції SuperAdmin (системний адміністратор)

SuperAdmin має **повний доступ** до всіх функцій Admin, а також додаткові права на створення, редагування та видалення даних.

### 3.1. CRUD операції з продуктами

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminProductsController.cs`

**Додаткові ендпоінти (тільки SuperAdmin):**
- `POST /api/admin/products` — створення нового продукту
- `PUT /api/admin/products/{id}` — оновлення продукту
- `DELETE /api/admin/products/{id}` — видалення продукту

**Авторизація:** `[Authorize(Roles = "SuperAdmin")]` на методі

**Функціональність:**
- Створення продуктів з повною інформацією (назва, калорійність, макронутрієнти, теги, алергени)
- Редагування існуючих продуктів
- Видалення продуктів з системи
- Валідація даних (перевірка ID, наявність обов'язкових полів)
- Локалізовані повідомлення про помилки та успіх

### 3.2. CRUD операції з рецептами

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminRecipesController.cs`

**Додаткові ендпоінти (тільки SuperAdmin):**
- `POST /api/admin/recipes` — створення нового рецепту
- `PUT /api/admin/recipes/{id}` — оновлення рецепту
- `DELETE /api/admin/recipes/{id}` — видалення рецепту

**Авторизація:** `[Authorize(Roles = "SuperAdmin")]` на методі

**Функціональність:**
- Створення рецептів з повною інформацією (назва, опис, продукти, порції)
- Редагування існуючих рецептів
- Видалення рецептів з системи
- Валідація даних та зв'язків з продуктами

### 3.3. Системні CRUD операції з планами дієт

**Контролер:** `FitnessProject/Controllers/DailyDietPlansController.cs`

**Додаткові ендпоінти (тільки SuperAdmin):**
- `POST /api/dailydietplans` — створення плану вручну
- `PUT /api/dailydietplans/{id}` — оновлення плану
- `DELETE /api/dailydietplans/{id}` — видалення плану
- `POST /api/dailydietplans/generate` — генерація нового плану
- `POST /api/dailydietplans/{id}/regenerate` — перегенерація існуючого плану
- `POST /api/dailydietplans/{id}/check-corrections` — перевірка корекцій
- `POST /api/dailydietplans/{id}/apply-correction` — застосування корекції

**Авторизація:** `[Authorize(Roles = "SuperAdmin")]` на методі

**Функціональність:**

1. **Генерація плану:**
   - Викликає `IMealPlanGeneratorService.GenerateMealPlanAsync()`
   - Створює повний план з прийомами їжі та рецептами
   - Повертає детальну інформацію про згенерований план

2. **Перегенерація плану:**
   - Видаляє старий план та генерує новий для того ж користувача та дати
   - Зберігає оригінальні параметри (userId, date, templateDietPlanId)

3. **Перевірка корекцій:**
   - Викликає `IDietCorrectionService.CheckAndSuggestCorrectionsAsync()`
   - Аналізує активність та сон користувача
   - Повертає список рекомендацій для корекції дієти

4. **Застосування корекції:**
   - Викликає `IDietCorrectionService.ApplyCorrectionAsync()`
   - Оновлює калорійність та макронутрієнти плану
   - Перебалансовує прийоми їжі та порції рецептів
   - Встановлює прапорець `IsCorrected = true`

### 3.4. Управління локалізацією

**Контролер:** `FitnessProject/Controllers/AdminControllers/AdminLocalizationController.cs`

**Сервіс:** `FitnessProject/BLL/Services/LocalizationAdminService.cs`

**Ендпоінти (тільки SuperAdmin):**
- `GET /api/admin/localization/keys?culture=uk` — отримання всіх ключів локалізації для культури
- `GET /api/admin/localization/missing?base=uk&compare=en` — знаходження відсутніх ключів у порівнянні з базовою культурою
- `PUT /api/admin/localization/keys` — оновлення значення ключа локалізації
- `POST /api/admin/localization/export` — експорт всіх ключів локалізації (uk + en)
- `POST /api/admin/localization/import` — імпорт ключів локалізації

**Авторизація:** `[Authorize(Roles = "SuperAdmin")]` на рівні класу

**Підтримувані культури:**
- `uk` (українська) — основна мова
- `en` (англійська) — міжнародна мова

**Алгоритм роботи з .resx файлами:**

1. **Читання .resx файлів:**
   - Використовується `System.Xml.Linq.XDocument` для парсингу XML
   - Читаються всі елементи `<data>` з атрибутами `name` та значеннями `<value>`
   - Повертається `Dictionary<string, string>` (ключ → значення)

2. **Запис .resx файлів:**
   - Створюється валідний XML документ з необхідними заголовками:
     - `resmimetype`: `text/microsoft-resx`
     - `version`: `2.0`
     - `reader` та `writer`: посилання на .NET класи
   - Ключі сортуються за алфавітом для читабельності
   - Зберігається структура з `xml:space="preserve"` для збереження форматування

3. **Нормалізація культури:**
   - `uk`/`ua` → `uk`
   - `en` → `en`
   - Інші → помилка `ArgumentException`

**Шлях до ресурсів:**
```
{ContentRootPath}/Resources/Shared.{culture}.resx
```

**Функціональність:**

1. **Отримання ключів:**
   - Читає .resx файл для вказаної культури
   - Повертає всі ключі та їх значення

2. **Пошук відсутніх ключів:**
   - Порівнює ключі базової та порівняльної культур
   - Повертає ключі, які є в базовій, але відсутні в порівняльній

3. **Оновлення ключа:**
   - Читає поточний .resx файл
   - Оновлює значення ключа
   - Зберігає файл з оновленими даними

4. **Експорт:**
   - Експортує всі ключі для обох культур
   - Повертає структуру `{ Uk: {...}, En: {...} }`

5. **Імпорт:**
   - Приймає структуру з ключами для обох культур
   - Перезаписує відповідні .resx файли

## 4. Локалізація системи

### 4.1. Архітектура локалізації

**Ресурси:**
- `FitnessProject/Resources/Shared.resx` — базовий файл (fallback)
- `FitnessProject/Resources/Shared.uk.resx` — українські переклади
- `FitnessProject/Resources/Shared.en.resx` — англійські переклади

**Маркерний клас:** `FitnessProject/Resources/SharedResources.cs`

**Сервіс локалізації:** `IStringLocalizer<SharedResources>`

### 4.2. Налаштування локалізації в Program.cs

**Реєстрація:**
```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
```

**Конфігурація RequestLocalization:**
```csharp
var supportedCultures = new[] { new CultureInfo("uk-UA"), new CultureInfo("en-US") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("uk-UA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true
};
app.UseRequestLocalization(localizationOptions);
```

**Custom middleware для локалі з claims:**
```csharp
app.Use(async (context, next) =>
{
    var localeClaim = context.User?.FindFirst("locale")?.Value;
    if (!string.IsNullOrWhiteSpace(localeClaim))
    {
        var culture = new CultureInfo(localeClaim);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
    await next();
});
```

**Алгоритм визначення локалі:**
1. Перевіряється claim `locale` з профілю користувача (після автентифікації)
2. Якщо claim відсутній, використовується заголовок `Accept-Language`
3. За замовчуванням — `uk-UA`

### 4.3. Використання локалізації в контролерах

**Приклад:**
```csharp
private readonly IStringLocalizer<SharedResources> _localizer;

public AdminProductsController(IProductService productService, IStringLocalizer<SharedResources> localizer)
{
    _productService = productService;
    _localizer = localizer;
}

[HttpGet("{id}")]
public async Task<ActionResult<ProductResponseDto>> GetById(int id)
{
    var product = await _productService.GetByIdAsync(id);
    if (product == null)
        return NotFound(new { error = _localizer["Errors.NotFound"] });
    return Ok(product);
}
```

**Ключі локалізації:**
- `Errors.NotFound` — "Не знайдено" / "Not found"
- `Errors.BadRequest` — "Невірний запит" / "Bad request"
- `Errors.IdMismatch` — "ID не співпадає" / "ID mismatch"
- `Recommendations.DietCorrection.Suggest` — шаблон рекомендації
- `Recommendations.DietCorrection.Reason.*` — причини корекції

## 5. Конвертація одиниць виміру

### 5.1. Unit Conversion Service

**Розташування:** `FitnessProject/BLL/Services/UnitConversionService.cs`

**Інтерфейс:** `FitnessProject/BLL/Services/Interfaces/IUnitConversionService.cs`

**Призначення:** Конвертація між метричною та імперською системами виміру залежно від локалі користувача.

**Константи:**
- `GramsPerOunce = 28.3495` — грамів в одній унції
- `MillilitersPerFluidOunce = 29.5735` — мілілітрів в одній рідкій унції

**Методи:**

1. **DeterminePreferredUnits(string? locale):**
   - Визначає переважну систему одиниць на основі локалі
   - `en-US` / `en-GB` → `Imperial`
   - Інші → `Metric`

2. **ConvertWeight(decimal value, PreferredUnits from, PreferredUnits to, int precision = 1):**
   - Конвертує вагу між грамами та унціями
   - Формула: `ounces = grams / 28.3495` або `grams = ounces * 28.3495`
   - Округлення до вказаної точності

3. **ConvertVolume(decimal value, PreferredUnits from, PreferredUnits to, int precision = 1):**
   - Конвертує об'єм між мілілітрами та рідкими унціями
   - Формула: `fluidOunces = ml / 29.5735` або `ml = fluidOunces * 29.5735`
   - Округлення до вказаної точності

**Використання в DietCorrectionService:**
```csharp
var units = _unitConversionService.DeterminePreferredUnits(locale);
var protein = _unitConversionService.FormatWeight(deltas.ProteinDelta, units);
// Повертає "150.5 g" або "5.3 oz" залежно від локалі
```

## 6. Структура даних та DTO

### 6.1. DTO для адміністрування

**AdminOverviewDto:**
- `BuildVersion` — версія збірки
- `Commit` — хеш коміту
- `Database` — назва БД
- `Connection` — маскована строка підключення
- `LastMigration` — остання міграція
- `Uptime` — час роботи
- `StartedAtUtc` — час запуску

**LocalizationKeyUpdateDto:**
- `Key` — ключ локалізації
- `Culture` — культура (uk/en)
- `Value` — нове значення

**LocalizationExportDto:**
- `Uk` — Dictionary<string, string> для української
- `En` — Dictionary<string, string> для англійської

**LocalizationImportDto:**
- `Uk` — опціональний Dictionary для імпорту
- `En` — опціональний Dictionary для імпорту

## 7. Безпека та обмеження

### 7.1. Захист від несанкціонованого доступу

1. **Basic Authentication:**
   - Всі адмін-ендпоінти вимагають автентифікації
   - Credentials передаються через заголовок `Authorization: Basic <base64>`

2. **Role-Based Authorization:**
   - Використовується `[Authorize(Roles = "...")]` на рівні класу та методів
   - Admin має доступ тільки до GET операцій
   - SuperAdmin має доступ до всіх операцій

3. **Валідація даних:**
   - Перевірка ID на відповідність
   - Валідація обов'язкових полів
   - Обробка винятків з локалізованими повідомленнями

### 7.2. Логування та моніторинг

- Детальне логування автентифікації
- Логування помилок з маскованими даними
- Відстеження часу роботи сервера
- Моніторинг міграцій БД

## 8. Інтеграція з іншими модулями

### 8.1. Інтеграція з генерацією планів

- SuperAdmin може генерувати та перегенерувати плани через `MealPlanGeneratorService`
- Доступ до всіх параметрів генерації

### 8.2. Інтеграція з корекцією дієт

- SuperAdmin може перевіряти та застосовувати корекції через `DietCorrectionService`
- Доступ до рекомендацій та їх застосування

### 8.3. Інтеграція з локалізацією

- Всі повідомлення системи локалізовані
- SuperAdmin може керувати перекладами через `LocalizationAdminService`
- Автоматична конвертація одиниць виміру залежно від локалі

## 9. Послідовність виконання запитів

### 9.1. Типовий запит Admin

1. Користувач надсилає запит з заголовком `Authorization: Basic <credentials>`
2. `BasicAuthenticationHandler` перевіряє credentials
3. Створюються claims з роллю `Admin`
4. `AuthorizationMiddleware` перевіряє `[Authorize(Roles = "Admin,SuperAdmin")]`
5. Контролер виконує GET операцію
6. Повертається локалізована відповідь

### 9.2. Типовий запит SuperAdmin (CRUD)

1. Користувач надсилає POST/PUT/DELETE запит з credentials
2. Автентифікація та перевірка ролі `SuperAdmin`
3. Валідація даних у контролері
4. Виклик відповідного сервісу (Create/Update/Delete)
5. Збереження змін у БД через репозиторій
6. Повернення локалізованої відповіді (успіх/помилка)

## 10. Технічні деталі реалізації

### 10.1. Dependency Injection

Всі сервіси та репозиторії зареєстровані в `Program.cs`:
- `IUserRepository` → `UserRepository`
- `ILocalizationAdminService` → `LocalizationAdminService`
- `IUnitConversionService` → `UnitConversionService`
- Контролери отримують залежності через конструктор

### 10.2. Обробка помилок

- Використання `try-catch` блоків у контролерах
- Повернення стандартних HTTP статусів (400, 404, 500)
- Локалізовані повідомлення про помилки
- Логування винятків для діагностики

### 10.3. Swagger UI

- Налаштована підтримка Basic Authentication
- Можливість тестування всіх ендпоінтів безпосередньо з Swagger
- Автоматичне додавання заголовка `Authorization` до запитів

## 11. Тестування

### 11.1. Технології та інструменти

**Фреймворк тестування:** xUnit (.NET)
- Використовується для написання unit-тестів
- Підтримка асинхронних тестів через `Task`

**Мокування залежностей:** Moq
- Створення mock-об'єктів для репозиторіїв та сервісів
- Ізоляція тестованої логіки від зовнішніх залежностей

**In-Memory Database:** Microsoft.EntityFrameworkCore.InMemory (v8.0.6)
- Використовується для тестування сервісів, що працюють з БД
- Швидке виконання тестів без необхідності реальної БД

### 11.2. Покриття тестами

**Модулі, покриті unit-тестами:**

1. **Статистика (StatisticsService):**
   - Денна агрегація даних (телеметрія, сон, тренування)
   - Тижнева агрегація з розрахунком трендів
   - Порівняння з попереднім тижнем
   - Обробка edge cases (відсутність даних, ділення на нуль)

2. **Генерація планів дієт (MealPlanGeneratorService):**
   - Розрахунок калорій та макронутрієнтів
   - Фільтрація продуктів за обмеженнями
   - Вибір рецептів з урахуванням толерантності
   - Обмеження повторів рецептів
   - Створення коректних сутностей Meal та MealRecipe

3. **Корекція дієт (DietCorrectionService, ActivityMonitorService, SleepAnalysisService):**
   - Моніторинг активності (кроки, пульс, інтенсивність)
   - Аналіз якості сну
   - Розрахунок корекцій калорій та макронутрієнтів
   - Перебалансування прийомів їжі

4. **Фільтрація продуктів (ProductFilterHelper, MedicalRestrictionsMapper):**
   - Фільтрація за дієтичними обмеженнями (Vegan, Vegetarian, GlutenFree тощо)
   - Фільтрація за медичними станами (Diabetes, Hypertension, KidneyDisease)
   - Використання ProductTags enum для детермінованої фільтрації
   - Обробка комбінацій обмежень

5. **Парсинг медичних обмежень (MedicalRestrictionsParser):**
   - Парсинг JSON формату
   - Fallback на CSV формат при помилках
   - Обробка некоректних даних

### 11.3. Мануальне тестування

**Swagger UI:**
- Всі адмін-ендпоінти протестовані вручну через Swagger UI
- Перевірка Basic Authentication з різними ролями (Admin, SuperAdmin)
- Тестування CRUD операцій для продуктів, рецептів, планів дієт
- Перевірка локалізації повідомлень (uk/en)
- Тестування ендпоінтів локалізації (export/import, update keys)

**Тестові дані:**
- Створені тестові користувачі з ролями Admin та SuperAdmin
- Заповнена тестова база даних продуктами та рецептами
- Різноманітні профілі користувачів з різними обмеженнями

### 11.4. Підхід до тестування

**Unit-тести:**
- Використання моків для ізоляції тестованої логіки
- Покриття happy path та edge cases
- Перевірка коректності розрахунків та алгоритмів
- Тестування обробки помилок та валідації даних

**Інтеграційне тестування:**
- Мануальне тестування через Swagger UI
- Перевірка повного циклу запитів (автентифікація → авторизація → виконання)
- Тестування інтеграції між сервісами

**Тестові сценарії:**
- Доступ Admin до GET операцій
- Доступ SuperAdmin до всіх операцій
- Блокування неавторизованих користувачів
- Перевірка локалізації відповідей
- Валідація даних при створенні/оновленні

## Висновок

Система адміністрування забезпечує повний контроль над контентом та налаштуваннями системи через два рівні доступу: Admin (читання) та SuperAdmin (повний доступ). Реалізовано безпечну автентифікацію, локалізацію всіх повідомлень, конвертацію одиниць виміру та управління ресурсами локалізації. Архітектура дозволяє легко розширювати функціональність та додавати нові ролі та права доступу. Система протестована через unit-тести (xUnit, Moq) та мануальне тестування через Swagger UI, що забезпечує високу якість та надійність реалізації.

