namespace Portfolio.Api.Services;

/// <summary>
/// The colors GitHub uses for each language (from github-linguist), so the language
/// bars on the site match what visitors see on GitHub. Add entries as needed.
/// </summary>
public static class LanguageColors
{
    /// <summary>Gray used for any language not in the table below.</summary>
    private const string Fallback = "#8b949e";

    private static readonly Dictionary<string, string> Colors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C"] = "#555555",
        ["C#"] = "#178600",
        ["C++"] = "#f34b7d",
        ["CMake"] = "#da3434",
        ["CSS"] = "#663399",
        ["Dockerfile"] = "#384d54",
        ["Go"] = "#00ADD8",
        ["GLSL"] = "#5686a5",
        ["HLSL"] = "#aace60",
        ["HTML"] = "#e34c26",
        ["Java"] = "#b07219",
        ["JavaScript"] = "#f1e05a",
        ["Jupyter Notebook"] = "#DA5B0B",
        ["Kotlin"] = "#A97BFF",
        ["Lex"] = "#DBCA00",
        ["Makefile"] = "#427819",
        ["PHP"] = "#4F5D95",
        ["PowerShell"] = "#012456",
        ["Python"] = "#3572A5",
        ["Ruby"] = "#701516",
        ["Rust"] = "#dea584",
        ["SCSS"] = "#c6538c",
        ["ShaderLab"] = "#222c37",
        ["Shell"] = "#89e051",
        ["SQL"] = "#e38c00",
        ["Swift"] = "#F05138",
        ["TypeScript"] = "#3178c6",
        ["Vue"] = "#41b883",
        ["Yacc"] = "#4B6C4B",
    };

    /// <summary>Returns the hex color for a language, or a neutral gray if unknown.</summary>
    public static string For(string language) =>
        Colors.TryGetValue(language, out var color) ? color : Fallback;
}
