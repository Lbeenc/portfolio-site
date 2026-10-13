import type { LanguageShare, Project } from './api'

/** A language plus how many projects use it. */
export type LanguageCount = { name: string; color: string; count: number }

/**
 * Counts how many projects use each language (at any percentage), most used first.
 * Powers the filter chips on the projects page and the language list on the home page.
 */
export function countLanguages(projects: Project[]): LanguageCount[] {
  const counts = new Map<string, LanguageCount>()
  for (const project of projects) {
    for (const lang of project.languages) {
      const entry = counts.get(lang.name) ?? { name: lang.name, color: lang.color, count: 0 }
      entry.count++
      counts.set(lang.name, entry)
    }
  }
  return [...counts.values()].sort((a, b) => b.count - a.count || a.name.localeCompare(b.name))
}

/**
 * Blends every project's language breakdown into one overall breakdown, with each
 * project weighted equally (so one huge repo doesn't drown out the rest).
 * The result has the same shape as a single project's languages, so it can be
 * drawn with the same LanguageBar component.
 */
export function overallLanguages(projects: Project[]): LanguageShare[] {
  const totals = new Map<string, LanguageShare>()
  let projectsWithCode = 0

  for (const project of projects) {
    if (project.languages.length === 0) continue
    projectsWithCode++
    for (const lang of project.languages) {
      const entry = totals.get(lang.name) ?? { name: lang.name, color: lang.color, percent: 0 }
      entry.percent += lang.percent
      totals.set(lang.name, entry)
    }
  }

  // Average across projects so the percentages add up to 100.
  return [...totals.values()]
    .map((l) => ({ ...l, percent: Math.round((l.percent / projectsWithCode) * 10) / 10 }))
    .sort((a, b) => b.percent - a.percent)
}
