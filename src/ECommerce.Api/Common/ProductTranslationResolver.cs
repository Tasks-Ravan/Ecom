using ECommerce.Api.Domain;

namespace ECommerce.Api.Common;

public static class ProductTranslationResolver
{
    public static (string Language, ProductTranslation Translation) Resolve(
        IReadOnlyDictionary<string, ProductTranslation> translations, string? requestedLanguage)
    {
        if (translations.Count == 0)
            throw new InvalidOperationException("Product must have at least one translation.");
        var language = requestedLanguage?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Split('-', 2)[0].ToLowerInvariant();
        if (language is not null && TryGet(translations, language, out var selected)) return (language, selected);
        if (TryGet(translations, "en", out var english)) return ("en", english);
        var first = translations.First();
        return (first.Key, first.Value);
    }

    private static bool TryGet(IReadOnlyDictionary<string, ProductTranslation> translations, string language,
        out ProductTranslation translation)
    {
        foreach (var entry in translations)
            if (string.Equals(entry.Key, language, StringComparison.OrdinalIgnoreCase))
            {
                translation = entry.Value;
                return true;
            }
        translation = null!;
        return false;
    }
}
