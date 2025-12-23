using System.Collections.Generic;
using System.Linq;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Xunit;

namespace FitnessProject.Tests.Services;

public class ProductFilterHelperTests
{
    [Fact]
    public void Allergens_FilterOutMatching()
    {
        var products = new[]
        {
            P("Milk Bar", allergens: "Milk"),
            P("Apple", allergens: null)
        };
        var filtered = ProductFilterHelper.FilterProductsByAllergens(products, new List<string> { "milk" }).ToList();
        Assert.Single(filtered);
        Assert.Equal("Apple", filtered[0].ProductName);
    }

    [Fact]
    public void Dietary_Vegan_ExcludesAnimalAndAllergen()
    {
        var products = new[]
        {
            P("Chicken Breast", tags: ProductTags.Meat),
            P("Tofu", allergens: null, tags: ProductTags.None),
            P("Yogurt", allergens: "Milk", tags: ProductTags.Dairy),
            P("Honey Cookie", tags: ProductTags.Honey)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.Vegan).ToList();
        Assert.Single(filtered);
        Assert.Equal("Tofu", filtered[0].ProductName);
    }

    [Fact]
    public void Dietary_Vegetarian_AllowsFishButNoMeat()
    {
        var products = new[]
        {
            P("Chicken Soup", tags: ProductTags.Meat),
            P("Salmon Fillet", tags: ProductTags.Fish),
            P("Broccoli", tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.Vegetarian).ToList();
        Assert.Equal(2, filtered.Count);
        Assert.DoesNotContain(filtered, p => p.ProductName.Contains("Chicken"));
        Assert.Contains(filtered, p => p.ProductName.Contains("Salmon"));
    }

    [Fact]
    public void Dietary_GlutenFree_ExcludesGlutenKeywordAndAllergen()
    {
        var products = new[]
        {
            P("Whole Wheat Bread", restriction: "wheat,gluten", allergens: "Gluten", tags: ProductTags.Gluten),
            P("Rice", restriction: null, allergens: null, tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.GlutenFree).ToList();
        Assert.Single(filtered);
        Assert.Equal("Rice", filtered[0].ProductName);
    }

    [Fact]
    public void Dietary_LactoseFree_ExcludesDairyAndAllergen()
    {
        var products = new[]
        {
            P("Cheese", restriction: "dairy", allergens: "Milk", tags: ProductTags.Dairy),
            P("Almond Milk", restriction: "plant_milk", allergens: null, tags: ProductTags.PlantMilk)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.LactoseFree).ToList();
        Assert.Single(filtered);
        Assert.Equal("Almond Milk", filtered[0].ProductName);
    }

    [Fact]
    public void Dietary_Halal_ExcludesPorkAlcohol()
    {
        var products = new[]
        {
            P("Pork Sausage", tags: ProductTags.Pork),
            P("Beer", restriction: "alcohol", tags: ProductTags.Alcohol),
            P("Chicken", restriction: null, tags: ProductTags.Meat)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.Halal).ToList();
        Assert.Single(filtered);
        Assert.Equal("Chicken", filtered[0].ProductName);
    }

    [Fact]
    public void Dietary_Kosher_ExcludesPorkShellfish()
    {
        var products = new[]
        {
            P("Pork Chop", tags: ProductTags.Pork),
            P("Shrimp", restriction: "shellfish", tags: ProductTags.Shellfish),
            P("Salmon", tags: ProductTags.Fish)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.Kosher).ToList();
        Assert.Single(filtered);
        Assert.Equal("Salmon", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_Diabetes_ExcludesSugarHoneyHighGi()
    {
        var products = new[]
        {
            P("Honey", restriction: "high_gi", tags: ProductTags.Honey | ProductTags.Sugar | ProductTags.HighGI),
            P("Brown Rice", restriction: "high_gi", tags: ProductTags.HighGI),
            P("Tofu", tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "Diabetes" }, includeAdvisory: true).ToList();
        Assert.Single(filtered);
        Assert.Equal("Tofu", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_Hypertension_ExcludesSalt()
    {
        var products = new[]
        {
            P("Salted Nuts", restriction: "high_sodium", tags: ProductTags.HighSodium),
            P("Unsalted Nuts", tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "Hypertension" }, includeAdvisory: true).ToList();
        Assert.Single(filtered);
        Assert.Equal("Unsalted Nuts", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_KidneyDisease_AdvisoryFalse_AllowsLegumes()
    {
        var products = new[]
        {
            P("Chickpeas", restriction: "legume,high_protein", tags: ProductTags.Legume),
            P("Tofu", restriction: "high_protein", tags: ProductTags.HighProtein)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "KidneyDisease" }, includeAdvisory: false).ToList();
        Assert.Single(filtered); 
        Assert.Equal("Chickpeas", filtered[0].ProductName);
    }

    [Fact]
    public void Combined_Vegan_Diabetes_Allergens()
    {
        var products = new[]
        {
            P("Chicken", tags: ProductTags.Meat),
            P("Honey Bar", tags: ProductTags.Honey),
            P("Tofu", allergens: "Milk", tags: ProductTags.Dairy),
            P("Lentils", tags: ProductTags.None)
        };

        var filtered = ProductFilterHelper.FilterProductsByRestrictions(
            products,
            new BLL.DTO.MedicalRestrictions.UserMedicalRestrictionsDto
            {
                Allergens = new List<string> { "Milk" },
                MedicalConditions = new List<string> { "Diabetes" },
                DietaryRestriction = DietaryRestrictionType.Vegan
            },
            includeAdvisory: true).ToList();

        Assert.Single(filtered);
        Assert.Equal("Lentils", filtered[0].ProductName);
    }

    [Fact]
    public void Dietary_Vegan_UkrainianKeywords()
    {
        var products = new[]
        {
            P("курка тушкована", tags: ProductTags.Meat),
            P("тофу", tags: ProductTags.None),
            P("медова гранола", tags: ProductTags.Honey)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.Vegan).ToList();
        Assert.Single(filtered);
        Assert.Equal("тофу", filtered[0].ProductName.ToLower());
    }

    [Fact]
    public void Dietary_GlutenFree_UkrainianKeywords()
    {
        var products = new[]
        {
            P("Хліб пшениця", tags: ProductTags.Gluten),
            P("Гречка", tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByDietaryRestrictions(products, DietaryRestrictionType.GlutenFree).ToList();
        Assert.Single(filtered);
        Assert.Equal("Гречка", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_Diabetes_UkrainianKeywords()
    {
        var products = new[]
        {
            P("Солодкий сік", tags: ProductTags.Sugar | ProductTags.HighGI, restriction: "солодкий"),
            P("Сироп агави", tags: ProductTags.Sugar | ProductTags.HighGI, restriction: "сироп"),
            P("Тофу", tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "Diabetes" }, includeAdvisory: true).ToList();
        Assert.Single(filtered);
        Assert.Equal("Тофу", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_Hypertension_UkrainianKeywords()
    {
        var products = new[]
        {
            P("Солоний сир", restriction: "солоний", tags: ProductTags.Dairy | ProductTags.HighSodium),
            P("Несолоний сир", tags: ProductTags.Dairy)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "Hypertension" }, includeAdvisory: true).ToList();
        Assert.Single(filtered);
        Assert.Equal("Несолоний сир", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_KidneyDisease_UkrainianModerateAndStrict()
    {
        var products = new[]
        {
            P("Білкова паста", restriction: "білок", tags: ProductTags.HighProtein),
            P("Сіль калійна", restriction: "калій", tags: ProductTags.Potassium),
            P("Овочевий салат", tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "KidneyDisease" }, includeAdvisory: true).ToList();
        Assert.Single(filtered);
        Assert.Equal("Овочевий салат", filtered[0].ProductName);
    }

    [Fact]
    public void Medical_LactoseIntolerance_UkrainianKeywords()
    {
        var products = new[]
        {
            P("Молоко", tags: ProductTags.Dairy),
            P("Арахісова паста", allergens: null, tags: ProductTags.None)
        };
        var filtered = ProductFilterHelper.FilterProductsByMedicalConditions(products, new List<string> { "LactoseIntolerance" }, includeAdvisory: true).ToList();
        Assert.Single(filtered);
        Assert.Equal("Арахісова паста", filtered[0].ProductName);
    }

    private static Product P(string name, string? restriction = null, string? allergens = null, ProductTags tags = ProductTags.None) =>
        new()
        {
            ProductName = name,
            Restriction = restriction,
            Allergens = allergens,
            Tags = tags
        };
}

