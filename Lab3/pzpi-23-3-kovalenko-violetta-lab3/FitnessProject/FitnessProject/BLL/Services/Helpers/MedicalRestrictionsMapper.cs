using System.Text.RegularExpressions;

namespace FitnessProject.BLL.Services.Helpers;

public static class MedicalRestrictionsMapper
{
    private enum RestrictionSeverity
    {
        Strict,
        Moderate,
        Advisory
    }

    private record KeywordRule(string Keyword, RestrictionSeverity Severity);

    private static readonly Dictionary<string, List<KeywordRule>> MedicalConditionToKeywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Diabetes",
                new List<KeywordRule>
                {
                    new("high_gi", RestrictionSeverity.Strict),
                    new("high_glycemic", RestrictionSeverity.Strict),
                    new("sugar", RestrictionSeverity.Strict),
                    new("sugary", RestrictionSeverity.Strict),
                    new("sweet", RestrictionSeverity.Strict),
                    new("солодкий", RestrictionSeverity.Strict),
                    new("солодощ", RestrictionSeverity.Strict),
                    new("dessert", RestrictionSeverity.Strict),
                    new("десерт", RestrictionSeverity.Strict),
                    new("candy", RestrictionSeverity.Strict),
                    new("цукер", RestrictionSeverity.Strict),
                    new("jam", RestrictionSeverity.Strict),
                    new("варення", RestrictionSeverity.Strict),
                    new("honey", RestrictionSeverity.Strict),
                    new("syrup", RestrictionSeverity.Strict),
                    new("сироп", RestrictionSeverity.Strict),
                    new("juice", RestrictionSeverity.Strict),
                    new("сік", RestrictionSeverity.Strict),
                    new("soda", RestrictionSeverity.Strict),
                    new("fructose", RestrictionSeverity.Strict),
                    new("glucose", RestrictionSeverity.Strict),
                    new("sucrose", RestrictionSeverity.Strict),
                    new("sweetened", RestrictionSeverity.Strict),
                    new("без цукру", RestrictionSeverity.Advisory),
                    new("sugar-free", RestrictionSeverity.Advisory),
                    new("цукор", RestrictionSeverity.Strict),
                    new("мед", RestrictionSeverity.Strict),
                    new("солод", RestrictionSeverity.Strict)
                }
            },
            {
                "Hypertension",
                new List<KeywordRule>
                {
                    new("high_sodium", RestrictionSeverity.Strict),
                    new("sodium", RestrictionSeverity.Strict),
                    new("salt", RestrictionSeverity.Strict),
                    new("salty", RestrictionSeverity.Strict),
                    new("солоний", RestrictionSeverity.Strict),
                    new("сіль", RestrictionSeverity.Strict),
                    new("маринад", RestrictionSeverity.Moderate),
                    new("pickled", RestrictionSeverity.Moderate),
                    new("smoked", RestrictionSeverity.Moderate),
                    new("копчений", RestrictionSeverity.Moderate)
                }
            },
            {
                "KidneyDisease",
                new List<KeywordRule>
                {
                    new("high_protein", RestrictionSeverity.Strict),
                    new("protein", RestrictionSeverity.Moderate),
                    new("high_sodium", RestrictionSeverity.Strict),
                    new("sodium", RestrictionSeverity.Strict),
                    new("salt", RestrictionSeverity.Strict),
                    new("potassium", RestrictionSeverity.Moderate),
                    new("phosphorus", RestrictionSeverity.Moderate),
                    new("legume", RestrictionSeverity.Advisory),
                    new("bean", RestrictionSeverity.Advisory),
                    new("nuts", RestrictionSeverity.Advisory),
                    new("сіль", RestrictionSeverity.Strict),
                    new("білок", RestrictionSeverity.Moderate),
                    new("калій", RestrictionSeverity.Moderate),
                    new("фосфор", RestrictionSeverity.Moderate)
                }
            },
            {
                "CeliacDisease",
                new List<KeywordRule>
                {
                    new("gluten", RestrictionSeverity.Strict),
                    new("wheat", RestrictionSeverity.Strict),
                    new("barley", RestrictionSeverity.Strict),
                    new("rye", RestrictionSeverity.Strict),
                    new("malt", RestrictionSeverity.Strict),
                    new("oat", RestrictionSeverity.Moderate),
                    new("ячмінь", RestrictionSeverity.Strict),
                    new("жито", RestrictionSeverity.Strict),
                    new("пшениця", RestrictionSeverity.Strict),
                    new("овес", RestrictionSeverity.Moderate),
                    new("глютен", RestrictionSeverity.Strict),
                    new("клейковина", RestrictionSeverity.Strict)
                }
            },
            {
                "LactoseIntolerance",
                new List<KeywordRule>
                {
                    new("milk", RestrictionSeverity.Strict),
                    new("lactose", RestrictionSeverity.Strict),
                    new("dairy", RestrictionSeverity.Strict),
                    new("cheese", RestrictionSeverity.Strict),
                    new("butter", RestrictionSeverity.Strict),
                    new("cream", RestrictionSeverity.Strict),
                    new("yogurt", RestrictionSeverity.Strict),
                    new("kefir", RestrictionSeverity.Strict),
                    new("whey", RestrictionSeverity.Strict),
                    new("casein", RestrictionSeverity.Strict),
                    new("ice cream", RestrictionSeverity.Strict),
                    new("молоко", RestrictionSeverity.Strict),
                    new("лактоза", RestrictionSeverity.Strict),
                    new("молоч", RestrictionSeverity.Strict),
                    new("сир", RestrictionSeverity.Strict),
                    new("домашній сир", RestrictionSeverity.Strict),
                    new("вершк", RestrictionSeverity.Strict),
                    new("йогурт", RestrictionSeverity.Strict),
                    new("кефір", RestrictionSeverity.Strict),
                    new("сироватк", RestrictionSeverity.Strict)
                }
            }
        };

    public static bool ShouldExcludeProduct(
        string productName,
        string? restriction,
        string? allergens,
        List<string> medicalConditions,
        bool includeAdvisory = true)
    {
        if (medicalConditions == null || medicalConditions.Count == 0)
        {
            return false;
        }

        restriction ??= string.Empty;
        productName ??= string.Empty;
        allergens ??= string.Empty;

        var normalizedConditions = medicalConditions
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .ToList();

        if (normalizedConditions.Count == 0)
        {
            return false;
        }

        var searchText = $"{restriction} {productName} {allergens}".ToLowerInvariant();
        var tokens = Tokenize(searchText);

        var keywordRules = GetExcludedKeywordRules(normalizedConditions);

        foreach (var rule in keywordRules)
        {
            var isAdvisory = rule.Severity == RestrictionSeverity.Advisory;
            if (isAdvisory && !includeAdvisory)
            {
                continue;
            }

            if (Matches(rule.Keyword, searchText, tokens))
            {
                return true;
            }
        }

        return false;
    }

    private static List<KeywordRule> GetExcludedKeywordRules(List<string> medicalConditions)
    {
        var result = new List<KeywordRule>();

        foreach (var condition in medicalConditions)
        {
            if (MedicalConditionToKeywords.TryGetValue(condition, out var keywords))
            {
                result.AddRange(keywords);
            }
        }

        return result;
    }

    private static bool Matches(string keyword, string searchText, IReadOnlyCollection<string> tokens)
    {
        var normalizedKeyword = keyword.ToLowerInvariant();

        if (tokens.Any(t => t == normalizedKeyword))
        {
            return true;
        }

        return searchText.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        return Regex
            .Split(text.ToLowerInvariant(), @"[^a-zA-Zа-яА-ЯіїєґІЇЄҐ0-9]+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }
}
