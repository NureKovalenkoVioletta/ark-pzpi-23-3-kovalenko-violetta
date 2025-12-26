# Тестирование блока рекомендаций по корректировке рациона

Цель: вручную проверить эндпоинты коррекции (`check-corrections`, `apply-correction`, `recommendations/corrections`) и логику триггеров (высокая/низкая активность, недосып, аномальный пульс).

## Предподготовка
1) Пересоздать БД с сидом (чтобы id шли с 1): вызвать `DbInitializer.InitializeAsync(context, forceRecreate: true)`.
2) Пользователь/устройства:
   - UserId=1 (user1@example.com, locale en-US) — для кейсов EN.
   - UserId=4 (diabetic.veg@example.com, locale uk-UA) — для проверки локализации (uk).
   - Есть привязанные устройства и тренировочные данные из сидера.
3) План питания: сгенерировать новый через `POST /api/dailydietplans/generate` с `userId` нужного пользователя. Запомнить `dailyDietPlanId`.

```json
{
  "dbInit": {
    "forceRecreate": true,
    "method": "DbInitializer.InitializeAsync(context, forceRecreate: true)"
  },
  "users": [
    {
      "userId": 1,
      "email": "user1@example.com",
      "locale": "en-US",
      "purpose": "EN test cases"
    },
    {
      "userId": 4,
      "email": "diabetic.veg@example.com",
      "locale": "uk-UA",
      "purpose": "Localization (uk) test cases"
    }
  ],
  "devices": "Use seeded devices linked to the above users",
  "generatePlan": {
    "endpoint": "POST /api/dailydietplans/generate",
    "payloadExample": {
      "userId": 1,
      "date": null,
      "templateDietPlanId": null
    },
    "note": "Generate a new plan for the desired userId and record dailyDietPlanId for testing"
  }
}
```

### Профили для генерации планов (создать, если нет в БД)
```json
POST /api/userprofiles
{
  "userId": 1,
  "firstName": "Alex",
  "lastName": "Walker",
  "sex": 0,
  "heightCm": 182.0,
  "currentWeightKg": 78.0,
  "activityLevel": 3,
  "goalType": 2,
  "preferredUnits": 0,
  "birthDate": "1989-04-10T00:00:00",
  "medicalConditions": "{\"Allergens\":[\"Milk\"],\"MedicalConditions\":[\"Hypertension\"],\"DietaryRestriction\":\"Pescatarian\"}"
}

POST /api/userprofiles
{
  "userId": 4,
  "firstName": "Iryna",
  "lastName": "Vegan",
  "sex": 0,
  "heightCm": 170.0,
  "currentWeightKg": 64.0,
  "activityLevel": 1,
  "goalType": 1,
  "preferredUnits": 0,
  "birthDate": "1992-09-18T00:00:00",
  "medicalConditions": "{\"Allergens\":[\"Gluten\"],\"MedicalConditions\":[\"CeliacDisease\"],\"DietaryRestriction\":\"Vegan\"}"
}
```

### Девайсы для юзеров 1 и 4
```json
POST /api/devices
{
  "userId": 1,
  "deviceType": 0,
  "connectionType": 0,
  "serial": "DEV-USER1-001"
}

POST /api/devices
{
  "userId": 4,
  "deviceType": 0,
  "connectionType": 0,
  "serial": "DEV-USER4-001"
}
```

## Полезные эндпоинты
- `POST /api/dailydietplans/{id}/check-corrections`
- `POST /api/dailydietplans/{id}/apply-correction` (body: `{ "recommendationId": <id> }`)
- `GET  /api/recommendations/corrections?userId=...`

## Тестовые сценарии

### 1. Высокая активность (шаги > 30% среднего)
**Подготовка данных:**
- Добавить телеметрию шагов за последние 7 дней (среднее ~8000), а за тестовый день — ~12000.
Пример батча (даты подставить под сегодняшнюю):  
```
POST /api/telemetry/batch
{
  "items": [
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-09T08:00:00Z", "value": 8000 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-10T08:00:00Z", "value": 8200 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-11T08:00:00Z", "value": 7800 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-12T08:00:00Z", "value": 8100 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-13T08:00:00Z", "value": 7900 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-14T08:00:00Z", "value": 8050 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-15T08:00:00Z", "value": 8000 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-16T08:00:00Z", "value": 12000 } // тестовый день
  ]
}
```
**Шаги:**
1. `POST /api/dailydietplans/generate` с `userId=1`, получить `dailyDietPlanId`.
2. `POST /api/dailydietplans/{id}/check-corrections`.
3. Ожидание: ответ содержит хотя бы одну Recommendation с `RecommendationType=DietCorrection`, payload — калории ↑, углеводы ↑, жиры ↓ (AdjustMacrosForHighActivity) + калории +10%.

### 2. Низкая активность (шаги < -30% среднего)
**Подготовка:** Среднее прошлых дней ~8000, тестовый день ~5000.
Изменить последний элемент батча на `value: 5000`.
**Шаги:** повторить, ожидание: payload с углеводы ↓, белки ↑, калории -5%.

### 3. Недосып
**Подготовка:** Добавить SleepRecord за 3 дня с TotalSleepMinutes < 360 или DeepSleepMinutes/Total < 20%.  
Пример:  
```
POST /api/sleeprecords
{
  "deviceId": 1,
  "date": "2025-12-14T00:00:00Z",
  "totalSleepMinutes": 320,
  "deepSleepMinutes": 50,
  "lightSleepMinutes": 200,
  "awakeMinutes": 30,
  "sleepQuality": 0.55
}
```
(добавить 3 дня подряд).  
**Шаги:** `check-corrections` → ожидание: углеводы ↓, белки ↑ (AdjustMacrosForSleepDeprivation).

### 4. Аномальный пульс
**Подготовка:** Добавить HeartRate в тестовый день < 40 или > 100.  
Пример:  
```
POST /api/telemetry
{
  "deviceId": 1,
  "telemetryType": 0,
  "timestamp": "2025-12-16T09:00:00Z",
  "value": 120
}
```
**Ожидание:** `check-corrections` → калории -10%, белки ↑, углеводы ↓ (AdjustForAbnormalHeartRate).

### 5. Комбинированный кейс (высокая активность + недосып)
**Подготовка:** Совместить данные из сценариев 1 и 3.
**Ожидание:** применятся обе корректировки (high activity + sleep deprivation).

### 6. Локализация (uk)
**Подготовка:** userId=4 (locale uk-UA), данные как в сценарии 1 или 3.
**Ожидание:** поля reason и текст меню в рекомендациях на украинском.

### 7. Применение корректировки
1. Выполнить `check-corrections`, получить RecommendationId.
2. `POST /api/dailydietplans/{id}/apply-correction` body: `{ "recommendationId": <id> }`.
3. Проверить план `GET /api/dailydietplans/{id}` — калории/БЖУ обновились, Recommendation.Status = Applied.

### 8. Получение списка корректировок
`GET /api/recommendations/corrections?userId=1` → только DietCorrection со статусом New.

## Ожидаемые ответы (формат)
- `check-corrections`: 200 OK, `[ { recommendationId, recommendationType: "DietCorrection", recommendationPayload: "{...macro...}", recommendationStatus: "New" } ]`
- `apply-correction`: 200 OK, `DailyDietPlanResponseDto` с новыми калориями/БЖУ.
- `recommendations/corrections`: 200 OK, список DietCorrection (New), фильтр по userId работает.

---

## Полный быстрый сценарий проверки (шаги + JSON)

### 1) Подготовить профиль и девайс (если нет)
```json
POST /api/userprofiles
{
  "userId": 1,
  "firstName": "Alex",
  "lastName": "Walker",
  "sex": 0,
  "heightCm": 182.0,
  "currentWeightKg": 78.0,
  "activityLevel": 3,
  "goalType": 2,
  "preferredUnits": 0,
  "birthDate": "1989-04-10T00:00:00",
  "medicalConditions": "{\"Allergens\":[\"Milk\"],\"MedicalConditions\":[\"Hypertension\"],\"DietaryRestriction\":\"Pescatarian\"}"
}

POST /api/devices
{
  "userId": 1,
  "deviceType": 0,
  "connectionType": 0,
  "serial": "DEV-USER1-001"
}
```

### 2) Сгенерировать план
```json
POST /api/dailydietplans/generate
{
  "userId": 1
}
```
Запомнить `dailyDietPlanId`.

### 3) Поднять активность (шаги > +30% к среднему)
```json
POST /api/telemetry/batch
{
  "items": [
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-09T08:00:00Z", "value": 8000 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-10T08:00:00Z", "value": 8200 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-11T08:00:00Z", "value": 7800 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-12T08:00:00Z", "value": 8100 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-13T08:00:00Z", "value": 7900 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-14T08:00:00Z", "value": 8050 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-15T08:00:00Z", "value": 8000 },
    { "deviceId": 1, "telemetryType": 1, "timestamp": "2025-12-16T08:00:00Z", "value": 12000 }
  ]
}
```

### 4) Получить рекомендации
```json
POST /api/dailydietplans/{dailyDietPlanId}/check-corrections
```
Ожидание: в ответе есть `RecommendationType = "DietCorrection"`, payload с новыми калориями/БЖУ.

### 5) Применить корректировку
```json
POST /api/dailydietplans/{dailyDietPlanId}/apply-correction
{
  "recommendationId": <ID_из_шага_4>
}
```

### 6) Проверить результат
- План:
```json
GET /api/dailydietplans/{dailyDietPlanId}
```
Ожидание: `DailyPlanCalories/Fat/Carbs/Protein` обновлены, `IsCorrected = true`.

- Приёмы и порции:
```json
GET /api/dailydietplans/{dailyDietPlanId}/meals
```
Ожидание: `MealTargetCalories/Protein/Fat/Carbs` пересчитаны под новые макро; `PortionsMetadata` пересчитаны; состав блюд не менялся.

**Что проверять численно (после apply-correction):**
1. План (totals):
   - `DailyPlanCalories` = `recommendationPayload.Calories`
   - `DailyPlanProtein`  = `recommendationPayload.ProteinGrams`
   - `DailyPlanFat`      = `recommendationPayload.FatGrams`
   - `DailyPlanCarbs`    = `recommendationPayload.CarbsGrams`
2. Приёмы (каждый `Meal`):
   - Рассчитать коэффициенты по каждому макро:  
     `kCal = newCaloriesPlan / oldCaloriesPlan`  
     `kP = newProteinPlan / oldProteinPlan` (если старое значение > 0)  
     `kF = newFatPlan / oldFatPlan` (если > 0)  
     `kC = newCarbsPlan / oldCarbsPlan` (если > 0)
   - Ожидаемые новые таргеты приёма:  
     `MealTargetProtein ≈ oldMealProtein * kP`  
     `MealTargetFat ≈ oldMealFat * kF`  
     `MealTargetCarbs ≈ oldMealCarbs * kC`  
     `MealTargetCalories` = `4*Protein + 9*Fat + 4*Carbs` (после округления до 0.1)
3. Порции (`MealRecipe.PortionsMetadata`):
   - Должны измениться, если пересчитаны таргеты (точные числа зависят от рецепта), но состав блюд не меняется.

**Числовой пример (payload из реального ответа):**  
План до: 2103 ккал, P=155.8, F=73.8, C=203.9  
План после: 3306.38 ккал, P=200.4, F=84.6, C=360.8  
Коэф: kCal=1.57, kP=1.29, kF=1.15, kC=1.77  
Если приём был ~450 ккал с 18P / 8F / 45C → станет ~494 ккал с ≈23.1P / 9.2F / 79.7C; `PortionsMetadata` пересчитаны под новую калорийность.

Пример: если было 2100 ккал, стало 2310 ккал — `kCal ≈ 1.1`; белки с 160 → 176 (kP=1.1), жиры 70 → 63 (если fat уменьшили, тогда kF < 1), углеводы 220 → 242 (kC ≈ 1.1). Тогда у каждого приёма белки/жиры/углеводы умножаются на свой коэффициент, калории пересчитываются по формуле выше.

### 7) Список коррекций
```json
GET /api/recommendations/corrections?userId=1
```
Ожидание: DietCorrection со статусом New (после применения выбранная станет Applied).

---

### Кейс недосыпа (при необходимости)
Добавить SleepRecords (< 6 часов или deep < 20%) за 3 дня:
```json
POST /api/sleeprecords
{
  "deviceId": 1,
  "date": "2025-12-14T00:00:00Z",
  "totalSleepMinutes": 320,
  "deepSleepMinutes": 50,
  "lightSleepMinutes": 200,
  "awakeMinutes": 30,
  "sleepQuality": 0.55
}
```
(повторить на 3 даты). Затем повторить шаги 4–6. Ожидание: углеводы ↓, белки ↑, порции пересчитаны.

