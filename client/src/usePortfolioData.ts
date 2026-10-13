import { useEffect, useState } from 'react'
import { fetchProfile, fetchProjects, fetchResume, type Profile, type Project, type Resume } from './api'

/** Everything the pages need, plus loading/error state. */
export type PortfolioData = {
  profile: Profile | null
  projects: Project[]
  resume: Resume | null
  loading: boolean
  error: string | null
}

/**
 * Loads the profile, projects and résumé once when the app starts.
 * All pages share this data, so it's fetched at the top (in App) and passed down.
 */
export function usePortfolioData(): PortfolioData {
  const [data, setData] = useState<PortfolioData>({
    profile: null,
    projects: [],
    resume: null,
    loading: true,
    error: null,
  })

  useEffect(() => {
    // If the component unmounts before the requests finish, don't update state.
    let cancelled = false

    // Fire all requests at the same time instead of one after the other.
    Promise.all([fetchProfile(), fetchProjects(), fetchResume()])
      .then(([profile, projects, resume]) => {
        if (!cancelled) setData({ profile, projects, resume, loading: false, error: null })
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          const message = err instanceof Error ? err.message : 'Something went wrong'
          setData((prev) => ({ ...prev, loading: false, error: message }))
        }
      })

    return () => {
      cancelled = true
    }
  }, [])

  return data
}
