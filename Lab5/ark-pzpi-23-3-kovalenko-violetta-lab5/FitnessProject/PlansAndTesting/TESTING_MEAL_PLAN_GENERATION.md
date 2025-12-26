# Тест-план: MF-2 Формування персонального харчового плану

Набор тест-кейсов для автоматических (xUnit) и/или мануальных проверок сервиса генерации дневного плана.

## Основные компоненты
- `MealPlanGeneratorService`
- `ProductFilterHelper`, `MedicalRestrictionsParser/Mapper`
- `MacroNutrientsCalculator`, `CalorieCalculator`
- Репозитории: UserProfile, Products, Recipes, RecipeProducts, Meal/MealRecipe, DailyDietPlan

## Юнит-тесты (калькуляции и фильтры)
1) CalorieCalculator / MacroNutrientsCalculator
   - BMR корректен для Male/Female/Other.
   - TDEE для всех ActivityLevel.
   - CalculateCaloriesByGoal: WeightLoss/WeightGain/Maintenance/HealthCorrection.
   - CalculateMacros: округления, carbs >= 0 при избытке protein+fat.
2) ProductFilterHelper
   - Allergens: продукт с `Allergens` включает “Eggs” → исключён; без — остаётся.
   - Dietary:
     - Vegetarian исключает meat/chicken/pork.
     - Vegan исключает meat/fish/egg/milk/cheese/butter/honey + Allergens Eggs/Milk/Fish.
     - GlutenFree исключает restriction/Allergens с gluten/wheat/barley/rye.
     - LactoseFree исключает молочное и allergen Milk.
     - Halal исключает pork/alcohol.
     - Kosher исключает pork/shellfish.
   - MedicalConditions:
     - Diabetes: sugar/high_gi/honey/syrup/juice/…
     - Hypertension: salt/high_sodium.
     - KidneyDisease: high_protein/sodium/potassium/phosphorus/legume advisory → исключены при includeAdvisory=true.
     - Celiac: gluten/wheat/barley/rye/malt/oat.
     - LactoseIntolerance: milk/dairy/cheese/cream/yogurt…
   - includeAdvisory=true (по умолчанию): advisory тоже исключаются; если false — остаются.
   - Комбинация diet + medical + allergens: продукт исключается при любом срабатывании.
3) MedicalRestrictionsParser
   - JSON корректный → заполняет Allergens/MedicalConditions/DietaryRestriction.
   - Простой CSV → распознаёт enum DietaryRestrictionType, остальное в MedicalConditions.
   - Case-insensitive enum.
4) MedicalRestrictionsMapper.ShouldExcludeProduct
   - Проверка по productName/restriction/allergens, токенизация, advisory flag.
5) Macro распределение (DistributeCaloriesByMealTime/BalanceMacrosForMeals)
   - Сумма распределения = суточное; доли соответствуют константам; макросы масштабируются пропорционально.

## Интеграционные/сервисные тесты MealPlanGeneratorService
### Базовые сценарии
- Happy-path: профиль с goal, продукты/рецепты доступны → план создаётся, Meals/MealRecipes созданы, макро близко к целям (± толерансы).
- Пустые рецепты/продукты → не падает, ожидаемое поведение (пустое меню или fallback).

### Ограничения и фильтры
- Веган + диабет: разрешённые продукты без мяса/рыбы/молочки/яиц/сахара/мёда/сиропа/высокого ГИ; рецепты только из allowedProductIds.
- Целиакия + аллерген gluten + DietaryRestriction=GlutenFree: исключены продукты/рецепты с gluten/wheat/barley/rye/oat/allergen Gluten.
- LactoseIntolerance + LactoseFree: молочное и allergen Milk исключены.
- Hypertension: исключить high_sodium/salt.
- KidneyDisease: исключить high_protein/high_sodium/potassium/phosphorus/legume advisory (includeAdvisory=true).
- Allergens: Allergens ["Eggs"] → рецепты с продуктами allergen Eggs не подбираются.
- Недавние рецепты (7 дней): рецепты из последних 7 дней не попадают.

### Подбор рецептов и таргеты
- Выбранные рецепты укладываются в CALORIE_TOLERANCE и MACRO_TOLERANCE.
- Если нет подходящих → выбирается ближайший доступный (fallback).
- Порядок mealTimes: Breakfast, Lunch, Dinner, Snack.
- Meal таргеты = сумма рецептов; MealRecipe содержит PortionsMetadata (если есть RecipeDetails).

## Проверки для итогового плана
- Все продукты рецептов ∈ allowedProductIds после фильтров.
- Рецепты не входят в recent-excluded.
- Макро дня близко к целям; суммы приёмов согласованы с распределением.
- DailyPlanStatus = Planned, поля имени/дат заполнены.

## Набор тестовых данных (пример)
- Продукты: Meat/Fish/Dairy/Eggs/Chicken/Salmon/Broccoli/Rice/Oatmeal/Tofu/Lentils/Chickpeas/Spinach/Quinoa/Buckwheat/CoconutMilk/Chia.
- Allergens: Milk, Eggs, Fish, Gluten.
- restriction: high_gi, gluten, dairy, high_protein, high_sodium, etc.
- Рецепты:
  - Vegan Lentil Stew (lentils/spinach) — vegan ok.
  - Quinoa Veggie Bowl (quinoa/chickpeas/spinach/bell pepper) — GF/vegan ok.
  - Tofu Buckwheat Stir-Fry — GF/vegan ok.
  - Salmon Quinoa Bowl — pescatarian ok.
  - Fruit Yogurt Bowl — lactose fails.
- recipeProducts соответствуют выше.

## Структура автотестов (предложение)
- `Tests/Services/MealPlanGeneratorServiceTests.cs` — интеграция сервиса (моки репозиториев).
- `Tests/Helpers/ProductFilterHelperTests.cs`, `MedicalRestrictionsParserTests.cs`, `MedicalRestrictionsMapperTests.cs`, `MacroNutrientsCalculatorTests.cs`, `CalorieCalculatorTests.cs`.

## Что ассерить в сервисных тестах
- Коллекция рецептов в плане: все продукты разрешены фильтрами.
- Нет рецептов из списка recent (7 дней).
- Итоговые макро/калории дневного плана близки к рассчитанным целям (± tolerance).
- MealTarget* = суммам рецептов; PortionsMetadata заполнены (если recipeDetails есть).
- Кол-во приёмов = кол-ву подобранных рецептов; порядок приёмов соответствует MealTime.

