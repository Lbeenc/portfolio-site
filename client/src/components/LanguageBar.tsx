import type { LanguageShare } from '../api'

type Props = {
  languages: LanguageShare[]
  /** How many languages to list in the legend under the bar; the rest are summed as "Other". */
  maxLegend?: number
}

/**
 * GitHub-style stacked bar showing a project's language breakdown,
 * with a colored legend underneath (e.g. "● C# 92%  ● HLSL 8%").
 */
export default function LanguageBar({ languages, maxLegend = 4 }: Props) {
  if (languages.length === 0) {
    return <p className="muted small">No code detected</p>
  }

  // Legend shows the biggest few; anything beyond that is rolled into "Other".
  const shown = languages.slice(0, maxLegend)
  const otherPercent = languages.slice(maxLegend).reduce((sum, l) => sum + l.percent, 0)

  return (
    <div className="lang">
      {/* The bar: one colored segment per language, width = its percentage */}
      <div className="lang-bar" role="img" aria-label={languages.map((l) => `${l.name} ${l.percent}%`).join(', ')}>
        {languages.map((l) => (
          <span key={l.name} style={{ width: `${l.percent}%`, background: l.color }} title={`${l.name} ${l.percent}%`} />
        ))}
      </div>

      {/* The legend */}
      <ul className="lang-legend">
        {shown.map((l) => (
          <li key={l.name}>
            <span className="dot" style={{ background: l.color }} />
            {l.name} <span className="muted">{formatPercent(l.percent)}</span>
          </li>
        ))}
        {otherPercent > 0 && (
          <li>
            <span className="dot" style={{ background: '#8b949e' }} />
            Other <span className="muted">{formatPercent(otherPercent)}</span>
          </li>
        )}
      </ul>
    </div>
  )
}

/** 100 -> "100%", 3.25 -> "3.3%", 0.4 -> "<1%" */
function formatPercent(value: number) {
  if (value < 1) return '<1%'
  return `${value >= 10 ? Math.round(value) : value.toFixed(1)}%`
}
