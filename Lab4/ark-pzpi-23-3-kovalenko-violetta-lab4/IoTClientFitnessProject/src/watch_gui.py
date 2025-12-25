
import tkinter as tk
from tkinter import ttk, scrolledtext, messagebox
from datetime import datetime, timedelta
import threading
import time
import random
from typing import Optional
from api_client import ApiClient
from sensor_simulator import SensorSimulator
from statistics_calculator import StatisticsCalculator
from config_manager import ConfigManager


class WatchGUI:
    
    def __init__(self, api_client: ApiClient, sensor_simulator: SensorSimulator, 
                 stats_calculator: StatisticsCalculator, config_manager: ConfigManager):
        self.api_client = api_client
        self.sensor_simulator = sensor_simulator
        self.stats_calculator = stats_calculator
        self.config_manager = config_manager
        
        self.root = tk.Tk()
        self.root.title("Fitness Tracker - Apple Watch Simulator")
        self.root.geometry("600x700")
        
        self.simulation_running = False
        self.simulation_thread = None
        
        self.setup_ui()
        self.start_clock_update()
    
    def setup_ui(self):
        notebook = ttk.Notebook(self.root)
        notebook.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        self.home_tab = self.create_home_tab(notebook)
        self.diet_tab = self.create_diet_tab(notebook)
        self.recommendations_tab = self.create_recommendations_tab(notebook)
        self.statistics_tab = self.create_statistics_tab(notebook)
        self.sensors_tab = self.create_sensors_tab(notebook)
        
        notebook.add(self.home_tab, text="Головна")
        notebook.add(self.diet_tab, text="Дієта")
        notebook.add(self.recommendations_tab, text="Рекомендації")
        notebook.add(self.statistics_tab, text="Статистика")
        notebook.add(self.sensors_tab, text="Датчики")
    
    def create_home_tab(self, parent):
        frame = ttk.Frame(parent, padding="20")
        
        time_label = tk.Label(frame, text="", font=("Arial", 24, "bold"))
        time_label.pack(pady=20)
        self.time_label = time_label
        
        hr_frame = ttk.LabelFrame(frame, text="Пульс", padding="10")
        hr_frame.pack(fill=tk.X, pady=10)
        self.heart_rate_label = tk.Label(hr_frame, text="-- bpm", font=("Arial", 18))
        self.heart_rate_label.pack()
        
        steps_frame = ttk.LabelFrame(frame, text="Кроки", padding="10")
        steps_frame.pack(fill=tk.X, pady=10)
        self.steps_label = tk.Label(steps_frame, text="--", font=("Arial", 18))
        self.steps_label.pack()
        
        activity_frame = ttk.LabelFrame(frame, text="Бал активності", padding="10")
        activity_frame.pack(fill=tk.X, pady=10)
        self.activity_label = tk.Label(activity_frame, text="--/100", font=("Arial", 18))
        self.activity_label.pack()
        
        self.btn_refresh = ttk.Button(frame, text="Оновити дані", command=self.update_home_data)
        self.btn_refresh.pack(pady=10)
        
        return frame
    
    def create_diet_tab(self, parent):
        frame = ttk.Frame(parent, padding="20")
        
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=10)
        
        btn_load = ttk.Button(btn_frame, text="Завантажити план", command=self.load_diet_plan)
        btn_load.pack(side=tk.LEFT, padx=5)
        
        btn_clear = ttk.Button(btn_frame, text="Очистити", command=self.clear_diet_display)
        btn_clear.pack(side=tk.LEFT, padx=5)
        
        self.diet_text = scrolledtext.ScrolledText(frame, height=25, wrap=tk.WORD)
        self.diet_text.pack(fill=tk.BOTH, expand=True)
        
        return frame
    
    def create_recommendations_tab(self, parent):
        frame = ttk.Frame(parent, padding="20")
        
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=10)
        
        btn_load = ttk.Button(btn_frame, text="Завантажити рекомендації", command=self.load_recommendations)
        btn_load.pack(side=tk.LEFT, padx=5)
        
        btn_clear = ttk.Button(btn_frame, text="Очистити", command=self.clear_recommendations_display)
        btn_clear.pack(side=tk.LEFT, padx=5)
        
        self.recommendations_text = scrolledtext.ScrolledText(frame, height=25, wrap=tk.WORD)
        self.recommendations_text.pack(fill=tk.BOTH, expand=True)
        
        return frame
    
    def create_statistics_tab(self, parent):
        frame = ttk.Frame(parent, padding="20")
        
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=10)
        
        btn_daily = ttk.Button(btn_frame, text="Статистика за день", command=self.load_daily_statistics)
        btn_daily.pack(side=tk.LEFT, padx=5)
        
        btn_weekly = ttk.Button(btn_frame, text="Статистика за тиждень", command=self.load_weekly_statistics)
        btn_weekly.pack(side=tk.LEFT, padx=5)
        
        btn_clear = ttk.Button(btn_frame, text="Очистити", command=self.clear_statistics_display)
        btn_clear.pack(side=tk.LEFT, padx=5)
        
        self.statistics_text = scrolledtext.ScrolledText(frame, height=25, wrap=tk.WORD)
        self.statistics_text.pack(fill=tk.BOTH, expand=True)
        
        return frame
    
    def create_sensors_tab(self, parent):
        frame = ttk.Frame(parent, padding="20")
        
        status_frame = ttk.LabelFrame(frame, text="Статус симуляції", padding="10")
        status_frame.pack(fill=tk.X, pady=10)
        
        self.simulation_status_label = tk.Label(status_frame, text="Зупинено", font=("Arial", 14))
        self.simulation_status_label.pack()
        
        btn_frame = ttk.Frame(frame)
        btn_frame.pack(fill=tk.X, pady=10)
        
        self.btn_start = ttk.Button(btn_frame, text="Запустити симуляцію кроків", command=self.start_simulation)
        self.btn_start.pack(side=tk.LEFT, padx=5)
        
        self.btn_stop = ttk.Button(btn_frame, text="Зупинити симуляцію", command=self.stop_simulation, state=tk.DISABLED)
        self.btn_stop.pack(side=tk.LEFT, padx=5)
        
        hr_frame = ttk.LabelFrame(frame, text="Вимірювання пульсу", padding="10")
        hr_frame.pack(fill=tk.X, pady=10)
        
        btn_hr = ttk.Button(hr_frame, text="Виміряти пульс", command=self.measure_heart_rate)
        btn_hr.pack(side=tk.LEFT, padx=5)
        
        self.last_hr_label = tk.Label(hr_frame, text="Останній пульс: не виміряно", font=("Arial", 9))
        self.last_hr_label.pack(side=tk.LEFT, padx=10)
        
        sleep_frame = ttk.LabelFrame(frame, text="Дані про сон", padding="10")
        sleep_frame.pack(fill=tk.X, pady=10)
        
        btn_sleep = ttk.Button(sleep_frame, text="Відправити дані про сон (сьогодні)", command=self.send_sleep_data)
        btn_sleep.pack(side=tk.LEFT, padx=5)
        
        self.last_sleep_label = tk.Label(sleep_frame, text="Останні дані про сон: не відправлено", font=("Arial", 9))
        self.last_sleep_label.pack(side=tk.LEFT, padx=10)
        
        log_frame = ttk.LabelFrame(frame, text="Лог відправки даних", padding="10")
        log_frame.pack(fill=tk.BOTH, expand=True, pady=10)
        
        self.log_text = scrolledtext.ScrolledText(log_frame, height=15, wrap=tk.WORD)
        self.log_text.pack(fill=tk.BOTH, expand=True)
        
        return frame
    
    def start_clock_update(self):
        self.update_clock()
        self.root.after(1000, self.start_clock_update)
    
    def update_clock(self):
        current_time = datetime.now().strftime("%H:%M:%S")
        current_date = datetime.now().strftime("%d.%m.%Y")
        self.time_label.config(text=f"{current_time}\n{current_date}")
    
    def update_home_data(self):
        if not hasattr(self, 'btn_refresh') or self.btn_refresh.cget('state') == tk.DISABLED:
            return
        
        try:
            self.btn_refresh.config(state=tk.DISABLED, text="Оновлення...")
            self.root.update_idletasks()
            
            user_id = self.api_client.user_id
            if user_id is None:
                self.heart_rate_label.config(text="Помилка: user_id")
                self.steps_label.config(text="не визначено")
                self.activity_label.config(text="Перевірте конфіг")
                return
            
            today = datetime.now()
            stats = self.api_client.get_daily_statistics(today, user_id)
            
            if not stats:
                self.heart_rate_label.config(text="Немає даних")
                self.steps_label.config(text="Немає даних")
                self.activity_label.config(text="Немає даних")
                return
            
            steps = stats.get('steps', 0)
            if steps:
                self.steps_label.config(text=f"{int(steps):,}")
            else:
                self.steps_label.config(text="0")
            
            hr_avg = stats.get('heartRateAvg')
            if hr_avg is not None:
                self.heart_rate_label.config(text=f"{float(hr_avg):.0f} bpm")
            else:
                self.heart_rate_label.config(text="Немає даних")
            
            activity_score = self._calculate_activity_score(stats)
            if activity_score is not None:
                self.activity_label.config(text=f"{activity_score}/100")
            else:
                self.activity_label.config(text="Немає даних")
            
            self.root.update_idletasks()
            
        except Exception as e:
            print(f"Помилка оновлення даних: {e}")
            import traceback
            traceback.print_exc()
            self.heart_rate_label.config(text="Помилка")
            self.steps_label.config(text="Помилка")
            self.activity_label.config(text="Помилка")
        finally:
            self.btn_refresh.config(state=tk.NORMAL, text="Оновити дані")
            self.root.update_idletasks()
    
    def _calculate_activity_score(self, stats):
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
    
    def load_diet_plan(self):
        self.diet_text.delete(1.0, tk.END)
        self.diet_text.insert(tk.END, "Завантаження плану дієти...\n")
        self.root.update()
        
        try:
            user_id = self.api_client.user_id
            
            print(f"[GUI] Завантаження профілю для user_id={user_id}")
            user_profile = self.api_client.get_user_profile(user_id)
            if user_profile:
                print(f"[GUI] Профіль завантажено: {user_profile.get('firstName', 'N/A')} {user_profile.get('lastName', 'N/A')}")
            else:
                print(f"[GUI] Профіль не знайдено")
            
            print(f"[GUI] Генерація нового плану дієти...")
            from datetime import datetime
            today = datetime.now().strftime('%Y-%m-%d')
            
            generated_plan = self.api_client.generate_daily_diet_plan(user_id, today)
            
            if not generated_plan:
                self.diet_text.delete(1.0, tk.END)
                text = "Не вдалося згенерувати план дієти.\nСпробуйте ще раз.\n\n"
                if user_profile:
                    text += self._format_user_profile(user_profile)
                self.diet_text.insert(tk.END, text)
                return
            
            plan_id = None
            if isinstance(generated_plan, dict):
                plan_data = generated_plan.get('plan')
                if isinstance(plan_data, dict):
                    plan_id = plan_data.get('dailyDietPlanId')
                    print(f"[GUI] Знайдено ID плану в полі 'plan': {plan_id}")
                if not plan_id:
                    plan_id = generated_plan.get('dailyDietPlanId')
                    if plan_id:
                        print(f"[GUI] Знайдено ID плану в корені: {plan_id}")
            
            if not plan_id:
                print(f"[GUI] Не вдалося отримати ID згенерованого плану. Структура: {list(generated_plan.keys()) if isinstance(generated_plan, dict) else type(generated_plan)}")
                plans = self.api_client.get_daily_diet_plans(user_id)
                if plans:
                    plans_sorted = sorted(plans, key=lambda p: p.get('dailyDietPlanId', 0), reverse=True)
                    plan_id = plans_sorted[0].get('dailyDietPlanId')
                    print(f"[GUI] Використано найновіший план зі списку: ID={plan_id}")
            
            if not plan_id:
                self.diet_text.delete(1.0, tk.END)
                text = "Не вдалося отримати ID згенерованого плану.\n\n"
                if user_profile:
                    text += self._format_user_profile(user_profile)
                self.diet_text.insert(tk.END, text)
                return
            
            print(f"[GUI] Завантаження деталей плану з прийомами: plan_id={plan_id}")
            plan_details = self.api_client.get_daily_diet_plan(plan_id)
            
            if not plan_details:
                self.diet_text.delete(1.0, tk.END)
                text = "Не вдалося завантажити деталі плану.\n\n"
                if user_profile:
                    text += self._format_user_profile(user_profile)
                self.diet_text.insert(tk.END, text)
                return
            
            profile_text = ""
            if user_profile:
                profile_text = self._format_user_profile(user_profile) + "\n" + "=" * 50 + "\n\n"
                print(f"[GUI] Профіль користувача завантажено: {user_profile.get('firstName', 'N/A')}")
            else:
                print(f"[GUI] Профіль користувача не знайдено для user_id={user_id}")
            
            self.diet_text.insert(1.0, profile_text)
            
            print(f"[GUI] Структура плану: {list(plan_details.keys()) if isinstance(plan_details, dict) else type(plan_details)}")
            self.display_diet_plan(plan_details)
            
        except Exception as e:
            self.diet_text.delete(1.0, tk.END)
            self.diet_text.insert(tk.END, f"Помилка: {str(e)}")
            import traceback
            self.diet_text.insert(tk.END, f"\n\nДеталі:\n{traceback.format_exc()}")
    
    def _format_user_profile(self, profile):
        text = "Профіль користувача\n"
        text += "-" * 50 + "\n"
        
        first_name = profile.get('firstName', '')
        last_name = profile.get('lastName', '')
        if first_name or last_name:
            text += f"Ім'я: {first_name} {last_name}\n"
        
        sex = profile.get('sex', '')
        if sex:
            sex_map = {'Male': 'Чоловік', 'Female': 'Жінка', 'Other': 'Інше'}
            text += f"Стать: {sex_map.get(sex, sex)}\n"
        
        height = profile.get('heightCm')
        if height:
            text += f"Зріст: {height} см\n"
        
        weight = profile.get('currentWeightKg')
        if weight:
            text += f"Вага: {weight} кг\n"
        
        activity = profile.get('activityLevel', '')
        if activity:
            activity_map = {
                'Sedentary': 'Малорухливий',
                'Light': 'Легка активність',
                'Moderate': 'Помірна активність',
                'Active': 'Активний',
                'VeryActive': 'Дуже активний'
            }
            text += f"Рівень активності: {activity_map.get(activity, activity)}\n"
        
        birth_date = profile.get('birthDate')
        if birth_date:
            try:
                from datetime import datetime
                date_obj = datetime.fromisoformat(birth_date.replace('Z', '+00:00'))
                age = (datetime.now() - date_obj).days // 365
                text += f"Вік: {age} років\n"
            except:
                pass
        
        medical = profile.get('medicalConditions')
        if medical:
            text += f"Медичні обмеження: {medical}\n"
        
        return text
    
    def display_diet_plan(self, plan_data):
        current_text = self.diet_text.get(1.0, tk.END)
        
        date_str = plan_data.get('dailyPlanCreatedAt')
        if date_str:
            try:
                from datetime import datetime
                date_obj = datetime.fromisoformat(date_str.replace('Z', '+00:00'))
                date = date_obj.strftime('%Y-%m-%d')
            except:
                date = date_str
        else:
            date = 'Невідома дата'
        
        meals = plan_data.get('meals', [])
        
        if not meals:
            if isinstance(plan_data, list):
                meals = plan_data
            elif 'mealDetails' in plan_data:
                meals = plan_data.get('mealDetails', [])
            elif 'Meals' in plan_data:
                meals = plan_data.get('Meals', [])
        
        print(f"[GUI] ========== ОБРОБКА ПЛАНУ ДІЄТИ ==========")
        print(f"[GUI] Знайдено прийомів їжі: {len(meals)}")
        print(f"[GUI] Повна структура plan_data: {list(plan_data.keys()) if isinstance(plan_data, dict) else type(plan_data)}")
        if meals:
            print(f"[GUI] Перший прийом ключі: {meals[0].keys() if isinstance(meals[0], dict) else type(meals[0])}")
            print(f"[GUI] Всі прийоми (mealTime): {[meal.get('mealTime', meal.get('MealTime', 'N/A')) for meal in meals]}")
            print(f"[GUI] Всі прийоми (mealOrder): {[meal.get('mealOrder', meal.get('MealOrder', 'N/A')) for meal in meals]}")
            for i, meal in enumerate(meals, 1):
                if isinstance(meal, dict):
                    print(f"[GUI] Прийом {i}: mealTime={meal.get('mealTime', meal.get('MealTime', 'N/A'))}, "
                          f"mealOrder={meal.get('mealOrder', meal.get('MealOrder', 'N/A'))}, "
                          f"calories={meal.get('mealTargetCalories', meal.get('MealTargetCalories', 0))}")
                else:
                    print(f"[GUI] Прийом {i}: НЕ СЛОВНИК! Тип: {type(meal)}")
        
        total_calories = 0
        total_protein = 0
        total_fat = 0
        total_carbs = 0
        
        for meal in meals:
            if not isinstance(meal, dict):
                continue
            total_calories += meal.get('mealTargetCalories', 0) or 0
            total_protein += meal.get('mealTargetProtein', 0) or 0
            total_fat += meal.get('mealTargetFat', 0) or 0
            total_carbs += meal.get('mealTargetCarbs', 0) or 0
        
        text = f"План дієти на {date}\n"
        text += "=" * 50 + "\n\n"
        text += f"Загальні показники:\n"
        text += f"  Калорії: {total_calories:.0f} ккал\n"
        text += f"  Білки: {total_protein:.1f} г\n"
        text += f"  Жири: {total_fat:.1f} г\n"
        text += f"  Вуглеводи: {total_carbs:.1f} г\n\n"
        
        if meals:
            text += f"Прийоми їжі (всього: {len(meals)}):\n"
            text += "-" * 50 + "\n"
            
            processed_meals = 0
            skipped_meals = 0
            
            print(f"[GUI] Початок обробки {len(meals)} прийомів...")
            
            for idx, meal in enumerate(meals, 1):
                print(f"[GUI] --- Обробка прийому {idx} з {len(meals)} ---")
                if not isinstance(meal, dict):
                    print(f"[GUI] Пропущено прийом {idx}: не є словником, тип: {type(meal)}")
                    skipped_meals += 1
                    continue
                
                processed_meals += 1
                meal_time = meal.get('mealTime', meal.get('MealTime', 'Невідомий час'))
                print(f"[GUI] Обробляється прийом {idx}: mealTime={meal_time}")
                calories = meal.get('mealTargetCalories', meal.get('MealTargetCalories', 0)) or 0
                protein = meal.get('mealTargetProtein', meal.get('MealTargetProtein', 0)) or 0
                fat = meal.get('mealTargetFat', meal.get('MealTargetFat', 0)) or 0
                carbs = meal.get('mealTargetCarbs', meal.get('MealTargetCarbs', 0)) or 0
                
                if isinstance(meal_time, (int, float)):
                    meal_time_map = {
                        1: "Сніданок",
                        2: "Обід", 
                        3: "Вечеря",
                        4: "Перекус"
                    }
                    meal_time = meal_time_map.get(int(meal_time), f"Прийом {int(meal_time)}")
                
                text += f"\n{idx}. {meal_time}\n"
                text += f"   Калорії: {calories:.0f} ккал\n"
                text += f"   Білки: {protein:.1f} г | Жири: {fat:.1f} г | Вуглеводи: {carbs:.1f} г\n"
                
                meal_recipes = meal.get('mealRecipes', meal.get('MealRecipes', []))
                print(f"[GUI] Прийом {idx} ({meal_time}): знайдено {len(meal_recipes)} рецептів")
                
                if meal_recipes:
                    text += "   Страви:\n"
                    for recipe in meal_recipes:
                        if not isinstance(recipe, dict):
                            continue
                        recipe_id = recipe.get('recipeId', recipe.get('RecipeId'))
                        if not recipe_id:
                            continue
                            
                        recipe_name = f'Рецепт {recipe_id}'
                        
                        try:
                            recipe_details = self.api_client.get_recipe(recipe_id)
                            if recipe_details:
                                recipe_name = recipe_details.get('recipeName', recipe_details.get('RecipeName', recipe_name))
                        except Exception as e:
                            print(f"[GUI] Помилка завантаження рецепта {recipe_id}: {e}")
                        
                        portions_metadata = recipe.get('portionsMetadata', recipe.get('PortionsMetadata', ''))
                        
                        portions = 1
                        if portions_metadata:
                            try:
                                import json
                                if isinstance(portions_metadata, str):
                                    portions_data = json.loads(portions_metadata)
                                    portions = portions_data.get('portions', 1)
                                else:
                                    portions = float(portions_metadata)
                            except:
                                try:
                                    portions = float(portions_metadata)
                                except:
                                    portions = 1
                        
                        text += f"     - {recipe_name} ({portions} порцій)\n"
                else:
                    text += "   Страви: не вказано\n"
                    print(f"[GUI] Прийом {idx} не містить рецептів. Ключі: {list(meal.keys())}")
            
            print(f"[GUI] ========== РЕЗУЛЬТАТ ОБРОБКИ ==========")
            print(f"[GUI] Всього прийомів отримано: {len(meals)}")
            print(f"[GUI] Оброблено успішно: {processed_meals}")
            print(f"[GUI] Пропущено: {skipped_meals}")
            print(f"[GUI] Довжина тексту для відображення: {len(text)} символів")
        else:
            text += "Прийоми їжі не знайдено.\n"
            text += f"Структура даних: {list(plan_data.keys())}\n"
        
        print(f"[GUI] Додавання тексту до віджета. Довжина тексту: {len(text)} символів")
        print(f"[GUI] Кількість рядків у тексті: {text.count(chr(10))}")
        self.diet_text.insert(tk.END, text)
        
        final_text = self.diet_text.get(1.0, tk.END)
        final_lines = final_text.count('\n')
        print(f"[GUI] Після додавання: всього рядків у віджеті: {final_lines}")
        print(f"[GUI] Кількість входжень 'Прийоми їжі': {final_text.count('Прийоми їжі')}")
        print(f"[GUI] Кількість входжень 'Сніданок': {final_text.count('Сніданок')}")
        print(f"[GUI] Кількість входжень 'Обід': {final_text.count('Обід')}")
        print(f"[GUI] Кількість входжень 'Вечеря': {final_text.count('Вечеря')}")
        print(f"[GUI] Кількість входжень 'Перекус': {final_text.count('Перекус')}")
    
    def clear_diet_display(self):
        self.diet_text.delete(1.0, tk.END)
    
    def load_recommendations(self):
        self.recommendations_text.delete(1.0, tk.END)
        self.recommendations_text.insert(tk.END, "Завантаження рекомендацій...\n")
        self.root.update()
        
        try:
            user_id = self.api_client.user_id
            recommendations = self.api_client.get_recommendations(user_id)
            
            if not recommendations:
                self.recommendations_text.delete(1.0, tk.END)
                self.recommendations_text.insert(tk.END, "Рекомендації не знайдено.")
                return
            
            self.display_recommendations(recommendations)
            
        except Exception as e:
            self.recommendations_text.insert(tk.END, f"\nПомилка: {str(e)}")
    
    def display_recommendations(self, recommendations):
        self.recommendations_text.delete(1.0, tk.END)
        
        text = "Рекомендації\n"
        text += "=" * 50 + "\n\n"
        
        for i, rec in enumerate(recommendations, 1):
            rec_type = rec.get('recommendationType', 'Unknown')
            message = rec.get('message', 'Немає повідомлення')
            created = rec.get('createdAt', '')
            is_read = rec.get('isRead', False)
            
            status = "Прочитано" if is_read else "Нове"
            
            text += f"{i}. {status}\n"
            text += f"   Тип: {rec_type}\n"
            text += f"   Дата: {created}\n"
            text += f"   {message}\n"
            text += "-" * 50 + "\n\n"
        
        self.recommendations_text.insert(1.0, text)
    
    def clear_recommendations_display(self):
        self.recommendations_text.delete(1.0, tk.END)
    
    def load_daily_statistics(self):
        self.statistics_text.delete(1.0, tk.END)
        self.statistics_text.insert(tk.END, "Завантаження статистики...\n")
        self.root.update_idletasks()
        
        try:
            user_id = self.api_client.user_id
            
            if user_id is None:
                self.statistics_text.delete(1.0, tk.END)
                self.statistics_text.insert(tk.END, "Помилка: user_id не визначено!\n")
                self.statistics_text.insert(tk.END, f"Перевірте конфігурацію: {self.config_manager.config_path}\n")
                self.statistics_text.insert(tk.END, f"Має бути встановлено: device.user_id\n")
                return
            
            today = datetime.now()
            
            self.statistics_text.insert(tk.END, f"Запит статистики:\n")
            self.statistics_text.insert(tk.END, f"  Користувач: {user_id}\n")
            self.statistics_text.insert(tk.END, f"  Дата: {today.date()}\n")
            self.statistics_text.insert(tk.END, f"  Сервер: {self.api_client.base_url}\n")
            self.root.update_idletasks()
            
            stats = self.api_client.get_daily_statistics(today, user_id)
            
            if stats is None:
                self.statistics_text.delete(1.0, tk.END)
                self.statistics_text.insert(tk.END, "Статистика не знайдена (сервер повернув None)\n\n")
                self.statistics_text.insert(tk.END, f"Можливі причини:\n")
                self.statistics_text.insert(tk.END, f"1. Немає даних телеметрії для користувача {user_id}\n")
                self.statistics_text.insert(tk.END, f"2. Сервер не працює або недоступний\n")
                self.statistics_text.insert(tk.END, f"3. Неправильний user_id або дата\n\n")
                self.statistics_text.insert(tk.END, f"Перевірте консоль для деталей помилки.\n")
                return
            
            if isinstance(stats, dict) and len(stats) == 0:
                self.statistics_text.delete(1.0, tk.END)
                self.statistics_text.insert(tk.END, "Сервер повернув порожній об'єкт\n")
                self.statistics_text.insert(tk.END, f"Це означає, що для користувача {user_id} немає даних.\n")
                return
            
            self.statistics_text.insert(tk.END, f"Отримано дані, обробка...\n")
            self.root.update_idletasks()
            
            self.display_daily_statistics(stats)
            
        except Exception as e:
            self.statistics_text.delete(1.0, tk.END)
            self.statistics_text.insert(tk.END, f"Помилка завантаження статистики:\n")
            self.statistics_text.insert(tk.END, f"{str(e)}\n\n")
            import traceback
            self.statistics_text.insert(tk.END, f"Деталі:\n{traceback.format_exc()}")
    
    def load_weekly_statistics(self):
        self.statistics_text.delete(1.0, tk.END)
        self.statistics_text.insert(tk.END, "Завантаження статистики...\n")
        self.root.update()
        
        try:
            user_id = self.api_client.user_id
            today = datetime.now()
            week_start = today - timedelta(days=today.weekday())
            stats = self.api_client.get_weekly_statistics(week_start, user_id)
            
            if not stats:
                self.statistics_text.delete(1.0, tk.END)
                self.statistics_text.insert(tk.END, "Статистика не знайдена.")
                return
            
            self.display_weekly_statistics(stats)
            
        except Exception as e:
            self.statistics_text.insert(tk.END, f"\nПомилка: {str(e)}")
    
    def display_daily_statistics(self, stats):
        self.statistics_text.delete(1.0, tk.END)
        
        text = "Статистика за день\n"
        text += "=" * 50 + "\n\n"
        
        if not stats:
            text += "Дані не отримано\n"
            self.statistics_text.insert(1.0, text)
            return
        
        date = stats.get('date', datetime.now().date().isoformat())
        text += f"Дата: {date}\n\n"
        
        text += "Телеметрія:\n"
        steps = stats.get('steps', 0)
        if steps:
            text += f"  Кроки: {int(steps):,}\n"
        else:
            text += "  Кроки: 0\n"
        
        hr_avg = stats.get('heartRateAvg')
        hr_min = stats.get('heartRateMin')
        hr_max = stats.get('heartRateMax')
        hr_count = stats.get('heartRateSamples', 0)
        
        if hr_avg is not None:
            text += f"  Середній пульс: {float(hr_avg):.1f} bpm\n"
            if hr_min is not None and hr_max is not None:
                text += f"  Мін/Макс: {int(hr_min)} / {int(hr_max)} bpm\n"
            if hr_count > 0:
                text += f"  Зразків пульсу: {hr_count}\n"
        else:
            text += "  Пульс: немає даних\n"
        
        text += "\nСон:\n"
        total_sleep = stats.get('totalSleepMinutes', 0)
        deep_sleep = stats.get('deepSleepMinutes', 0)
        light_sleep = stats.get('lightSleepMinutes', 0)
        awake = stats.get('awakeMinutes', 0)
        quality = stats.get('sleepQualityAvg')
        
        if total_sleep > 0:
            text += f"  Загальний сон: {total_sleep} хв\n"
            if deep_sleep > 0:
                text += f"  Глибокий сон: {deep_sleep} хв\n"
            if light_sleep > 0:
                text += f"  Легкий сон: {light_sleep} хв\n"
            if awake > 0:
                text += f"  Без сну: {awake} хв\n"
            if quality is not None:
                text += f"  Якість: {float(quality):.1f}%\n"
        else:
            text += "  Дані про сон відсутні\n"
        
        text += "\nТренування:\n"
        count = stats.get('trainingCount', 0)
        if count > 0:
            text += f"  Кількість: {count}\n"
            duration = stats.get('trainingDurationMinutes', 0)
            if duration > 0:
                text += f"  Тривалість: {duration} хв\n"
            intensity = stats.get('trainingIntensityAvg')
            if intensity is not None:
                text += f"  Середня інтенсивність: {float(intensity):.1f}\n"
            calories = stats.get('trainingCalories', 0)
            if calories > 0:
                text += f"  Калорії: {float(calories):.0f} ккал\n"
        else:
            text += "  Тренувань не було\n"
        
        self.statistics_text.insert(1.0, text)
    
    def display_weekly_statistics(self, stats):
        self.statistics_text.delete(1.0, tk.END)
        
        text = "Статистика за тиждень\n"
        text += "=" * 50 + "\n\n"
        
        text += f"Період: {stats.get('startDate', '')} - {stats.get('endDate', '')}\n\n"
        
        text += "Загальні показники:\n"
        text += f"  Всього кроків: {stats.get('totalSteps', 0):,}\n"
        
        hr_avg = stats.get('heartRateAvg')
        if hr_avg:
            text += f"  Середній пульс: {hr_avg:.1f} bpm\n"
        
        text += f"  Загальний сон: {stats.get('totalSleepMinutes', 0)} хв\n"
        text += f"  Тренувань: {stats.get('trainingCount', 0)}\n"
        text += f"  Калорій з тренувань: {stats.get('trainingCalories', 0):.0f} ккал\n"
        
        text += "\nТренди:\n"
        steps_trend = stats.get('stepsTrendPercent')
        if steps_trend is not None:
            text += f"  Кроки: {steps_trend:+.1f}%\n"
        
        hr_trend = stats.get('heartRateAvgTrendPercent')
        if hr_trend is not None:
            text += f"  Пульс: {hr_trend:+.1f}%\n"
        
        self.statistics_text.insert(1.0, text)
    
    def clear_statistics_display(self):
        self.statistics_text.delete(1.0, tk.END)
    
    def start_simulation(self):
        if self.simulation_running:
            return
        
        self.simulation_running = True
        self.btn_start.config(state=tk.DISABLED)
        self.btn_stop.config(state=tk.NORMAL)
        self.simulation_status_label.config(text="Працює", fg="green")
        
        self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Симуляція запущена\n")
        self.log_text.see(tk.END)
        
        self.simulation_thread = threading.Thread(target=self.simulation_loop, daemon=True)
        self.simulation_thread.start()
    
    def stop_simulation(self):
        self.simulation_running = False
        self.btn_start.config(state=tk.NORMAL)
        self.btn_stop.config(state=tk.DISABLED)
        self.simulation_status_label.config(text="Зупинено", fg="red")
        
        self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Симуляція зупинена\n")
        self.log_text.see(tk.END)
    
    def measure_heart_rate(self):
        try:
            now = datetime.now()
            hr = self.sensor_simulator.read_heart_rate()
            
            self.log_text.insert(tk.END, f"[{now.strftime('%H:%M:%S')}] Вимірювання пульсу...\n")
            self.log_text.see(tk.END)
            
            success = self.api_client.send_telemetry(0, hr, now)
            
            if success:
                self.stats_calculator.add_heart_rate(hr, now)
                self.last_hr_label.config(
                    text=f"Останній пульс: {hr} bpm ({now.strftime('%H:%M:%S')})",
                    fg="green"
                )
                self.log_text.insert(tk.END, f"[{now.strftime('%H:%M:%S')}] Пульс: {hr} bpm - відправлено\n")
            else:
                self.last_hr_label.config(text="Помилка відправки пульсу", fg="red")
                self.log_text.insert(tk.END, f"[{now.strftime('%H:%M:%S')}] Помилка відправки пульсу\n")
            
            self.log_text.see(tk.END)
            
        except Exception as e:
            self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Помилка: {str(e)}\n")
            self.log_text.see(tk.END)
    
    def simulation_loop(self):
        config = self.config_manager.load_config()
        steps_interval = float(config['sensors']['steps_interval'])
        
        self.log_text.insert(tk.END, f"[Налаштування] Кроки: окрема телеметрія кожні {steps_interval}с (1-7 кроків)\n")
        self.log_text.see(tk.END)
        
        last_steps_send = time.time()
        
        while self.simulation_running:
            now = datetime.now()
            current_time = time.time()
            
            steps_elapsed = current_time - last_steps_send
            
            if steps_elapsed >= steps_interval:
                random_steps = random.randint(1, 7)
                
                success = self.api_client.send_telemetry(1, float(random_steps), now)
                
                if success:
                    self.stats_calculator.add_steps(random_steps, now)
                    self.log_text.insert(tk.END, f"[{now.strftime('%H:%M:%S')}] Кроки відправлено: {random_steps} кроків\n")
                else:
                    self.log_text.insert(tk.END, f"[{now.strftime('%H:%M:%S')}] Помилка відправки кроків\n")
                
                self.log_text.see(tk.END)
                last_steps_send = current_time
            
            time.sleep(0.1)
    
    def send_sleep_data(self):
        try:
            today = datetime.now()
            sleep_data = self.sensor_simulator.generate_sleep_data(today)
            
            self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Генерація даних про сон...\n")
            self.log_text.see(tk.END)
            
            success = self.api_client.send_sleep_record(sleep_data)
            
            if success:
                self.stats_calculator.add_sleep_record(sleep_data)
                total_sleep = sleep_data.get('totalSleepMinutes', 0)
                quality = sleep_data.get('sleepQuality', 0)
                self.last_sleep_label.config(
                    text=f"Останні дані: {total_sleep} хв, якість {quality:.1f}% ({today.date().isoformat()})",
                    fg="green"
                )
                self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Дані про сон відправлено: {total_sleep} хв, якість {quality:.1f}%\n")
            else:
                self.last_sleep_label.config(text="Помилка відправки даних про сон", fg="red")
                self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Помилка відправки даних про сон\n")
            
            self.log_text.see(tk.END)
            
        except Exception as e:
            self.log_text.insert(tk.END, f"[{datetime.now().strftime('%H:%M:%S')}] Помилка: {str(e)}\n")
            self.log_text.see(tk.END)
    
    def run(self):
        self.root.mainloop()

