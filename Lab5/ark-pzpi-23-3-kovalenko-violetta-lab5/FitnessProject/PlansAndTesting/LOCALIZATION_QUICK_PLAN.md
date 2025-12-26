# Локалізація (uk/en): швидкий та простий підхід

## Цілі
- Мінімум змін у коді, максимум системності.
- Підтримати uk (дефолт) та en для текстів API/рекомендацій.
- Конвертація одиниць (metric ↔ imperial) за локаллю користувача.

## Архітектура
- Формат ресурсів: `.resx` (вбудовано в .NET, кращий tooling) або `.json` (якщо зручніше). Рекомендую `.resx`.
- Файли: `Resources/Shared.uk.resx`, `Resources/Shared.en.resx`.
- Ключі: `Area.Feature.Key` (без мови в ключі), напр. `Recommendations.DietCorrection.SleepDeprived`.
- Дефолтна локаль: uk. Фолбек: якщо ключ відсутній у вибраній мові → брати uk і логувати warning.

## Визначення локалі
- Джерело: `User.Locale` (uk/en). Якщо немає — Accept-Language, далі дефолт uk.
- Middleware: `UseRequestLocalization` у `Program.cs` (cultures: uk-UA, en-US).
- У профілі користувача зберігати `Locale` (uk/en).

## Локалізація текстів
- Підключити `AddLocalization()` і `IStringLocalizer<SharedResources>`.
- У сервісах рекомендацій/повідомлень замінити магічні строки на локалізатор: `localizer["Recommendations.DietCorrection.SleepDeprived"]`.
- Параметри в текстах через плейсхолдери `{0}`, `{1}`.

## Одиниці виміру
- Сервіс `IUnitConversionService`: конвертує вагу/об’єм за локаллю (metric ↔ imperial).
- У відповідях API: якщо локаль en → повертати одиниці у imperial (унції/склянки/фунти), інакше metric.
- Формат чисел — через культуру (decimal separator).

## Адмін-управління локалізацією (SuperAdmin)
- Ендпоінти `api/admin/localization` (лише SuperAdmin):
  - GET `/keys?culture=uk|en` — список ключів і значень.
  - GET `/missing?base=uk&compare=en` — ключі, відсутні у compare.
  - PUT `/keys` — оновити значення ключа `{ key, culture, value }`.
  - POST `/export` — віддати zip/json з обома мовами.
  - POST `/import` (dev/feature-flag) — прийняти zip/json і оновити ресурси.
- Джерело даних: `.resx` (`Shared.*`), читання через `ResXResourceReader`, оновлення — запис у resx + інвалідація кешу локалізатора (dev може вимагати перезапуск).
- Захист і аудит: роль SuperAdmin, лог дій (хто/коли/який ключ).
- Валідація: обмеження довжини значень; перевірка парності ключів (uk/en) після оновлення.

## Адмін-можливості (SuperAdmin, мінімум)
- Read-only перегляд ключів та значень (uk/en) через адмін-ендпоінт.
- Перевірка повноти: ендпоінт, що порівнює ключі uk vs en і показує missing/extra.
- (Dev) Експорт/імпорт ресурсів у JSON/zip для правок перекладів.

## Інтеграція в код
1) `Program.cs`: `AddLocalization`, `UseRequestLocalization` (cultures uk-UA, en-US).
2) Додати папку `Resources/` з `Shared.uk.resx` і `Shared.en.resx`.
3) Додати `SharedResources` (порожній клас для прив’язки локалізатора).
4) В `DietCorrectionService`, генераторі планів, валідаційних помилках — замінити строки на `IStringLocalizer`.
5) Додати `IUnitConversionService` + діюче впровадження; у відповідях вибирати одиниці за локаллю.

## Тестування
- Юніт: локалізатор повертає uk/en для ключа, фолбек працює.
- Інтеграційні: запити з локаллю uk/en → тексти і одиниці вірні.
- Перевірка повноти ключів: тест/скрипт порівнює списки ключів uk vs en.

## Етапи
1) Інфраструктура: middleware + ресурси + локалізатор.
2) Тексти рекомендацій/помилок → локалізатор.
3) Конвертація одиниць у відповідях.
4) Адмін-ендпоінт перевірки повноти ключів (опційно).
5) Тести (юнит + інтеграційні).

