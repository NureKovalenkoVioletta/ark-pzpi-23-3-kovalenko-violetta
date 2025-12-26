# Звіт: MF-3. Корекція раціону на основі динамічних даних

## Призначення
Автоматичне виявлення змін у активності/пульсі/сні, формування рекомендацій щодо корекції калорій і макронутрієнтів, локалізований опис змін, опційне застосування до поточного денного плану з ребалансом прийомів.

## Алгоритми та послідовність
1) **Моніторинг активності (ActivityMonitorService)**  
   - Збирає дані за день і за тиждень (кроки, пульс, інтенсивність тренувань).  
   - Обчислює середні/сумарні показники за 7 днів, порівнює поточний день із тижневими середніми.  
   - Тригери (ActivityThresholds):  
     - Різке зростання кроків: >30% від тижневого середнього → StepsSpike=true.  
     - Зміна інтенсивності: >20% від середнього → TrainingIntensityChangePercent.  
     - Аномальний пульс у спокої: <40 або >100.  
   - DTO: `ActivityChangeResult` (StepsChangePercent, StepsSpike, HeartRateAvg, HeartRateAnomaly, TrainingIntensityChangePercent).

2) **Аналіз сну (SleepAnalysisService)**  
   - Береться вікно останніх днів (типово 3) зі SleepRecords.  
   - Обчислює середні: TotalSleepMinutes, DeepSleep%, SleepQuality.  
   - Пороги (SleepThresholds): TotalSleepMinutes < 360, DeepSleep% < 20%, Quality < 60% → IsSleepDeprived=true.  
   - DTO: `SleepQualityAnalysis` (AverageSleepHours, AverageDeepSleepPercent, AverageQuality, IsSleepDeprived).  
   - Метод `ShouldAdjustForSleepDeprivation` вирішує, чи потрібно коригувати раціон через недосип.

3) **Правила корекції (DietCorrectionHelper)**  
   - Калорії:  
     - HighActivity: +10% (AdjustCaloriesForHighActivity).  
     - LowActivity:  -5% (AdjustCaloriesForLowActivity).  
   - Макро:  
     - HighActivity: ↑вуглеводи (+10%), ↓жири (-5%), нормалізація калорій.  
     - LowActivity:  ↓вуглеводи (-10%), ↑білки (+10%), нормалізація калорій.  
     - SleepDeprivation: ↑білки (+10%), ↓вуглеводи (-10%).  
     - AbnormalHeartRate: ↓калорії (-5%), ↑білки, ↓вуглеводи, ↓жири (м’яка дієта).  
   - NormalizeToCalories контролює, щоб після зміни макро сумарні калорії відповідали цілі, збережено напрям зміни (наприклад, вуглеводи справді зросли при high activity).

4) **Розрахунок скоригованих таргетів (DietCorrectionService.CalculateCorrectedMacros)**  
   - Вхід: поточний план (калорії/БЖУ), результати Activity/Sleep.  
   - Застосовує правила (DietCorrectionHelper) у послідовності: висока/низька активність → недосип → аномальний пульс.  
   - Повертає новий `MacroNutrientsDto` (calories, protein, fat, carbs).

5) **Формування рекомендацій (DietCorrectionService.CheckAndSuggestCorrectionsAsync)**  
   - Отримує план, викликає ActivityMonitorService і SleepAnalysisService.  
   - Якщо зміни суттєві (HasMeaningfulChange) → створює Recommendation (DietCorrection) з JSON-пейлоадом нових макро.  
   - `CreateCorrectionRecommendation` задає тип DietCorrection, статус New, payload з макро.  
   - `SuggestMenuChanges` генерує локалізований текст (uk/en) з дельтами калорій/БЖУ, використовує UnitConversionService для одиниць (г/oz).  
   - Локалізація: IStringLocalizer<SharedResources>, тексти причин (HighActivity/LowActivity/SleepDeprived/HeartRateAnomaly) і підсумковий опис змін.

6) **Застосування корекції (DietCorrectionService.ApplyCorrectionAsync)**  
   - Завантажує Recommendation, парсить нові макро.  
   - Оновлює DailyDietPlan (калорії, БЖУ, IsCorrected=true), Recommendation.Status=Applied.  
   - Викликає `RebalanceMealsAsync`:  
     - Масштабує MealTarget калорії/БЖУ пропорційно новим денним таргетам.  
     - Для кожного MealRecipe перераховує PortionsMetadata через PortionCalculator (per-recipe target calories = MealTarget / кількість рецептів).  
     - Якщо RecipeDetails відсутні — очищує PortionsMetadata.  
   - Після ребалансу план зберігається через Meal/MealRecipe репозиторії.

7) **Автоперебудова меню (опціонально)**  
   - У поточній реалізації — ребаланс існуючих прийомів (масштабування макро та порцій).  
   - Повна регенерація меню може бути додана як окрема опція, але не активована за замовчуванням.

## Використані компоненти
- **Сервіси**: `ActivityMonitorService`, `SleepAnalysisService`, `DietCorrectionService`, `DietCorrectionHelper`, `UnitConversionService`.
- **DTO**: `ActivityChangeResult`, `WeeklyActivityAverage`, `SleepQualityAnalysis`, `MacroNutrientsDto`, `ApplyCorrectionDto`, `RecommendationResponseDto`.
- **Репозиторії**: DailyDietPlan, Recommendation, Meal, MealRecipe, Recipe (для деталей порцій).
- **Конфіг/константи**: `ActivityThresholds`, `SleepThresholds`, толеранси для макро/калорій, пороги для пульсу.

## Алгоритмічні деталі
- Поріг значущих змін: перевіряється delta між поточним планом і скоригованими макро (HasMeaningfulChange).
- Трекінг використання рецептів при ребалансі не змінюється (масштабування існуючих прийомів).
- Локалізатор: тексти причин і підсумку, фолбек на дефолтні рядки, якщо ключі відсутні.
- Конвертація одиниць: для білків/жирів/вуглеводів — г або oz (за локаллю користувача); калорії не конвертуються.

## Потік даних (корекція)
1) `POST /api/dailydietplans/{id}/check-corrections`: виклик DietCorrectionService.CheckAndSuggestCorrectionsAsync → створює Recommendation (якщо є зміни).  
2) `POST /api/dailydietplans/{id}/apply-correction`: ApplyCorrectionAsync → оновлює план, ребалансує прийоми, Recommendation → Applied.  
3) `GET /api/recommendations/corrections`: повертає всі DietCorrection зі статусом New (фільтр userId опціонально).

## Важливі аспекти
- Детект активності/сну базується на історії (останній тиждень / останні дні).  
- Корекції безпечно обробляють порожні дані (немає семплів/сну/тренувань → нейтральні значення).  
- Ребаланс зберігає Meal/MealRecipe через репозиторії, щоб нові таргети/порції були персистентні.  
- Локалізація охоплює тексти причин і опис змін; константи порогів винесені у конфіг-класи.

