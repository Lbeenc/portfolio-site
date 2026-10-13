import { Link } from 'react-router-dom'
import type { PortfolioData } from '../usePortfolioData'
import { countLanguages, overallLanguages } from '../languageStats'
import LanguageBar from '../components/LanguageBar'
import ProjectCard from '../components/ProjectCard'

/**
 * Landing page: intro about you, a summary of the languages you use,
 * and the featured projects from appsettings.json.
 */
export default function HomePage({ profile, projects, loading }: PortfolioData) {
  if (loading) return <p className="muted">Loading…</p>
  if (!profile) return null // App already shows the error message

  const featured = projects.filter((p) => p.featured)
  const languageCounts = countLanguages(projects)

  return (
    <>
      {/* ---- Hero / intro ---- */}
      <section className="hero">
        <img className="avatar" src={profile.avatarUrl} alt={`${profile.displayName}'s avatar`} />
        <div>
          <h1>{profile.displayName}</h1>
          {profile.headline && <p className="headline">{profile.headline}</p>}
          {profile.bio && <p className="bio">{profile.bio}</p>}

          <div className="hero-actions">
            <Link className="button primary" to="/projects">
              Browse projects
            </Link>
            <Link className="button" to="/resume">
              Résumé
            </Link>
            <a className="button" href={profile.gitHubUrl} target="_blank" rel="noreferrer">
              @{profile.username} on GitHub
            </a>
            {profile.links.map((link) => (
              <a key={link.label} className="button" href={link.url} target="_blank" rel="noreferrer">
                {link.label}
              </a>
            ))}
          </div>
        </div>
      </section>

      {/* ---- Education ---- */}
      {profile.education.length > 0 && (
        <section className="card education">
          <h2 className="section-label">Education</h2>
          {profile.education.map((e) => (
            <div key={`${e.school}-${e.degree}`} className="degree">
              <span className="degree-icon" aria-hidden="true">🎓</span>
              <div>
                <strong>{e.degree}</strong>
                {e.minor && <span className="muted"> · Minor in {e.minor}</span>}
                <div className="muted">
                  {e.school}
                  {e.graduated && <> · {e.graduated}</>}
                </div>
                {e.details && <p className="degree-details">{e.details}</p>}
              </div>
            </div>
          ))}
        </section>
      )}

      {/* ---- Quick stats + overall language mix ---- */}
      <section className="card stats">
        <div className="stat-row">
          <div className="stat">
            <strong>{projects.length}</strong>
            <span className="muted">projects</span>
          </div>
          <div className="stat">
            <strong>{languageCounts.length}</strong>
            <span className="muted">languages</span>
          </div>
        </div>
        <h2 className="section-label">Languages across all projects</h2>
        <LanguageBar languages={overallLanguages(projects)} maxLegend={6} />

        {/* Each language links to the projects page pre-filtered to it */}
        <div className="chips">
          {languageCounts.slice(0, 10).map((lang) => (
            <Link key={lang.name} className="chip" to={`/projects?lang=${encodeURIComponent(lang.name)}`}>
              <span className="dot" style={{ background: lang.color }} />
              {lang.name} <span className="muted">{lang.count}</span>
            </Link>
          ))}
        </div>
      </section>

      {/* ---- Featured projects ---- */}
      {featured.length > 0 && (
        <section>
          <div className="section-heading">
            <h2>Featured projects</h2>
            <Link to="/projects">See all {projects.length} →</Link>
          </div>
          <div className="project-grid">
            {featured.map((project) => (
              <ProjectCard key={project.repoName} project={project} />
            ))}
          </div>
        </section>
      )}
    </>
  )
}
