using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Portfolio.Api.Options;
using Portfolio.Api.Services;

// ASP.NET only serves files from wwwroot if the folder exists at startup. The React
// build creates it, so make sure it's there even if the server starts first.
Directory.CreateDirectory("wwwroot");

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration ----
// Bind the "GitHub" and "Portfolio" sections of appsettings.json to strongly typed classes.
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));
builder.Services.Configure<PortfolioOptions>(builder.Configuration.GetSection("Portfolio"));

// ---- GitHub HTTP client ----
// A named HttpClient with the headers GitHub's API expects. The factory handles
// connection pooling so we don't exhaust sockets.
builder.Services.AddHttpClient(GitHubClient.HttpClientName, (services, http) =>
{
    var options = services.GetRequiredService<IOptions<GitHubOptions>>().Value;

    http.BaseAddress = new Uri("https://api.github.com/");
    http.DefaultRequestHeaders.UserAgent.ParseAdd("portfolio-site"); // GitHub rejects requests without a User-Agent
    http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

    // Optional token raises the rate limit from 60 to 5,000 requests/hour.
    if (!string.IsNullOrWhiteSpace(options.Token))
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
});

// Singleton so the in-memory GitHub cache lives for the lifetime of the app.
builder.Services.AddSingleton<GitHubClient>();
builder.Services.AddSingleton<PortfolioService>();

var app = builder.Build();

// ---- API endpoints ----
// The React app calls these. In development, Vite proxies /api to this server
// (see client/vite.config.ts); in production they're served from the same origin.
var api = app.MapGroup("/api");

api.MapGet("/profile", (PortfolioService portfolio, CancellationToken ct) =>
    portfolio.GetProfileAsync(ct));

api.MapGet("/projects", (PortfolioService portfolio, CancellationToken ct) =>
    portfolio.GetProjectsAsync(ct));

api.MapGet("/resume", (PortfolioService portfolio) => portfolio.GetResume());

// ---- Serve the built React app ----
// `npm run build` in client/ writes the site into wwwroot/. Any non-API URL
// (e.g. /projects) falls back to index.html so React Router can handle it.
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
