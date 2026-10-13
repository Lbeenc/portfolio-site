namespace Portfolio.Api.Options;

/// <summary>
/// Settings for talking to the GitHub REST API.
/// Bound from the "GitHub" section of appsettings.json (or environment variables
/// such as GitHub__Token, or `dotnet user-secrets`).
/// </summary>
public sealed class GitHubOptions
{
    /// <summary>The GitHub account whose public repos are shown on the site.</summary>
    public string Username { get; set; } = "Lbeenc";

    /// <summary>
    /// Public repos owned by someone else that you worked on (team projects, hackathons),
    /// written as "owner/name", e.g. "ddgroleau/bbna-hackmw26". Each costs 2 extra requests.
    /// </summary>
    public List<string> ExtraRepos { get; set; } = [];

    /// <summary>
    /// Optional personal access token. Without one, GitHub allows only 60 requests
    /// per hour per IP; with one the limit is 5,000. A token with no scopes at all
    /// (read-only public data) is enough. NEVER commit this value; set it with
    /// `dotnet user-secrets set "GitHub:Token" "..."` or the GitHub__Token env var.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// How long (in minutes) to reuse the GitHub data before fetching it again.
    /// Loading every project costs one request for the repo list plus one per repo
    /// for its language breakdown, so caching keeps us well under the rate limit.
    /// </summary>
    public int CacheMinutes { get; set; } = 60;
}
