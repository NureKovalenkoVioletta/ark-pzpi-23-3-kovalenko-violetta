# Тест-план: MF-3 Корекція раціону на основі динамічних даних

Покрытие юнит/интеграционных тестов для блоку корекцій раціону (activity + sleep → рекомендації/аплай + ребаланс меню).

## Юнит: ActivityMonitorService
- Агрегация steps/HR/intensity за день и неделю (пусто → 0/false).
- Шип по шагам (StepsSpikeThreshold), изменение інтенсивності тренувань, аномалії HR (RestHeartRateLow/High).
- % зміни кроків тиждень до тижня; fallback при нестачі даних.

## Юнит: SleepAnalysisService
- Агрегація total/deep/light/awake, average quality.
- IsSleepDeprived за порогами (TotalSleepMinutesCritical, DeepSleepPercentCritical, SleepQualityCritical).
- ShouldAdjustForSleepDeprivation → true/false по граничних кейсах; пусті дані → false.

## Юнит: DietCorrectionHelper
- AdjustCaloriesForHighActivity/LowActivity — застосування відсотків.
- AdjustMacrosForHigh/Low/Recovery/SleepDeprivation/AbnormalHeartRate — перевірка зміни макросів і NormalizeToCalories.
- NormalizeToCalories збережує загальні калорії після корекції.

## Юнит: DietCorrectionService
- CalculateCorrectedMacros: high activity, low activity, sleep deprivation, абн. HR, комбінації.
- CreateCorrectionRecommendation: тип DietCorrection, payload JSON з новими макросами.
- SuggestMenuChanges: локалізація uk/en, тексти reason.
- CheckAndSuggestCorrectionsAsync:
  - Дані активності/сну відсутні → немає рекомендації.
  - Є зміни + пороги виконані → повертає Recommendation.
- ApplyCorrectionAsync:
  - Оновлює DailyDietPlan макроси, IsCorrected = true.
  - Викликає RebalanceMealsAsync (масштаб таргетів і PortionsMetadata).

## Юнит: RebalanceMealsAsync
- Масштабування MealTarget* пропорційно новим денним макро.
- PortionsMetadata перераховані через PortionCalculator; MealRecipe оновлені.
- Без деталей рецепта — порожній metadata, не падає.

## Інтеграційні/сервісні
- check-corrections (POST /api/dailydietplans/{id}/check-corrections):
  - Без змін → 204/empty.
  - Є зміни (sleep deprivation/high activity) → Recommendation створено, текст локалі збігається.
- apply-correction (POST /api/dailydietplans/{id}/apply-correction):
  - Застосовує макро/калорії, IsCorrected=true, ребаланс страв/порцій.
  - Meal targets сумуються до нових денних макро з допуском.
- GET /api/recommendations/corrections — повертає створені корекції.

## Локалізація і константи
- Перевірка uk/en для reason/menu changes.
- Числові пороги з конфігів (ActivityThresholds, SleepThresholds) використовуються; зміна конфігів впливає на логіку.

## Дані/кейси
- Activity: низькі/високі кроки, інтенсивність тренувань, HR низький/високий.
- Sleep: недосип, нормальний сон.
- Порожні дані телеметрії/сну → немає рекомендацій.
- Already corrected план → повторне apply не ламає цілісність (ідеально — idempotency або відмова).

## Перевірки цілісності
- Після apply: DailyPlanStatus не змінюється (Planned), IsCorrected=true.
- Meal/MealRecipe збережені (Update/Save виклики).
- Сума MealTargetCalories узгоджується з новими денними калоріями (допуск).

