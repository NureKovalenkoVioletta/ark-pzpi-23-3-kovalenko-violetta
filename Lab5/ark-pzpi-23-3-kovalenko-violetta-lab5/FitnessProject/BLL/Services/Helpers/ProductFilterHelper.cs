using FitnessProject.BLL.DTO.MedicalRestrictions;
using FitnessProject.Entities;
using FitnessProject.Enums;

namespace FitnessProject.BLL.Services.Helpers;

public static class ProductFilterHelper
{
    public static IEnumerable<Product> FilterProductsByRestrictions(
        IEnumerable<Product> products, 
        UserMedicalRestrictionsDto restrictions,
        bool includeAdvisory = true)
    {
        var filtered = products;

        if (restrictions.Allergens.Any())
        {
            filtered = FilterProductsByAllergens(filtered, restrictions.Allergens);
        }

        if (restrictions.DietaryRestriction.HasValue && restrictions.DietaryRestriction.Value != DietaryRestrictionType.None)
        {
            filtered = FilterProductsByDietaryRestrictions(filtered, restrictions.DietaryRestriction.Value);
        }

        if (restrictions.MedicalConditions.Any())
        {
            filtered = FilterProductsByMedicalConditions(filtered, restrictions.MedicalConditions, includeAdvisory);
        }

        return filtered;
    }

    public static IEnumerable<Product> FilterProductsByAllergens(
        IEnumerable<Product> products, 
        List<string> userAllergens)
    {
        if (userAllergens == null || !userAllergens.Any())
        {
            return products;
        }

        return products.Where(p => 
        {
            // Перевірка через поле Allergens
            if (!string.IsNullOrWhiteSpace(p.Allergens))
            {
                if (userAllergens.Any(allergen => 
                    p.Allergens.Contains(allergen, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            // Перевірка через теги
            bool hasEggAllergen = userAllergens.Any(a => a.Equals("Eggs", StringComparison.OrdinalIgnoreCase));
            if (hasEggAllergen && (p.Tags & ProductTags.Egg) != 0)
            {
                return false;
            }

            bool hasMilkAllergen = userAllergens.Any(a => a.Equals("Milk", StringComparison.OrdinalIgnoreCase));
            if (hasMilkAllergen && (p.Tags & ProductTags.Dairy) != 0)
            {
                return false;
            }

            bool hasFishAllergen = userAllergens.Any(a => a.Equals("Fish", StringComparison.OrdinalIgnoreCase));
            if (hasFishAllergen && ((p.Tags & ProductTags.Fish) != 0 || (p.Tags & ProductTags.Shellfish) != 0))
            {
                return false;
            }

            bool hasNutsAllergen = userAllergens.Any(a => a.Equals("Nuts", StringComparison.OrdinalIgnoreCase));
            if (hasNutsAllergen && ((p.Tags & ProductTags.TreeNut) != 0 || (p.Tags & ProductTags.Peanut) != 0))
            {
                return false;
            }

            bool hasPeanutsAllergen = userAllergens.Any(a => a.Equals("Peanuts", StringComparison.OrdinalIgnoreCase));
            if (hasPeanutsAllergen && (p.Tags & ProductTags.Peanut) != 0)
            {
                return false;
            }

            bool hasGlutenAllergen = userAllergens.Any(a => a.Equals("Gluten", StringComparison.OrdinalIgnoreCase));
            if (hasGlutenAllergen && (p.Tags & ProductTags.Gluten) != 0)
            {
                return false;
            }

            bool hasSoyAllergen = userAllergens.Any(a => a.Equals("Soy", StringComparison.OrdinalIgnoreCase));
            if (hasSoyAllergen && (p.Tags & ProductTags.Soy) != 0)
            {
                return false;
            }

            return true;
        });
    }

    public static IEnumerable<Product> FilterProductsByDietaryRestrictions(
        IEnumerable<Product> products, 
        DietaryRestrictionType restriction)
    {
        return restriction switch
        {
            DietaryRestrictionType.Vegetarian => FilterVegetarian(products),
            DietaryRestrictionType.Vegan => FilterVegan(products),
            DietaryRestrictionType.Pescatarian => FilterPescatarian(products),
            DietaryRestrictionType.GlutenFree => FilterGlutenFree(products),
            DietaryRestrictionType.LactoseFree => FilterLactoseFree(products),
            DietaryRestrictionType.Halal => FilterHalal(products),
            DietaryRestrictionType.Kosher => FilterKosher(products),
            _ => products
        };
    }

    public static IEnumerable<Product> FilterProductsByMedicalConditions(
        IEnumerable<Product> products, 
        List<string> medicalConditions,
        bool includeAdvisory = true)
    {
        if (medicalConditions == null || !medicalConditions.Any())
        {
            return products;
        }

        return products.Where(p => !ShouldExcludeByTags(p, medicalConditions, includeAdvisory));
    }

    private static IEnumerable<Product> FilterVegetarian(IEnumerable<Product> products)
    {
        var forbidden = ProductTags.Meat | ProductTags.Pork;
        return products.Where(p => (p.Tags & forbidden) == 0);
    }

    private static IEnumerable<Product> FilterVegan(IEnumerable<Product> products)
    {
        var forbiddenTags = ProductTags.Meat | ProductTags.Pork | ProductTags.Fish | ProductTags.Shellfish | ProductTags.Dairy | ProductTags.Egg | ProductTags.Honey;
        return products.Where(p => (p.Tags & forbiddenTags) == 0);
    }

    private static IEnumerable<Product> FilterPescatarian(IEnumerable<Product> products)
    {
        var forbidden = ProductTags.Meat | ProductTags.Pork;
        return products.Where(p => (p.Tags & forbidden) == 0);
    }

    private static IEnumerable<Product> FilterGlutenFree(IEnumerable<Product> products)
    {
        return products.Where(p => (p.Tags & ProductTags.Gluten) == 0);
    }

    private static IEnumerable<Product> FilterLactoseFree(IEnumerable<Product> products)
    {
        return products.Where(p =>
        {
            var hasDairy = (p.Tags & ProductTags.Dairy) != 0;
            var isPlantMilk = (p.Tags & ProductTags.PlantMilk) != 0;
            return !(hasDairy && !isPlantMilk);
        });
    }

    private static IEnumerable<Product> FilterHalal(IEnumerable<Product> products)
    {
        return products.Where(p => (p.Tags & ProductTags.Alcohol) == 0 && (p.Tags & ProductTags.Pork) == 0);
    }

    private static IEnumerable<Product> FilterKosher(IEnumerable<Product> products)
    {
        return products.Where(p => (p.Tags & ProductTags.Pork) == 0 && (p.Tags & ProductTags.Shellfish) == 0);
    }

    private static bool ShouldExcludeByTags(Product p, List<string> medicalConditions, bool includeAdvisory)
    {
        if (p.Tags == ProductTags.None)
        {
            return false;
        }

        bool hasCondition(string name) => medicalConditions.Any(c => c.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (hasCondition("Diabetes"))
        {
            if ((p.Tags & ProductTags.HighGI) != 0 || (p.Tags & ProductTags.Sugar) != 0)
            {
                return true;
            }
        }

        if (hasCondition("Hypertension"))
        {
            if ((p.Tags & ProductTags.HighSodium) != 0)
            {
                return true;
            }
        }

        if (hasCondition("KidneyDisease"))
        {
            if ((p.Tags & ProductTags.HighProtein) != 0 || (p.Tags & ProductTags.HighSodium) != 0)
            {
                return true;
            }
            if (includeAdvisory)
            {
                if ((p.Tags & ProductTags.Potassium) != 0 || (p.Tags & ProductTags.Phosphorus) != 0 || (p.Tags & ProductTags.Legume) != 0)
                {
                    return true;
                }
            }
        }

        if (hasCondition("CeliacDisease"))
        {
            if ((p.Tags & ProductTags.Gluten) != 0)
            {
                return true;
            }
        }

        if (hasCondition("LactoseIntolerance"))
        {
            var hasDairy = (p.Tags & ProductTags.Dairy) != 0;
            var isPlantMilk = (p.Tags & ProductTags.PlantMilk) != 0;
            if (hasDairy && !isPlantMilk)
            {
                return true;
            }
        }

        return false;
    }

}

