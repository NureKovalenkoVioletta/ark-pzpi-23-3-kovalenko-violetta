from typing import Dict, List, Optional
from base_api_client import BaseApiClient


class DietApiClient(BaseApiClient):
    
    def get_daily_diet_plans(self, user_id: Optional[int] = None) -> List[Dict]:
        if user_id is None:
            user_id = self.user_id
        
        url = f"{self.base_url}/api/dailydietplans"
        params = {"userId": user_id}
        
        print(f"[API] Запит планів дієти: user_id={user_id}")
        response = self._make_request('GET', url, params=params)
        
        if response:
            plans = response.json()
            print(f"[API] Отримано планів: {len(plans) if isinstance(plans, list) else 'не список'}")
            if isinstance(plans, list) and len(plans) > 0:
                for i, plan in enumerate(plans):
                    plan_id = plan.get('dailyDietPlanId')
                    plan_date = plan.get('dailyPlanCreatedAt', 'N/A')
                    print(f"[API]   План {i+1}: ID={plan_id}, дата={plan_date}")
            return plans if isinstance(plans, list) else []
        return []
    
    def get_daily_diet_plan(self, plan_id: int) -> Optional[Dict]:
        url = f"{self.base_url}/api/dailydietplans/{plan_id}/meals"
        
        print(f"[API] Запит плану дієти: plan_id={plan_id}")
        response = self._make_request('GET', url)
        
        if response:
            data = response.json()
            print(f"[API] Отримано план дієти: {list(data.keys()) if isinstance(data, dict) else type(data)}")
            if isinstance(data, dict):
                meals = data.get('meals', [])
                print(f"[API] ========== ДАНІ ВІД СЕРВЕРА ==========")
                print(f"[API] Знайдено прийомів їжі: {len(meals)}")
                if meals:
                    print(f"[API] Перший прийом ключі: {list(meals[0].keys()) if isinstance(meals[0], dict) else type(meals[0])}")
                    for i, meal in enumerate(meals, 1):
                        if isinstance(meal, dict):
                            print(f"[API] Прийом {i}: mealTime={meal.get('mealTime', meal.get('MealTime', 'N/A'))}, "
                                  f"mealOrder={meal.get('mealOrder', meal.get('MealOrder', 'N/A'))}, "
                                  f"mealId={meal.get('mealId', meal.get('MealId', 'N/A'))}")
                        else:
                            print(f"[API] Прийом {i}: НЕ СЛОВНИК! Тип: {type(meal)}")
                else:
                    print(f"[API] ПРИЙОМІВ НЕ ЗНАЙДЕНО! Структура даних: {list(data.keys())}")
            return data
        return None
    
    def generate_daily_diet_plan(self, user_id: Optional[int] = None, date: Optional[str] = None) -> Optional[Dict]:
        if user_id is None:
            user_id = self.user_id
        
        from datetime import datetime
        if date is None:
            date = datetime.now().strftime('%Y-%m-%d')
        
        url = f"{self.base_url}/api/dailydietplans/generate"
        payload = {
            "userId": user_id,
            "date": date
        }
        
        print(f"[API] Генерація нового плану дієти: user_id={user_id}, date={date}")
        response = self._make_request('POST', url, payload=payload)
        
        if response:
            data = response.json()
            print(f"[API] План дієти згенеровано")
            print(f"[API] Структура відповіді: {list(data.keys()) if isinstance(data, dict) else type(data)}")
            
            if isinstance(data, dict):
                plan = data.get('plan', {})
                if plan:
                    plan_id = plan.get('dailyDietPlanId')
                    print(f"[API] ID згенерованого плану: {plan_id}")
                else:
                    plan_id = data.get('dailyDietPlanId')
                    if plan_id:
                        print(f"[API] ID згенерованого плану (в корені): {plan_id}")
            
            return data
        return None
    
    def get_recipe(self, recipe_id: int) -> Optional[Dict]:
        url = f"{self.base_url}/api/recipes/{recipe_id}"
        
        response = self._make_request('GET', url)
        
        if response:
            return response.json()
        return None

