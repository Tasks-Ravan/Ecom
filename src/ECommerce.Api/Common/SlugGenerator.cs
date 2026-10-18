using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerce.Api.Common;

public static partial class SlugGenerator
{
    public static string Generate(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Replace("'", "").Replace("’", "")
            .Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        var slug = NonSlugCharacters().Replace(builder.ToString(), "-").Trim('-');
        return MultipleDashes().Replace(slug, "-");
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonSlugCharacters();

    [GeneratedRegex("-+", RegexOptions.CultureInvariant)]
    private static partial Regex MultipleDashes();
}
