using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Portfolio.Api.Options;

namespace Portfolio.Api.Services;

/// <summary>
/// Fetches raw data from the GitHub REST API and caches it in memory.
///
/// Registered as a singleton so the cache is shared by every request. A refresh costs
/// 1 request for the user, 1 per 100 repos for the repo list, and 1 per extra repo.
/// Language breakdowns cost 1 request per repo, so they are remembered (and saved to
/// App_Data/github-languages.json) and only re-fetched for repos that got a new push.
/// If a refresh fails (e.g. rate limited), the last good data keeps being served.
/// </summary>
public sealed class GitHubClient(
    IHttpClientFactory httpClientFactory,
    IOptions<GitHubOptions> options,
    IHostEnvironment environment,
    ILogger<GitHubClient> logger)
{
    /// <summary>Name of the HttpClient configured in Program.cs.</summary>
    public const string HttpClientName = "GitHub";

    // GitHub returns snake_case JSON (e.g. "stargazers_count"), so map it automatically.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly GitHubOptions _options = options.Value;

    // Only one refresh may run at a time; other callers wait and then reuse its result.
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    // The most recent successful snapshot, and when it should be refreshed.
    private GitHubSnapshot? _snapshot;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    // Where successful language lookups are saved between restarts.
    private readonly string _languageCacheFile =
        Path.Combine(environment.ContentRootPath, "App_Data", "github-languages.json");

    // Successful language lookups keyed by "owner/name", each tagged with the repo's
    // pushed_at time. A repo's languages can only change when code is pushed, so while
    // pushed_at is unchanged we reuse the saved result instead of asking GitHub again.
    private ConcurrentDictionary<string, CachedLanguages>? _languageCache;

    /// <summary>
    /// Returns cached GitHub data, refreshing it first if the cache has expired.
    /// </summary>
    public async Task<GitHubSnapshot> GetSnapshotAsync(CancellationToken ct)
    {
        // Fast path: cache is still fresh, no locking needed.
        if (_snapshot is not null && DateTimeOffset.UtcNow < _expiresAt)
            return _snapshot;

        await _refreshLock.WaitAsync(ct);
        try
        {
            // Another request may have refreshed while we were waiting for the lock.
            if (_snapshot is not null && DateTimeOffset.UtcNow < _expiresAt)
                return _snapshot;

            try
            {
                _snapshot = await FetchSnapshotAsync(ct);

                // If some language lookups failed (usually the rate limit), those repos are
                // showing only their main language, so try again sooner than usual.
                var minutes = _snapshot.Complete ? _options.CacheMinutes : Math.Min(10, _options.CacheMinutes);
                _expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);
            }
            catch (Exception ex) when (_snapshot is not null && ex is not OperationCanceledException)
            {
                // Refresh failed but we have older data: keep serving it and try again in
                // 5 minutes rather than hammering GitHub on every page load.
                logger.LogWarning(ex, "GitHub refresh failed; serving cached data from {FetchedAt}", _snapshot.FetchedAt);
                _expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);
            }

            return _snapshot;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary>Downloads the user profile, every public repo, and each repo's languages.</summary>
    private async Task<GitHubSnapshot> FetchSnapshotAsync(CancellationToken ct)
    {
        var http = httpClientFactory.CreateClient(HttpClientName);
        var user = _options.Username;
        logger.LogInformation("Fetching GitHub data for {User}", user);

        // 1. Profile (avatar, repo count, etc.).
        var profile = await GetJsonAsync<GitHubUser>(http, $"users/{user}", ct)
                      ?? throw new InvalidOperationException($"GitHub user '{user}' not found.");

        // 2. All public repos. GitHub pages results at max 100 per page, so keep
        //    requesting pages until one comes back short.
        var repos = new List<GitHubRepo>();
        for (var page = 1; ; page++)
        {
            var batch = await GetJsonAsync<List<GitHubRepo>>(
                http, $"users/{user}/repos?type=owner&per_page=100&page={page}", ct) ?? [];
            repos.AddRange(batch);
            if (batch.Count < 100) break;
        }

        // 2b. Team repos that live on someone else's account (GitHub:ExtraRepos).
        foreach (var fullName in _options.ExtraRepos.Where(n => n.Contains('/')))
        {
            var extra = await GetJsonAsync<GitHubRepo>(http, $"repos/{fullName.Trim()}", ct);
            if (extra is null)
                logger.LogWarning("Extra repo {Repo} not found (it may be private or renamed)", fullName);
            else if (!repos.Any(r => r.FullName.Equals(extra.FullName, StringComparison.OrdinalIgnoreCase)))
                repos.Add(extra);
        }

        // 2c. Other people's repos you've recently contributed to, from your public activity.
        if (_options.DiscoverContributions)
        {
            foreach (var fullName in await GetRecentContributionsAsync(http, user, ct))
            {
                if (repos.Any(r => r.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase))) continue;
                var found = await GetJsonAsync<GitHubRepo>(http, $"repos/{fullName}", ct);
                if (found is { Fork: false }) repos.Add(found);
            }
        }

        // 3. Language byte counts for each repo. Reuse saved results for repos that haven't
        //    been pushed to since; fetch the rest a few at a time so we don't open dozens
        //    of connections at once.
        var cache = _languageCache ??= LoadLanguageCache();
        var languages = new Dictionary<string, Dictionary<string, long>>(StringComparer.OrdinalIgnoreCase);
        var languagesLock = new object();
        var failedLookups = 0;
        var fetchedLookups = 0;
        await Parallel.ForEachAsync(
            repos,
            new ParallelOptions { MaxDegreeOfParallelism = 6, CancellationToken = ct },
            async (repo, token) =>
            {
                Dictionary<string, long> result;
                if (cache.TryGetValue(repo.FullName, out var saved) && saved.PushedAt == repo.PushedAt)
                {
                    result = saved.Languages; // unchanged since last time: no request needed
                }
                else
                {
                    (result, var succeeded) = await TryGetLanguagesAsync(http, repo, token);
                    Interlocked.Increment(ref fetchedLookups);
                    if (succeeded) cache[repo.FullName] = new CachedLanguages(repo.PushedAt, result);
                    else Interlocked.Increment(ref failedLookups);
                }
                lock (languagesLock) languages[repo.FullName] = result;
            });

        logger.LogInformation("Languages: {Fetched} fetched from GitHub, {Reused} reused", fetchedLookups, repos.Count - fetchedLookups);
        if (failedLookups > 0)
            logger.LogWarning("{Count} language lookups failed; will refresh again in a few minutes", failedLookups);
        if (fetchedLookups > failedLookups)
            SaveLanguageCache(cache);

        return new GitHubSnapshot(profile, repos, languages, DateTimeOffset.UtcNow, Complete: failedLookups == 0);
    }

    /// <summary>
    /// Names ("owner/name") of repos owned by someone else that the user pushed to or opened a
    /// pull request on, from their public events (GitHub keeps ~90 days, at most 300 events).
    /// Failures only log: this is a bonus on top of the configured repos, never a reason to fail.
    /// </summary>
    private async Task<List<string>> GetRecentContributionsAsync(HttpClient http, string user, CancellationToken ct)
    {
        var names = new List<string>();
        try
        {
            for (var page = 1; page <= 3; page++)
            {
                var events = await GetJsonAsync<List<GitHubEvent>>(
                    http, $"users/{user}/events/public?per_page=100&page={page}", ct) ?? [];
                foreach (var e in events)
                {
                    var name = e.Repo.Name;
                    var isContribution = e.Type is "PushEvent" or "PullRequestEvent";
                    var isOwnRepo = name.StartsWith(user + "/", StringComparison.OrdinalIgnoreCase);
                    if (isContribution && !isOwnRepo && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
                        names.Add(name);
                }
                if (events.Count < 100) break;
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("Could not load recent contributions: {Message}", ex.Message);
        }
        return names;
    }

    /// <summary>Reads saved language lookups from disk; starts empty if there's no usable file.</summary>
    private ConcurrentDictionary<string, CachedLanguages> LoadLanguageCache()
    {
        try
        {
            if (File.Exists(_languageCacheFile))
            {
                var saved = JsonSerializer.Deserialize<Dictionary<string, CachedLanguages>>(File.ReadAllText(_languageCacheFile));
                if (saved is not null)
                    return new ConcurrentDictionary<string, CachedLanguages>(saved, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            logger.LogWarning("Ignoring unreadable language cache {File}: {Message}", _languageCacheFile, ex.Message);
        }
        return new ConcurrentDictionary<string, CachedLanguages>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Saves language lookups to disk. Failing to save is harmless, so errors are only logged.</summary>
    private void SaveLanguageCache(ConcurrentDictionary<string, CachedLanguages> cache)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_languageCacheFile)!);
            File.WriteAllText(_languageCacheFile, JsonSerializer.Serialize(cache));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning("Could not save language cache {File}: {Message}", _languageCacheFile, ex.Message);
        }
    }

    /// <summary>
    /// Gets a repo's language breakdown, e.g. { "C#": 52000, "ShaderLab": 3000 }.
    /// If the request fails, falls back to the repo's single primary language so one
    /// bad call doesn't break the whole page. Also reports whether the real lookup worked.
    /// </summary>
    private async Task<(Dictionary<string, long> Languages, bool Succeeded)> TryGetLanguagesAsync(HttpClient http, GitHubRepo repo, CancellationToken ct)
    {
        try
        {
            var result = await GetJsonAsync<Dictionary<string, long>>(http, $"repos/{repo.Owner.Login}/{repo.Name}/languages", ct);
            if (result is { Count: > 0 }) return (result, true);
            if (result is not null) return ([], true); // repo genuinely has no code
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("Could not load languages for {Repo}: {Message}", repo.Name, ex.Message);
        }

        // Fallback: treat the primary language as 100% (or nothing if GitHub has none).
        return (repo.Language is null ? [] : new() { [repo.Language] = 1 }, false);
    }

    /// <summary>GETs a GitHub API path and deserializes the JSON, or returns null on 404.</summary>
    private static async Task<T?> GetJsonAsync<T>(HttpClient http, string path, CancellationToken ct)
    {
        using var response = await http.GetAsync(path, ct);

        if (response.StatusCode == HttpStatusCode.NotFound) return default;

        // 403/429 here almost always means the rate limit was hit. Make the error say so.
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
        {
            var remaining = response.Headers.TryGetValues("x-ratelimit-remaining", out var values) ? values.FirstOrDefault() : "?";
            throw new HttpRequestException(
                $"GitHub refused '{path}' ({(int)response.StatusCode}); rate-limit remaining: {remaining}. " +
                "Set GitHub:Token to raise the limit.", null, response.StatusCode);
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct);
    }
}

/// <summary>Everything fetched from GitHub in one refresh.</summary>
public sealed record GitHubSnapshot(
    GitHubUser User,
    IReadOnlyList<GitHubRepo> Repos,
    IReadOnlyDictionary<string, Dictionary<string, long>> LanguagesByRepo, // "owner/name" -> language -> bytes
    DateTimeOffset FetchedAt,
    bool Complete); // false if some language lookups failed and used the fallback

// ---- Shapes of the GitHub JSON we read. Only the fields we use are declared. ----

public sealed record GitHubUser(
    string Login,
    string? Name,
    string? Bio,
    string AvatarUrl,
    string HtmlUrl,
    int PublicRepos);

public sealed record GitHubRepo(
    string Name,
    string FullName, // "owner/name"
    string? Description,
    string HtmlUrl,
    string? Homepage,
    string? Language,
    int StargazersCount,
    bool Fork,
    bool Archived,
    List<string>? Topics,
    DateTimeOffset CreatedAt,
    DateTimeOffset PushedAt,
    GitHubOwner Owner);

public sealed record GitHubOwner(string Login);

/// <summary>One item from a user's public activity, e.g. a push to a repo.</summary>
public sealed record GitHubEvent(string Type, GitHubEventRepo Repo);

public sealed record GitHubEventRepo(string Name); // "owner/name"

/// <summary>A saved language lookup and the push time it was taken at.</summary>
public sealed record CachedLanguages(DateTimeOffset PushedAt, Dictionary<string, long> Languages);
