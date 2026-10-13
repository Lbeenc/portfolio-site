using Microsoft.Extensions.Options;
using Portfolio.Api.Models;
using Portfolio.Api.Options;

namespace Portfolio.Api.Services;

/// <summary>
/// Turns raw GitHub data into what the site shows: applies the hidden/featured
/// lists and per-repo overrides from appsettings.json, tidies repo names, and
/// converts language byte counts into percentages.
///
/// Uses IOptionsMonitor so edits to appsettings.json show up on the next page
/// load without restarting the server or re-fetching from GitHub.
/// </summary>
public sealed class PortfolioService(GitHubClient gitHub, IOptionsMonitor<PortfolioOptions> portfolioOptions)
{
    /// <summary>Builds the list of projects, featured ones first, then most recently worked on.</summary>
    public async Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken ct)
    {
        var snapshot = await gitHub.GetSnapshotAsync(ct);
        var options = portfolioOptions.CurrentValue;

        var hidden = new HashSet<string>(options.HiddenRepos, StringComparer.OrdinalIgnoreCase);
        var ignoredLanguages = new HashSet<string>(options.IgnoredLanguages, StringComparer.OrdinalIgnoreCase);

        // Map each featured repo name to its position so we can sort by it.
        var featuredOrder = options.FeaturedRepos
            .Select((name, index) => (name, index))
            .ToDictionary(x => x.name, x => x.index, StringComparer.OrdinalIgnoreCase);

        return snapshot.Repos
            .Where(repo => !repo.Fork && !hidden.Contains(repo.Name)) // only your own work
            .Select(repo => ToProject(repo, snapshot, options, ignoredLanguages, featuredOrder.ContainsKey(repo.Name)))
            .OrderBy(p => featuredOrder.GetValueOrDefault(p.RepoName, int.MaxValue)) // featured first, in listed order
            .ThenByDescending(p => p.PushedAt)                                       // then newest first
            .ToList();
    }

    /// <summary>Builds the profile shown in the header and on the home page.</summary>
    public async Task<Profile> GetProfileAsync(CancellationToken ct)
    {
        var user = (await gitHub.GetSnapshotAsync(ct)).User;
        var options = portfolioOptions.CurrentValue;

        // Prefer the configured text; fall back to whatever is on the GitHub profile.
        return new Profile(
            DisplayName: FirstNonEmpty(options.DisplayName, user.Name, user.Login)!,
            Headline: options.Headline,
            Bio: FirstNonEmpty(options.Bio, user.Bio) ?? "",
            Username: user.Login,
            AvatarUrl: user.AvatarUrl,
            GitHubUrl: user.HtmlUrl,
            PublicRepos: user.PublicRepos,
            Education: options.Education
                .Select(e => new Education(e.School, e.Degree, e.Minor, e.Graduated, e.Details, e.Coursework))
                .ToList(),
            Links: options.Links
                .Where(l => !string.IsNullOrWhiteSpace(l.Url))
                .Select(l => new ProfileLink(l.Label, l.Url))
                .ToList());
    }

    /// <summary>
    /// Builds the Résumé page content straight from appsettings.json (no GitHub call needed).
    /// Empty entries are skipped so half-filled placeholders never show up on the site.
    /// </summary>
    public Resume GetResume()
    {
        var resume = portfolioOptions.CurrentValue.Resume;

        return new Resume(
            Summary: resume.Summary,
            PdfUrl: FirstNonEmpty(resume.PdfUrl),
            Experience: resume.Experience
                .Where(e => !string.IsNullOrWhiteSpace(e.Title))
                .Select(e => new Experience(e.Title, e.Organization, e.Location, e.Start, e.End,
                    e.Highlights.Where(h => !string.IsNullOrWhiteSpace(h)).ToList()))
                .ToList(),
            SkillGroups: resume.SkillGroups
                .Where(g => g.Skills.Count > 0)
                .Select(g => new SkillGroup(g.Name, g.Skills))
                .ToList());
    }

    /// <summary>Combines a GitHub repo with its overrides into a Project card.</summary>
    private static Project ToProject(
        GitHubRepo repo, GitHubSnapshot snapshot, PortfolioOptions options, HashSet<string> ignoredLanguages, bool featured)
    {
        options.Overrides.TryGetValue(repo.Name, out var custom);
        var (autoTitle, course) = RepoNameFormatter.Format(repo.Name);

        return new Project(
            RepoName: repo.Name,
            Title: FirstNonEmpty(custom?.Title, autoTitle)!,
            Course: course,
            Description: FirstNonEmpty(custom?.Description, repo.Description),
            RepoUrl: repo.HtmlUrl,
            DemoUrl: FirstNonEmpty(custom?.DemoUrl, repo.Homepage),
            Award: FirstNonEmpty(custom?.Award),
            TeamOwner: repo.Owner.Login.Equals(snapshot.User.Login, StringComparison.OrdinalIgnoreCase) ? null : repo.Owner.Login,
            Languages: ToLanguageShares(snapshot.LanguagesByRepo.GetValueOrDefault(repo.FullName), ignoredLanguages),
            Topics: repo.Topics ?? [],
            Stars: repo.StargazersCount,
            Featured: featured,
            CreatedAt: repo.CreatedAt,
            PushedAt: repo.PushedAt);
    }

    /// <summary>
    /// Converts GitHub's byte counts ({ "C#": 9000, "HLSL": 1000 }) into percentages
    /// ([C# 90%, HLSL 10%]), largest first. Ignored languages are removed before the
    /// math, and anything under 0.1% is dropped as noise.
    /// </summary>
    private static List<LanguageShare> ToLanguageShares(Dictionary<string, long>? bytesByLanguage, HashSet<string> ignored)
    {
        var kept = (bytesByLanguage ?? []).Where(kv => !ignored.Contains(kv.Key)).ToList();
        if (kept.Count == 0) return [];

        double total = kept.Sum(kv => kv.Value);
        return kept
            .Select(kv => new LanguageShare(kv.Key, Math.Round(kv.Value / total * 100, 1), LanguageColors.For(kv.Key)))
            .Where(l => l.Percent >= 0.1)
            .OrderByDescending(l => l.Percent)
            .ToList();
    }

    /// <summary>Returns the first value that isn't null or whitespace, or null if none.</summary>
    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
