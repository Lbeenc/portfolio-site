import type { PortfolioData } from '../usePortfolioData'

/**
 * Live websites from the "Websites" list in appsettings.json, each with
 * a link to the site and (optionally) its source code.
 */
export default function WebsitesPage({ profile, loading }: PortfolioData) {
  if (loading) return <p className="muted">Loading websites…</p>
  if (!profile) return null // App already shows the error message

  return (
    <>
      <div className="page-heading">
        <h1>Websites</h1>
        <p className="muted">Sites I've built that are live on the web.</p>
      </div>

      <div className="project-grid">
        {profile.websites.map((site) => (
          <article key={site.url} className="card project-card">
            <div className="card-top">
              <div className="tags">
                <span className="tag">{new URL(site.url).hostname}</span>
              </div>
              <h3 className="project-title">
                <a href={site.url} target="_blank" rel="noreferrer">
                  {site.name}
                </a>
              </h3>
              {site.description && <p className="description">{site.description}</p>}
            </div>

            <div className="card-bottom">
              {site.tech.length > 0 && (
                <div className="tags">
                  {site.tech.map((t) => (
                    <span key={t} className="tag tag-topic">
                      {t}
                    </span>
                  ))}
                </div>
              )}
              <div className="card-meta">
                <span className="links">
                  <a href={site.url} target="_blank" rel="noreferrer">
                    Visit site ↗
                  </a>
                  {site.repoUrl && (
                    <a href={site.repoUrl} target="_blank" rel="noreferrer">
                      Code ↗
                    </a>
                  )}
                </span>
              </div>
            </div>
          </article>
        ))}
      </div>
    </>
  )
}
