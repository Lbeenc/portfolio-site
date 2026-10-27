import { NavLink, Route, Routes } from 'react-router-dom'
import { usePortfolioData } from './usePortfolioData'
import HomePage from './pages/HomePage'
import ProjectsPage from './pages/ProjectsPage'
import ResumePage from './pages/ResumePage'
import WebsitesPage from './pages/WebsitesPage'
import Aquarium from './components/Aquarium'
import Shrimp from './components/Shrimp'

// Year for the footer copyright, computed once when the site loads.
const currentYear = new Date().getFullYear()

/**
 * Site shell: header with navigation, the current page, and a footer.
 * Data is loaded once here and handed to whichever page is showing.
 */
export default function App() {
  const data = usePortfolioData()
  const name = data.profile?.displayName ?? 'Portfolio'

  return (
    <div className="app">
      {/* Light rays and rising bubbles in the background, behind everything */}
      <Aquarium />

      <header className="site-header">
        <div className="container header-inner">
          <NavLink to="/" className="brand" aria-label={`${name} – home`}>
            <Shrimp size={44} />
            <span className="brand-name">{name}</span>
          </NavLink>
          <nav className="nav">
            {/* `end` makes "Home" active only on "/" exactly, not on every page */}
            <NavLink to="/" end>
              Home
            </NavLink>
            <NavLink to="/projects">Projects</NavLink>
            <NavLink to="/websites">Websites</NavLink>
            <NavLink to="/resume">Résumé</NavLink>
            {data.profile && (
              <a href={data.profile.gitHubUrl} target="_blank" rel="noreferrer">
                GitHub ↗
              </a>
            )}
          </nav>
        </div>
      </header>

      <main className="container main">
        {/* Shown on every page if the API call failed */}
        {data.error && (
          <div className="notice error">
            Couldn't load projects: {data.error}. Is the API running?
          </div>
        )}

        <Routes>
          <Route path="/" element={<HomePage {...data} />} />
          <Route path="/projects" element={<ProjectsPage {...data} />} />
          <Route path="/websites" element={<WebsitesPage {...data} />} />
          <Route path="/resume" element={<ResumePage {...data} />} />
          <Route path="*" element={<p className="muted">Page not found.</p>} />
        </Routes>
      </main>

      <footer className="site-footer">
        <div className="container">
          © {currentYear} {name} · Built with React, ASP.NET Core &amp; the GitHub API
        </div>
      </footer>
    </div>
  )
}
