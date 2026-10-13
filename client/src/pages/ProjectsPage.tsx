import { useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { Project } from '../api'
import type { PortfolioData } from '../usePortfolioData'
import { countLanguages } from '../languageStats'
import ProjectCard from '../components/ProjectCard'

/** Ways the project list can be ordered. */
type SortOrder = 'featured' | 'recent' | 'name'

/**
 * Every project in a scrollable grid, with a search box, language filter chips,
 * and a sort menu. The selected language is kept in the URL (?lang=C%23) so a
 * filtered view can be linked to — the home page's language chips use this.
 */
export default function ProjectsPage({ projects, loading }: PortfolioData) {
  const [searchParams, setSearchParams] = useSearchParams()
  const selectedLanguage = searchParams.get('lang')

  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<SortOrder>('featured')

  // Language chips with "how many projects use this" counts.
  const languages = useMemo(() => countLanguages(projects), [projects])

  // Apply the language filter, search text, and sort order.
  // useMemo means this only re-runs when one of those inputs changes.
  const visible = useMemo(() => {
    const query = search.trim().toLowerCase()
    const filtered = projects.filter(
      (p) =>
        (!selectedLanguage || p.languages.some((l) => l.name === selectedLanguage)) &&
        (!query || matchesSearch(p, query)),
    )
    return sortProjects(filtered, sort)
  }, [projects, selectedLanguage, search, sort])

  /** Clicking the active chip clears the filter; clicking another selects it. */
  function toggleLanguage(name: string) {
    setSearchParams(name === selectedLanguage ? {} : { lang: name })
  }

  if (loading) return <p className="muted">Loading projects…</p>

  return (
    <>
      <div className="page-heading">
        <h1>Projects</h1>
        <p className="muted">
          Everything I've built, pulled live from GitHub. Filter by language or search by name or course.
        </p>
      </div>

      {/* ---- Controls: search + sort ---- */}
      <div className="toolbar">
        <input
          className="search"
          type="search"
          placeholder="Search projects…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label="Search projects"
        />
        <select value={sort} onChange={(e) => setSort(e.target.value as SortOrder)} aria-label="Sort projects">
          <option value="featured">Featured first</option>
          <option value="recent">Recently updated</option>
          <option value="name">Name (A–Z)</option>
        </select>
      </div>

      {/* ---- Language filter chips ---- */}
      <div className="chips">
        {languages.map((lang) => (
          <button
            key={lang.name}
            type="button"
            className={`chip${lang.name === selectedLanguage ? ' active' : ''}`}
            onClick={() => toggleLanguage(lang.name)}
            aria-pressed={lang.name === selectedLanguage}
          >
            <span className="dot" style={{ background: lang.color }} />
            {lang.name} <span className="muted">{lang.count}</span>
          </button>
        ))}
      </div>

      <p className="muted small result-count">
        Showing {visible.length} of {projects.length}
        {selectedLanguage && <> using <strong>{selectedLanguage}</strong></>}
      </p>

      {/* ---- The projects ---- */}
      {visible.length > 0 ? (
        <div className="project-grid">
          {visible.map((project) => (
            <ProjectCard key={project.repoName} project={project} />
          ))}
        </div>
      ) : (
        <p className="muted">No projects match those filters.</p>
      )}
    </>
  )
}

/** True if the search text appears in the title, repo name, course, description, or topics. */
function matchesSearch(p: Project, query: string) {
  return [p.title, p.repoName, p.course, p.description, ...p.topics]
    .some((field) => field?.toLowerCase().includes(query))
}

/** Returns a sorted copy (never mutates the original array). */
function sortProjects(list: Project[], sort: SortOrder): Project[] {
  switch (sort) {
    case 'recent':
      return [...list].sort((a, b) => b.pushedAt.localeCompare(a.pushedAt))
    case 'name':
      return [...list].sort((a, b) => a.title.localeCompare(b.title))
    case 'featured':
      // The API already returns featured first, then most recent.
      return list
  }
}
