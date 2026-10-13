import type { Project } from '../api'
import LanguageBar from './LanguageBar'

/**
 * One project: title, course tag, description, language breakdown, and links.
 * Used on both the home page (featured) and the projects page (all).
 */
export default function ProjectCard({ project }: { project: Project }) {
  // Many repos have no description yet, so fall back to something still useful.
  const description =
    project.description ??
    (project.course ? `Course project for ${project.course}.` : 'No description yet.')

  return (
    <article className={`card project-card${project.featured ? ' featured' : ''}`}>
      <div className="card-top">
        {/* Small tags above the title */}
        <div className="tags">
          {project.featured && <span className="tag tag-featured">★ Featured</span>}
          {project.course && <span className="tag">{project.course}</span>}
          {project.teamOwner && <span className="tag tag-topic">Team project</span>}
        </div>

        {/* Award ribbon, e.g. a hackathon placement */}
        {project.award && <p className="award">🏆 {project.award}</p>}

        <h3 className="project-title">
          <a href={project.repoUrl} target="_blank" rel="noreferrer">
            {project.title}
          </a>
        </h3>

        <p className={project.description ? 'description' : 'description muted'}>{description}</p>

        {/* GitHub topics, if the repo has any */}
        {project.topics.length > 0 && (
          <div className="tags">
            {project.topics.map((topic) => (
              <span key={topic} className="tag tag-topic">
                #{topic}
              </span>
            ))}
          </div>
        )}
      </div>

      <div className="card-bottom">
        <LanguageBar languages={project.languages} />

        <div className="card-meta">
          <span className="muted small">Updated {formatDate(project.pushedAt)}</span>
          {project.stars > 0 && <span className="muted small">★ {project.stars}</span>}
          <span className="links">
            {project.demoUrl && (
              <a href={project.demoUrl} target="_blank" rel="noreferrer">
                Live demo ↗
              </a>
            )}
            <a href={project.repoUrl} target="_blank" rel="noreferrer">
              Code ↗
            </a>
          </span>
        </div>
      </div>
    </article>
  )
}

/** "2025-03-14T..." -> "Mar 2025" */
function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', year: 'numeric' })
}
