from typing import Dict, List, Optional
from datetime import datetime
from base_api_client import BaseApiClient


class TelemetryApiClient(BaseApiClient):
    
    def send_telemetry(self, telemetry_type: int, value: float, 
                      timestamp: datetime, metadata: Optional[Dict] = None) -> bool:
        url = f"{self.base_url}/api/telemetry/receive"
        
        payload = {
            "deviceId": self.device_id,
            "timestamp": timestamp.isoformat(),
            "telemetryType": telemetry_type,
            "value": float(value),
            "metadata": metadata
        }
        
        response = self._make_request('POST', url, payload=payload)
        
        if response:
            print(f"[API] Телеметрія відправлена: тип={telemetry_type}, значення={value}")
            return True
        else:
            print(f"[API] Не вдалося відправити телеметрію: тип={telemetry_type}, значення={value}")
            return False
    
    def send_telemetry_batch(self, items: List[Dict]) -> bool:
        url = f"{self.base_url}/api/telemetry/receive/batch"
        
        payload = {
            "items": items
        }
        
        response = self._make_request('POST', url, payload=payload)
        
        if response:
            print(f"[API] Батч телеметрії відправлено: {len(items)} записів")
            return True
        else:
            print(f"[API] Не вдалося відправити батч телеметрії: {len(items)} записів")
            return False
    
    def send_sleep_record(self, sleep_data: Dict) -> bool:
        url = f"{self.base_url}/api/SleepRecords"
        
        payload = {
            "deviceId": self.device_id,
            **sleep_data
        }
        
        if 'date' in payload and isinstance(payload['date'], datetime):
            payload['date'] = payload['date'].date().isoformat()
        if 'startTime' in payload and isinstance(payload['startTime'], datetime):
            payload['startTime'] = payload['startTime'].isoformat()
        if 'endTime' in payload and isinstance(payload['endTime'], datetime):
            payload['endTime'] = payload['endTime'].isoformat()
        
        response = self._make_request('POST', url, payload=payload)
        
        if response:
            print(f"[API] Дані про сон відправлено: {sleep_data.get('totalSleepMinutes', 0)} хвилин")
            return True
        else:
            print(f"[API] Не вдалося відправити дані про сон")
            return False

