using FitnessProject.Enums;

namespace FitnessProject.BLL.Services.Helpers;

public static class CalorieCalculator
{
    private const decimal BMR_WEIGHT_MULTIPLIER = 10m;
    private const decimal BMR_HEIGHT_MULTIPLIER = 6.25m;
    private const decimal BMR_AGE_MULTIPLIER = 5m;
    private const decimal BMR_MALE_CONSTANT = 5m;
    private const decimal BMR_FEMALE_CONSTANT = 161m;
    private const decimal BMR_OTHER_CONSTANT = 78m;

    private const decimal TDEE_SEDENTARY = 1.2m;
    private const decimal TDEE_LIGHTLY_ACTIVE = 1.375m;
    private const decimal TDEE_MODERATELY_ACTIVE = 1.55m;
    private const decimal TDEE_VERY_ACTIVE = 1.725m;
    private const decimal TDEE_EXTREMELY_ACTIVE = 1.9m;

    /**
    Розрахунок бзового обміну речовин (BMR) за формулой Миффлина-Сан Жеора
    Для чоловіків: BMR = (10 × вага в кг) + (6,25 × зріст в см) – (5 × вік) + 5;
    для жінок: BMR = (10 × вага в кг) + (6,25 × зріст в см) – (5 × вік) – 161. 
    */
    public static decimal CalculateBMR(decimal weight, decimal height, int age, Sex sex)
    {
        var baseBMR = BMR_WEIGHT_MULTIPLIER * weight + BMR_HEIGHT_MULTIPLIER * height - BMR_AGE_MULTIPLIER * age;

        return sex switch
        {
            Sex.Male => baseBMR + BMR_MALE_CONSTANT,
            Sex.Female => baseBMR - BMR_FEMALE_CONSTANT,
            _ => baseBMR - BMR_OTHER_CONSTANT
        };
    }

    /**
    Розрахунок загального щоденного енергетичного витрати (TDEE)
    TDEE = BMR × коефіцієнт активності
    */
    public static decimal CalculateTDEE(decimal bmr, ActivityLevel activityLevel)
    {
        var multiplier = activityLevel switch
        {
            ActivityLevel.Sedentary => TDEE_SEDENTARY,
            ActivityLevel.LightlyActive => TDEE_LIGHTLY_ACTIVE,
            ActivityLevel.ModeratelyActive => TDEE_MODERATELY_ACTIVE,
            ActivityLevel.VeryActive => TDEE_VERY_ACTIVE,
            ActivityLevel.ExtremelyActive => TDEE_EXTREMELY_ACTIVE,
            _ => TDEE_SEDENTARY
        };

        return bmr * multiplier;
    }

    /**
    Розрахунок віку на основі дати народження
    Визначає кількість повних років
    */
    public static int CalculateAge(DateTime? birthDate)
    {
        if (birthDate == null)
        {
            throw new ArgumentException("BirthDate is required to calculate age");
        }

        var today = DateTime.Today;
        var age = today.Year - birthDate.Value.Year;

        if (birthDate.Value.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}

