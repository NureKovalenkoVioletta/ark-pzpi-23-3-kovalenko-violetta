using System;

namespace FitnessProject.Enums;

[Flags]
public enum ProductTags
{
    None = 0,
    Meat = 1 << 0,
    Fish = 1 << 1,
    Dairy = 1 << 2,
    Egg = 1 << 3,
    Honey = 1 << 4,
    Gluten = 1 << 5,
    PlantMilk = 1 << 6,
    Alcohol = 1 << 7,
    Shellfish = 1 << 8,
    Peanut = 1 << 9,
    TreeNut = 1 << 10,
    Soy = 1 << 11,
    Sesame = 1 << 12,
    Legume = 1 << 13,
    HighSodium = 1 << 14,
    HighGI = 1 << 15,
    HighProtein = 1 << 16,
    Potassium = 1 << 17,
    Phosphorus = 1 << 18,
    Sugar = 1 << 19,
    Pork = 1 << 20
}

