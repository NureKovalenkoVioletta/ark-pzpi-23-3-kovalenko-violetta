# Звіт: MF-1. Збір даних з фітнес-браслета

## Призначення
Прийом телеметрії та даних про сон/тренування від підключених девайсів, збереження в БД та підготовка до подальшої аналітики, статистики та корекції раціону.

## Алгоритми та логіка
- **Прийом телеметрії (TelemetryController)**:
  - `POST /api/telemetry/receive` — приймає один пакет `TelemetryReceiveDto`.
  - `POST /api/telemetry/receive/batch` — приймає батч `TelemetryReceiveBatchDto`.
  - Валідація DTO через `ModelState`.
  - Обробка: `ITelemetryProcessingService.ProcessTelemetryAsync` / `ProcessBatchAsync`.
  - Успішна відповідь локалізована (uk/en).
- **Збереження сировини**:
  - Всі сирі семпли (HeartRate, Steps, ін.) пишуться в `TelemetrySample` через `ITelemetrySampleRepository/Service`.
  - Тренування (`TrainingSession`), сон (`SleepRecord`) мають власні контролери/сервіси для CRUD (адмін-read/write, користувацькі потоки — read/write залежно від ролей, у тестовому варіанті без авторизації).
- **Локалізація**:
  - Локаль визначається через claim `locale` (BasicAuth) або Accept-Language (middleware `UseRequestLocalization` з uk-UA/en-US).
  - Повідомлення успіху/помилок — через `IStringLocalizer<SharedResources>`.
- **Одиниці виміру**:
  - Сервіс `IUnitConversionService` конвертує metric ↔ imperial за локаллю користувача (використовується в інших блоках; MF-1 зберігає значення без конвертації, але відповіді можуть локалізувати текст).

## Серверні компоненти
- **Контролери**:
  - `TelemetryController`: прийом одиночних та батчів телеметрії, локалізовані відповіді.
  - `TelemetrySamplesController`: CRUD сирих сэмплів (читання/тестові сценарії).
  - `SleepRecordsController`: CRUD записів сну.
  - `TrainingSessionsController`: CRUD сесій тренувань.
- **Сервіси/репозиторії**:
  - `ITelemetryProcessingService` — обробка вхідних сэмплів (поточна реалізація: збереження сировини).
  - `ITelemetrySampleService/Repository`, `ISleepRecordService/Repository`, `ITrainingSessionService/Repository` — збереження і вибірка даних.

## Математика / розрахунки
- На етапі MF-1 складна математика не застосовується: дані зберігаються “як є”.
- Підрахунки (суми кроків, середнє HR, тривалість/інтенсивність тренувань, агрегація сну) виконуються в наступних блоках (MF-4, корекція в MF-3).

## Локалізовані повідомлення (приклади ключів)
- `Telemetry.ReceiveSuccess`
- `Telemetry.BatchSuccess`
- Помилки: `Errors.BadRequest`, `Errors.NotFound`, `Errors.IdMismatch` тощо (спільні ключі).

## Основні DTO (усічено)
- `TelemetryReceiveDto`, `TelemetryReceiveBatchDto`
- `TelemetrySampleCreate/Update/ResponseDto`
- `SleepRecordCreate/Update/ResponseDto`
- `TrainingSessionCreate/Update/ResponseDto`

## Потоки даних
1) Девайс / клієнт надсилає HR/кроки/калорії → `TelemetryController`.
2) Контролер викликає `ITelemetryProcessingService` → репозиторій `TelemetrySample`.
3) Аналогічно для сну/тренувань: контролер → сервіс → репозиторій.
4) Дані доступні для агрегації (MF-4) та корекції (MF-3).

