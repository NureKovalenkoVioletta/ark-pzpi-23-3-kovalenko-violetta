from typing import Dict, Optional
from datetime import datetime
from base_api_client import BaseApiClient


class StatisticsApiClient(BaseApiClient):
    
    def get_daily_statistics(self, date: datetime, user_id: Optional[int] = None) -> Optional[Dict]:
        if user_id is None:
            user_id = self.user_id
        
        if user_id is None:
            print(f"[API] Помилка: user_id не визначено!")
            return None
        
        date_str = date.strftime("%Y-%m-%d")
        url = f"{self.base_url}/api/statistics/daily/{date_str}"
        params = {"userId": user_id}
        
        print(f"[API] Запит статистики за день:")
        print(f"[API]   URL: {url}")
        print(f"[API]   Параметри: {params}")
        print(f"[API]   User ID: {user_id} (тип: {type(user_id)})")
        
        response = self._make_request('GET', url, params=params)
        
        if response:
            try:
                print(f"[API] Статус відповіді: {response.status_code}")
                data = response.json()
                print(f"[API] Отримано статистику: {len(str(data))} символів")
                print(f"[API] Структура даних: {list(data.keys()) if isinstance(data, dict) else type(data)}")
                return data
            except Exception as e:
                print(f"[API] Помилка парсингу JSON: {e}")
                print(f"[API] Відповідь (перші 500 символів): {response.text[:500]}")
                return None
        else:
            print(f"[API] Не вдалося отримати статистику (response=None)")
            print(f"[API] Перевірте:")
            print(f"[API] 1. Чи працює сервер на {self.base_url}")
            print(f"[API] 2. Чи правильний user_id: {user_id}")
            print(f"[API] 3. Чи є дані для цього користувача на сервері")
            return None
    
    def get_weekly_statistics(self, start_date: datetime, user_id: Optional[int] = None) -> Optional[Dict]:
        if user_id is None:
            user_id = self.user_id
        
        date_str = start_date.strftime("%Y-%m-%d")
        url = f"{self.base_url}/api/statistics/weekly/{date_str}"
        params = {"userId": user_id}
        
        response = self._make_request('GET', url, params=params)
        
        if response:
            return response.json()
        return None

