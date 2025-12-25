
from config_manager import ConfigManager
from api_client import ApiClient
from sensor_simulator import SensorSimulator
from statistics_calculator import StatisticsCalculator
from watch_gui import WatchGUI


def main():
    print("=" * 50)
    print("Fitness Tracker - Apple Watch Simulator")
    print("=" * 50)
    print("\nІніціалізація компонентів...")
    
    try:
        config_manager = ConfigManager()
        print("ConfigManager створено")
        
        api_client = ApiClient(config_manager)
        print(f"ApiClient створено (сервер: {api_client.base_url})")
        
        sensor_simulator = SensorSimulator()
        print("SensorSimulator створено")
        
        stats_calculator = StatisticsCalculator()
        print("StatisticsCalculator створено")
        
        print("\nЗапуск GUI...")
        gui = WatchGUI(api_client, sensor_simulator, stats_calculator, config_manager)
        print("GUI створено")
        
        print("\n" + "=" * 50)
        print("Додаток готовий до роботи!")
        print("=" * 50)
        print("\nІнструкції:")
        print("1. Перейдіть на вкладку 'Датчики' для запуску симуляції")
        print("2. Використовуйте вкладку 'Головна' для перегляду поточних даних")
        print("3. Завантажуйте план дієти, рекомендації та статистику з сервера")
        print("\n")
        
        gui.run()
        
    except Exception as e:
        print(f"\nПомилка ініціалізації: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()

