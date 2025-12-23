namespace FitnessProject.BLL.Configuration;

public static class ActivityThresholds
{
    public const decimal StepsSpikeThreshold = 0.30m;              // >30% к среднему за неделю
    public const decimal TrainingIntensityChangeThreshold = 0.20m; // >20% к среднему за неделю
    public const decimal RestHeartRateLow = 40m;                   // уд/мин
    public const decimal RestHeartRateHigh = 100m;                 // уд/мин
}

