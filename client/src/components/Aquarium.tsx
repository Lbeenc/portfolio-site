import type { CSSProperties } from 'react'

/*
 * The page background: light rays from the tank lid and bubbles rising through the water.
 * It sits fixed behind the page (pointer-events: none) so it never blocks clicks.
 * All movement is CSS animation (see the "Aquarium" section of index.css);
 * the bubbles are hidden and the rays hold still for visitors whose system is set to "reduce motion".
 */

/**
 * Small seeded random number generator, so the bubbles get the same "random"
 * positions on every render (and every visit) instead of jumping around.
 */
function seededRandom(seed: number) {
  return () => {
    seed = (seed + 0x6d2b79f5) | 0
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed)
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296
  }
}

const random = seededRandom(2026)

// Computed once when the module loads; each bubble gets its own column, size and speed.
const BUBBLES = Array.from({ length: 30 }, () => ({
  left: `${(random() * 100).toFixed(1)}%`,
  size: 4 + Math.round(random() * 12),
  duration: 8 + random() * 10, // seconds to rise from bottom to top
  delay: -random() * 18, // negative so some are already mid-rise on load
  drift: `${Math.round((random() - 0.5) * 40)}px`, // sideways wobble
}))

export default function Aquarium() {
  return (
    <div className="aquarium" aria-hidden="true">
      {/* Light from the tank's lid shining down into the water */}
      <div className="light-rays" />

      {/* Bubbles rising from the bottom */}
      {BUBBLES.map((b, i) => (
        <span
          key={i}
          className="bubble"
          style={
            {
              left: b.left,
              width: b.size,
              height: b.size,
              animationDuration: `${b.duration}s`,
              animationDelay: `${b.delay}s`,
              '--drift': b.drift,
            } as CSSProperties
          }
        />
      ))}
    </div>
  )
}
