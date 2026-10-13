import { Link } from 'react-router-dom'
import type { PortfolioData } from '../usePortfolioData'
import { countLanguages } from '../languageStats'

/**
 * The résumé: summary, experience, education (with coursework), skills and
 * selected projects. Text content comes from the "Resume" and "Education"
 * sections of appsettings.json; programming languages are counted from GitHub.
 *
 * "Save as PDF" uses the browser's print dialog. The print styles in index.css
 * (@media print) turn the page into a plain black-on-white document for this.
 */
export default function ResumePage({ profile, projects, resume, loading }: PortfolioData) {
  if (loading) return <p className="muted">Loading résumé…</p>
  if (!profile || !resume) return null // App already shows the error message

  // Languages for the Skills section: your own "Languages" group in appsettings.json if
  // there is one, otherwise the languages counted from your GitHub repos (most-used first).
  const hasLanguageGroup = resume.skillGroups.some((g) => g.name.toLowerCase() === 'languages')
  const languages = hasLanguageGroup ? [] : countLanguages(projects)
  const featured = projects.filter((p) => p.featured)
  const awarded = projects.filter((p) => p.award)

  return (
    <article className="resume card">
      {/* ---- Header: name, headline, contact links, PDF actions ---- */}
      <header className="resume-header">
        <div>
          <h1>{profile.displayName}</h1>
          {profile.headline && <p className="headline">{profile.headline}</p>}
          <p className="resume-contact">
            <a href={profile.gitHubUrl}>github.com/{profile.username}</a>
            {profile.links.map((link) => (
              <a key={link.label} href={link.url}>
                {/* Show LinkedIn etc. as readable text so the printed copy is useful too */}
                {prettyUrl(link.url)}
              </a>
            ))}
          </p>
        </div>

        {/* Buttons are hidden when printing */}
        <div className="resume-actions no-print">
          {resume.pdfUrl && (
            <a className="button primary" href={resume.pdfUrl} target="_blank" rel="noreferrer">
              Download PDF
            </a>
          )}
          <button type="button" className="button" onClick={() => window.print()}>
            Save as PDF
          </button>
        </div>
      </header>

      {/* ---- Summary ---- */}
      {resume.summary && (
        <section className="resume-section">
          <h2>Summary</h2>
          <p>{resume.summary}</p>
        </section>
      )}

      {/* ---- Experience (hidden until you add some in appsettings.json) ---- */}
      {resume.experience.length > 0 && (
        <section className="resume-section">
          <h2>Experience</h2>
          {resume.experience.map((job) => (
            <div key={`${job.title}-${job.organization}-${job.start}`} className="resume-item">
              <div className="resume-item-head">
                <div>
                  <strong>{job.title}</strong>
                  {job.organization && <span> · {job.organization}</span>}
                  {job.location && <span className="muted"> · {job.location}</span>}
                </div>
                <span className="muted resume-dates">{dateRange(job.start, job.end)}</span>
              </div>
              {job.highlights.length > 0 && (
                <ul>
                  {job.highlights.map((h) => (
                    <li key={h}>{h}</li>
                  ))}
                </ul>
              )}
            </div>
          ))}
        </section>
      )}

      {/* ---- Education ---- */}
      {profile.education.length > 0 && (
        <section className="resume-section">
          <h2>Education</h2>
          {profile.education.map((e) => (
            <div key={`${e.school}-${e.degree}`} className="resume-item">
              <div className="resume-item-head">
                <div>
                  <strong>{e.degree}</strong>
                  {e.minor && <span> · Minor in {e.minor}</span>}
                  <div className="muted">{e.school}</div>
                </div>
                {e.graduated && <span className="muted resume-dates">{e.graduated}</span>}
              </div>
              {e.details && <p className="coursework">{e.details}</p>}
              {e.coursework.length > 0 && (
                <p className="coursework">
                  <span className="muted">Relevant coursework: </span>
                  {e.coursework.join(' · ')}
                </p>
              )}
            </div>
          ))}
        </section>
      )}

      {/* ---- Awards: any project with an "Award" override ---- */}
      {awarded.length > 0 && (
        <section className="resume-section">
          <h2>Awards</h2>
          {awarded.map((p) => (
            <div key={p.repoName} className="resume-item">
              <strong>{p.award}</strong>
              <span className="muted"> · {p.title}</span>
            </div>
          ))}
        </section>
      )}

      {/* ---- Skills: languages from GitHub, plus the groups from appsettings.json ---- */}
      <section className="resume-section">
        <h2>Technical Skills</h2>
        <dl className="skills">
          {languages.length > 0 && (
            <div className="skill-row">
              <dt>Languages</dt>
              <dd>{languages.map((l) => l.name).join(', ')}</dd>
            </div>
          )}
          {resume.skillGroups.map((group) => (
            <div key={group.name} className="skill-row">
              <dt>{group.name}</dt>
              <dd>{group.skills.join(', ')}</dd>
            </div>
          ))}
        </dl>
      </section>

      {/* ---- Selected projects: the featured ones, linked to GitHub ---- */}
      {featured.length > 0 && (
        <section className="resume-section">
          <h2>Selected Projects</h2>
          {featured.map((p) => (
            <div key={p.repoName} className="resume-item">
              <div className="resume-item-head">
                <div>
                  <a href={p.repoUrl}>
                    <strong>{p.title}</strong>
                  </a>
                  {p.course && <span className="muted"> · {p.course}</span>}
                  {p.teamOwner && <span className="muted"> · Team project</span>}
                </div>
                <span className="muted resume-dates">
                  {p.languages.slice(0, 3).map((l) => l.name).join(', ')}
                </span>
              </div>
              {p.description && <p className="resume-project-desc">{p.description}</p>}
            </div>
          ))}
          <p className="no-print">
            <Link to="/projects">See all {projects.length} projects →</Link>
          </p>
        </section>
      )}
    </article>
  )
}

/** "Jun 2025" + "Aug 2025" -> "Jun 2025 – Aug 2025"; handles either side being missing. */
function dateRange(start: string | null, end: string | null) {
  return [start, end].filter(Boolean).join(' – ')
}

/** "https://www.linkedin.com/in/name/" -> "linkedin.com/in/name"; "mailto:a@b.c" -> "a@b.c" */
function prettyUrl(url: string) {
  return url.replace(/^mailto:/, '').replace(/^https?:\/\/(www\.)?/, '').replace(/\/$/, '')
}
