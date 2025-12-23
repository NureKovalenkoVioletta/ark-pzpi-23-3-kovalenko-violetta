using System.Collections.Generic;
using FitnessProject.BLL.Services.Helpers;
using Xunit;

namespace FitnessProject.Tests.Services;

public class MedicalRestrictionsMapperTests
{
    [Fact]
    public void Diabetes_Strict_Sugar_Excluded()
    {
        var conditions = new List<string> { "Diabetes" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Honey Bar", "sugar", "Honey", conditions, includeAdvisory: true);
        Assert.True(excluded);
    }

    [Fact]
    public void Diabetes_Advisory_Allowed_WhenIncludeAdvisoryFalse()
    {
        var conditions = new List<string> { "Diabetes" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Без цукру батончик", "без цукру", null, conditions, includeAdvisory: false);
        Assert.False(excluded);
    }

    [Fact]
    public void Hypertension_HighSodium_Excluded()
    {
        var conditions = new List<string> { "Hypertension" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Salted nuts", "high_sodium", null, conditions, includeAdvisory: true);
        Assert.True(excluded);
    }

    [Fact]
    public void KidneyDisease_HighProtein_Strict_Excluded()
    {
        var conditions = new List<string> { "KidneyDisease" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Protein bar", "high_protein", null, conditions, includeAdvisory: true);
        Assert.True(excluded);
    }

    [Fact]
    public void KidneyDisease_Legume_Advisory_RespectsFlag()
    {
        var conditions = new List<string> { "KidneyDisease" };
        var excludedAdvisoryOn = MedicalRestrictionsMapper.ShouldExcludeProduct("Chickpeas", "legume", null, conditions, includeAdvisory: true);
        var excludedAdvisoryOff = MedicalRestrictionsMapper.ShouldExcludeProduct("Chickpeas", "legume", null, conditions, includeAdvisory: false);
        Assert.True(excludedAdvisoryOn);
        Assert.False(excludedAdvisoryOff);
    }

    [Fact]
    public void Celiac_Gluten_Excluded()
    {
        var conditions = new List<string> { "CeliacDisease" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Bread", "gluten", "Gluten", conditions, includeAdvisory: true);
        Assert.True(excluded);
    }

    [Fact]
    public void LactoseIntolerance_Milk_Excluded()
    {
        var conditions = new List<string> { "LactoseIntolerance" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Yogurt", "milk", "Milk", conditions, includeAdvisory: true);
        Assert.True(excluded);
    }

    [Fact]
    public void MultipleConditions_AnyMatch_Excludes()
    {
        var conditions = new List<string> { "Diabetes", "Hypertension" };
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Salty sweet bar", "high_sodium", "sugar", conditions, includeAdvisory: true);
        Assert.True(excluded);
    }

    [Fact]
    public void NoConditions_DoesNotExclude()
    {
        var excluded = MedicalRestrictionsMapper.ShouldExcludeProduct("Any", null, null, new List<string>(), includeAdvisory: true);
        Assert.False(excluded);
    }
}

