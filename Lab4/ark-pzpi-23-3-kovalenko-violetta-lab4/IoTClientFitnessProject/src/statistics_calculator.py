
import statistics
from datetime import datetime, timedelta
from typing import Dict, List, Optional
from collections import defaultdict


class StatisticsCalculator:
    
    def __init__(self):
        self.heart_rate_samples = []
        self.steps_samples = []
        self.sleep_records = []
        self.daily_steps_accumulator = {}
        
    def add_heart_rate(self, value: float, timestamp: datetime):
        self.heart_rate_samples.append((timestamp, value))
    
    def add_steps(self, value: int, timestamp: datetime):
        date_str = timestamp.date().isoformat()
        
        if date_str in self.daily_steps_accumulator:
            last_value = self.daily_steps_accumulator[date_str]
            if value >= last_value:
                self.daily_steps_accumulator[date_str] = value
            else:
                self.daily_steps_accumulator[date_str] = last_value + value
        else:
            self.daily_steps_accumulator[date_str] = value
        
        self.steps_samples.append((timestamp, self.daily_steps_accumulator[date_str]))
    
    def add_sleep_record(self, sleep_data: Dict):
        self.sleep_records.append(sleep_data)
    
    def get_daily_heart_rate_stats(self, date: datetime) -> Optional[Dict]:
        date_start = date.replace(hour=0, minute=0, second=0, microsecond=0)
        date_end = date_start + timedelta(days=1)
        
        day_samples = [
            value for ts, value in self.heart_rate_samples
            if date_start <= ts < date_end
        ]
        
        if not day_samples:
            return None
        
        return {
            "date": date.date().isoformat(),
            "count": len(day_samples),
            "min": float(min(day_samples)),
            "max": float(max(day_samples)),
            "avg": float(statistics.mean(day_samples)),
            "median": float(statistics.median(day_samples))
        }
    
    def get_daily_steps_total(self, date: datetime) -> Optional[Dict]:
        date_str = date.date().isoformat()
        
        if date_str in self.daily_steps_accumulator:
            total_steps = self.daily_steps_accumulator[date_str]
        else:
            date_start = date.replace(hour=0, minute=0, second=0, microsecond=0)
            date_end = date_start + timedelta(days=1)
            
            day_samples = [
                value for ts, value in self.steps_samples
                if date_start <= ts < date_end
            ]
            
            if not day_samples:
                return None
            
            total_steps = max(day_samples) if day_samples else 0
        
        date_start = date.replace(hour=0, minute=0, second=0, microsecond=0)
        date_end = date_start + timedelta(days=1)
        
        day_samples_count = len([
            ts for ts, value in self.steps_samples
            if date_start <= ts < date_end
        ])
        
        return {
            "date": date_str,
            "totalSteps": int(total_steps),
            "samplesCount": day_samples_count
        }
    
    def get_weekly_heart_rate_trend(self, start_date: datetime) -> Optional[Dict]:
        week_data = []
        
        for day_offset in range(7):
            current_date = start_date + timedelta(days=day_offset)
            day_stats = self.get_daily_heart_rate_stats(current_date)
            if day_stats:
                week_data.append(day_stats["avg"])
        
        if len(week_data) < 2:
            return None
        
        first_half = week_data[:3] if len(week_data) >= 3 else week_data[:len(week_data)//2]
        second_half = week_data[-3:] if len(week_data) >= 3 else week_data[len(week_data)//2:]
        
        if not first_half or not second_half:
            return None
        
        first_avg = statistics.mean(first_half)
        second_avg = statistics.mean(second_half)
        
        if first_avg == 0:
            return None
        
        trend_percent = ((second_avg - first_avg) / first_avg) * 100
        
        return {
            "startDate": start_date.date().isoformat(),
            "firstHalfAvg": round(float(first_avg), 2),
            "secondHalfAvg": round(float(second_avg), 2),
            "trendPercent": round(trend_percent, 2),
            "daysWithData": len(week_data)
        }
    
    def get_weekly_steps_trend(self, start_date: datetime) -> Optional[Dict]:
        week_data = []
        
        for day_offset in range(7):
            current_date = start_date + timedelta(days=day_offset)
            day_stats = self.get_daily_steps_total(current_date)
            if day_stats:
                week_data.append(day_stats["totalSteps"])
        
        if len(week_data) < 2:
            return None
        
        first_half = week_data[:3] if len(week_data) >= 3 else week_data[:len(week_data)//2]
        second_half = week_data[-3:] if len(week_data) >= 3 else week_data[len(week_data)//2:]
        
        if not first_half or not second_half:
            return None
        
        first_avg = statistics.mean(first_half)
        second_avg = statistics.mean(second_half)
        
        if first_avg == 0:
            return None
        
        trend_percent = ((second_avg - first_avg) / first_avg) * 100
        
        return {
            "startDate": start_date.date().isoformat(),
            "firstHalfAvg": round(float(first_avg), 2),
            "secondHalfAvg": round(float(second_avg), 2),
            "trendPercent": round(trend_percent, 2),
            "totalStepsWeek": sum(week_data),
            "daysWithData": len(week_data)
        }
    
    def get_sleep_statistics(self, days: int = 7) -> Optional[Dict]:
        if not self.sleep_records:
            return None
        
        recent_records = sorted(
            self.sleep_records,
            key=lambda x: x.get('date', ''),
            reverse=True
        )[:days]
        
        if not recent_records:
            return None
        
        total_sleep_list = [r.get('totalSleepMinutes', 0) for r in recent_records]
        deep_sleep_list = [r.get('deepSleepMinutes', 0) for r in recent_records]
        quality_list = [r.get('sleepQuality', 0) for r in recent_records if r.get('sleepQuality')]
        
        return {
            "periodDays": days,
            "recordsCount": len(recent_records),
            "avgTotalSleep": round(statistics.mean(total_sleep_list), 1),
            "avgDeepSleep": round(statistics.mean(deep_sleep_list), 1),
            "avgQuality": round(statistics.mean(quality_list), 1) if quality_list else None,
            "minTotalSleep": min(total_sleep_list),
            "maxTotalSleep": max(total_sleep_list)
        }
    
    def get_activity_score(self, date: datetime) -> Optional[float]:
        hr_stats = self.get_daily_heart_rate_stats(date)
        steps_stats = self.get_daily_steps_total(date)
        
        if not hr_stats or not steps_stats:
            return None
        
        hr_score = (hr_stats["avg"] - 60) / 40 * 50
        steps_score = min(steps_stats["totalSteps"] / 10000 * 50, 50)
        
        total_score = hr_score + steps_score
        return round(total_score, 1)
    
    def clear_old_data(self, days_to_keep: int = 30):
        cutoff_date = datetime.now() - timedelta(days=days_to_keep)
        
        self.heart_rate_samples = [
            (ts, val) for ts, val in self.heart_rate_samples
            if ts >= cutoff_date
        ]
        
        self.steps_samples = [
            (ts, val) for ts, val in self.steps_samples
            if ts >= cutoff_date
        ]
        
        self.sleep_records = [
            rec for rec in self.sleep_records
            if datetime.fromisoformat(rec.get('date', '2000-01-01')).date() >= cutoff_date.date()
        ]


if __name__ == "__main__":
    from datetime import datetime, timedelta
    import random
    
    calc = StatisticsCalculator()
    
    now = datetime.now()
    
    print("Тестування StatisticsCalculator\n")
    
    for i in range(20):
        hr = random.randint(60, 100)
        calc.add_heart_rate(hr, now - timedelta(minutes=30-i*2))
    
    for i in range(10):
        steps = random.randint(0, 50) * (i + 1)
        calc.add_steps(steps, now - timedelta(minutes=30-i*3))
    
    sleep_data = {
        "date": now.date().isoformat(),
        "totalSleepMinutes": 420,
        "deepSleepMinutes": 84,
        "lightSleepMinutes": 294,
        "awakeMinutes": 42,
        "sleepQuality": 85.0
    }
    calc.add_sleep_record(sleep_data)
    
    print("1. Статистика пульсу за сьогодні:")
    hr_stats = calc.get_daily_heart_rate_stats(now)
    if hr_stats:
        print(f"   Середній: {hr_stats['avg']:.1f} bpm")
        print(f"   Мін/Макс: {hr_stats['min']:.0f} / {hr_stats['max']:.0f} bpm")
        print(f"   Медіана: {hr_stats['median']:.1f} bpm")
        print(f"   Зразків: {hr_stats['count']}")
    
    print("\n2. Кроки за сьогодні:")
    steps_stats = calc.get_daily_steps_total(now)
    if steps_stats:
        print(f"   Всього кроків: {steps_stats['totalSteps']}")
        print(f"   Зразків: {steps_stats['samplesCount']}")
    
    print("\n3. Статистика сну (останні 7 днів):")
    sleep_stats = calc.get_sleep_statistics(7)
    if sleep_stats:
        print(f"   Середній сон: {sleep_stats['avgTotalSleep']} хв")
        print(f"   Середній глибокий сон: {sleep_stats['avgDeepSleep']} хв")
        print(f"   Середня якість: {sleep_stats['avgQuality']}%")
    
    print("\n4. Оцінка активності за сьогодні:")
    activity = calc.get_activity_score(now)
    if activity:
        print(f"   Бал активності: {activity}/100")
    
    print("\n5. Тренд пульсу за тиждень:")
    week_start = now - timedelta(days=7)
    hr_trend = calc.get_weekly_heart_rate_trend(week_start)
    if hr_trend:
        print(f"   Перша половина: {hr_trend['firstHalfAvg']:.1f} bpm")
        print(f"   Друга половина: {hr_trend['secondHalfAvg']:.1f} bpm")
        print(f"   Зміна: {hr_trend['trendPercent']:+.1f}%")

