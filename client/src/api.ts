// Types and fetch helpers for the C# API.
// These shapes mirror server/Portfolio.Api/Models/PortfolioModels.cs — keep them in sync.

/** How much of a project is written in one language. */
export type LanguageShare = {
  name: string
  percent: number // 0–100
  color: string // hex color GitHub uses for this language
}

/** One project card, built from a GitHub repo plus overrides in appsettings.json. */
export type Project = {
  repoName: string
  title: string
  course: string | null
  description: string | null
  repoUrl: string
  demoUrl: string | null
  award: string | null // e.g. "1st Place, Hybrid Teams — Hack Midwest 2026"
  teamOwner: string | null // set when the repo is on a teammate's account
  languages: LanguageShare[] // largest first
  topics: string[]
  stars: number
  featured: boolean
  createdAt: string // ISO date string
  pushedAt: string // ISO date string — last time code was pushed
}

export type ProfileLink = { label: string; url: string }

/** A live website shown on the Websites page. */
export type Website = {
  name: string
  url: string
  description: string | null
  tech: string[]
  repoUrl: string | null
}

/** One degree, e.g. B.S. in Computer Science from UMSL. */
export type Education = {
  school: string
  degree: string
  minor: string | null
  graduated: string | null // free text, e.g. "May 2026"
  details: string | null // optional sentence, e.g. what a certificate covered
  coursework: string[] // relevant courses, shown on the Résumé page
}

/** One position on the résumé. */
export type Experience = {
  title: string
  organization: string
  location: string | null
  start: string | null // free text, e.g. "Jun 2025"
  end: string | null // free text, e.g. "Present"
  highlights: string[] // bullet points
  projects: ExperienceProject[] // optional named projects within the role
}

/** A named project within one position, e.g. "Codebase Q&A Tool". */
export type ExperienceProject = {
  name: string
  tech: string | null // e.g. "Azure AI Search, RAG, Cursor"
  highlights: string[]
}

/** A named group of skills, e.g. "Tools". */
export type SkillGroup = { name: string; skills: string[] }

/** Résumé content from appsettings.json (education and languages come from elsewhere). */
export type Resume = {
  summary: string
  pdfUrl: string | null
  experience: Experience[]
  skillGroups: SkillGroup[]
}

/** Info about the site owner for the header and home page. */
export type Profile = {
  displayName: string
  headline: string
  bio: string
  username: string
  avatarUrl: string
  gitHubUrl: string
  publicRepos: number
  education: Education[]
  links: ProfileLink[]
  websites: Website[]
}

/** GETs JSON from the API and throws a readable error if the request fails. */
async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(path)
  if (!response.ok) {
    throw new Error(`Request to ${path} failed (${response.status})`)
  }
  return (await response.json()) as T
}

export const fetchProfile = () => getJson<Profile>('/api/profile')
export const fetchProjects = () => getJson<Project[]>('/api/projects')
export const fetchResume = () => getJson<Resume>('/api/resume')
