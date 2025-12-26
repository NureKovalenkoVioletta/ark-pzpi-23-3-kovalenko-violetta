from typing import Dict, Optional
from base_api_client import BaseApiClient


class UserApiClient(BaseApiClient):
    
    def get_user_profile(self, user_id: Optional[int] = None) -> Optional[Dict]:
        if user_id is None:
            user_id = self.user_id
        
        if user_id is None:
            print(f"[API] Помилка: user_id не визначено для завантаження профілю")
            return None
        
        url = f"{self.base_url}/api/userprofiles"
        params = {}
        
        print(f"[API] Запит профілю користувача: user_id={user_id}")
        response = self._make_request('GET', url, params=params)
        
        if response:
            try:
                profiles = response.json()
                print(f"[API] Отримано профілів: {len(profiles) if isinstance(profiles, list) else 'не список'}")
                if isinstance(profiles, list):
                    for profile in profiles:
                        profile_user_id = profile.get('userId')
                        print(f"[API] Перевірка профілю: userId={profile_user_id}, шукаємо {user_id}")
                        if profile_user_id == user_id:
                            print(f"[API] Знайдено профіль для користувача {user_id}")
                            return profile
                print(f"[API] Профіль для користувача {user_id} не знайдено")
            except Exception as e:
                print(f"[API] Помилка парсингу профілів: {e}")
        return None
    
    def get_user_profile_details(self, profile_id: int) -> Optional[Dict]:
        url = f"{self.base_url}/api/userprofiles/{profile_id}/details"
        
        response = self._make_request('GET', url)
        
        if response:
            return response.json()
        return None

