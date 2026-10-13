# Portfolio site

My personal portfolio, with a freshwater fish tank theme. An **ASP.NET Core (C#)** backend pulls my public repos from the
**GitHub API**, and a **React** frontend shows them as scrollable project cards with
each project's language breakdown.

```
portfolio-site/
├── server/Portfolio.Api/        C# API + hosts the built site
│   ├── Program.cs               App setup and the /api endpoints
│   ├── appsettings.json         ← your bio, featured projects, descriptions
│   ├── Services/
│   │   ├── GitHubClient.cs      Calls GitHub, caches results in memory
│   │   ├── PortfolioService.cs  Turns repos into project cards (overrides, sorting, %)
│   │   ├── RepoNameFormatter.cs "CMP_SCI-3410-Snake-game" → "Snake Game" + "CMP SCI 3410"
│   │   └── LanguageColors.cs    GitHub's color for each language
│   ├── Options/                 Typed classes for the appsettings sections
│   └── Models/                  JSON shapes sent to the React app
└── client/                      React + TypeScript (Vite)
    └── src/
        ├── App.tsx              Header, routes, footer
        ├── api.ts               Types + fetch helpers (mirror Models/)
        ├── pages/               HomePage, ProjectsPage, ResumePage
        └── components/          ProjectCard, LanguageBar, Aquarium (background rays + bubbles), Shrimp (header logo)
```

## Run it locally

You need the .NET 10 SDK and Node.js. Use two terminals:

```bash
cd server/Portfolio.Api
dotnet run
```

```bash
cd client
npm install
npm run dev
```

Open http://localhost:5173. Vite forwards `/api/*` requests to the C# server on port 5115,
and edits to React files show up instantly.

## Make it yours

Everything is in [`server/Portfolio.Api/appsettings.json`](server/Portfolio.Api/appsettings.json)
under `Portfolio`. Changes apply on the next page refresh, with no restart needed:

| Setting | What it does |
| --- | --- |
| `DisplayName`, `Headline`, `Bio` | Text in the home page intro |
| `Education` | Degrees in the Education section (school, degree, minor, graduation date, `Coursework`) |
| `Resume` | Résumé page: `Summary`, `Experience` (jobs with bullet points), `SkillGroups`, and an optional `PdfUrl` |
| `Links` | Extra buttons (LinkedIn, email, résumé…). Empty URLs are hidden. |
| `FeaturedRepos` | Shown on the home page and pinned to the top of Projects, in order |
| `HiddenRepos` | Repos to leave off the site |
| `IgnoredLanguages` | Build-file "languages" (Makefile, CMake) left out of the percentages |
| `Overrides` | Per-repo `Title`, `Description`, `DemoUrl`, `Award` (shown as a 🏆 badge and on the Résumé) |
| `GitHub:ExtraRepos` | Team repos on other people's accounts, as `"owner/name"`. Shown with a "Team project" tag |

Most repos don't have a description yet. You can add one either in `Overrides` here, or on
GitHub (the ⚙ next to "About" on the repo page). The GitHub description is used automatically,
along with repo **topics** (shown as tags) and the **website** field (shown as a "Live demo" link).

## GitHub rate limit (recommended: add a token)

Without a token GitHub allows **60 requests per hour**. Each repo's language breakdown
costs one request, so the server saves them in `server/Portfolio.Api/App_Data/github-languages.json`
and only asks again for repos that got a new push. A normal refresh, or a restart, costs
about 3 requests. Only the very first run (or deleting that file) fetches every repo,
about 50 requests. Results are refreshed every 60 minutes (`GitHub:CacheMinutes`), and if
GitHub refuses a request, the site keeps showing the last good data.

A token still helps if you have many repos or restart often. Create a
[fine-grained token](https://github.com/settings/personal-access-tokens/new) with
**Public repositories (read-only)** access and no extra permissions, then run:

```bash
cd server/Portfolio.Api
dotnet user-secrets init
dotnet user-secrets set "GitHub:Token" "<your token>"
```

User secrets live outside the project folder, so the token can't be committed by accident.
In production, set the `GitHub__Token` environment variable instead.

## Build for production

```bash
cd client
npm run build
```

This writes the site into `server/Portfolio.Api/wwwroot/`. Then the C# app serves both
the site and the API from one place:

```bash
cd server/Portfolio.Api
dotnet publish -c Release -o ../../publish
```

You can also build a container from the [`Dockerfile`](Dockerfile), which does both steps.

## Deploying to Google Cloud Run (curtisbeen.dev)

Cloud Run builds the `Dockerfile` straight from GitHub, so nothing needs to be installed
locally. Every push to `main` redeploys the site.

### 1. One-time setup

1. Push this repo to GitHub (public or private both work).
2. In the [Google Cloud console](https://console.cloud.google.com/), create a project
   (e.g. `curtis-portfolio`) and turn on billing. The site should stay within Cloud Run's
   free tier, but Google requires a billing account. Set a budget alert (Billing →
   Budgets & alerts, e.g. $1) so you hear about any charge.
3. Create a GitHub token for the server: [fine-grained token](https://github.com/settings/personal-access-tokens/new)
   with **Public repositories (read-only)** access only.
4. In the console, open **Secret Manager** → **Create secret**, name it `github-token`,
   and paste the token as the value.

### 2. Create the service

1. Open **Cloud Run** → **Deploy container** → **Service**, and choose
   **Continuously deploy from a repository (source or function)**.
2. **Set up with Cloud Build**: connect GitHub, pick this repo, branch `^main$`, and
   build type **Dockerfile** (path `/Dockerfile`).
3. Service settings:
   - **Region:** `us-central1` (inside the free tier, and it supports custom domains)
   - **Authentication:** Allow unauthenticated invocations (it's a public website)
   - **Billing:** Request-based. **Minimum instances:** 0. **Maximum instances:** 2
     (caps the cost if something unexpected sends heavy traffic)
   - **Container → Variables & secrets → Reference a secret:** name `GitHub__Token`,
     secret `github-token`, version `latest`. If it asks, grant the service account the
     *Secret Manager Secret Accessor* role.
4. Click **Create**. The first build takes a few minutes. You'll get a URL like
   `https://portfolio-xxxxx-uc.a.run.app`, so check that the site works there.

### 3. Point curtisbeen.dev at it

1. In Cloud Run, open **Manage custom domains** → **Add mapping**, pick the service,
   and enter `curtisbeen.dev`. Google asks you to verify you own the domain the first time.
2. Google shows DNS records (A and AAAA records for the bare domain). Add exactly those
   at your registrar.
   - **On Cloudflare**, set each record to **DNS only** (grey cloud), not Proxied.
     Otherwise Google can't issue the HTTPS certificate.
3. Wait for the certificate (from 15 minutes to a few hours). `.dev` sites only work over
   HTTPS, so the domain won't load until the certificate is ready.
4. Optional: add `www.curtisbeen.dev` as a second mapping (CNAME to `ghs.googlehosted.com`).

### Updating the site

Edit `appsettings.json` or the code, commit, and push to `main`. Cloud Run rebuilds and
switches over automatically.

Language lookups are saved inside the container. A new deploy starts with an empty
cache and refetches languages for every repo, about 50 requests. That's well within
the token's limit of 5,000 an hour.
