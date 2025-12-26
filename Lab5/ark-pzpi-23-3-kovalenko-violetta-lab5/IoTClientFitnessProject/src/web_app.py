from flask import Flask, render_template, jsonify, request
from flask_cors import CORS
from datetime import datetime, timedelta
import threading
import time
import random
from api_client import ApiClient
from sensor_simulator import SensorSimulator
from statistics_calculator import StatisticsCalculator
from config_manager import ConfigManager

app = Flask(__name__)
CORS(app)

config_manager = ConfigManager()
api_client = ApiClient(config_manager)
sensor_simulator = SensorSimulator()
stats_calculator = StatisticsCalculator()

print(f"[WebApp] Config loaded, API base URL: {api_client.base_url}")
print(f"[WebApp] User ID: {api_client.user_id}")

simulation_running = False
simulation_thread = None
simulation_logs = []

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/api/time')
def get_time():
    return jsonify({
        'time': datetime.now().strftime('%H:%M:%S'),
        'date': datetime.now().strftime('%d.%m.%Y')
    })

@app.route('/api/home-data')
def get_home_data():
    try:
        user_id = api_client.user_id
        if user_id is None:
            return jsonify({'error': 'user_id not defined'}), 400
        
        today = datetime.now()
        stats = api_client.get_daily_statistics(today, user_id)
        
        if not stats:
            return jsonify({
                'heartRate': 'Немає даних',
                'steps': 'Немає даних',
                'activity': 'Немає даних'
            })
        
        steps = stats.get('steps', 0)
        hr_avg = stats.get('heartRateAvg')
        activity_score = _calculate_activity_score(stats)
        
        return jsonify({
            'heartRate': f"{float(hr_avg):.0f} bpm" if hr_avg is not None else "Немає даних",
            'steps': f"{int(steps):,}" if steps else "0",
            'activity': f"{activity_score}/100" if activity_score is not None else "Немає даних"
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

def _calculate_activity_score(stats):
    try:
        hr_avg = stats.get('heartRateAvg')
        steps = stats.get('steps', 0)
        
        if hr_avg is None or steps is None:
            return None
        
        hr_score = (float(hr_avg) - 60) / 40 * 50
        steps_score = min(float(steps) / 10000 * 50, 50)
        
        total_score = hr_score + steps_score
        return round(total_score, 1)
    except:
        return None

@app.route('/api/diet-plan', methods=['GET', 'POST'])
def diet_plan():
    if request.method == 'POST':
        try:
            user_id = api_client.user_id
            user_profile = api_client.get_user_profile(user_id)
            
            today = datetime.now().strftime('%Y-%m-%d')
            
            generated_plan = api_client.generate_daily_diet_plan(user_id, today)
            
            if not generated_plan:
                return jsonify({'error': 'Failed to generate plan', 'profile': user_profile}), 500
            
            plan_id = None
            if isinstance(generated_plan, dict):
                plan_data = generated_plan.get('plan')
                if isinstance(plan_data, dict):
                    plan_id = plan_data.get('dailyDietPlanId')
                if not plan_id:
                    plan_id = generated_plan.get('dailyDietPlanId')
            
            if not plan_id:
                plans = api_client.get_daily_diet_plans(user_id)
                if plans:
                    plans_sorted = sorted(plans, key=lambda p: p.get('dailyDietPlanId', 0), reverse=True)
                    plan_id = plans_sorted[0].get('dailyDietPlanId')
            
            if not plan_id:
                return jsonify({'error': 'Failed to get plan ID', 'profile': user_profile}), 500
            
            plan_details = api_client.get_daily_diet_plan(plan_id)
            return jsonify({'plan': plan_details, 'profile': user_profile})
        except Exception as e:
            return jsonify({'error': str(e)}), 500
    
    return jsonify({'error': 'Use POST to generate plan'}), 400

@app.route('/api/recommendations')
def recommendations():
    try:
        user_id = api_client.user_id
        recommendations = api_client.get_recommendations(user_id)
        return jsonify(recommendations or [])
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/statistics/<period>')
def statistics(period):
    try:
        user_id = api_client.user_id
        today = datetime.now()
        
        if period == 'daily':
            stats = api_client.get_daily_statistics(today, user_id)
        elif period == 'weekly':
            week_start = today - timedelta(days=today.weekday())
            stats = api_client.get_weekly_statistics(week_start, user_id)
        else:
            return jsonify({'error': 'Invalid period'}), 400
        
        return jsonify(stats or {})
    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({'error': str(e)}), 500

@app.route('/api/simulation/start', methods=['POST'])
def start_simulation():
    global simulation_running, simulation_thread, simulation_logs
    
    if simulation_running:
        return jsonify({'status': 'already_running'})
    
    simulation_running = True
    simulation_logs = []
    simulation_thread = threading.Thread(target=simulation_loop, daemon=True)
    simulation_thread.start()
    
    print(f"[API] Simulation started")
    return jsonify({'status': 'started'})

@app.route('/api/simulation/stop', methods=['POST'])
def stop_simulation():
    global simulation_running
    simulation_running = False
    return jsonify({'status': 'stopped'})

@app.route('/api/simulation/status')
def simulation_status():
    return jsonify({'running': simulation_running})

@app.route('/api/simulation/logs')
def simulation_logs_endpoint():
    global simulation_logs
    logs = simulation_logs[-100:] if simulation_logs else []
    return jsonify({'logs': logs})

@app.route('/api/heart-rate', methods=['POST'])
def measure_heart_rate():
    try:
        now = datetime.now()
        hr = sensor_simulator.read_heart_rate()
        print(f"[API] Sending heart rate: {hr} to {api_client.base_url}")
        success = api_client.send_telemetry(0, hr, now)
        print(f"[API] Heart rate send result: {success}")
        
        if success:
            stats_calculator.add_heart_rate(hr, now)
            return jsonify({
                'success': True,
                'heartRate': hr,
                'time': now.strftime('%H:%M:%S')
            })
        else:
            return jsonify({'success': False, 'error': 'Failed to send telemetry'}), 500
    except Exception as e:
        print(f"[API] Error in measure_heart_rate: {e}")
        import traceback
        traceback.print_exc()
        return jsonify({'success': False, 'error': str(e)}), 500

@app.route('/api/sleep', methods=['POST'])
def send_sleep_data():
    try:
        today = datetime.now()
        sleep_data = sensor_simulator.generate_sleep_data(today)
        print(f"[API] Sending sleep data to {api_client.base_url}")
        success = api_client.send_sleep_record(sleep_data)
        print(f"[API] Sleep data send result: {success}")
        
        if success:
            stats_calculator.add_sleep_record(sleep_data)
            return jsonify({
                'success': True,
                'sleepData': sleep_data
            })
        else:
            return jsonify({'success': False, 'error': 'Failed to send sleep record'}), 500
    except Exception as e:
        print(f"[API] Error in send_sleep_data: {e}")
        import traceback
        traceback.print_exc()
        return jsonify({'success': False, 'error': str(e)}), 500

def simulation_loop():
    global simulation_running, simulation_logs
    config = config_manager.load_config()
    steps_interval = float(config['sensors']['steps_interval'])
    last_steps_send = time.time()
    
    print(f"[Simulation] Loop started, interval: {steps_interval}s")
    simulation_logs.append(f"[{datetime.now().strftime('%H:%M:%S')}] Симуляція запущена")
    simulation_logs.append(f"[{datetime.now().strftime('%H:%M:%S')}] [Налаштування] Кроки: окрема телеметрія кожні {steps_interval}с (1-7 кроків)")
    
    while simulation_running:
        now = datetime.now()
        current_time = time.time()
        steps_elapsed = current_time - last_steps_send
        
        if steps_elapsed >= steps_interval:
            random_steps = random.randint(1, 7)
            print(f"[Simulation] Sending {random_steps} steps to {api_client.base_url}")
            success = api_client.send_telemetry(1, float(random_steps), now)
            print(f"[Simulation] Steps send result: {success}")
            
            log_msg = f"[{now.strftime('%H:%M:%S')}] Кроки відправлено: {random_steps} кроків"
            if not success:
                log_msg = f"[{now.strftime('%H:%M:%S')}] Помилка відправки кроків"
            
            simulation_logs.append(log_msg)
            if len(simulation_logs) > 100:
                simulation_logs.pop(0)
            
            if success:
                stats_calculator.add_steps(random_steps, now)
            
            last_steps_send = current_time
        
        time.sleep(0.1)
    
    simulation_logs.append(f"[{datetime.now().strftime('%H:%M:%S')}] Симуляція зупинена")

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=False)

