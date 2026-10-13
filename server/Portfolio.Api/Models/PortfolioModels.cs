namespace Portfolio.Api.Models;

// These records are what the API sends to the React client (serialized as camelCase JSON).
// Keep them in sync with client/src/api.ts.

/// <summary>One project card on the site, built from a GitHub repo plus any overrides.</summary>
public sealed record Project(
    string RepoName,                  // Exact GitHub repo name (used as a stable key)
    string Title,                     // Human-friendly title, e.g. "Snake Game"
    string? Course,                   // Course code pulled from the repo name, e.g. "CMP SCI 3410"
    string? Description,              // Override, else GitHub description, else null
    string RepoUrl,                   // Link to the repo on GitHub
    string? DemoUrl,                  // Live demo link if there is one
    string? Award,                    // e.g. "1st Place, Hybrid Teams — Hack Midwest 2026"
    string? TeamOwner,                // Set when the repo lives on someone else's account (a team project)
    IReadOnlyList<LanguageShare> Languages, // Language breakdown, largest first
    IReadOnlyList<string> Topics,     // GitHub topics/tags on the repo
    int Stars,
    bool Featured,
    DateTimeOffset CreatedAt,
    DateTimeOffset PushedAt);         // Last time code was pushed (better "last worked on" than updated_at)

/// <summary>How much of a repo is written in a given language.</summary>
public sealed record LanguageShare(
    string Name,     // e.g. "C#"
    double Percent,  // 0–100, rounded to one decimal place
    string Color);   // Hex color GitHub uses for this language, e.g. "#178600"

/// <summary>Header/hero information about the site owner.</summary>
public sealed record Profile(
    string DisplayName,
    string Headline,
    string Bio,
    string Username,
    string AvatarUrl,
    string GitHubUrl,
    int PublicRepos,
    IReadOnlyList<Education> Education,
    IReadOnlyList<ProfileLink> Links);

/// <summary>One degree for the Education section.</summary>
public sealed record Education(
    string School, string Degree, string? Minor, string? Graduated, string? Details, IReadOnlyList<string> Coursework);

/// <summary>Everything on the Résumé page that isn't already in the profile or projects.</summary>
public sealed record Resume(
    string Summary,
    string? PdfUrl,
    IReadOnlyList<Experience> Experience,
    IReadOnlyList<SkillGroup> SkillGroups);

/// <summary>One position on the résumé.</summary>
public sealed record Experience(
    string Title, string Organization, string? Location, string? Start, string? End, IReadOnlyList<string> Highlights);

/// <summary>A named group of skills, e.g. "Frameworks & Tools".</summary>
public sealed record SkillGroup(string Name, IReadOnlyList<string> Skills);

/// <summary>A contact link shown on the home page.</summary>
public sealed record ProfileLink(string Label, string Url);
