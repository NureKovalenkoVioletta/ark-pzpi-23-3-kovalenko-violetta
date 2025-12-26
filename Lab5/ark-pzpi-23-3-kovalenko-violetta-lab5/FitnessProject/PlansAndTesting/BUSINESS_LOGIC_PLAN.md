# План реализации бизнес-логики фитнес-системы

## Приоритизация функций

### КРИТИЧЕСКИЙ ПРИОРИТЕТ (MVP - Минимально жизнеспособный продукт)
Эти функции необходимы для базовой работы системы.

### ВЫСОКИЙ ПРИОРИТЕТ
Функции, которые значительно улучшают пользовательский опыт и являются ключевыми для системы.

### СРЕДНИЙ ПРИОРИТЕТ
Важные функции, но могут быть реализованы после основных.

### НИЗКИЙ ПРИОРИТЕТ
Функции, которые улучшают систему, но не критичны для базовой работы.

---

## MF-1. Збір даних з фітнес браслета
**Приоритет: КРИТИЧЕСКИЙ** ⭐⭐⭐⭐⭐

### Описание
Система получает в реальном времени данные с фитнес-браслета: пульс, шаги, интенсивность тренировки, часы сна.

### Зависимости
- Нет (базовая функция)

### Детальный план реализации

#### 1. Проектирование структуры данных
- [x] Определить формат входящих данных от браслета (JSON)
  + **Как**: Создать DTO `TelemetryReceiveDto` в `FitnessProject/BLL/DTO/Telemetry/TelemetryReceiveDto.cs` ✅ СОЗДАНО
  + **Структура**: DeviceId, Timestamp, TelemetryType (enum: HeartRate, Steps, Sleep и др.), Value, Metadata (Dictionary<string, object>? для дополнительных данных, например для Sleep: TotalSleepMinutes, DeepSleepMinutes и т.д.)
  + **Использовать**: Data Annotations для валидации ([Required], [Range]), JsonPropertyName для маппинга
  + **Батчинг**: Создать `TelemetryReceiveBatchDto` с массивом Items (List<TelemetryReceiveDto>), ограничение до 1000 элементов ✅ СОЗДАНО
  + **Примечание**: Training будет отдельной структурой, реализуем позже
- [ ] Спроектировать модель данных для хранения телеметрии
  + **Как**: Использовать существующие сущности TelemetrySample, SleepRecord, TrainingSession
  + **Дополнить**: Проверить, что все необходимые поля есть (Timestamp, DeviceId, значения)
- [ ] Определить частоту сбора данных (реал-тайм, батчинг)
  + **Как**: Реализовать поддержку обоих режимов - это стандартный подход для фитнес-браслетов
  + **Реал-тайм**: Одиночные запросы POST /api/telemetry/receive - для критичных данных (пульс каждые 1-5 минут)
  + **Батчинг**: Массив данных в одном запросе (TelemetryReceiveBatchDto) - браслет накапливает данные и отправляет пакетами (например, каждые 15 минут или при достижении 50-100 записей)
  + **Преимущества**: Батчинг снижает нагрузку на сервер и батарею браслета, реал-тайм нужен для мониторинга в реальном времени
  + **Реализация**: Два эндпоинта - один принимает один объект, второй - массив объектов
- [ ] Спроектировать таблицу/сущность для временных меток
  + **Как**: Использовать Timestamp в существующих сущностях
  + **Добавить**: Индекс на Timestamp для быстрого поиска по датам

#### 2. API для приема данных
- [ ] Создать эндпоинт POST /api/telemetry/receive
  + **Где**: Создать `TelemetryController.cs` в `FitnessProject/Controllers/`
  + **Метод**: `[HttpPost("receive")] public async Task<IActionResult> ReceiveTelemetry([FromBody] TelemetryReceiveDto dto)`
  + **Логика**: Вызвать `ITelemetryProcessingService.ProcessTelemetryAsync(dto)`
  + **Ответ**: 200 OK с подтверждением или 400 BadRequest при ошибке
- [ ] Реализовать валидацию входящих данных
  + **Как**: Использовать Data Annotations в DTO (Required, Range, RegularExpression)
  - **Дополнительно**: Создать валидатор `TelemetryReceiveDtoValidator` используя FluentValidation (если используется)
  - **Проверки**: DeviceId существует, Timestamp в разумных пределах, Value в допустимом диапазоне
- [ ] Добавить аутентификацию устройства (DeviceId + токен)
  - **Как**: Создать middleware или атрибут `[DeviceAuth]` для проверки токена
  - **Логика**: Проверить DeviceId в БД, проверить токен из заголовка Authorization
  - **Альтернатива**: Использовать API Key в заголовке X-Device-Key
- [ ] Реализовать обработку ошибок и retry-логику
  - **Как**: Использовать try-catch в контроллере, логировать ошибки через ILogger
  - **Retry**: Для критических ошибок (например, временная недоступность БД) - возвращать 503 Service Unavailable
  - **Логирование**: Записывать все ошибки в лог с контекстом (DeviceId, Timestamp, ошибка)

#### 3. Обработка данных
- [ ] Создать сервис TelemetryProcessingService
  + **Где**: `FitnessProject/BLL/Services/TelemetryProcessingService.cs`
  + **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/ITelemetryProcessingService.cs`
  + **Зависимости**: ITelemetrySampleRepository, ISleepRecordRepository, ITrainingSessionRepository, IDeviceRepository, ILogger
  + **Методы**: `ProcessTelemetryAsync(TelemetryReceiveDto dto)`, `ProcessBatchAsync(List<TelemetryReceiveDto> dtos)`
- [ ] Реализовать парсинг данных по типам (пульс, шаги, сон)
  + **Как**: Использовать switch по TelemetryType из DTO
  + **Пульс/Шаги**: Создать TelemetrySample с TelemetryType.HeartRate или TelemetryType.Steps
  + **Сон**: Если TelemetryType = Sleep (или специальный тип), создать SleepRecord, парсить Metadata для TotalSleepMinutes, DeepSleepMinutes и т.д.
  + **Примечание**: Training будет отдельной структурой, реализуем позже
- [ ] Добавить нормализацию данных (единицы измерения, диапазоны)
  + **Как**: Создать helper-класс `TelemetryNormalizer` или методы в сервисе
  + **Нормализация**: Конвертировать единицы (например, шаги в метры если нужно), округлять значения
  + **Валидация диапазонов**: Пульс 40-220, шаги >= 0, время сна > 0
- [ ] Реализовать дедупликацию данных (защита от дублей)
  + **Как**: Перед сохранением проверять существование записи с тем же DeviceId, Timestamp, TelemetryType
  + **Метод**: `CheckDuplicateAsync(DeviceId, Timestamp, TelemetryType)` в репозитории
  + **Логика**: Если дубль найден - пропустить или обновить существующую запись

#### 4. Сохранение данных
+ [ ] Реализовать сохранение в TelemetrySamples (пульс, шаги)
  - **Как**: Использовать ITelemetrySampleRepository.AddAsync()
  - **Маппинг**: Создать TelemetrySample из TelemetryReceiveDto
  - **Поля**: DeviceId, Timestamp, TelemetryType, TelemetryValue
+ [ ] Реализовать сохранение в SleepRecords (данные о сне)
  - **Как**: Использовать ISleepRecordRepository.AddAsync()
  - **Маппинг**: Парсить Metadata JSON из DTO в поля SleepRecord
  - **Поля**: DeviceId, Date, TotalSleepMinutes, DeepSleepMinutes, LightSleepMinutes, AwakeMinutes, SleepQuality, StartTime, EndTime
+ [ ] Реализовать сохранение в TrainingSessions (тренировки)
  - **Как**: Использовать ITrainingSessionRepository.AddAsync()
  - **Маппинг**: Парсить Metadata JSON из DTO в поля TrainingSession
  - **Поля**: DeviceId, StartTime, EndTime, Type, Intensity, DurationInMin, CaloriesEstimated
+ [ ] Добавить транзакционность операций
  - **Как**: Использовать ApplicationDbContext.Database.BeginTransactionAsync()
  - **Логика**: Обернуть все операции сохранения в транзакцию, при ошибке - откат
  - **Использовать**: using var transaction = await _context.Database.BeginTransactionAsync()

#### 5. Обработка аномалий
- [ ] Реализовать валидацию диапазонов (пульс 40-220, шаги >= 0)
  - **Как**: Создать класс `TelemetryValidator` с методами ValidateHeartRate(), ValidateSteps()
  - **Логика**: Если значение вне диапазона - либо отклонить, либо пометить как аномальное и сохранить с флагом
  - **Константы**: Вынести пороговые значения в конфигурацию или константы класса
- [ ] Добавить логирование аномальных значений
  - **Как**: Использовать ILogger.LogWarning() при обнаружении аномалий
  - **Логировать**: DeviceId, Timestamp, Тип данных, Значение, Ожидаемый диапазон
  - **Метрики**: Можно добавить счетчик аномалий для мониторинга
- [ ] Реализовать фильтрацию выбросов (outliers)
  - **Как**: Использовать статистические методы (например, значения > 3 стандартных отклонений от среднего)
  - **Альтернатива**: Сравнение с историческими данными пользователя (если пульс > 50% от среднего - выброс)
  - **Действие**: Либо не сохранять, либо сохранять с флагом IsAnomaly = true

#### 6. Оптимизация производительности
- [ ] Реализовать батчинг для массовой вставки
  - **Как**: Использовать AddRangeAsync() вместо множественных AddAsync()
  - **Размер батча**: Обрабатывать по 100-500 записей за раз
  - **Метод**: Создать ProcessBatchAsync() который принимает List<TelemetryReceiveDto>
- [ ] Добавить индексы на таблицы для быстрого поиска
  - **Где**: В ApplicationDbContext.OnModelCreating() или через миграцию
  - **Индексы**: 
    - TelemetrySamples: (DeviceId, Timestamp), (TelemetryType, Timestamp)
    - SleepRecords: (DeviceId, Date)
    - TrainingSessions: (DeviceId, StartTime)
- [ ] Реализовать кэширование последних значений
  - **Как**: Использовать IMemoryCache для кэширования последних значений по DeviceId
  - **Ключ**: $"telemetry_last_{deviceId}_{telemetryType}"
  - **TTL**: 5-10 минут
  - **Использование**: Для быстрого доступа к последним значениям без запроса к БД

### Оценка сложности: ВЫСОКАЯ
### Время реализации: 3-4 дня

---

## MF-2. Формування персонального харчового плану
**Приоритет: КРИТИЧЕСКИЙ** ⭐⭐⭐⭐⭐

### Описание
Система генерирует индивидуальный план питания на основе статических (профиль пользователя) и динамических данных (активность, цели).

### Зависимости
- MF-1 (данные активности)
- UserProfile (статические данные)
- TemplateDietPlan (шаблоны)

### Детальный план реализации

#### 1. Алгоритм расчета базовых потребностей
- [ ] Реализовать расчет BMR (Basal Metabolic Rate) по формуле Миффлина-Сан Жеора
  - **Где**: Создать класс `CalorieCalculator` в `FitnessProject/BLL/Services/Helpers/CalorieCalculator.cs`
  - **Метод**: `CalculateBMR(decimal weight, decimal height, int age, Sex sex) : decimal`
  - **Формула для мужчин**: BMR = 10 × вес(кг) + 6.25 × рост(см) - 5 × возраст(лет) + 5
  - **Формула для женщин**: BMR = 10 × вес(кг) + 6.25 × рост(см) - 5 × возраст(лет) - 161
  - **Использовать**: Данные из UserProfile (CurrentWeightKg, HeightCm, BirthDate для расчета возраста, Sex)
- [ ] Реализовать расчет TDEE (Total Daily Energy Expenditure) с учетом уровня активности
  - **Метод**: `CalculateTDEE(decimal bmr, ActivityLevel activityLevel) : decimal`
  - **Множители**: 
    - Сидячий образ жизни (Sedentary): BMR × 1.2
    - Легкая активность (Light): BMR × 1.375
    - Умеренная активность (Moderate): BMR × 1.55
    - Высокая активность (Active): BMR × 1.725
    - Очень высокая активность (VeryActive): BMR × 1.9
  - **Использовать**: ActivityLevel из UserProfile

#### 2. Расчет макронутриентов по целям
- [ ] Реализовать логику для цели "Похудение" (дефицит калорий 15-20%)
  - **Метод**: `CalculateCaloriesForWeightLoss(decimal tdee) : decimal`
  - **Логика**: TDEE × 0.80-0.85 (дефицит 15-20%)
  - **Использовать**: GoalType.WeightLoss из Goals или UserProfile
- [ ] Реализовать логику для цели "Набор массы" (профицит калорий 10-15%)
  - **Метод**: `CalculateCaloriesForWeightGain(decimal tdee) : decimal`
  - **Логика**: TDEE × 1.10-1.15 (профицит 10-15%)
  - **Использовать**: GoalType.WeightGain
- [ ] Реализовать логику для цели "Поддержание" (баланс калорий)
  - **Метод**: `CalculateCaloriesForMaintenance(decimal tdee) : decimal`
  - **Логика**: TDEE (без изменений)
  - **Использовать**: GoalType.WeightMaintenance
- [ ] Реализовать распределение БЖУ:
  - **Класс**: Создать `MacroNutrientsCalculator` в `FitnessProject/BLL/Services/Helpers/MacroNutrientsCalculator.cs`
  - **Метод**: `CalculateMacros(decimal calories, decimal weight, GoalType goalType) : MacroNutrientsDto`
  - **Белки**: 25-30% от калорий (1.6-2.2 г/кг веса) = калории × 0.275 / 4 (4 ккал/г белка)
  - **Жиры**: 25-30% от калорий (0.8-1 г/кг веса) = калории × 0.275 / 9 (9 ккал/г жира)
  - **Углеводы**: 40-50% от калорий (остаток) = (калории - белки×4 - жиры×9) / 4 (4 ккал/г углеводов)
  - **DTO**: Создать `MacroNutrientsDto` с полями: Calories, ProteinGrams, FatGrams, CarbsGrams

#### 3. Учет медицинских ограничений
- [ ] Реализовать парсинг MedicalConditions из UserProfile
  - **Как**: MedicalConditions хранится как строка, нужно парсить (JSON или разделитель)
  - **Альтернатива**: Создать отдельную таблицу UserMedicalConditions с связью many-to-many
  - **Метод**: `ParseMedicalConditions(string medicalConditions) : List<string>` в helper-классе
- [ ] Создать маппинг ограничений на исключаемые продукты
  - **Где**: Создать класс `MedicalRestrictionsMapper` в `FitnessProject/BLL/Services/Helpers/`
  - **Метод**: `GetExcludedProducts(List<string> conditions) : List<int>` (список ProductId для исключения)
  - **Маппинг**: Словарь или конфигурация (например, "Диабет" -> исключить продукты с высоким ГИ)
  - **Хранение**: Можно в appsettings.json или в БД в таблице MedicalRestrictionProducts
- [ ] Реализовать фильтрацию продуктов по аллергенам
  - **Как**: Использовать поле Allergens в Product
  - **Метод**: `FilterProductsByAllergens(IEnumerable<Product> products, string userAllergens) : IEnumerable<Product>`
  - **Логика**: Исключить продукты, где Allergens содержит аллергены пользователя
- [ ] Добавить учет диетических ограничений (вегетарианство, веганство и т.д.)
  - **Как**: Добавить поле DietaryRestrictions в UserProfile (enum или строка)
  - **Метод**: `FilterProductsByDietaryRestrictions(IEnumerable<Product> products, DietaryRestriction restriction) : IEnumerable<Product>`
  - **Логика**: Исключить мясо для вегетарианцев, все животные продукты для веганов и т.д.

#### 4. Генерация дневного меню
- [ ] Создать сервис MealPlanGeneratorService
  - **Где**: `FitnessProject/BLL/Services/MealPlanGeneratorService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IMealPlanGeneratorService.cs`
  - **Зависимости**: IRecipeRepository, IProductRepository, IDailyDietPlanRepository, IMealRepository, IMealRecipeRepository
  - **Методы**: `GenerateMealPlanAsync(int userId, DateTime date, MacroNutrientsDto targets) : DailyDietPlan`
- [ ] Реализовать алгоритм подбора блюд из базы рецептов
  - **Метод**: `SelectRecipesForMeal(decimal targetCalories, MacroNutrientsDto targets, List<int> excludedProductIds) : List<Recipe>`
  - **Алгоритм**: 
    1. Получить все рецепты из БД
    2. Отфильтровать по исключенным продуктам
    3. Отсортировать по близости к целевым калориям
    4. Выбрать рецепты, которые в сумме дают нужные калории и БЖУ
  - **Использовать**: LINQ для фильтрации и сортировки
- [ ] Реализовать распределение калорий по приемам пищи:
  - **Метод**: `DistributeCaloriesByMealTime(decimal totalCalories) : Dictionary<MealTime, decimal>`
  - **Завтрак (Breakfast)**: totalCalories × 0.275 (25-30%, среднее 27.5%)
  - **Обед (Lunch)**: totalCalories × 0.325 (30-35%, среднее 32.5%)
  - **Ужин (Dinner)**: totalCalories × 0.275 (25-30%, среднее 27.5%)
  - **Перекусы (Snack)**: totalCalories × 0.125 (10-15%, среднее 12.5%)
- [ ] Реализовать балансировку макронутриентов по приемам пищи
  - **Метод**: `BalanceMacrosForMeal(MealTime mealTime, decimal mealCalories, MacroNutrientsDto dailyTargets) : MacroNutrientsDto`
  - **Логика**: Распределить БЖУ пропорционально калориям приема пищи
  - **Проверка**: Сумма всех приемов пищи должна равняться дневным целям
- [ ] Добавить разнообразие меню (избегать повторений)
  - **Как**: Проверить последние N дней (например, 7) и исключить рецепты, которые уже использовались
  - **Метод**: `GetRecentlyUsedRecipes(int userId, DateTime date, int days) : List<int>` - получить RecipeId за последние дни
  - **Логика**: При подборе рецептов исключать те, что в списке недавно использованных

#### 5. Расчет порций
- [ ] Реализовать расчет граммов продуктов для достижения целевых калорий
  - **Метод**: `CalculatePortionSize(Recipe recipe, decimal targetCalories) : decimal`
  - **Формула**: portionMultiplier = targetCalories / recipe.RecipeCaloriesPerPortion
  - **Результат**: Каждый RecipeProduct.QuantityGrams умножить на portionMultiplier
- [ ] Учесть RecipeProductsGrams из рецептов
  - **Как**: Использовать существующую структуру Recipe -> RecipeProducts -> Product
  - **Расчет**: Для каждого RecipeProduct: newQuantityGrams = originalQuantityGrams × portionMultiplier
  - **Сохранение**: При создании MealRecipe сохранить рассчитанные порции (можно в отдельной таблице или в Metadata)
- [ ] Реализовать округление порций до разумных значений
  - **Метод**: `RoundPortion(decimal grams) : decimal`
  - **Правила**: Округлять до 5-10 грамм (например, 127г -> 125г, 133г -> 130г)
  - **Использовать**: Math.Round(grams / 5) * 5 для округления до 5г
- [ ] Добавить валидацию минимальных/максимальных порций
  - **Константы**: Минимум 10г, максимум 1000г на один продукт в рецепте
  - **Валидация**: Если рассчитанная порция < 10г - увеличить до 10г, если > 1000г - разделить на несколько приемов
  - **Метод**: `ValidatePortionSize(decimal grams) : decimal`

#### 6. Создание DailyDietPlan
- [ ] Реализовать создание DailyDietPlan с рассчитанными параметрами
  - **Метод**: `CreateDailyDietPlanAsync(int userId, DateTime date, MacroNutrientsDto targets) : DailyDietPlan`
  - **Поля**: UserId, TemplateDietPlanId (nullable), DailyDietPlanName, DailyPlanCalories, DailyPlanFat, DailyPlanCarbs, DailyPlanProtein, DailyPlanNumberOfMeals, DailyPlanStatus, DailyPlanCreatedAt
  - **Использовать**: IDailyDietPlanRepository.AddAsync()
- [ ] Связать с TemplateDietPlan (если используется шаблон)
  - **Логика**: Если пользователь выбрал шаблон - установить TemplateDietPlanId
  - **Альтернатива**: Если шаблон не выбран - TemplateDietPlanId = null (полностью кастомный план)
- [ ] Создать связанные Meal записи с MealRecipes
  - **Метод**: `CreateMealsForPlanAsync(DailyDietPlan plan, Dictionary<MealTime, List<Recipe>> mealsByTime) : Task`
  - **Для каждого MealTime**: 
    1. Создать Meal с DailyDietPlanId, MealTime, MealOrder, целевыми калориями и БЖУ
    2. Для каждого Recipe в этом приеме пищи создать MealRecipe
  - **Использовать**: IMealRepository.AddAsync(), IMealRecipeRepository.AddAsync()
- [ ] Сохранить расчетные параметры для последующего анализа
  - **Как**: Все расчетные значения уже в DailyDietPlan (калории, БЖУ)
  - **Дополнительно**: Можно сохранить исходные данные расчета (BMR, TDEE) в отдельном поле или таблице CalculationHistory

#### 7. API эндпоинты
- [ ] POST /api/diet-plans/generate - генерация нового плана
  - **Где**: Добавить в `DailyDietPlansController.cs`
  - **Метод**: `[HttpPost("generate")] public async Task<ActionResult<DailyDietPlanResponseDto>> GeneratePlan([FromBody] GenerateDietPlanDto dto)`
  - **DTO**: Создать `GenerateDietPlanDto` с полями: UserId, Date (optional, по умолчанию сегодня), TemplateDietPlanId (optional)
  - **Логика**: Вызвать IMealPlanGeneratorService.GenerateMealPlanAsync()
  - **Ответ**: Вернуть DailyDietPlanResponseDto с деталями
- [ ] GET /api/diet-plans/{id}/meals - получение меню на день
  - **Где**: Добавить в `DailyDietPlansController.cs`
  - **Метод**: `[HttpGet("{id}/meals")] public async Task<ActionResult<DailyDietPlanMealsDto>> GetMeals(int id)`
  - **Логика**: Получить DailyDietPlan по ID с Include(m => m.Meals).ThenInclude(m => m.MealRecipes).ThenInclude(mr => mr.Recipe)
  - **DTO**: Создать `DailyDietPlanMealsDto` со списком MealDetailsDto
- [ ] POST /api/diet-plans/{id}/regenerate - перегенерация плана
  - **Где**: Добавить в `DailyDietPlansController.cs`
  - **Метод**: `[HttpPost("{id}/regenerate")] public async Task<ActionResult<DailyDietPlanResponseDto>> RegeneratePlan(int id)`
  - **Логика**: Получить существующий план, удалить старые Meals, сгенерировать новый план с теми же параметрами
  - **Использовать**: IMealPlanGeneratorService.GenerateMealPlanAsync() с параметрами из существующего плана

### Оценка сложности: ОЧЕНЬ ВЫСОКАЯ
### Время реализации: 5-7 дней

---

## MF-3. Корекція раціону на основі динамічних даних
**Приоритет: ВЫСОКИЙ** ⭐⭐⭐⭐

### Описание
Система автоматически предлагает адаптировать рацион при изменении активности, пульса, качества сна, недосыпе или аномальных показателях.

### Зависимости
- MF-1 (динамические данные)
- MF-2 (базовый план питания)

### Детальный план реализации

#### 1. Мониторинг изменений активности
- [ ] Реализовать сервис ActivityMonitorService
  - **Где**: `FitnessProject/BLL/Services/ActivityMonitorService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IActivityMonitorService.cs`
  - **Зависимости**: ITelemetrySampleRepository, ITrainingSessionRepository, ILogger
  - **Методы**: `CheckActivityChangesAsync(int userId, DateTime date) : ActivityChangeResult`
- [ ] Определить пороговые значения для триггеров:
  - **Класс**: Создать `ActivityThresholds` в `FitnessProject/BLL/Configuration/ActivityThresholds.cs`
  - **Резкое изменение шагов**: >30% от среднего за неделю
  - **Изменение интенсивности тренировок**: Изменение средней интенсивности >20%
  - **Аномальные значения пульса**: Пульс в покое < 40 или > 100 уд/мин
  - **Хранение**: Константы или конфигурация в appsettings.json
- [ ] Реализовать сравнение текущих данных с историческими (средние за неделю)
  - **Метод**: `GetWeeklyAverageAsync(int userId, DateTime date) : WeeklyActivityAverage`
  - **Логика**: Получить данные за последние 7 дней, рассчитать средние значения
  - **DTO**: Создать `WeeklyActivityAverage` с полями: AverageSteps, AverageHeartRate, AverageTrainingIntensity
  - **Сравнение**: Сравнить текущий день с WeeklyActivityAverage, определить процент изменения

#### 2. Анализ качества сна
- [ ] Реализовать анализ SleepRecords за последние дни
  - **Метод**: `AnalyzeSleepQualityAsync(int userId, DateTime date, int days = 3) : SleepQualityAnalysis`
  - **Логика**: Получить SleepRecords за последние N дней через ISleepRecordRepository
  - **Анализ**: Рассчитать средние значения TotalSleepMinutes, DeepSleepMinutes, SleepQuality
  - **DTO**: Создать `SleepQualityAnalysis` с полями: AverageSleepHours, AverageDeepSleepPercent, AverageQuality, IsSleepDeprived
- [ ] Определить критерии недосыпа:
  - **Класс**: Создать `SleepThresholds` в `FitnessProject/BLL/Configuration/SleepThresholds.cs`
  - **Общее время сна**: < 6 часов (360 минут) - критично
  - **Глубокий сон**: < 20% от общего времени сна
  - **Качество сна**: < 60% (если SleepQuality есть в данных)
  - **Метод**: `IsSleepDeprived(SleepRecord record) : bool` - проверка одного дня
- [ ] Реализовать триггеры для корректировки питания при недосыпе
  - **Метод**: `ShouldAdjustForSleepDeprivation(SleepQualityAnalysis analysis) : bool`
  - **Логика**: Если IsSleepDeprived = true или среднее качество сна < 60% за 3 дня - вернуть true
  - **Использование**: Вызывать из DietCorrectionService для принятия решения о корректировке

#### 3. Правила корректировки калорий
- [ ] Реализовать увеличение калорий при высокой активности (+10-15%)
  - **Метод**: `AdjustCaloriesForHighActivity(decimal currentCalories, decimal activityIncreasePercent) : decimal`
  - **Логика**: currentCalories × (1 + activityIncreasePercent / 100), где activityIncreasePercent = 10-15%
  - **Условие**: Если шаги > 30% от среднего или интенсивность тренировок высокая
- [ ] Реализовать уменьшение калорий при низкой активности (-5-10%)
  - **Метод**: `AdjustCaloriesForLowActivity(decimal currentCalories, decimal activityDecreasePercent) : decimal`
  - **Логика**: currentCalories × (1 - activityDecreasePercent / 100), где activityDecreasePercent = 5-10%
  - **Условие**: Если шаги < 70% от среднего или активность низкая несколько дней подряд
- [ ] Реализовать корректировку при недосыпе (уменьшение углеводов, увеличение белков)
  - **Метод**: `AdjustMacrosForSleepDeprivation(MacroNutrientsDto currentMacros) : MacroNutrientsDto`
  - **Логика**: 
    - Уменьшить углеводы на 5-10%
    - Увеличить белки на 5-10%
    - Жиры оставить без изменений или немного увеличить
  - **Применение**: Вызывать при IsSleepDeprived = true
- [ ] Реализовать корректировку при аномальном пульсе (консультация с врачом, мягкая диета)
  - **Метод**: `AdjustForAbnormalHeartRate(MacroNutrientsDto currentMacros) : MacroNutrientsDto`
  - **Логика**: 
    - Уменьшить общие калории на 5-10%
    - Увеличить белки (для стабилизации)
    - Уменьшить простые углеводы
  - **Дополнительно**: Создать Recommendation с типом HealthWarning и текстом "Рекомендуется консультация с врачом"

#### 4. Правила корректировки макронутриентов
- [ ] При высокой активности: увеличение углеводов (+5-10%)
  - **Метод**: `AdjustMacrosForHighActivity(MacroNutrientsDto currentMacros) : MacroNutrientsDto`
  - **Логика**: Увеличить углеводы на 5-10%, уменьшить жиры на 2-5% (чтобы калории остались теми же)
  - **Применение**: Когда активность > 30% от среднего
- [ ] При низкой активности: уменьшение углеводов (-5-10%), увеличение белков
  - **Метод**: `AdjustMacrosForLowActivity(MacroNutrientsDto currentMacros) : MacroNutrientsDto`
  - **Логика**: Уменьшить углеводы на 5-10%, увеличить белки на 5-10%
  - **Применение**: Когда активность < 70% от среднего
- [ ] При недосыпе: увеличение белков, уменьшение простых углеводов
  - **Метод**: `AdjustMacrosForSleepDeprivation(MacroNutrientsDto currentMacros) : MacroNutrientsDto`
  - **Логика**: Увеличить белки на 5-10%, уменьшить углеводы на 5-10% (особенно простые)
  - **Применение**: Когда IsSleepDeprived = true
- [ ] При восстановлении: увеличение белков и полезных жиров
  - **Метод**: `AdjustMacrosForRecovery(MacroNutrientsDto currentMacros) : MacroNutrientsDto`
  - **Логика**: Увеличить белки на 5-10%, увеличить жиры на 3-5%, немного уменьшить углеводы
  - **Применение**: После интенсивных тренировок или при низкой активности (восстановительный период)

#### 5. Генерация рекомендаций по корректировке
- [ ] Создать сервис DietCorrectionService
  - **Где**: `FitnessProject/BLL/Services/DietCorrectionService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IDietCorrectionService.cs`
  - **Зависимости**: IActivityMonitorService, ISleepRecordRepository, IDailyDietPlanRepository, IRecommendationRepository, IMealPlanGeneratorService
  - **Методы**: `CheckAndSuggestCorrectionsAsync(int userId, int dailyDietPlanId) : List<Recommendation>`, `ApplyCorrectionAsync(int dailyDietPlanId, int recommendationId) : DailyDietPlan`
- [ ] Реализовать создание Recommendation записей с типом Correction
  - **Метод**: `CreateCorrectionRecommendation(int userId, int? mealId, string reason, MacroNutrientsDto suggestedMacros) : Recommendation`
  - **Поля**: RecommendationType = RecommendationType.DietCorrection, RecommendationPayload = JSON с новыми значениями калорий и БЖУ, RecommendationStatus = Pending
  - **Сохранение**: Использовать IRecommendationRepository.AddAsync()
- [ ] Реализовать расчет новых целевых значений калорий и БЖУ
  - **Метод**: `CalculateCorrectedMacros(DailyDietPlan currentPlan, ActivityChangeResult activity, SleepQualityAnalysis sleep) : MacroNutrientsDto`
  - **Логика**: 
    1. Получить текущие калории и БЖУ из DailyDietPlan
    2. Применить правила корректировки на основе activity и sleep
    3. Вернуть новые значения
  - **Использовать**: Методы из пункта 3 и 4
- [ ] Реализовать предложение конкретных изменений в меню
  - **Метод**: `SuggestMenuChanges(DailyDietPlan currentPlan, MacroNutrientsDto newTargets) : string`
  - **Логика**: Сравнить текущий план с новыми целями, сформировать текстовое описание изменений
  - **Пример**: "Рекомендуется увеличить калории на 150 ккал, добавить 20г белков, уменьшить углеводы на 30г"
  - **Сохранение**: Текст сохранить в Recommendation.RecommendationPayload

#### 6. Автоматическое применение корректировок
- [ ] Реализовать опцию автоматической корректировки (с подтверждением пользователя)
  - **Как**: Добавить поле AutoApplyCorrections в UserProfile (bool)
  - **Логика**: Если AutoApplyCorrections = true - автоматически применять, иначе только создавать Recommendation
  - **Альтернатива**: Всегда создавать Recommendation, пользователь подтверждает через API
- [ ] Реализовать создание нового DailyDietPlan с скорректированными параметрами
  - **Метод**: `ApplyCorrectionAsync(int dailyDietPlanId, int recommendationId) : DailyDietPlan`
  - **Логика**: 
    1. Получить Recommendation с новыми параметрами из RecommendationPayload
    2. Получить текущий DailyDietPlan
    3. Создать новый DailyDietPlan с новыми параметрами (или обновить существующий)
    4. Перегенерировать Meals с новыми параметрами через IMealPlanGeneratorService
    5. Обновить Recommendation.Status = Applied
- [ ] Сохранить историю корректировок для анализа эффективности
  - **Как**: Создать таблицу DietPlanCorrections или использовать существующую Recommendation
  - **Поля**: OriginalPlanId, CorrectedPlanId, RecommendationId, CorrectionDate, Reason, OriginalMacros (JSON), CorrectedMacros (JSON)
  - **Использование**: Для анализа эффективности корректировок в MF-6 и MF-11

#### 7. API эндпоинты
- [ ] POST /api/diet-plans/{id}/check-corrections - проверка необходимости корректировки
  - **Где**: Добавить в `DailyDietPlansController.cs`
  - **Метод**: `[HttpPost("{id}/check-corrections")] public async Task<ActionResult<List<RecommendationResponseDto>>> CheckCorrections(int id)`
  - **Логика**: Вызвать IDietCorrectionService.CheckAndSuggestCorrectionsAsync(userId, id)
  - **Ответ**: Список RecommendationResponseDto с предложенными корректировками
- [ ] POST /api/diet-plans/{id}/apply-correction - применение корректировки
  - **Где**: Добавить в `DailyDietPlansController.cs`
  - **Метод**: `[HttpPost("{id}/apply-correction")] public async Task<ActionResult<DailyDietPlanResponseDto>> ApplyCorrection(int id, [FromBody] ApplyCorrectionDto dto)`
  - **DTO**: Создать `ApplyCorrectionDto` с полем RecommendationId
  - **Логика**: Вызвать IDietCorrectionService.ApplyCorrectionAsync(id, dto.RecommendationId)
  - **Ответ**: Обновленный DailyDietPlanResponseDto
- [ ] GET /api/recommendations/corrections - получение рекомендаций по корректировке
  - **Где**: Добавить в `RecommendationsController.cs`
  - **Метод**: `[HttpGet("corrections")] public async Task<ActionResult<List<RecommendationResponseDto>>> GetCorrectionRecommendations([FromQuery] int? userId)`
  - **Логика**: Получить все Recommendation с типом DietCorrection и статусом Pending
  - **Фильтр**: Если userId указан - только для этого пользователя


  ВОТ ТУТ ОСТАНОВИЛАСЬ
- [ ] Автопересборка меню после применения корректировки (опционально)
  - **Где**: `DietCorrectionService.ApplyCorrectionAsync` или отдельный сервис
  - **Логика**: после обновления целевых калорий/БЖУ пересчитать/перегенерировать блюда в плане
  - **Опции**: 
    - Перегенерация всего плана (повторный вызов генератора по тем же ограничениям)
    - Локальная корректировка порций/блюд под новые цели
  - **Флаг**: включать по настройке, чтобы не ломать текущее меню без явного запроса

### Оценка сложности: ВЫСОКАЯ
### Время реализации: 4-5 дней

---

## MF-4. Перегляд денних і тижневих показників
**Приоритет: ВЫСОКИЙ** ⭐⭐⭐⭐

### Описание
Пользователь видит статистику за пульсом, шагами, фазами сна и тренировками за день и неделю.

### Зависимости
- MF-1 (данные телеметрии)

### Детальный план реализации

#### 1. Агрегация дневных данных
- [ ] Создать сервис StatisticsService
  - **Где**: `FitnessProject/BLL/Services/StatisticsService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IStatisticsService.cs`
  - **Зависимости**: ITelemetrySampleRepository, ISleepRecordRepository, ITrainingSessionRepository, IDeviceRepository
  - **Методы**: `GetDailyStatisticsAsync(int userId, DateTime date) : DailyStatisticsDto`, `GetWeeklyStatisticsAsync(int userId, DateTime startDate) : WeeklyStatisticsDto`
- [ ] Реализовать агрегацию TelemetrySamples за день:
  - **Метод**: `AggregateTelemetryForDay(int userId, int deviceId, DateTime date) : TelemetryAggregate`
  - **Средний пульс**: TelemetrySamples.Where(t => t.TelemetryType == HeartRate && t.Timestamp.Date == date).Average(t => t.TelemetryValue)
  - **Минимальный/максимальный пульс**: Min() и Max() по тому же фильтру
  - **Количество записей**: Count() по тому же фильтру
  - **Шаги**: TelemetrySamples.Where(t => t.TelemetryType == Steps && t.Timestamp.Date == date).Sum(t => t.TelemetryValue)
- [ ] Реализовать агрегацию шагов за день (сумма)
  - **Как**: Использовать TelemetryType.Steps из TelemetrySamples
  - **Метод**: Включить в AggregateTelemetryForDay() или отдельный метод
  - **Логика**: Sum всех значений Steps за день
- [ ] Реализовать агрегацию SleepRecords за день:
  - **Метод**: `AggregateSleepForDay(int userId, int deviceId, DateTime date) : SleepAggregate`
  - **Общее время сна**: SleepRecords.Where(s => s.Date.Date == date).Sum(s => s.TotalSleepMinutes)
  - **Время глубокого/легкого сна**: Sum(s => s.DeepSleepMinutes) и Sum(s => s.LightSleepMinutes)
  - **Качество сна**: SleepRecords.Where(s => s.Date.Date == date).Average(s => s.SleepQuality) ?? 0
- [ ] Реализовать агрегацию TrainingSessions за день:
  - **Метод**: `AggregateTrainingsForDay(int userId, int deviceId, DateTime date) : TrainingAggregate`
  - **Количество тренировок**: TrainingSessions.Where(t => t.StartTime.Date == date).Count()
  - **Общая длительность**: Sum(t => t.DurationInMin)
  - **Средняя интенсивность**: Average(t => (int)t.Intensity) - нужно конвертировать enum в int
  - **Сожженные калории**: Sum(t => t.CaloriesEstimated ?? 0)

#### 2. Агрегация недельных данных
- [ ] Реализовать группировку данных по дням недели
  - **Метод**: `GetWeeklyStatisticsAsync(int userId, DateTime startDate) : WeeklyStatisticsDto`
  - **Логика**: Получить данные за 7 дней начиная с startDate, сгруппировать по датам
  - **Структура**: Dictionary<DateTime, DailyStatisticsDto> для каждого дня недели
- [ ] Реализовать расчет средних значений за неделю
  - **Метод**: `CalculateWeeklyAverages(Dictionary<DateTime, DailyStatisticsDto> dailyStats) : WeeklyAverages`
  - **Расчет**: Среднее значение всех метрик за 7 дней
  - **Метрики**: Средний пульс, средние шаги, среднее время сна, средняя интенсивность тренировок
- [ ] Реализовать расчет трендов (рост/падение показателей)
  - **Метод**: `CalculateTrends(Dictionary<DateTime, DailyStatisticsDto> dailyStats) : TrendAnalysis`
  - **Логика**: Сравнить первые 3 дня с последними 3 днями недели
  - **Расчет**: ((последние 3 дня - первые 3 дня) / первые 3 дня) × 100 = процент изменения
  - **Результат**: TrendDirection (Increasing, Decreasing, Stable) для каждой метрики
- [ ] Реализовать сравнение с предыдущей неделей
  - **Метод**: `CompareWithPreviousWeek(int userId, DateTime currentWeekStart) : WeekComparisonDto`
  - **Логика**: Получить статистику за текущую неделю и за предыдущую (startDate - 7 дней)
  - **Сравнение**: Рассчитать процент изменения для каждой метрики
  - **DTO**: Создать `WeekComparisonDto` с полями: CurrentWeek, PreviousWeek, PercentageChange для каждой метрики

#### 3. DTO для статистики
- [ ] Создать DailyStatisticsDto
  - **Где**: `FitnessProject/BLL/DTO/Statistics/DailyStatisticsDto.cs`
  - **Поля**: Date, UserId, AverageHeartRate, MinHeartRate, MaxHeartRate, TotalSteps, TotalSleepMinutes, DeepSleepMinutes, LightSleepMinutes, SleepQuality, TrainingCount, TotalTrainingDuration, AverageTrainingIntensity, TotalCaloriesBurned
- [ ] Создать WeeklyStatisticsDto
  - **Где**: `FitnessProject/BLL/DTO/Statistics/WeeklyStatisticsDto.cs`
  - **Поля**: StartDate, EndDate, UserId, DailyStatistics (List<DailyStatisticsDto>), WeeklyAverages (WeeklyAveragesDto), Trends (TrendAnalysisDto)
- [ ] Создать StatisticsComparisonDto (для сравнения периодов)
  - **Где**: `FitnessProject/BLL/DTO/Statistics/StatisticsComparisonDto.cs`
  - **Поля**: Period1 (DailyStatisticsDto или WeeklyStatisticsDto), Period2, PercentageChanges (Dictionary<string, decimal>), Improvements (List<string>), Declines (List<string>)

#### 4. API эндпоинты
- [ ] GET /api/statistics/daily/{date} - статистика за день
  - **Где**: Создать `StatisticsController.cs` в `FitnessProject/Controllers/`
  - **Метод**: `[HttpGet("daily/{date}")] public async Task<ActionResult<DailyStatisticsDto>> GetDailyStatistics(DateTime date, [FromQuery] int? userId)`
  - **Логика**: Вызвать IStatisticsService.GetDailyStatisticsAsync(userId ?? currentUserId, date)
  - **Параметры**: date в формате YYYY-MM-DD, userId опционально (для админов)
- [ ] GET /api/statistics/weekly/{startDate} - статистика за неделю
  - **Где**: В `StatisticsController.cs`
  - **Метод**: `[HttpGet("weekly/{startDate}")] public async Task<ActionResult<WeeklyStatisticsDto>> GetWeeklyStatistics(DateTime startDate, [FromQuery] int? userId)`
  - **Логика**: Вызвать IStatisticsService.GetWeeklyStatisticsAsync(userId ?? currentUserId, startDate)
  - **Параметры**: startDate - первый день недели
- [ ] GET /api/statistics/comparison - сравнение периодов
  - **Где**: В `StatisticsController.cs`
  - **Метод**: `[HttpGet("comparison")] public async Task<ActionResult<StatisticsComparisonDto>> ComparePeriods([FromQuery] DateTime startDate1, [FromQuery] DateTime endDate1, [FromQuery] DateTime startDate2, [FromQuery] DateTime endDate2, [FromQuery] int? userId)`
  - **Логика**: Получить статистику за оба периода, сравнить, вернуть StatisticsComparisonDto

#### 5. Оптимизация запросов
- [ ] Реализовать кэширование статистики
  - **Как**: Использовать IMemoryCache в StatisticsService
  - **Ключ**: $"statistics_daily_{userId}_{date:yyyy-MM-dd}" или $"statistics_weekly_{userId}_{startDate:yyyy-MM-dd}"
  - **TTL**: 5-10 минут для дневной статистики, 15-30 минут для недельной
  - **Инвалидация**: При получении новых данных телеметрии - очистить кэш для этого дня
- [ ] Добавить индексы для быстрой агрегации
  - **Где**: В ApplicationDbContext.OnModelCreating() или через миграцию
  - **Индексы**: 
    - TelemetrySamples: (DeviceId, Timestamp), (TelemetryType, Timestamp)
    - SleepRecords: (DeviceId, Date)
    - TrainingSessions: (DeviceId, StartTime)
  - **Использование**: Для быстрой фильтрации по датам и типам
- [ ] Реализовать пагинацию для больших объемов данных
  - **Как**: Если нужно получить статистику за длительный период - разбить на страницы
  - **Метод**: Добавить параметры page и pageSize в методы получения статистики
  - **Альтернатива**: Ограничить максимальный период (например, не более 30 дней за раз)

### Оценка сложности: СРЕДНЯЯ
### Время реализации: 2-3 дня

---

## MF-5. Перегляд рекомендацій та порад
**Приоритет: СРЕДНИЙ** ⭐⭐⭐

### Описание
Система предоставляет краткие объяснения и индивидуальные рекомендации, основанные на данных за конкретный период.

### Зависимости
- MF-1 (данные)
- MF-3 (корректировки)
- MF-4 (статистика)

### Детальный план реализации

#### 1. Типы рекомендаций
- [ ] Определить типы RecommendationType:
  - **Где**: Проверить существующий enum `RecommendationType` в `FitnessProject/Enums/RecommendationType.cs`
  - **Добавить если нет**: DietCorrection, ActivityAdvice, SleepAdvice, HealthWarning, GeneralTip
  - **Использование**: Уже используется в сущности Recommendation

#### 2. Генерация рекомендаций
- [ ] Создать сервис RecommendationGeneratorService
  - **Где**: `FitnessProject/BLL/Services/RecommendationGeneratorService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IRecommendationGeneratorService.cs`
  - **Зависимости**: IStatisticsService, IRecommendationRepository, IDailyDietPlanRepository, IGoalRepository (если будет)
  - **Методы**: `GenerateRecommendationsAsync(int userId, DateTime date) : List<Recommendation>`
- [ ] Реализовать правила генерации рекомендаций на основе:
  - **Статистики активности**: Если активность низкая несколько дней - создать ActivityAdvice
  - **Качества сна**: Если сон плохой - создать SleepAdvice
  - **Соблюдения диеты**: Если план не выполняется - создать DietCorrection
  - **Прогресса к целям**: Если прогресс медленный - создать GeneralTip с советами
  - **Методы**: Отдельные методы для каждого типа рекомендаций: `GenerateActivityAdvice()`, `GenerateSleepAdvice()`, и т.д.
- [ ] Реализовать шаблоны рекомендаций с параметризацией
  - **Где**: Создать класс `RecommendationTemplates` в `FitnessProject/BLL/Services/Helpers/RecommendationTemplates.cs`
  - **Метод**: `GetTemplate(RecommendationType type, Dictionary<string, object> parameters) : string`
  - **Примеры**: "Ваша активность снизилась на {percent}%. Рекомендуем увеличить количество шагов.", "Качество сна {quality}%. Попробуйте ложиться спать раньше."

#### 3. Персонализация рекомендаций
- [ ] Учитывать цели пользователя (MF-7)
  - **Как**: Получить активную Goal через IGoalRepository (или из UserProfile)
  - **Логика**: Генерировать рекомендации, соответствующие цели (например, для похудения - советы по дефициту калорий)
  - **Метод**: Передавать GoalType в методы генерации рекомендаций
- [ ] Учитывать медицинские ограничения
  - **Как**: Получить MedicalConditions из UserProfile
  - **Логика**: Не генерировать рекомендации, которые противоречат ограничениям (например, не рекомендовать интенсивные тренировки при проблемах с сердцем)
- [ ] Учитывать историю рекомендаций (не повторять одни и те же)
  - **Метод**: `GetRecentRecommendations(int userId, int days = 7) : List<Recommendation>`
  - **Логика**: Перед созданием новой рекомендации проверить, не была ли похожая создана недавно
  - **Сравнение**: Сравнивать по типу и ключевым словам в RecommendationPayload
- [ ] Приоритизировать рекомендации по важности
  - **Как**: Добавить поле Priority в Recommendation или использовать RecommendationStatus
  - **Приоритеты**: Critical > High > Medium > Low
  - **Логика**: HealthWarning - Critical, DietCorrection - High, ActivityAdvice/SleepAdvice - Medium, GeneralTip - Low

#### 4. Хранение и статусы
- [ ] Использовать существующую сущность Recommendation
  - **Проверить**: Есть ли все необходимые поля (RecommendationId, MealInstanceId, RecommendationType, RecommendationStatus, RecommendationPayload)
  - **Дополнить если нужно**: Поле Priority (int) или использовать RecommendationStatus для приоритета
- [ ] Реализовать статусы: Pending, Read, Applied, Dismissed
  - **Где**: Проверить enum `RecommendationStatus` в `FitnessProject/Enums/RecommendationStatus.cs`
  - **Добавить если нет**: Pending, Read, Applied, Dismissed
  - **Использование**: При создании - Pending, при просмотре - Read, при применении - Applied, при отклонении - Dismissed
- [ ] Сохранять связь с Meal (если рекомендация относится к конкретному приему пищи)
  - **Как**: Использовать существующее поле MealInstanceId в Recommendation
  - **Логика**: Если рекомендация относится к конкретному Meal - установить MealInstanceId, иначе null

#### 5. API эндпоинты
- [ ] GET /api/recommendations - получение всех рекомендаций
  - **Где**: В `RecommendationsController.cs` (уже есть базовый метод, дополнить)
  - **Метод**: `[HttpGet] public async Task<ActionResult<List<RecommendationResponseDto>>> GetAll([FromQuery] int? userId, [FromQuery] RecommendationType? type, [FromQuery] RecommendationStatus? status)`
  - **Логика**: Получить рекомендации с фильтрацией по userId, type, status
  - **Использовать**: IRecommendationRepository.FindAsync() с предикатом
- [ ] GET /api/recommendations/active - получение активных рекомендаций
  - **Где**: В `RecommendationsController.cs`
  - **Метод**: `[HttpGet("active")] public async Task<ActionResult<List<RecommendationResponseDto>>> GetActive([FromQuery] int? userId)`
  - **Логика**: Получить рекомендации со статусом Pending или Read для пользователя
- [ ] PUT /api/recommendations/{id}/status - обновление статуса
  - **Где**: В `RecommendationsController.cs`
  - **Метод**: `[HttpPut("{id}/status")] public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRecommendationStatusDto dto)`
  - **DTO**: Создать `UpdateRecommendationStatusDto` с полем Status
  - **Логика**: Обновить Recommendation.Status через IRecommendationRepository.UpdateAsync()
- [ ] POST /api/recommendations/generate - ручная генерация рекомендаций
  - **Где**: В `RecommendationsController.cs`
  - **Метод**: `[HttpPost("generate")] public async Task<ActionResult<List<RecommendationResponseDto>>> Generate([FromBody] GenerateRecommendationsDto dto)`
  - **DTO**: Создать `GenerateRecommendationsDto` с полями UserId, Date (optional)
  - **Логика**: Вызвать IRecommendationGeneratorService.GenerateRecommendationsAsync()

### Оценка сложности: СРЕДНЯЯ
### Время реализации: 2-3 дня

---

## MF-6. Відстеження прогресу
**Приоритет: ВЫСОКИЙ** ⭐⭐⭐⭐

### Описание
Пользователь получает отчеты по изменению физической активности, пульса и качества сна за выбранный период. Система показывает влияние изменений в диете на эти параметры.

### Зависимости
- MF-1 (данные)
- MF-2 (планы питания)
- MF-4 (статистика)

### Детальный план реализации

#### 1. Анализ трендов
- [ ] Создать сервис ProgressTrackingService
  - **Где**: `FitnessProject/BLL/Services/ProgressTrackingService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IProgressTrackingService.cs`
  - **Зависимости**: IStatisticsService, IDailyDietPlanRepository, ITelemetrySampleRepository, ISleepRecordRepository
  - **Методы**: `GetTrendsAsync(int userId, DateTime startDate, DateTime endDate) : TrendAnalysisDto`, `GetCorrelationsAsync(int userId, DateTime startDate, DateTime endDate) : CorrelationAnalysisDto`
- [ ] Реализовать расчет трендов для метрик:
  - **Метод**: `CalculateTrend(List<decimal> values) : TrendResult` - использует линейную регрессию
  - **Средний пульс**: Получить DailyStatistics за период, извлечь AverageHeartRate, рассчитать тренд
  - **Количество шагов**: Извлечь TotalSteps, рассчитать тренд
  - **Качество сна**: Извлечь SleepQuality, рассчитать тренд
  - **Соблюдение диеты**: Рассчитать % выполнения плана (фактические калории / целевые калории)
  - **Результат**: TrendDirection (Increasing, Decreasing, Stable) и Slope (наклон линии тренда)
- [ ] Реализовать статистические методы (линейная регрессия для трендов)
  - **Где**: Создать класс `StatisticalMethods` в `FitnessProject/BLL/Services/Helpers/StatisticalMethods.cs`
  - **Метод**: `CalculateLinearRegression(List<decimal> yValues) : (decimal slope, decimal intercept)`
  - **Формула**: slope = (n×Σ(xy) - Σ(x)×Σ(y)) / (n×Σ(x²) - (Σ(x))²), где x - дни, y - значения метрики
  - **Использование**: Для расчета наклона тренда (положительный = рост, отрицательный = падение)

#### 2. Корреляционный анализ
- [ ] Реализовать анализ влияния изменений диеты на активность
  - **Метод**: `AnalyzeDietActivityCorrelation(int userId, DateTime startDate, DateTime endDate) : CorrelationResult`
  - **Логика**: 
    1. Получить DailyDietPlan за период с изменениями калорий/БЖУ
    2. Получить DailyStatistics за тот же период с TotalSteps
    3. Рассчитать коэффициент корреляции Пирсона между изменениями диеты и активностью
  - **Результат**: CorrelationCoefficient (-1 до 1), где > 0.5 = сильная положительная корреляция
- [ ] Реализовать анализ влияния изменений диеты на пульс
  - **Метод**: `AnalyzeDietHeartRateCorrelation(int userId, DateTime startDate, DateTime endDate) : CorrelationResult`
  - **Логика**: Аналогично, но сравнивать с AverageHeartRate
  - **Особенность**: Учитывать задержку эффекта (изменения диеты могут влиять на пульс через 1-2 дня)
- [ ] Реализовать анализ влияния изменений диеты на сон
  - **Метод**: `AnalyzeDietSleepCorrelation(int userId, DateTime startDate, DateTime endDate) : CorrelationResult`
  - **Логика**: Сравнивать изменения БЖУ (особенно белков и углеводов) с SleepQuality
  - **Паттерн**: Больше белков вечером может улучшать сон
- [ ] Реализовать выявление паттернов (например, больше белков = лучше сон)
  - **Метод**: `IdentifyPatterns(int userId, DateTime startDate, DateTime endDate) : List<Pattern>`
  - **Логика**: 
    1. Разделить период на сегменты (например, по неделям)
    2. Для каждого сегмента рассчитать средние значения БЖУ и метрик
    3. Найти сегменты с лучшими метриками, определить общие характеристики диеты
    4. Вернуть паттерны (например, "Высокое потребление белков (>120г/день) коррелирует с улучшением сна на 15%")

#### 3. Отчеты по периодам
- [ ] Реализовать генерацию отчетов за:
  - **Метод**: `GenerateReportAsync(int userId, DateTime startDate, DateTime endDate) : ProgressReportDto`
  - **Неделя**: endDate = startDate + 7 дней
  - **Месяц**: endDate = startDate.AddMonths(1)
  - **Квартал**: endDate = startDate.AddMonths(3)
  - **Произвольный период**: Использовать переданные startDate и endDate
  - **Валидация**: Максимальный период - 1 год
- [ ] Включить в отчеты:
  - **Графики изменений метрик**: TimeSeriesDataDto для каждой метрики (пульс, шаги, сон, калории)
  - **Список примененных корректировок диеты**: Получить Recommendation с типом DietCorrection и статусом Applied за период
  - **Эффективность корректировок**: Сравнить метрики до и после каждой корректировки, рассчитать % улучшения
  - **Достижение целей**: Получить активную Goal, рассчитать прогресс (текущий вес / целевой вес), процент выполнения
  - **DTO**: Создать `ProgressReportDto` со всеми этими полями

#### 4. Визуализация данных
- [ ] Создать DTO для графиков (TimeSeriesDataDto)
  - **Где**: `FitnessProject/BLL/DTO/Progress/TimeSeriesDataDto.cs`
  - **Поля**: MetricName (string), DataPoints (List<DataPointDto>), Unit (string)
  - **DataPointDto**: Date (DateTime), Value (decimal), Label (string, optional)
- [ ] Реализовать агрегацию данных для графиков (по дням/неделям)
  - **Метод**: `AggregateForChart(List<DailyStatisticsDto> dailyStats, AggregationType type) : TimeSeriesDataDto`
  - **AggregationType**: Daily (по дням), Weekly (по неделям - средние значения)
  - **Логика**: Для Weekly - сгруппировать по неделям, рассчитать средние значения
- [ ] Подготовить данные для фронтенда в удобном формате
  - **Формат**: JSON с массивом точек данных, готовый для использования в Chart.js, D3.js и т.д.
  - **Структура**: `{ metric: "HeartRate", unit: "bpm", data: [{ date: "2024-01-01", value: 75 }, ...] }`

#### 5. API эндпоинты
- [ ] GET /api/progress/trends - получение трендов
  - **Где**: Создать `ProgressController.cs` в `FitnessProject/Controllers/`
  - **Метод**: `[HttpGet("trends")] public async Task<ActionResult<TrendAnalysisDto>> GetTrends([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать IProgressTrackingService.GetTrendsAsync()
- [ ] GET /api/progress/correlations - анализ корреляций
  - **Где**: В `ProgressController.cs`
  - **Метод**: `[HttpGet("correlations")] public async Task<ActionResult<CorrelationAnalysisDto>> GetCorrelations([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать IProgressTrackingService.GetCorrelationsAsync()
- [ ] GET /api/progress/report - генерация отчета за период
  - **Где**: В `ProgressController.cs`
  - **Метод**: `[HttpGet("report")] public async Task<ActionResult<ProgressReportDto>> GetReport([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать IProgressTrackingService.GenerateReportAsync()
- [ ] GET /api/progress/goals - прогресс к целям
  - **Где**: В `ProgressController.cs`
  - **Метод**: `[HttpGet("goals")] public async Task<ActionResult<GoalProgressDto>> GetGoalProgress([FromQuery] int userId)`
  - **Логика**: Получить активную Goal, рассчитать прогресс, вернуть GoalProgressDto

### Оценка сложности: ВЫСОКАЯ
### Время реализации: 4-5 дней

---

## MF-7. Налаштування персональних цілей
**Приоритет: ВЫСОКИЙ** ⭐⭐⭐⭐

### Описание
Пользователь задает цель: похудение, набор массы, поддержание, коррекция питания под болезнь или состояние здоровья.

### Зависимости
- UserProfile (базовые данные)

### Детальный план реализации

#### 1. Расширение UserProfile
- [ ] Добавить поле GoalType (enum):
  - **Где**: Проверить, есть ли уже enum GoalType, если нет - создать в `FitnessProject/Enums/GoalType.cs`
  - **Значения**: WeightLoss, WeightGain, WeightMaintenance, HealthCorrection
  - **В UserProfile**: Добавить поле `public GoalType? GoalType { get; set; }` (nullable, так как может не быть цели)
  - **Миграция**: Создать миграцию для добавления поля в БД
- [ ] Добавить поле TargetWeight (целевой вес)
  - **Где**: В UserProfile добавить `public decimal? TargetWeight { get; set; }`
  - **Тип**: decimal для точности, nullable
  - **Миграция**: Добавить в миграцию
- [ ] Добавить поле TargetDate (срок достижения цели)
  - **Где**: В UserProfile добавить `public DateTime? TargetDate { get; set; }`
  - **Миграция**: Добавить в миграцию
- [ ] Добавить поле CurrentGoalId (связь с активной целью)
  - **Альтернатива**: Вместо этого создать отдельную таблицу Goals (см. пункт 2)
  - **Если в UserProfile**: Добавить `public int? CurrentGoalId { get; set; }` с внешним ключом на Goals

#### 2. Создание сущности Goal
- [ ] Создать таблицу Goals
  - **Где**: Создать `FitnessProject/Entities/Goal.cs`
  - **Поля**: 
    - GoalId (int, PK)
    - UserId (int, FK на Users)
    - GoalType (GoalType enum)
    - TargetWeight (decimal)
    - StartWeight (decimal) - вес на момент создания цели
    - TargetDate (DateTime) - срок достижения
    - StartDate (DateTime) - дата начала цели
    - Status (GoalStatus enum: Active, Completed, Cancelled)
    - CreatedAt (DateTime)
- [ ] Поля: GoalId, UserId, GoalType, TargetWeight, StartWeight, TargetDate, StartDate, Status
  - **Дополнительно**: Можно добавить поле Description (string, nullable) для описания цели
- [ ] Реализовать миграцию БД
  - **Где**: В ApplicationDbContext добавить `public DbSet<Goal> Goals { get; set; }`
  - **Конфигурация**: В OnModelCreating() настроить таблицу Goals, связи, индексы
  - **Миграция**: Создать миграцию через `dotnet ef migrations add AddGoalsTable`

#### 3. Валидация целей
- [ ] Реализовать проверку реалистичности цели (например, похудение > 1 кг/неделю - нереально)
  - **Где**: Создать класс `GoalValidator` в `FitnessProject/BLL/Services/Helpers/GoalValidator.cs`
  - **Метод**: `ValidateGoalRealistic(Goal goal) : ValidationResult`
  - **Логика**: 
    - Рассчитать необходимую скорость изменения веса: (TargetWeight - StartWeight) / количество недель до TargetDate
    - Для похудения: максимум 0.5-1 кг/неделю
    - Для набора массы: максимум 0.5-1 кг/неделю
    - Если скорость нереалистична - вернуть ошибку
- [ ] Реализовать проверку совместимости с медицинскими ограничениями
  - **Метод**: `ValidateGoalCompatibility(Goal goal, UserProfile profile) : ValidationResult`
  - **Логика**: Проверить MedicalConditions, если есть серьезные ограничения - предупредить или запретить определенные цели
  - **Пример**: При диабете - не рекомендовать агрессивное похудение
- [ ] Реализовать расчет рекомендуемого срока достижения цели
  - **Метод**: `CalculateRecommendedTargetDate(GoalType goalType, decimal startWeight, decimal targetWeight) : DateTime`
  - **Логика**: 
    - Рассчитать разницу в весе
    - Для похудения: разница / 0.5 (кг/неделю) = количество недель
    - Для набора: разница / 0.5 (кг/неделю) = количество недель
    - Вернуть StartDate + количество недель

#### 4. Применение целей
- [ ] Интегрировать цели в MF-2 (генерация плана питания)
  - **Где**: В MealPlanGeneratorService добавить получение активной цели
  - **Метод**: В GenerateMealPlanAsync() получить активную Goal через IGoalRepository
  - **Использование**: Передать GoalType в CalorieCalculator для расчета калорий по цели
- [ ] Реализовать пересчет калорий и БЖУ при изменении цели
  - **Где**: В GoalService при обновлении цели
  - **Метод**: `RecalculateDietPlanForGoal(int goalId) : Task`
  - **Логика**: Получить активный DailyDietPlan пользователя, пересчитать калории и БЖУ на основе новой цели, обновить план
- [ ] Реализовать уведомления о прогрессе к цели
  - **Где**: Создать метод в GoalService или отдельный сервис GoalProgressService
  - **Метод**: `CheckGoalProgress(int goalId) : GoalProgressDto`
  - **Логика**: Рассчитать текущий прогресс (процент от StartWeight до TargetWeight), создать Recommendation если прогресс медленный

#### 5. API эндпоинты
- [ ] POST /api/goals - создание новой цели
  - **Где**: Создать `GoalsController.cs` в `FitnessProject/Controllers/`
  - **Метод**: `[HttpPost] public async Task<ActionResult<GoalResponseDto>> Create([FromBody] GoalCreateDto dto)`
  - **DTO**: Создать `GoalCreateDto` с полями: UserId, GoalType, TargetWeight, TargetDate
  - **Логика**: Валидация через GoalValidator, создание Goal через IGoalService, установка StartWeight из UserProfile
- [ ] PUT /api/goals/{id} - обновление цели
  - **Где**: В `GoalsController.cs`
  - **Метод**: `[HttpPut("{id}")] public async Task<ActionResult<GoalResponseDto>> Update(int id, [FromBody] GoalUpdateDto dto)`
  - **Логика**: Обновить Goal, если изменилась цель - пересчитать план питания
- [ ] GET /api/goals/active - получение активной цели
  - **Где**: В `GoalsController.cs`
  - **Метод**: `[HttpGet("active")] public async Task<ActionResult<GoalResponseDto>> GetActive([FromQuery] int userId)`
  - **Логика**: Получить Goal со статусом Active для пользователя
- [ ] GET /api/goals/history - история целей
  - **Где**: В `GoalsController.cs`
  - **Метод**: `[HttpGet("history")] public async Task<ActionResult<List<GoalResponseDto>>> GetHistory([FromQuery] int userId)`
  - **Логика**: Получить все Goals пользователя, отсортированные по StartDate
- [ ] DELETE /api/goals/{id} - отмена цели
  - **Где**: В `GoalsController.cs`
  - **Метод**: `[HttpDelete("{id}")] public async Task<IActionResult> Cancel(int id)`
  - **Логика**: Установить Status = Cancelled вместо удаления (сохранить историю)

### Оценка сложности: СРЕДНЯЯ
### Время реализации: 2-3 дня

---

## MF-8. Повідомлення про відхилення у стані організму
**Приоритет: ВЫСОКИЙ** ⭐⭐⭐⭐

### Описание
Система информирует о низкой активности, повышенном пульсе, недосыпе или других факторах, влияющих на план питания.

### Зависимости
- MF-1 (данные телеметрии)

### Детальный план реализации

#### 1. Определение пороговых значений
- [ ] Создать конфигурацию пороговых значений:
  - **Где**: Создать класс `HealthThresholds` в `FitnessProject/BLL/Configuration/HealthThresholds.cs`
  - **Константы**:
    - LowActivitySteps = 5000 (низкая активность: < 5000 шагов/день)
    - ElevatedRestingHeartRate = 100 (повышенный пульс: > 100 уд/мин в покое)
    - SleepDeprivationHours = 6 (недосып: < 6 часов сна)
    - AbnormalHeartRateMin = 40, AbnormalHeartRateMax = 200 (аномальный пульс)
  - **Альтернатива**: Хранить в appsettings.json для возможности изменения без перекомпиляции
- [ ] Реализовать персонализацию порогов (на основе истории пользователя)
  - **Метод**: `GetPersonalizedThresholds(int userId) : PersonalizedThresholds`
  - **Логика**: Рассчитать средние значения пользователя за последний месяц, установить пороги как среднее ± 20%
  - **Пример**: Если средний пульс пользователя 75 - порог повышенного пульса = 90 (75 × 1.2)

#### 2. Мониторинг в реальном времени
- [ ] Создать сервис HealthMonitorService
  - **Где**: `FitnessProject/BLL/Services/HealthMonitorService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IHealthMonitorService.cs`
  - **Зависимости**: IStatisticsService, ITelemetrySampleRepository, ISleepRecordRepository, IRecommendationRepository
  - **Методы**: `CheckHealthStatusAsync(int userId, DateTime date) : List<HealthWarning>`, `MonitorAllUsersAsync() : Task`
- [ ] Реализовать фоновую задачу для проверки данных
  - **Как**: Использовать IHostedService или BackgroundService в ASP.NET Core
  - **Где**: Создать `HealthMonitoringBackgroundService.cs` в `FitnessProject/BackgroundServices/`
  - **Логика**: Запускать каждые 30-60 минут, проверять всех активных пользователей
  - **Регистрация**: Зарегистрировать в Program.cs как HostedService
- [ ] Реализовать сравнение текущих значений с пороговыми
  - **Метод**: `CheckThresholds(int userId, DateTime date, PersonalizedThresholds thresholds) : List<ThresholdViolation>`
  - **Логика**: Получить статистику за день, сравнить каждую метрику с пороговым значением
  - **Результат**: Список нарушений с типом (LowActivity, ElevatedHeartRate, SleepDeprivation, AbnormalHeartRate)
- [ ] Реализовать определение критичности отклонений
  - **Метод**: `DetermineSeverity(ThresholdViolation violation) : WarningSeverity`
  - **Логика**: 
    - AbnormalHeartRate (< 40 или > 200) = Critical
    - SleepDeprivation несколько дней подряд = High
    - ElevatedHeartRate = Medium
    - LowActivity = Low
  - **Enum**: Создать WarningSeverity (Critical, High, Medium, Low)

#### 3. Генерация предупреждений
- [ ] Использовать Recommendation с типом HealthWarning
  - **Как**: Создавать Recommendation с RecommendationType = HealthWarning
  - **Метод**: `CreateHealthWarning(int userId, ThresholdViolation violation, WarningSeverity severity) : Recommendation`
  - **Поля**: RecommendationType = HealthWarning, RecommendationPayload = JSON с деталями нарушения, RecommendationStatus = Pending
- [ ] Реализовать приоритеты предупреждений:
  - **Как**: Использовать WarningSeverity для определения приоритета
  - **Critical**: Требует немедленного внимания - создать Recommendation с высоким приоритетом, возможно отправить уведомление
  - **High**: Важно - создать Recommendation, показать в приложении
  - **Medium**: Стоит обратить внимание - создать Recommendation
  - **Low**: Информационное - создать Recommendation, но не приоритизировать
- [ ] Реализовать шаблоны сообщений для каждого типа отклонения
  - **Где**: В RecommendationTemplates добавить шаблоны для HealthWarning
  - **Шаблоны**: 
    - LowActivity: "Ваша активность сегодня низкая ({steps} шагов). Рекомендуем увеличить активность."
    - ElevatedHeartRate: "Обнаружен повышенный пульс в покое ({heartRate} уд/мин). Рекомендуем проконсультироваться с врачом."
    - SleepDeprivation: "Вы недосыпаете ({hours} часов). Качественный сон важен для здоровья."
    - AbnormalHeartRate: "Обнаружен аномальный пульс ({heartRate} уд/мин). Немедленно обратитесь к врачу!"

#### 4. Уведомления
- [ ] Интегрировать с системой уведомлений (если есть)
  - **Как**: Если есть сервис уведомлений - использовать его, иначе создать базовую реализацию
  - **Интерфейс**: Создать `INotificationService` с методом `SendNotificationAsync(int userId, string message, NotificationType type)`
  - **Использование**: Вызывать при создании Critical предупреждений
- [ ] Реализовать отправку email/SMS для критических предупреждений
  - **Как**: Использовать библиотеки для email (MailKit) или SMS (Twilio, если нужно)
  - **Условие**: Только для Critical предупреждений
  - **Метод**: `SendCriticalAlertAsync(int userId, HealthWarning warning) : Task`
  - **Регистрация**: Добавить в Program.cs, настроить SMTP/SMS провайдер
- [ ] Реализовать in-app уведомления
  - **Как**: Использовать SignalR для real-time уведомлений или просто возвращать через API
  - **Альтернатива**: Клиент будет периодически опрашивать GET /api/health-warnings/active
  - **Реализация**: При создании Recommendation с типом HealthWarning - клиент получит его через API

#### 5. История предупреждений
- [ ] Сохранять все предупреждения в Recommendation
  - **Как**: Все HealthWarning сохраняются как Recommendation с типом HealthWarning
  - **Уже реализовано**: RecommendationRepository сохраняет все рекомендации
- [ ] Реализовать фильтрацию по типу и статусу
  - **Где**: В IRecommendationRepository добавить метод `FindByTypeAndStatusAsync(RecommendationType type, RecommendationStatus? status)`
  - **Использование**: Для получения активных предупреждений (Status = Pending или Read)
- [ ] Реализовать аналитику частоты предупреждений
  - **Метод**: `GetWarningFrequency(int userId, DateTime startDate, DateTime endDate) : WarningFrequencyDto`
  - **Логика**: Подсчитать количество предупреждений каждого типа за период
  - **DTO**: Создать `WarningFrequencyDto` с полями: TotalWarnings, ByType (Dictionary<ThresholdViolationType, int>), AveragePerWeek

#### 6. API эндпоинты
- [ ] GET /api/health-warnings - получение активных предупреждений
  - **Где**: Создать `HealthWarningsController.cs` или добавить в `RecommendationsController.cs`
  - **Метод**: `[HttpGet] public async Task<ActionResult<List<RecommendationResponseDto>>> GetActiveWarnings([FromQuery] int? userId)`
  - **Логика**: Получить Recommendation с типом HealthWarning и статусом Pending/Read
- [ ] GET /api/health-warnings/history - история предупреждений
  - **Где**: В том же контроллере
  - **Метод**: `[HttpGet("history")] public async Task<ActionResult<List<RecommendationResponseDto>>> GetWarningHistory([FromQuery] int userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)`
  - **Логика**: Получить все HealthWarning за период, отсортированные по дате
- [ ] POST /api/health-warnings/check - ручная проверка состояния
  - **Где**: В том же контроллере
  - **Метод**: `[HttpPost("check")] public async Task<ActionResult<List<RecommendationResponseDto>>> CheckHealthStatus([FromBody] CheckHealthStatusDto dto)`
  - **DTO**: Создать `CheckHealthStatusDto` с полями UserId, Date (optional, по умолчанию сегодня)
  - **Логика**: Вызвать IHealthMonitorService.CheckHealthStatusAsync() для ручной проверки

### Оценка сложности: СРЕДНЯЯ
### Время реализации: 2-3 дня

---

## MF-9. Генерація рекомендацій для корекції поведінки
**Приоритет: СРЕДНИЙ** ⭐⭐⭐

### Описание
Приложение предлагает конкретные действия для улучшения эффективности диеты: изменение соотношения БЖУ, замена продуктов, оптимизация времени приема пищи, адаптация меню.

### Зависимости
- MF-2 (план питания)
- MF-3 (корректировки)
- MF-6 (анализ прогресса)

### Детальный план реализации

#### 1. Анализ текущего состояния
- [ ] Реализовать анализ соблюдения диеты (% выполнения по калориям и БЖУ)
  - **Метод**: `AnalyzeDietCompliance(int userId, int dailyDietPlanId) : DietComplianceDto`
  - **Логика**: Сравнить фактические калории/БЖУ (если есть данные о потреблении) с целевыми из DailyDietPlan
  - **Расчет**: (фактические / целевые) × 100 = процент выполнения
  - **DTO**: Создать `DietComplianceDto` с полями: CaloriesCompliance, ProteinCompliance, FatCompliance, CarbsCompliance
- [ ] Реализовать анализ эффективности текущего плана (на основе MF-6)
  - **Метод**: `AnalyzePlanEffectiveness(int userId, int dailyDietPlanId) : PlanEffectivenessDto`
  - **Логика**: Использовать корреляционный анализ из MF-6, определить, улучшились ли метрики после применения плана
  - **Метрики**: Изменение активности, пульса, качества сна после начала плана
- [ ] Реализовать выявление проблемных зон (недостаток белков, избыток углеводов и т.д.)
  - **Метод**: `IdentifyProblemAreas(MacroNutrientsDto current, MacroNutrientsDto target) : List<ProblemArea>`
  - **Логика**: Сравнить текущие БЖУ с целевыми, определить отклонения > 10%
  - **Результат**: Список проблем (например, "Недостаток белков: текущее 80г, целевое 120г")

#### 2. Рекомендации по БЖУ
- [ ] Реализовать анализ текущего соотношения БЖУ
  - **Метод**: `AnalyzeMacroRatio(MacroNutrientsDto current) : MacroRatioAnalysis`
  - **Логика**: Рассчитать процентное соотношение БЖУ от общих калорий
  - **Сравнение**: Сравнить с оптимальными диапазонами (белки 25-30%, жиры 25-30%, углеводы 40-50%)
- [ ] Реализовать предложения по изменению:
  - **Метод**: `SuggestMacroAdjustments(MacroRatioAnalysis analysis, List<ProblemArea> problems) : List<MacroAdjustment>`
  - **Увеличение/уменьшение белков**: Если белки < 25% - предложить увеличить на X грамм
  - **Изменение соотношения жиров**: Если жиры вне диапазона - предложить корректировку
  - **Корректировка углеводов**: Если углеводы > 50% - предложить уменьшить, заменив на белки
- [ ] Реализовать обоснование рекомендаций (на основе данных)
  - **Метод**: `GenerateMacroJustification(MacroAdjustment adjustment, PlanEffectivenessDto effectiveness) : string`
  - **Логика**: Использовать данные из анализа эффективности для обоснования (например, "Увеличение белков улучшило качество сна на 15%")

#### 3. Рекомендации по продуктам
- [ ] Реализовать анализ разнообразия продуктов в меню
  - **Метод**: `AnalyzeProductDiversity(int dailyDietPlanId) : ProductDiversityAnalysis`
  - **Логика**: Подсчитать количество уникальных продуктов в меню, определить повторяющиеся продукты
  - **Критерий**: Если один продукт используется > 3 раз в неделю - низкое разнообразие
- [ ] Реализовать предложения замены продуктов:
  - **Метод**: `SuggestProductReplacements(int dailyDietPlanId, MacroAdjustment adjustment) : List<ProductReplacement>`
  - **На более питательные аналоги**: Найти продукты с похожим БЖУ, но большей питательной ценностью (витамины, минералы)
  - **На продукты с лучшим БЖУ профилем**: Если нужно увеличить белки - предложить заменить углеводный продукт на белковый
  - **На продукты, соответствующие ограничениям**: Учесть аллергии и диетические ограничения из UserProfile
- [ ] Реализовать предложения добавления продуктов для баланса
  - **Метод**: `SuggestProductAdditions(int dailyDietPlanId, List<ProblemArea> problems) : List<ProductSuggestion>`
  - **Логика**: Если недостаток определенного макронутриента - предложить продукты, богатые этим макронутриентом

#### 4. Оптимизация времени приема пищи
- [ ] Реализовать анализ распределения калорий по времени
  - **Метод**: `AnalyzeMealTiming(int dailyDietPlanId) : MealTimingAnalysis`
  - **Логика**: Получить все Meal с их MealTime, проанализировать распределение калорий
  - **Проблемы**: Если ужин > 30% калорий или поздно вечером - это проблема
- [ ] Реализовать рекомендации по времени:
  - **Метод**: `SuggestMealTiming(int userId, MealTimingAnalysis analysis) : List<MealTimingRecommendation>`
  - **Завтрак**: Оптимальное время 7-9 утра, должен содержать 25-30% калорий
  - **Обед**: Оптимальное время 12-14 часов, должен содержать 30-35% калорий
  - **Ужин**: Не позже 19-20 часов, должен содержать 25-30% калорий
  - **Перекусы**: Между основными приемами (10-11, 15-16, 17-18 часов)
- [ ] Учитывать активность пользователя (тренировки)
  - **Логика**: Если есть тренировка - рекомендовать прием пищи за 1-2 часа до и после тренировки
  - **До тренировки**: Углеводы для энергии
  - **После тренировки**: Белки для восстановления

#### 5. Адаптация меню к состоянию
- [ ] Реализовать адаптацию при усталости (больше углеводов)
  - **Метод**: `AdaptMenuForFatigue(int dailyDietPlanId, FatigueLevel level) : MenuAdaptation`
  - **Логика**: Если активность низкая и сон плохой - увеличить углеводы на 10-15% для энергии
  - **Определение усталости**: Низкая активность + плохой сон + низкий пульс в покое
- [ ] Реализовать адаптацию при высокой активности (больше калорий)
  - **Метод**: `AdaptMenuForHighActivity(int dailyDietPlanId, ActivityLevel level) : MenuAdaptation`
  - **Логика**: Если активность > 30% от среднего - увеличить калории на 10-15%, увеличить углеводы
- [ ] Реализовать адаптацию при восстановлении (больше белков)
  - **Метод**: `AdaptMenuForRecovery(int dailyDietPlanId) : MenuAdaptation`
  - **Логика**: После интенсивных тренировок - увеличить белки на 10-15% для восстановления мышц
- [ ] Реализовать адаптацию при стрессе (стабилизирующие продукты)
  - **Метод**: `AdaptMenuForStress(int dailyDietPlanId, StressLevel level) : MenuAdaptation`
  - **Логика**: Увеличить магний (орехи, бананы), уменьшить кофеин, добавить продукты с триптофаном (индейка, молоко)

#### 6. Генерация конкретных действий
- [ ] Создать сервис BehaviorCorrectionService
  - **Где**: `FitnessProject/BLL/Services/BehaviorCorrectionService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IBehaviorCorrectionService.cs`
  - **Зависимости**: IDailyDietPlanRepository, IRecommendationRepository, IMealPlanGeneratorService, IProgressTrackingService
  - **Методы**: `GenerateBehaviorRecommendationsAsync(int userId, int dailyDietPlanId) : List<Recommendation>`, `ApplyBehaviorRecommendationAsync(int recommendationId) : Task`
- [ ] Реализовать создание Recommendation с конкретными действиями
  - **Метод**: `CreateBehaviorRecommendation(int userId, int? mealId, BehaviorRecommendationType type, string action, string justification) : Recommendation`
  - **Типы**: MacroAdjustment, ProductReplacement, MealTiming, MenuAdaptation
  - **Поля**: RecommendationType = GeneralTip или DietCorrection, RecommendationPayload = JSON с деталями действия
- [ ] Реализовать приоритизацию рекомендаций
  - **Метод**: `PrioritizeRecommendations(List<Recommendation> recommendations) : List<Recommendation>`
  - **Критерии**: 
    - Макро-корректировки = High priority
    - Замена продуктов = Medium priority
    - Время приема пищи = Medium priority
    - Адаптация меню = Low priority
- [ ] Реализовать возможность применения рекомендаций (автоматическое изменение плана)
  - **Метод**: `ApplyBehaviorRecommendationAsync(int recommendationId) : Task`
  - **Логика**: 
    1. Получить Recommendation с деталями из RecommendationPayload
    2. Применить изменения к DailyDietPlan (обновить калории/БЖУ, заменить продукты, изменить время)
    3. Перегенерировать Meals если нужно
    4. Обновить Recommendation.Status = Applied

#### 7. API эндпоинты
- [ ] GET /api/recommendations/behavior - получение рекомендаций по поведению
  - **Где**: В `RecommendationsController.cs` или создать `BehaviorRecommendationsController.cs`
  - **Метод**: `[HttpGet("behavior")] public async Task<ActionResult<List<RecommendationResponseDto>>> GetBehaviorRecommendations([FromQuery] int userId, [FromQuery] int? dailyDietPlanId)`
  - **Логика**: Вызвать IBehaviorCorrectionService.GenerateBehaviorRecommendationsAsync()
- [ ] POST /api/recommendations/{id}/apply - применение рекомендации
  - **Где**: В `RecommendationsController.cs`
  - **Метод**: `[HttpPost("{id}/apply")] public async Task<ActionResult<DailyDietPlanResponseDto>> ApplyRecommendation(int id)`
  - **Логика**: Вызвать IBehaviorCorrectionService.ApplyBehaviorRecommendationAsync(id)
  - **Ответ**: Обновленный DailyDietPlanResponseDto
- [ ] GET /api/recommendations/analysis - анализ текущего состояния
  - **Где**: В `RecommendationsController.cs`
  - **Метод**: `[HttpGet("analysis")] public async Task<ActionResult<BehaviorAnalysisDto>> GetBehaviorAnalysis([FromQuery] int userId, [FromQuery] int dailyDietPlanId)`
  - **Логика**: Вызвать методы анализа из BehaviorCorrectionService, вернуть сводку
  - **DTO**: Создать `BehaviorAnalysisDto` с полями: Compliance, Effectiveness, ProblemAreas, Suggestions

### Оценка сложности: ВЫСОКАЯ
### Время реализации: 4-5 дней

---

## MF-10. Збереження історії даних
**Приоритет: КРИТИЧЕСКИЙ** ⭐⭐⭐⭐⭐

### Описание
Вся информация доступна пользователю в любое время. Система формирует хронологию изменений в состоянии здоровья и питании.

### Зависимости
- Все функции (история всех операций)

### Детальный план реализации

#### 1. Аудит изменений
- [ ] Реализовать логирование всех изменений в ключевых сущностях:
  - **Где**: Создать таблицу `AuditLog` в БД
  - **Сущность**: Создать `FitnessProject/Entities/AuditLog.cs`
  - **Поля**: AuditLogId, EntityType (string), EntityId (int), Action (enum: Create, Update, Delete), UserId, Timestamp, OldValues (JSON), NewValues (JSON), Changes (JSON)
  - **DailyDietPlan**: Логировать изменения калорий, БЖУ, статуса
  - **UserProfile**: Логировать изменения веса, роста, целей
  - **Goals**: Логировать создание, изменение, отмену целей
- [ ] Использовать подход Audit Trail (таблицы истории)
  - **Как**: Перехватывать изменения через SaveChangesAsync() в ApplicationDbContext
  - **Метод**: Переопределить SaveChangesAsync(), перед сохранением записать изменения в AuditLog
  - **Альтернатива**: Использовать библиотеку EntityFrameworkCore.Auditing или реализовать вручную
- [ ] Сохранять: кто, когда, что изменил, старое/новое значение
  - **Кто**: UserId из текущего контекста (получить из IHttpContextAccessor)
  - **Когда**: Timestamp = DateTime.UtcNow
  - **Что**: EntityType и EntityId
  - **Старое/новое**: Сериализовать в JSON через JsonSerializer

#### 2. История телеметрии
- [ ] Уже реализовано через TelemetrySamples, SleepRecords, TrainingSessions
  - **Проверить**: Все данные сохраняются с Timestamp, можно получить историю через репозитории
- [ ] Реализовать оптимизацию хранения (архивация старых данных)
  - **Метод**: Создать фоновую задачу `DataArchivingService` (IHostedService)
  - **Логика**: Раз в месяц архивировать данные старше 6 месяцев в отдельную таблицу ArchiveTelemetrySamples
  - **Альтернатива**: Удалять данные старше 1 года (если не требуется долгосрочное хранение)
- [ ] Реализовать быстрый доступ к истории
  - **Метод**: Создать `HistoryService` с методами `GetTelemetryHistoryAsync(int userId, DateTime startDate, DateTime endDate)`
  - **Оптимизация**: Использовать индексы на Timestamp, кэширование для часто запрашиваемых периодов

#### 3. История планов питания
- [ ] Реализовать версионирование DailyDietPlan
  - **Как**: Добавить поле Version (int) в DailyDietPlan или создать отдельную таблицу DietPlanVersions
  - **Альтернатива**: Создать таблицу `DietPlanHistory` с полями: HistoryId, DailyDietPlanId, Version, SnapshotData (JSON), CreatedAt
  - **Логика**: При каждом изменении плана создавать новую версию
- [ ] Сохранять все версии планов (включая корректировки)
  - **Метод**: В DailyDietPlanService при UpdateAsync() создавать запись в DietPlanHistory
  - **Содержимое**: Сохранять полный снимок плана (калории, БЖУ, список Meals) в JSON
- [ ] Реализовать связь между версиями (что изменилось)
  - **Метод**: `ComparePlanVersions(int planId, int version1, int version2) : PlanVersionDiff`
  - **Логика**: Сравнить два снимка, определить изменения (калории изменились на X, белки на Y и т.д.)
  - **DTO**: Создать `PlanVersionDiff` с полями: ChangedFields (Dictionary<string, Change>), ChangeSummary (string)

#### 4. История рекомендаций
- [ ] Уже реализовано через Recommendation
  - **Проверить**: Все Recommendation сохраняются с датой создания, можно получить историю
- [ ] Реализовать фильтрацию и поиск по истории
  - **Метод**: В IRecommendationRepository добавить `GetHistoryAsync(int userId, DateTime? startDate, DateTime? endDate, RecommendationType? type)`
  - **Фильтры**: По дате, типу, статусу (Applied, Dismissed)
  - **Поиск**: По тексту в RecommendationPayload (если нужно)
- [ ] Реализовать аналитику эффективности рекомендаций
  - **Метод**: `AnalyzeRecommendationEffectiveness(int userId, DateTime startDate, DateTime endDate) : RecommendationEffectivenessDto`
  - **Логика**: Для рекомендаций со статусом Applied - сравнить метрики до и после применения
  - **Расчет**: Процент улучшения для каждой метрики, средняя эффективность по типам рекомендаций

#### 5. Хронология событий
- [ ] Создать сущность EventHistory или ActivityLog
  - **Где**: Создать `FitnessProject/Entities/EventHistory.cs`
  - **Поля**: EventId, UserId, EventType (enum), EventDate, EntityType (string), EntityId (int), Description (string), Metadata (JSON)
  - **EventType**: PlanCreated, CorrectionApplied, GoalChanged, WarningReceived, RecommendationGenerated
- [ ] Реализовать запись всех значимых событий:
  - **Создание плана питания**: В DailyDietPlanService при создании плана - записать EventHistory
  - **Применение корректировки**: В DietCorrectionService при применении - записать EventHistory
  - **Изменение цели**: В GoalService при изменении - записать EventHistory
  - **Получение предупреждения**: В HealthMonitorService при создании предупреждения - записать EventHistory
- [ ] Реализовать временную шкалу событий
  - **Метод**: `GetTimelineAsync(int userId, DateTime startDate, DateTime endDate) : List<EventHistoryDto>`
  - **Логика**: Получить все EventHistory за период, отсортировать по EventDate
  - **DTO**: Создать `EventHistoryDto` для отображения на временной шкале

#### 6. API эндпоинты
- [ ] GET /api/history/telemetry - история телеметрии
  - **Где**: Создать `HistoryController.cs` в `FitnessProject/Controllers/`
  - **Метод**: `[HttpGet("telemetry")] public async Task<ActionResult<TelemetryHistoryDto>> GetTelemetryHistory([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать HistoryService.GetTelemetryHistoryAsync()
- [ ] GET /api/history/diet-plans - история планов питания
  - **Где**: В `HistoryController.cs`
  - **Метод**: `[HttpGet("diet-plans")] public async Task<ActionResult<List<DietPlanHistoryDto>>> GetDietPlanHistory([FromQuery] int userId, [FromQuery] int? planId)`
  - **Логика**: Получить все версии планов для пользователя или конкретного плана
- [ ] GET /api/history/recommendations - история рекомендаций
  - **Где**: В `HistoryController.cs`
  - **Метод**: `[HttpGet("recommendations")] public async Task<ActionResult<List<RecommendationResponseDto>>> GetRecommendationHistory([FromQuery] int userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)`
  - **Логика**: Вызвать IRecommendationRepository.GetHistoryAsync()
- [ ] GET /api/history/timeline - хронология всех событий
  - **Где**: В `HistoryController.cs`
  - **Метод**: `[HttpGet("timeline")] public async Task<ActionResult<List<EventHistoryDto>>> GetTimeline([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать HistoryService.GetTimelineAsync()
- [ ] GET /api/history/export - экспорт истории (JSON/CSV)
  - **Где**: В `HistoryController.cs`
  - **Метод**: `[HttpGet("export")] public async Task<IActionResult> ExportHistory([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "json")`
  - **Логика**: Получить всю историю, сериализовать в JSON или CSV, вернуть файл

#### 7. Оптимизация и архивация
- [ ] Реализовать политику архивации (данные старше X месяцев)
  - **Как**: Создать фоновую задачу `DataArchivingService` (IHostedService)
  - **Политика**: Архивировать данные старше 6 месяцев в отдельные таблицы Archive*
  - **Метод**: Раз в месяц запускать архивацию, перемещать старые записи
- [ ] Реализовать сжатие старых данных
  - **Как**: Для JSON полей (Metadata, SnapshotData) использовать сжатие при архивации
  - **Метод**: Использовать GZip для сжатия больших JSON перед сохранением в архив
- [ ] Реализовать быстрый доступ к недавней истории
  - **Как**: Использовать кэширование для последних 30 дней истории
  - **Ключ**: $"history_{userId}_{entityType}_{days}"
  - **TTL**: 1 час для недавней истории

### Оценка сложности: СРЕДНЯЯ
### Время реализации: 3-4 дня

---

## MF-11. Базові інструменти аналізу стану користувача
**Приоритет: СРЕДНИЙ** ⭐⭐⭐

### Описание
Система оценивает влияние изменений в диете на физическую активность, пульс и качество сна. Пользователь получает информацию о влиянии изменений меню на активность и пульс, эффективности адаптаций плана питания, тенденциях в сне и общем состоянии.

### Зависимости
- MF-1 (данные)
- MF-2 (планы питания)
- MF-3 (корректировки)
- MF-6 (отслеживание прогресса)

### Детальный план реализации

#### 1. Корреляционный анализ
- [ ] Реализовать расчет корреляции между:
  - **Метод**: `CalculateCorrelation(List<decimal> x, List<decimal> y) : decimal` - коэффициент корреляции Пирсона
  - **Изменениями калорий и активностью**: Получить изменения калорий по дням, изменения активности (шаги), рассчитать корреляцию
  - **Изменениями БЖУ и пульсом**: Получить изменения БЖУ, изменения пульса, рассчитать корреляцию для каждого макронутриента
  - **Изменениями диеты и качеством сна**: Получить изменения диеты, изменения качества сна, рассчитать корреляцию
  - **Формула**: r = Σ((x - x̄)(y - ȳ)) / √(Σ(x - x̄)² × Σ(y - ȳ)²)
- [ ] Использовать статистические методы (коэффициент корреляции Пирсона)
  - **Где**: В StatisticalMethods добавить метод CalculatePearsonCorrelation()
  - **Интерпретация**: r > 0.7 = сильная положительная, r < -0.7 = сильная отрицательная, |r| < 0.3 = слабая
- [ ] Реализовать визуализацию корреляций
  - **DTO**: Создать `CorrelationMatrixDto` с матрицей корреляций между всеми метриками
  - **Формат**: Готовый для отображения в виде тепловой карты (heatmap) на фронтенде

#### 2. Анализ эффективности корректировок
- [ ] Реализовать сравнение метрик до и после корректировки диеты
  - **Метод**: `AnalyzeCorrectionEffectiveness(int correctionId) : CorrectionEffectivenessDto`
  - **Логика**: Получить метрики за 3 дня до корректировки и 7 дней после, сравнить средние значения
  - **Метрики**: Активность, пульс, качество сна, соблюдение диеты
- [ ] Реализовать расчет эффективности (% улучшения)
  - **Метод**: `CalculateImprovementPercentage(decimal before, decimal after) : decimal`
  - **Формула**: ((after - before) / before) × 100
  - **Применение**: Для каждой метрики рассчитать процент улучшения
- [ ] Реализовать ранжирование корректировок по эффективности
  - **Метод**: `RankCorrectionsByEffectiveness(int userId, DateTime startDate, DateTime endDate) : List<RankedCorrection>`
  - **Логика**: Получить все корректировки за период, рассчитать эффективность каждой, отсортировать по убыванию
- [ ] Реализовать выявление наиболее эффективных паттернов
  - **Метод**: `IdentifyEffectivePatterns(int userId) : List<EffectivePattern>`
  - **Логика**: Найти корректировки с эффективностью > 20%, определить общие характеристики (тип корректировки, изменения БЖУ)

#### 3. Анализ влияния на активность
- [ ] Реализовать анализ влияния изменений меню на:
  - **Метод**: `AnalyzeDietImpactOnActivity(int userId, DateTime startDate, DateTime endDate) : ActivityImpactAnalysis`
  - **Количество шагов**: Рассчитать корреляцию между изменениями калорий/БЖУ и изменениями шагов
  - **Интенсивность тренировок**: Рассчитать корреляцию между изменениями диеты и интенсивностью тренировок
  - **Общая активность**: Комплексный анализ всех метрик активности
- [ ] Реализовать временные задержки (эффект может проявиться через несколько дней)
  - **Метод**: `AnalyzeDelayedImpact(List<DietChange> changes, List<ActivityMetric> metrics, int delayDays) : DelayedImpactAnalysis`
  - **Логика**: Сравнить изменения диеты с метриками активности через N дней (1, 2, 3, 7 дней)
  - **Результат**: Определить оптимальную задержку для анализа влияния

#### 4. Анализ влияния на пульс
- [ ] Реализовать анализ влияния изменений диеты на:
  - **Метод**: `AnalyzeDietImpactOnHeartRate(int userId, DateTime startDate, DateTime endDate) : HeartRateImpactAnalysis`
  - **Средний пульс в покое**: Рассчитать корреляцию между изменениями диеты и пульсом в покое
  - **Пульс во время активности**: Сравнить пульс во время тренировок до и после изменений диеты
  - **Восстановление пульса после тренировок**: Анализировать скорость восстановления пульса (время до возврата к норме)
- [ ] Реализовать выявление продуктов/паттернов, влияющих на пульс
  - **Метод**: `IdentifyHeartRateAffectingPatterns(int userId) : List<HeartRatePattern>`
  - **Логика**: Найти дни с аномальным пульсом, определить общие характеристики диеты в эти дни (продукты, БЖУ)
  - **Результат**: Список паттернов (например, "Высокое потребление кофеина коррелирует с повышенным пульсом")

#### 5. Анализ влияния на сон
- [ ] Реализовать анализ влияния изменений диеты на:
  - **Метод**: `AnalyzeDietImpactOnSleep(int userId, DateTime startDate, DateTime endDate) : SleepImpactAnalysis`
  - **Общее время сна**: Рассчитать корреляцию между изменениями диеты и временем сна
  - **Качество сна**: Сравнить SleepQuality до и после изменений диеты
  - **Глубокий сон**: Анализировать изменения DeepSleepMinutes
  - **Время засыпания**: Анализировать время засыпания (если есть данные)
- [ ] Реализовать выявление оптимального времени приема пищи для сна
  - **Метод**: `FindOptimalMealTimingForSleep(int userId) : OptimalMealTiming`
  - **Логика**: Найти дни с лучшим качеством сна, определить время последнего приема пищи в эти дни
  - **Результат**: Рекомендация оптимального времени ужина (например, не позже 19:00)

#### 6. Трендовый анализ
- [ ] Реализовать выявление долгосрочных трендов (месяцы)
  - **Метод**: `AnalyzeLongTermTrends(int userId, int months) : LongTermTrendAnalysis`
  - **Логика**: Получить данные за последние N месяцев, рассчитать тренды для каждой метрики
  - **Использовать**: Линейная регрессия для определения направления тренда
- [ ] Реализовать сезонные паттерны
  - **Метод**: `IdentifySeasonalPatterns(int userId) : SeasonalPatterns`
  - **Логика**: Сгруппировать данные по месяцам/сезонам, найти повторяющиеся паттерны
  - **Пример**: "Активность снижается зимой", "Качество сна лучше летом"
- [ ] Реализовать прогнозирование на основе трендов
  - **Метод**: `ForecastMetrics(int userId, int daysAhead) : ForecastDto`
  - **Логика**: Использовать линейную регрессию для прогнозирования значений метрик на N дней вперед
  - **Ограничения**: Прогноз только на короткий период (7-14 дней) для точности

#### 7. Генерация инсайтов
- [ ] Создать сервис UserInsightsService
  - **Где**: `FitnessProject/BLL/Services/UserInsightsService.cs`
  - **Интерфейс**: `FitnessProject/BLL/Services/Interfaces/IUserInsightsService.cs`
  - **Зависимости**: IProgressTrackingService, IStatisticsService, IDailyDietPlanRepository
  - **Методы**: `GenerateInsightsAsync(int userId, DateTime startDate, DateTime endDate) : List<Insight>`
- [ ] Реализовать генерацию текстовых инсайтов на основе анализа
  - **Метод**: `GenerateInsightText(InsightType type, AnalysisData data) : string`
  - **Типы инсайтов**: CorrelationInsight, TrendInsight, PatternInsight, EffectivenessInsight
  - **Примеры**: "Увеличение белков на 20% улучшило качество сна на 15%", "Ваша активность растет на 5% в неделю"
- [ ] Реализовать приоритизацию инсайтов по важности
  - **Метод**: `PrioritizeInsights(List<Insight> insights) : List<Insight>`
  - **Критерии**: 
    - Высокая корреляция (> 0.7) = High priority
    - Сильный тренд = High priority
    - Эффективность корректировок > 20% = High priority
    - Остальные = Medium/Low priority
- [ ] Реализовать персонализацию инсайтов
  - **Логика**: Учитывать цели пользователя, медицинские ограничения, историю
  - **Пример**: Для пользователя с целью похудения - приоритизировать инсайты о калориях

#### 8. API эндпоинты
- [ ] GET /api/analytics/correlations - корреляционный анализ
  - **Где**: Создать `AnalyticsController.cs` в `FitnessProject/Controllers/`
  - **Метод**: `[HttpGet("correlations")] public async Task<ActionResult<CorrelationMatrixDto>> GetCorrelations([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать методы корреляционного анализа из UserInsightsService
- [ ] GET /api/analytics/effectiveness - эффективность корректировок
  - **Где**: В `AnalyticsController.cs`
  - **Метод**: `[HttpGet("effectiveness")] public async Task<ActionResult<List<RankedCorrection>>> GetEffectiveness([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)`
  - **Логика**: Вызвать UserInsightsService.RankCorrectionsByEffectiveness()
- [ ] GET /api/analytics/impact - влияние на метрики
  - **Где**: В `AnalyticsController.cs`
  - **Метод**: `[HttpGet("impact")] public async Task<ActionResult<ImpactAnalysisDto>> GetImpact([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string metric)`
  - **Логика**: Вызвать методы анализа влияния (на активность, пульс, сон) в зависимости от параметра metric
- [ ] GET /api/analytics/trends - трендовый анализ
  - **Где**: В `AnalyticsController.cs`
  - **Метод**: `[HttpGet("trends")] public async Task<ActionResult<LongTermTrendAnalysis>> GetTrends([FromQuery] int userId, [FromQuery] int months = 3)`
  - **Логика**: Вызвать UserInsightsService.AnalyzeLongTermTrends()
- [ ] GET /api/analytics/insights - получение инсайтов
  - **Где**: В `AnalyticsController.cs`
  - **Метод**: `[HttpGet("insights")] public async Task<ActionResult<List<InsightDto>>> GetInsights([FromQuery] int userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)`
  - **Логика**: Вызвать UserInsightsService.GenerateInsightsAsync()

### Оценка сложности: ОЧЕНЬ ВЫСОКАЯ
### Время реализации: 5-7 дней

---

## Итоговая приоритизация и последовательность реализации

### Фаза 1: MVP (Критический функционал) - 10-12 дней
1. **MF-1** - Збір даних з фітнес браслета (3-4 дня)
2. **MF-10** - Збереження історії даних (3-4 дня) - параллельно с MF-1
3. **MF-2** - Формування персонального харчового плану (5-7 дней)

### Фаза 2: Основной функционал - 10-12 дней
4. **MF-7** - Налаштування персональних цілей (2-3 дня)
5. **MF-4** - Перегляд денних і тижневих показників (2-3 дня)
6. **MF-8** - Повідомлення про відхилення (2-3 дня)
7. **MF-3** - Корекція раціону на основі динамічних даних (4-5 дней)

### Фаза 3: Расширенный функционал - 8-10 дней
8. **MF-6** - Відстеження прогресу (4-5 дней)
9. **MF-5** - Перегляд рекомендацій та порад (2-3 дня)
10. **MF-9** - Генерація рекомендацій для корекції поведінки (4-5 дней)

### Фаза 4: Продвинутая аналитика - 5-7 дней
11. **MF-11** - Базові інструменти аналізу стану користувача (5-7 дней)

---

## Общая оценка времени: 33-41 день

## Рекомендации по оптимизации времени

1. **Параллельная разработка**: MF-1 и MF-10 можно разрабатывать параллельно
2. **Упрощение**: Начать с базовых версий функций, затем добавлять сложность
3. **Приоритизация**: Если времени мало, сосредоточиться на Фазе 1 и 2 (MVP + основной функционал)
4. **Итеративный подход**: Реализовать базовую версию функции, протестировать, затем улучшать
