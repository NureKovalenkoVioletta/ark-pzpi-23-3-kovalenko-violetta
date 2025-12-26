import json
import os
from typing import Dict, Optional


class ConfigManager: 
    
    def __init__(self, config_path: str = None):

        if config_path is None:
            base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            docker_config_path = os.path.join(base_dir, "config", "config.docker.json")
            config_path = os.path.join(base_dir, "config", "config.json")
            default_config_path = os.path.join(base_dir, "config", "config.default.json")
            
            if os.getenv('DOCKER_ENV') == 'true' and os.path.exists(docker_config_path):
                config_path = docker_config_path
                print(f"Використовується Docker конфігурація: {config_path}")
        else:
            default_config_path = "config/config.default.json"
        
        self.config_path = config_path
        self.default_config_path = default_config_path
        
        self.default_config = {
            "server": {
                "base_url": "http://localhost:5006",  
                "timeout": 10,                        
                "retry_attempts": 3,                  
                "retry_delay": 5                      
            },
            "device": {
                "device_id": 1,                       
                "user_id": 1,                        
                "device_type": "FitnessTracker",     
                "connection_type": "WiFi"             
            },
            "sensors": {
                "heart_rate_interval": 5,             
                "steps_interval": 5,                  
                "blood_pressure_interval": 600        
            },
            "data": {
                "batch_size": 5,                      
                "send_interval": 15                   
            },
            "logging": {
                "level": "INFO",                      
                "file": "logs/iot_client.log"        
            }
        }
    
    def load_config(self) -> Dict:
        if os.path.exists(self.default_config_path):
            try:
                with open(self.default_config_path, 'r', encoding='utf-8') as f:
                    default_from_file = json.load(f)
                    self.default_config = self._merge_dicts(self.default_config, default_from_file)
            except Exception as e:
                print(f"Помилка завантаження дефолтної конфігурації: {e}")
        
        if os.path.exists(self.config_path):
            try:
                with open(self.config_path, 'r', encoding='utf-8') as f:
                    config = json.load(f)
                    merged_config = self._merge_dicts(self.default_config, config)
                    print(f"Конфігурацію завантажено з {self.config_path}")
                    return merged_config
            except json.JSONDecodeError as e:
                print(f" Помилка парсингу JSON у {self.config_path}: {e}")
                print(" Використовуються дефолтні значення")
                return self.default_config
            except Exception as e:
                print(f" Помилка завантаження конфігурації: {e}")
                return self.default_config
        else:
            print(f"Файл {self.config_path} не знайдено. Використовуються дефолтні значення.")
            return self.default_config.copy()
    
    def save_config(self, config: Dict):
        config_dir = os.path.dirname(self.config_path)
        if config_dir and not os.path.exists(config_dir):
            os.makedirs(config_dir, exist_ok=True)
        
        try:
            with open(self.config_path, 'w', encoding='utf-8') as f:
                json.dump(config, f, indent=2, ensure_ascii=False)
            print(f"Конфігурацію збережено у {self.config_path}")
        except Exception as e:
            print(f"Помилка збереження конфігурації: {e}")
    
    def reset_to_defaults(self):

        default_config = self.load_config()
        self.save_config(default_config)
        print("Конфігурацію скинуто до дефолтних значень")
    
    def update_setting(self, section: str, key: str, value):
        config = self.load_config()
        
        if section in config:
            if key in config[section]:
                config[section][key] = value
                self.save_config(config)
                print(f"Оновлено {section}.{key} = {value}")
            else:
                print(f"Ключ '{key}' не знайдено в секції '{section}'")
        else:
            print(f"Секція '{section}' не знайдена в конфігурації")
    
    def get_setting(self, section: str, key: str, default=None):

        config = self.load_config()
        return config.get(section, {}).get(key, default)
    
    def _merge_dicts(self, default: Dict, user: Dict) -> Dict:
        result = default.copy()
        
        for key, value in user.items():
            if key in result and isinstance(result[key], dict) and isinstance(value, dict):
                result[key] = self._merge_dicts(result[key], value)
            else:
                result[key] = value
        
        return result
    