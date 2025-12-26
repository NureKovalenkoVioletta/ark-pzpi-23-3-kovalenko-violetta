import random
from datetime import datetime, timedelta
from typing import Dict, List


class SensorSimulator:

    def __init__(self):
        self.steps_count = 0  
    
    def read_heart_rate(self) -> float:
        heart_rate = random.randint(60, 100)
        return float(heart_rate)
    
    def read_steps(self) -> int:
        new_steps = random.randint(0, 50)
        self.steps_count += new_steps
        return self.steps_count
    
    def reset_steps(self):
        self.steps_count = 0
        print("Лічильник кроків скинуто")
    
    def generate_sleep_data(self, date: datetime) -> Dict:
        total_sleep = random.randint(360, 540)
        
        deep_sleep_percent = random.uniform(0.15, 0.25)
        deep_sleep = int(total_sleep * deep_sleep_percent)
        
        light_sleep_percent = random.uniform(0.50, 0.70)
        light_sleep = int(total_sleep * light_sleep_percent)
        
        awake = total_sleep - deep_sleep - light_sleep
        
        quality = random.uniform(70.0, 95.0)
        
        start_time = date.replace(hour=22, minute=random.randint(0, 30), second=0, microsecond=0)
        
        end_time = start_time + timedelta(minutes=total_sleep)
        
        return {
            "date": date.date().isoformat(),
            "totalSleepMinutes": total_sleep,
            "deepSleepMinutes": deep_sleep,
            "lightSleepMinutes": light_sleep,
            "awakeMinutes": awake,
            "sleepQuality": round(quality, 1),
            "startTime": start_time.isoformat(),
            "endTime": end_time.isoformat()
        }
    
    def generate_telemetry_batch(self, count: int = 10) -> List[Dict]:
        batch = []
        now = datetime.now()
        
        for i in range(count):
            telemetry_type = random.choice([0, 1])
            
            if telemetry_type == 0:
                value = self.read_heart_rate()
            else:
                value = self.read_steps()
            
            timestamp = now - timedelta(minutes=random.randint(0, 30), seconds=random.randint(0, 59))
            
            item = {
                "deviceId": 1,
                "timestamp": timestamp.isoformat(),
                "telemetryType": telemetry_type,
                "value": float(value),
                "metadata": None
            }
            
            batch.append(item)
        
        return batch

