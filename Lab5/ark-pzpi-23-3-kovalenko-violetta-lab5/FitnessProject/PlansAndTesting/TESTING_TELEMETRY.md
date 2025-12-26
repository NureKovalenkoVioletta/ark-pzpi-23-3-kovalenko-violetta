# Руководство по тестированию API приема телеметрии

## Подготовка к тестированию

### Шаг 1: Получить DeviceId
Перед тестированием нужно узнать ID существующего устройства:
- **GET** `/api/devices` - получить список всех устройств
- Или использовать существующий DeviceId (обычно начинается с 1)

**Важно**: DeviceId должен существовать в БД, иначе получите ошибку "Device with ID X not found"

---

## Тестовые сценарии

### ✅ 1. Успешные тесты (должны вернуть 200 OK) (тест прошел)

#### 1.1. Отправка данных о пульсе (валидный)
**Эндпоинт**: `POST /api/telemetry/receive`

```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:30:00Z",
  "telemetryType": 0,
  "value": 75,
  "metadata": null
}
```
**Ожидаемый результат**: `200 OK` с сообщением "Telemetry received successfully"

---

#### 1.2. Отправка данных о шагах (валидный) (тест прошел)
**Эндпоинт**: `POST /api/telemetry/receive`

```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:35:00Z",
  "telemetryType": 1,
  "value": 5000,
  "metadata": null
}
```
**Ожидаемый результат**: `200 OK`

---

#### 1.3. Отправка данных о давлении (валидный) (тест прошел)
**Эндпоинт**: `POST /api/telemetry/receive`

```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:40:00Z",
  "telemetryType": 2,
  "value": 120,
  "metadata": null
}
```
**Ожидаемый результат**: `200 OK`

---

#### 1.4. Отправка данных о сне (валидный) (тест прошел)
**Эндпоинт**: `POST /api/telemetry/receive`

```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T08:00:00Z",
  "telemetryType": 0,
  "value": 0,
  "metadata": {
    "TotalSleepMinutes": 480,
    "DeepSleepMinutes": 120,
    "LightSleepMinutes": 300,
    "AwakeMinutes": 60,
    "SleepQuality": 85.5,
    "StartTime": "2024-01-14T23:00:00Z",
    "EndTime": "2024-01-15T07:00:00Z"
  }
}
```
**Ожидаемый результат**: `200 OK`

**Примечание**: Для сна `telemetryType` и `value` могут быть любыми, главное - наличие ключей в `metadata`

---

#### 1.5. Батч запрос (несколько записей)
**Эндпоинт**: `POST /api/telemetry/receive/batch` (тст прошел)

```json
{
  "items": [
    {
      "deviceId": 1,
      "timestamp": "2024-01-15T10:00:00Z",
      "telemetryType": 0,
      "value": 72,
      "metadata": null
    },
    {
      "deviceId": 1,
      "timestamp": "2024-01-15T10:05:00Z",
      "telemetryType": 0,
      "value": 75,
      "metadata": null
    },
    {
      "deviceId": 1,
      "timestamp": "2024-01-15T10:10:00Z",
      "telemetryType": 1,
      "value": 100,
      "metadata": null
    }
  ]
}
```
**Ожидаемый результат**: `200 OK` с сообщением "Batch of 3 items received successfully"

---

#### 1.6. Проверка дедупликации (отправка того же самого дважды) (тест прошел)
**Эндпоинт**: `POST /api/telemetry/receive`

Отправьте дважды один и тот же запрос:
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T11:00:00Z",
  "telemetryType": 0,
  "value": 80,
  "metadata": null
}
```

**Ожидаемый результат**: 
- Первый раз: `200 OK` (создана новая запись)
- Второй раз: `200 OK` (обновлена существующая запись, значение не изменилось)

---

### ❌ 2. Тесты на валидацию (должны вернуть 400 BadRequest) (тест прошел)

#### 2.1. Нереалистичный пульс (слишком высокий)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:30:00Z",
  "telemetryType": 0,
  "value": 300,
  "metadata": null
}
```
**Ожидаемый результат**: `400 BadRequest` с сообщением об ошибке валидации

---

#### 2.2. Нереалистичный пульс (слишком низкий) (тест прошел)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:30:00Z",
  "telemetryType": 0,
  "value": 20,
  "metadata": null
}
```
**Ожидаемый результат**: `400 BadRequest`

---

#### 2.3. Отрицательные шаги  (тест прошел) (непонятно сообщение)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:30:00Z",
  "telemetryType": 1,
  "value": -10,
  "metadata": null
}
```
**Ожидаемый результат**: `400 BadRequest`

---

#### 2.4. Нереалистичное время сна (больше 24 часов)  (тест прошел)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T08:00:00Z",
  "telemetryType": 0,
  "value": 0,
  "metadata": {
    "TotalSleepMinutes": 2000,
    "DeepSleepMinutes": 500,
    "LightSleepMinutes": 1200,
    "AwakeMinutes": 300
  }
}
```
**Ожидаемый результат**: `400 BadRequest`

---

#### 2.5. Данные о сне без обязательного поля TotalSleepMinutes (тест прошел)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T08:00:00Z",
  "telemetryType": 0,
  "value": 0,
  "metadata": {
    "DeepSleepMinutes": 120,
    "LightSleepMinutes": 300
  }
}
```
**Ожидаемый результат**: `400 BadRequest`

---

#### 2.6. Несуществующее устройство (тест прошел)
```json
{
  "deviceId": 99999,
  "timestamp": "2024-01-15T10:30:00Z",
  "telemetryType": 0,
  "value": 75,
  "metadata": null
}
```
**Ожидаемый результат**: `400 BadRequest` с сообщением "Device with ID 99999 not found"

---

#### 2.7. Отсутствие обязательных полей (тест прошел) (не совсем правильный вывод ошибки. Пишет про неправильный пульс, хотя сообщение должно быть более абстрактное, потому что у нас нет данных, а тут система указала на неправильные данные(пульс), который даже не вводился)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:30:00Z"
}
```
**Ожидаемый результат**: `400 BadRequest` (валидация Data Annotations)

---

#### 2.8. Отрицательное значение (для любого типа кроме BloodPressure) (тест прошел)
```json
{
  "deviceId": 1,
  "timestamp": "2024-01-15T10:30:00Z",
  "telemetryType": 1,
  "value": -5,
  "metadata": null
}
```
**Ожидаемый результат**: `400 BadRequest`

---

## Проверка результатов в БД

После успешных тестов проверьте, что данные сохранились:

### Проверка TelemetrySamples:
**GET** `/api/telemetry-samples` - получить все записи телеметрии

Или через детальный просмотр устройства:
**GET** `/api/devices/{deviceId}/details` - увидите все TelemetrySamples и SleepRecords

### Проверка SleepRecords:
**GET** `/api/sleep-records` - получить все записи о сне

### Проверка обновления LastSeen:
**GET** `/api/devices/{deviceId}` - проверьте, что поле `lastSeen` обновилось

---

## Значения TelemetryType enum:
- `0` = HeartRate (пульс)
- `1` = Steps (шаги)
- `2` = BloodPressure (давление)

---

## Полезные советы:

1. **Используйте разные timestamps** для каждого теста, чтобы избежать конфликтов
2. **Проверяйте LastSeen** - после каждого успешного запроса оно должно обновляться
3. **Тестируйте батчи** - отправляйте несколько записей одновременно
4. **Проверяйте дедупликацию** - отправьте один и тот же запрос дважды с одинаковым timestamp

---

## Примеры для копирования в Swagger:

### Минимальный валидный запрос (пульс):
```json
{"deviceId":1,"timestamp":"2024-01-15T10:30:00Z","telemetryType":0,"value":75}
```

### Минимальный валидный запрос (шаги):
```json
{"deviceId":1,"timestamp":"2024-01-15T10:35:00Z","telemetryType":1,"value":5000}
```

### Запрос с данными о сне:
```json
{"deviceId":1,"timestamp":"2024-01-15T08:00:00Z","telemetryType":0,"value":0,"metadata":{"TotalSleepMinutes":480,"DeepSleepMinutes":120,"LightSleepMinutes":300,"AwakeMinutes":60,"SleepQuality":85.5}}
```

