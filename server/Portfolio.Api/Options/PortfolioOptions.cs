namespace Portfolio.Api.Options;

/// <summary>
/// Hand-written content and curation for the portfolio, bound from the
/// "Portfolio" section of appsettings.json. GitHub supplies the raw data;
/// this is where you decide what is shown and how it is described.
/// </summary>
public sealed class PortfolioOptions
{
    /// <summary>Name shown in the page header and hero section.</summary>
    public string DisplayName { get; set; } = "";

    /// <summary>One-line headline under your name (e.g. "Computer Science student").</summary>
    public string Headline { get; set; } = "";

    /// <summary>A short paragraph about you for the home page.</summary>
    public string Bio { get; set; } = "";

    /// <summary>Degrees shown in the Education section of the home page, most recent first.</summary>
    public List<EducationEntry> Education { get; set; } = [];

    /// <summary>Content for the Résumé page (summary, experience, skills, PDF link).</summary>
    public ResumeOptions Resume { get; set; } = new();

    /// <summary>Optional contact links (LinkedIn, email, etc.) shown on the home page.</summary>
    public List<ContactLink> Links { get; set; } = [];

    /// <summary>Repo names to leave off the site entirely (case-insensitive).</summary>
    public List<string> HiddenRepos { get; set; } = [];

    /// <summary>
    /// Repo names to highlight. Featured projects appear on the home page and are
    /// sorted to the top of the projects page, in the order listed here.
    /// </summary>
    public List<string> FeaturedRepos { get; set; } = [];

    /// <summary>
    /// Languages to leave out of every breakdown (case-insensitive), e.g. build files
    /// like "Makefile" that GitHub counts as a language but aren't really your code.
    /// Percentages are recalculated from what's left.
    /// </summary>
    public List<string> IgnoredLanguages { get; set; } = [];

    /// <summary>
    /// Per-repo overrides keyed by the exact repo name. Use these to give a project
    /// a nicer title or a description when the GitHub repo doesn't have one.
    /// </summary>
    public Dictionary<string, RepoOverride> Overrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>One degree, e.g. B.S. in Computer Science from UMSL, May 2026.</summary>
public sealed class EducationEntry
{
    public string School { get; set; } = "";   // e.g. "University of Missouri–St. Louis"
    public string Degree { get; set; } = "";   // e.g. "B.S. in Computer Science"
    public string? Minor { get; set; }         // e.g. "Psychology"
    public string? Graduated { get; set; }     // Free text, e.g. "May 2026"
    public string? Details { get; set; }       // Optional sentence, e.g. what a certificate covered
    public List<string> Coursework { get; set; } = []; // Relevant courses, shown on the Résumé page
}

/// <summary>
/// The parts of the Résumé page that GitHub can't supply. Education comes from the
/// Education list above, and programming languages are worked out from your repos.
/// </summary>
public sealed class ResumeOptions
{
    /// <summary>A short professional summary at the top of the résumé.</summary>
    public string Summary { get; set; } = "";

    /// <summary>
    /// Optional link to a PDF of your résumé, shown as a "Download PDF" button.
    /// Put the file in client/public (e.g. client/public/resume.pdf) and set this to "/resume.pdf".
    /// </summary>
    public string? PdfUrl { get; set; }

    /// <summary>Jobs, internships, research, volunteering, etc. Most recent first.</summary>
    public List<ExperienceEntry> Experience { get; set; } = [];

    /// <summary>Skills beyond programming languages, grouped (e.g. "Frameworks & Tools").</summary>
    public List<SkillGroupEntry> SkillGroups { get; set; } = [];
}

/// <summary>One position on the résumé.</summary>
public sealed class ExperienceEntry
{
    public string Title { get; set; } = "";         // e.g. "Software Engineering Intern"
    public string Organization { get; set; } = "";  // e.g. company or lab name
    public string? Location { get; set; }           // e.g. "St. Louis, MO" or "Remote"
    public string? Start { get; set; }              // Free text, e.g. "Jun 2025"
    public string? End { get; set; }                // Free text, e.g. "Aug 2025" or "Present"
    public List<string> Highlights { get; set; } = []; // Bullet points: what you did and the impact
}

/// <summary>A named list of skills, e.g. { "Name": "Tools", "Skills": ["Git", "Unity"] }.</summary>
public sealed class SkillGroupEntry
{
    public string Name { get; set; } = "";
    public List<string> Skills { get; set; } = [];
}

/// <summary>A labelled link, e.g. { "Label": "LinkedIn", "Url": "https://..." }.</summary>
public sealed class ContactLink
{
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
}

/// <summary>Optional replacements for what GitHub reports about a repo.</summary>
public sealed class RepoOverride
{
    /// <summary>Title to show instead of the auto-cleaned repo name.</summary>
    public string? Title { get; set; }

    /// <summary>Description to show instead of the GitHub description.</summary>
    public string? Description { get; set; }

    /// <summary>Live demo URL to show instead of the GitHub "homepage" field.</summary>
    public string? DemoUrl { get; set; }

    /// <summary>An award or placement, e.g. "1st Place, Hybrid Teams — Hack Midwest 2026".</summary>
    public string? Award { get; set; }
}
