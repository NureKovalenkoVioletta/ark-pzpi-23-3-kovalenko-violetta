using System;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.Enums;
using Xunit;

namespace FitnessProject.Tests.Services;

public class CalorieMacroCalculatorTests
{
    [Fact]
    public void CalculateBMR_Male_Female_Other()
    {
        decimal weight = 70m; 
        decimal height = 175m; 
        int age = 30;

        var male = CalorieCalculator.CalculateBMR(weight, height, age, Sex.Male);
        var female = CalorieCalculator.CalculateBMR(weight, height, age, Sex.Female);
        var other = CalorieCalculator.CalculateBMR(weight, height, age, Sex.Other);


        Assert.Equal(1648.75m, male);   
        Assert.Equal(1482.75m, female); 
        Assert.Equal(1565.75m, other);  
    }

    [Fact]
    public void CalculateTDEE_ByActivity()
    {
        var bmr = 1600m;
        Assert.Equal(1920m, CalorieCalculator.CalculateTDEE(bmr, ActivityLevel.Sedentary));      // 1.2
        Assert.Equal(2760m, CalorieCalculator.CalculateTDEE(bmr, ActivityLevel.VeryActive));     // 1.725
    }

    [Fact]
    public void CalculateAge_ValidAndEdge()
    {
        var today = DateTime.Today;
        var birth30 = today.AddYears(-30);
        var birthAlmost30 = today.AddYears(-30).AddDays(1); 

        Assert.Equal(30, CalorieCalculator.CalculateAge(birth30));
        Assert.Equal(29, CalorieCalculator.CalculateAge(birthAlmost30));
    }

    [Fact]
    public void CalculateAge_Null_Throws()
    {
        Assert.Throws<ArgumentException>(() => CalorieCalculator.CalculateAge(null));
    }

    [Fact]
    public void CalculateCaloriesByGoal_AllGoals()
    {
        decimal tdee = 2000m;
        Assert.Equal(1650m, MacroNutrientsCalculator.CalculateCaloriesByGoal(tdee, GoalType.WeightLoss));       // *0.825
        Assert.Equal(2250m, MacroNutrientsCalculator.CalculateCaloriesByGoal(tdee, GoalType.WeightGain));       // *1.125
        Assert.Equal(2000m, MacroNutrientsCalculator.CalculateCaloriesByGoal(tdee, GoalType.WeightMaintenance)); // *1
        Assert.Equal(2000m, MacroNutrientsCalculator.CalculateCaloriesByGoal(tdee, GoalType.HealthCorrection));  // *1
        Assert.Equal(2000m, MacroNutrientsCalculator.CalculateCaloriesByGoal(tdee, null));                       // default maintenance
    }

    [Fact]
    public void CalculateMacros_Typical()
    {
        decimal calories = 2400m;
        decimal weight = 80m;

        var macros = MacroNutrientsCalculator.CalculateMacros(calories, weight, GoalType.WeightMaintenance);

        Assert.Equal(2400m, macros.Calories);
        Assert.Equal(165.0m, macros.ProteinGrams);
        Assert.Equal(73.3m, macros.FatGrams);
        Assert.Equal(270.0m, macros.CarbsGrams);
    }

    [Fact]
    public void CalculateMacros_CarbsNotNegative_WhenCaloriesTooLow()
    {
        decimal calories = 300m;
        decimal weight = 80m;

        var macros = MacroNutrientsCalculator.CalculateMacros(calories, weight, GoalType.WeightMaintenance);

        Assert.Equal(0m, macros.CarbsGrams);
    }

    [Fact]
    public void CalculateMacros_ProteinFromWeightIfGreater()
    {
        decimal calories = 1200m;
        decimal weight = 50m;

        var macros = MacroNutrientsCalculator.CalculateMacros(calories, weight, GoalType.WeightMaintenance);

        Assert.Equal(95.0m, macros.ProteinGrams);
    }

    [Fact]
    public void CalculateMacros_FatFromWeightIfGreater()
    {
        decimal calories = 1200m;
        decimal weight = 50m;

        var macros = MacroNutrientsCalculator.CalculateMacros(calories, weight, GoalType.WeightMaintenance);

        Assert.Equal(45.0m, macros.FatGrams);
    }
}

