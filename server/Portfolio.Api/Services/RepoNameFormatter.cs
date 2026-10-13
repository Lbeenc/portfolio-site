using System.Globalization;
using System.Text.RegularExpressions;

namespace Portfolio.Api.Services;

/// <summary>
/// Turns repo names like "CMP_SCI-3410-Video-Game-Development-Test-2-Snake-game"
/// into a readable title ("Video Game Development Test 2 Snake Game") plus a course
/// code ("CMP SCI 3410"). For anything this can't tidy up nicely, set a Title
/// override in appsettings.json.
/// </summary>
public static partial class RepoNameFormatter
{
    // Matches course codes anywhere in the name: "CMP_SCI-4250", "CMP-SCI-4760",
    // "CMP_SCI-4700P", "CS-4760", "CS4280". Group 1 = prefix, group 2 = number.
    [GeneratedRegex(@"\b(CMP[-_ ]?SCI|CS)[-_ ]?(\d{4}[A-Z]?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex CourseCode();

    // Runs of dashes, underscores and spaces become a single space.
    [GeneratedRegex(@"[-_\s]+")]
    private static partial Regex Separators();

    // Short words kept lowercase in titles (unless they start the title).
    private static readonly HashSet<string> SmallWords =
        new(["a", "an", "and", "as", "at", "by", "for", "in", "of", "on", "or", "the", "to", "with"], StringComparer.OrdinalIgnoreCase);

    /// <summary>Splits a repo name into a display title and an optional course code.</summary>
    public static (string Title, string? Course) Format(string repoName)
    {
        string? course = null;
        var remaining = repoName;

        // Pull out the course code (first one only) and remove it from the title.
        var match = CourseCode().Match(repoName);
        if (match.Success)
        {
            var prefix = match.Groups[1].Value.StartsWith("CMP", StringComparison.OrdinalIgnoreCase) ? "CMP SCI" : "CS";
            course = $"{prefix} {match.Groups[2].Value.ToUpperInvariant()}";
            remaining = repoName.Remove(match.Index, match.Length);
        }

        // Replace separators with spaces and capitalise each word.
        var words = Separators().Replace(remaining, " ").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var title = string.Join(' ', words.Select((word, i) => Capitalize(word, isFirst: i == 0)));

        // If stripping the course code left nothing (e.g. repo named just "CS-4760"),
        // fall back to the original name.
        return (title.Length > 0 ? title : repoName, course);
    }

    /// <summary>Uppercases the first letter only, so acronyms like "BST" or "LRU" survive.</summary>
    private static string Capitalize(string word, bool isFirst)
    {
        if (!isFirst && SmallWords.Contains(word)) return word.ToLowerInvariant();
        return char.ToUpper(word[0], CultureInfo.InvariantCulture) + word[1..];
    }
}
