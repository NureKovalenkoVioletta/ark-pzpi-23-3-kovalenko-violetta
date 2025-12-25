from datetime import datetime
from config_manager import ConfigManager
from api_client import ApiClient
from sensor_simulator import SensorSimulator

print("=" * 50)
print("ПОЧАТОК ТЕСТУВАННЯ")
print("=" * 50)

print("\n1. Ініціалізація компонентів...")
try:
    config = ConfigManager()
    print("   ConfigManager створено")
    
    api = ApiClient(config)
    print(f"   ApiClient створено (сервер: {api.base_url})")
    
    simulator = SensorSimulator()
    print("   SensorSimulator створено")
except Exception as e:
    print(f"   Помилка ініціалізації: {e}")
    exit(1)

print("\n2. Тест відправки пульсу...")
try:
    heart_rate = simulator.read_heart_rate()
    print(f"   Згенеровано пульс: {heart_rate} bpm")
    success = api.send_telemetry(0, heart_rate, datetime.now())
    print(f"   Результат: {'Відправлено' if success else 'Помилка'}")
except Exception as e:
    print(f"   Помилка: {e}")

print("\n3. Тест відправки кроків...")
try:
    steps = simulator.read_steps()
    print(f"   Згенеровано кроки: {steps}")
    success = api.send_telemetry(1, steps, datetime.now())
    print(f"   Результат: {'Відправлено' if success else 'Помилка'}")
except Exception as e:
    print(f"   Помилка: {e}")

print("\n4. Тест відправки даних про сон...")
try:
    sleep = simulator.generate_sleep_data(datetime.now())
    print(f"   Згенеровано сон: {sleep['totalSleepMinutes']} хвилин")
    success = api.send_sleep_record(sleep)
    print(f"   Результат: {'Відправлено' if success else 'Помилка'}")
except Exception as e:
    print(f"   Помилка: {e}")

print("\n" + "=" * 50)
print("ТЕСТУВАННЯ ЗАВЕРШЕНО")
print("=" * 50)