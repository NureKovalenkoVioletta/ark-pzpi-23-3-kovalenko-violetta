import requests
import time
from typing import Dict, Optional
from config_manager import ConfigManager


class BaseApiClient:
    
    def __init__(self, config_manager: ConfigManager):
        self.config_manager = config_manager
        config = config_manager.load_config()
        
        self.base_url = config['server']['base_url'].rstrip('/')
        self.timeout = config['server']['timeout']
        self.retry_attempts = config['server']['retry_attempts']
        self.retry_delay = config['server']['retry_delay']
        
        self.device_id = config['device']['device_id']
        self.user_id = config['device']['user_id']
        
        self.session = requests.Session()
        self.session.headers.update({
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        })
    
    def _make_request(self, method: str, url: str, payload: Optional[Dict] = None, 
                     params: Optional[Dict] = None) -> Optional[requests.Response]:
        last_error = None
        
        for attempt in range(1, self.retry_attempts + 1):
            try:
                if method.upper() == 'GET':
                    response = self.session.get(url, params=params, timeout=self.timeout)
                elif method.upper() == 'POST':
                    response = self.session.post(url, json=payload, timeout=self.timeout)
                elif method.upper() == 'PUT':
                    response = self.session.put(url, json=payload, timeout=self.timeout)
                elif method.upper() == 'DELETE':
                    response = self.session.delete(url, timeout=self.timeout)
                else:
                    print(f"[API] Невідомий HTTP метод: {method}")
                    return None
                
                response.raise_for_status()
                return response
                
            except requests.exceptions.Timeout:
                last_error = f"Таймаут запиту (більше {self.timeout} секунд)"
                if attempt < self.retry_attempts:
                    print(f"[API] Спроба {attempt}/{self.retry_attempts}: {last_error}. Повтор через {self.retry_delay} сек...")
                    time.sleep(self.retry_delay)
                    
            except requests.exceptions.ConnectionError:
                last_error = "Помилка підключення до сервера"
                if attempt < self.retry_attempts:
                    print(f"[API] Спроба {attempt}/{self.retry_attempts}: {last_error}. Повтор через {self.retry_delay} сек...")
                    time.sleep(self.retry_delay)
                    
            except requests.exceptions.HTTPError as e:
                last_error = f"HTTP помилка {e.response.status_code}: {e.response.text}"
                print(f"[API] {last_error}")
                return None
                
            except Exception as e:
                last_error = f"Невідома помилка: {str(e)}"
                if attempt < self.retry_attempts:
                    print(f"[API] Спроба {attempt}/{self.retry_attempts}: {last_error}. Повтор через {self.retry_delay} сек...")
                    time.sleep(self.retry_delay)
        
        print(f"[API] Не вдалося виконати запит після {self.retry_attempts} спроб. Остання помилка: {last_error}")
        return None

