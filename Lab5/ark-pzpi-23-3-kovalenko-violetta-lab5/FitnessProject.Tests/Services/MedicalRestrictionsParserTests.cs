using System.Linq;
using FitnessProject.BLL.DTO.MedicalRestrictions;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.Enums;
using Xunit;

namespace FitnessProject.Tests.Services;

public class MedicalRestrictionsParserTests
{
    [Fact]
    public void Parse_NullOrEmpty_ReturnsEmptyDto()
    {
        var r1 = MedicalRestrictionsParser.ParseMedicalConditions(null);
        var r2 = MedicalRestrictionsParser.ParseMedicalConditions(string.Empty);

        Assert.Null(r1.DietaryRestriction);
        Assert.Empty(r1.Allergens);
        Assert.Empty(r1.MedicalConditions);

        Assert.Null(r2.DietaryRestriction);
        Assert.Empty(r2.Allergens);
        Assert.Empty(r2.MedicalConditions);
    }

    [Fact]
    public void Parse_Json_WithArrays_AndEnum()
    {
        var json = """
        { "allergens":["Milk","Eggs"], "medicalConditions":["Diabetes","Hypertension"], "dietaryRestriction":"Vegan" }
        """;

        var result = MedicalRestrictionsParser.ParseMedicalConditions(json);

        Assert.Equal(DietaryRestrictionType.Vegan, result.DietaryRestriction);
        Assert.Equal(new[] { "Milk", "Eggs" }, result.Allergens);
        Assert.Equal(new[] { "Diabetes", "Hypertension" }, result.MedicalConditions);
    }

    [Fact]
    public void Parse_Json_CaseInsensitive_EnumAndProps()
    {
        var json = """
        { "dietaryrestriction":"vegetarian", "allergens":["milk"], "medicalconditions":["diabetes"] }
        """;

        var result = MedicalRestrictionsParser.ParseMedicalConditions(json);

        Assert.Equal(DietaryRestrictionType.Vegetarian, result.DietaryRestriction);
        Assert.Equal(new[] { "milk" }, result.Allergens);
        Assert.Equal(new[] { "diabetes" }, result.MedicalConditions);
    }

    [Fact]
    public void Parse_Csv_WithDietary_SetsDietAndMedical()
    {
        var csv = "Diabetes, Vegan";

        var result = MedicalRestrictionsParser.ParseMedicalConditions(csv);

        Assert.Equal(DietaryRestrictionType.Vegan, result.DietaryRestriction);
        Assert.Single(result.MedicalConditions);
        Assert.Equal("Diabetes", result.MedicalConditions.First());
    }

    [Fact]
    public void Parse_Csv_DietaryOnly()
    {
        var csv = "vegetarian";

        var result = MedicalRestrictionsParser.ParseMedicalConditions(csv);

        Assert.Equal(DietaryRestrictionType.Vegetarian, result.DietaryRestriction);
        Assert.Empty(result.MedicalConditions);
    }

    [Fact]
    public void Parse_Csv_MedicalOnly_Multiple()
    {
        var csv = "Diabetes,Hypertension, KidneyDisease";

        var result = MedicalRestrictionsParser.ParseMedicalConditions(csv);

        Assert.Null(result.DietaryRestriction);
        Assert.Equal(new[] { "Diabetes", "Hypertension", "KidneyDisease" }, result.MedicalConditions);
    }

    [Fact]
    public void Parse_MalformedJson_FallsBackToCsv()
    {
        var malformed = "{ allergens:[Milk], dietaryRestriction:Vegan, medicalConditions:[Diabetes] }"; // invalid JSON, will go to CSV split

        var result = MedicalRestrictionsParser.ParseMedicalConditions(malformed);

        Assert.Null(result.DietaryRestriction);
        Assert.Contains("{ allergens:[Milk]", result.MedicalConditions);
        Assert.Contains("dietaryRestriction:Vegan", result.MedicalConditions);
        Assert.Contains("medicalConditions:[Diabetes] }", result.MedicalConditions);
    }

    [Fact]
    public void Serialize_Then_Parse_Roundtrip()
    {
        var dto = new UserMedicalRestrictionsDto
        {
            Allergens = { "Milk", "Eggs" },
            MedicalConditions = { "Diabetes" },
            DietaryRestriction = DietaryRestrictionType.Vegan
        };

        var json = MedicalRestrictionsParser.SerializeMedicalConditions(dto);
        var parsed = MedicalRestrictionsParser.ParseMedicalConditions(json);

        Assert.Equal(dto.DietaryRestriction, parsed.DietaryRestriction);
        Assert.Equal(dto.Allergens, parsed.Allergens);
        Assert.Equal(dto.MedicalConditions, parsed.MedicalConditions);
    }
}

