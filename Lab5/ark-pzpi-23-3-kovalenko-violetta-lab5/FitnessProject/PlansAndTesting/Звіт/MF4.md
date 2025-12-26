# Звіт: MF-4. Перегляд денних і тижневих показників

## Призначення
Агрегація та показ ключових метрик користувача за день і тиждень: кроки, пульс, сон, тренування (кількість, тривалість, інтенсивність, калорії). Порівняння тижнів, тренди та захист від поділу на нуль/відсутності даних.

## Алгоритми та послідовність
1) **Денні показники (`StatisticsService.GetDailyStatisticsAsync`)**
   - Вхід: userId, date.
   - Дані: TelemetrySamples (кроки, HR), SleepRecords, TrainingSessions.
   - Агрегація:
     - HeartRate: Avg/Min/Max/Count вибірок за день.
     - Steps: сума кроків за день.
     - Sleep: Total/Deep/Light/Awake, середній SleepQuality (змішані null/значення).
     - Training: Count, Duration (хв), IntensityAvg (enum → decimal), Calories (з урахуванням null).
   - Фільтр за датою: тільки записи всередині дня.
   - Вихід: `DailyStatisticsDto` (TelemetryAggregateDto, SleepAggregateDto, TrainingAggregateDto).

2) **Тижневі показники (`GetWeeklyStatisticsAsync`)**
   - Вхід: userId, startDate (початок тижня).
   - Обхід 7 днів → виклик GetDailyStatisticsAsync → список `DailyStatisticsDto`.
   - Агрегація по тижню:
     - TotalSteps (сума), HeartRateAvg (середнє від середніх, без порожніх), SleepQualityAvg (середнє), TotalSleepMinutes (сума), TrainingDurationMinutes/Calories (сума), TrainingIntensityAvg (середнє).
   - Тренди (перші 3 дні vs останні 3 дні, пропускаючи нульові): Steps, HeartRateAvg, SleepMinutes, TrainingDuration, TrainingCalories. Якщо валідних даних < 6 або початкове середнє = 0 → trend = null (захист від /0).
   - Вихід: `WeeklyStatisticsDto` (Days = List<DailyStatisticsDto>, агрегати + тренди).

3) **Порівняння тижнів (`CompareWithPreviousWeek`)**
   - Вхід: userId, currentWeekStart.
   - Отримання currentWeek та previousWeek через `GetWeeklyStatisticsAsync`.
   - Розрахунок % зміни: Steps, HeartRateAvg, TotalSleep, TrainingDuration, TrainingCalories з `SafePercentChange` (null, якщо дільник 0 або обидва 0).
   - Вихід: `WeekComparisonDto` (CurrentWeek, PreviousWeek, проценти змін).

4) **Захист від відсутності даних**
   - Порожні вибірки HR/інтенсивності → avg = null (список → Any? → Average : null), щоб уникати `InvalidOperationException`.
   - Тренди: повертають null при недостатній кількості ненульових точок або при нульовому початковому середньому.
   - Датний фільтр у денній агрегації гарантує виключення записів поза межами дня.

## Використані компоненти
- **Сервіс**: `StatisticsService`.
- **DTO**: `TelemetryAggregateDto`, `SleepAggregateDto`, `TrainingAggregateDto`, `DailyStatisticsDto`, `WeeklyStatisticsDto`, `WeekComparisonDto`.
- **Репозиторії**: TelemetrySample, SleepRecord, TrainingSession, Device (зв’язок користувача з девайсами).

## Алгоритмічні деталі
- Інтенсивність тренування з enum → decimal (середнє).
- SleepQuality середнє рахується тільки по записах, де значення є (пропуск null).
- Тренди: (avgLast3 - avgFirst3) / avgFirst3 * 100%, з перевіркою, що avgFirst3 > 0.

## Потоки даних (API)
- Контролер `StatisticsController`:
  - `GET /api/statistics/daily/{date}?userId=` → `GetDailyStatisticsAsync`.
  - `GET /api/statistics/weekly/{startDate}?userId=` → `GetWeeklyStatisticsAsync`.
  - `GET /api/statistics/comparison?startDate&userId=` → `CompareWithPreviousWeek`.

## Локалізація
- Тексти відповідей — структуровані DTO без текстових повідомлень, локалізація не потрібна; обробка помилок використовує загальні ключі (якщо додавати).

## Перевірки/тести (покрито)
- Daily: відсутність девайсів → дефолт, HR/Steps/Training/Sleep агрегація, фільтр за датою.
- Weekly: агрегація сум/середніх, тренди = null при недостатніх даних.
- Comparison: обчислення % змін, без поділу на нуль.

