using System.Text.Json;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services.Helpers;

public static class PortionCalculator
{
    private const decimal MIN_PORTION_GRAMS = 10m;
    private const decimal MAX_PORTION_GRAMS = 1000m;
    private const int ROUND_STEP_GRAMS = 5;

    /**
    Розрахунок множника частини для рецепту
    */

    public static decimal CalculatePortionMultiplier(Recipe recipe, decimal targetCalories)
    {
        if (recipe.RecipeCaloriesPerPortion <= 0)
        {
            return 1m;
        }

        return targetCalories / recipe.RecipeCaloriesPerPortion;
    }

    /**
    Округлення порції до найближчого кратного 5 грам
    */
    public static decimal RoundPortion(decimal grams)
    {
        return Math.Round(grams / ROUND_STEP_GRAMS) * ROUND_STEP_GRAMS;
    }

    /**
    Валідація розміру порції
    */
    public static decimal ValidatePortionSize(decimal grams)
    {
        if (grams < MIN_PORTION_GRAMS)
        {
            return MIN_PORTION_GRAMS;
        }

        if (grams > MAX_PORTION_GRAMS)
        {
            return MAX_PORTION_GRAMS;
        }

        return grams;
    }

    /**
    Побудова метаданих порцій
    */
    public static string BuildPortionsMetadata(Recipe recipe, decimal portionMultiplier)
    {
        if (recipe.RecipeProducts == null || !recipe.RecipeProducts.Any())
        {
            return string.Empty;
        }

        var portions = recipe.RecipeProducts.Select(rp =>
        {
            var scaled = rp.QuantityGrams * portionMultiplier;
            var rounded = RoundPortion(scaled);
            var validated = ValidatePortionSize(rounded);

            return new PortionEntry
            {
                ProductId = rp.ProductId,
                BaseGrams = rp.QuantityGrams,
                PortionGrams = validated
            };
        }).ToList();

        return JsonSerializer.Serialize(portions);
    }

    private class PortionEntry
    {
        public int ProductId { get; set; }
        public decimal BaseGrams { get; set; }
        public decimal PortionGrams { get; set; }
    }
}


