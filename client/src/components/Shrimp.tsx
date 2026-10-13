import { useId } from 'react'

/**
 * The site logo: a green jade neocaridina shrimp drawn in SVG, facing right.
 *
 * Layout of the 120×70 drawing (head on the right, tail curling down on the left):
 *   antennae → rostrum + carapace (head) → segmented abdomen → tail fan
 * The antennae sway and the swimmerets (small legs under the abdomen) flutter via
 * CSS animations (.shrimp-antennae / .shrimp-swimmerets in index.css).
 */
export default function Shrimp({ size }: { size: number }) {
  // Unique id for the body gradient so it can't clash with anything else on the page.
  const jade = useId()

  return (
    <svg className="shrimp" width={size} height={size * (70 / 120)} viewBox="0 0 120 70" aria-hidden="true">
      <defs>
        {/* Jade green, lighter along the back like the real shrimp's translucent shell */}
        <linearGradient id={jade} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#9be8b0" />
          <stop offset="0.45" stopColor="#3fb36b" />
          <stop offset="1" stopColor="#1f7a45" />
        </linearGradient>
      </defs>

      {/* Long antennae sweeping back over the body (drawn first, so they sit behind) */}
      <g className="shrimp-antennae" fill="none" stroke="#2c8a52" strokeWidth="1.4" strokeLinecap="round">
        <path d="M101 27 C 112 8, 84 -2, 34 6" />
        <path d="M102 29 C 118 16, 98 6, 58 13" />
      </g>

      {/* Walking legs under the head */}
      <g stroke="#2f9a5c" strokeWidth="1.6" fill="none" strokeLinecap="round">
        <path d="M90 41 L94 50 L91 58" />
        <path d="M84 42 L86 52 L82 59" />
        <path d="M78 43 L78 53 L73 59" />
        <path d="M72 43 L70 52 L65 57" />
      </g>

      {/* Swimmerets: small paddles under the abdomen */}
      <g className="shrimp-swimmerets" stroke="#4cc27a" strokeWidth="2" strokeLinecap="round">
        <line x1="58" y1="42" x2="56" y2="50" />
        <line x1="50" y1="40" x2="47" y2="48" />
        <line x1="42" y1="38" x2="38" y2="45" />
      </g>

      {/* Tail fan: two side paddles (uropods) and a center lobe (telson) spreading from the tail tip */}
      <g fill="#5cc785" stroke="#1f6b40" strokeWidth="1" strokeLinejoin="round">
        <path d="M25 52 L5 55 Q3 62 12 64 Z" />
        <path d="M29 56 L22 69 Q29 71 33 64 Z" />
        <path d="M26 54 L9 65 Q14 71 22 67 Z" />
      </g>

      {/* Abdomen: curls down from the head toward the tail */}
      <path
        d="M63 21 Q42 15 28 27 Q18 37 22 52 L31 55 Q30 44 37 38 Q47 33 62 42 Z"
        fill={`url(#${jade})`}
        stroke="#1f6b40"
        strokeWidth="1"
      />
      {/* Segment lines across the abdomen */}
      <g fill="none" stroke="#1f6b40" strokeWidth="0.9" opacity="0.7">
        <path d="M53 18 Q50 28 52 38" />
        <path d="M43 19 Q39 28 42 36" />
        <path d="M34 24 Q31 32 35 39" />
        <path d="M26 33 Q26 40 30 45" />
      </g>

      {/* Rostrum: the pointed spike on the front of the head */}
      <path d="M97 26 L117 24 L100 32 Z" fill="#3fb36b" stroke="#1f6b40" strokeWidth="1" />

      {/* Carapace (head and thorax shell) */}
      <path
        d="M60 22 Q82 13 100 25 Q105 33 97 40 Q78 47 60 42 Q55 32 60 22 Z"
        fill={`url(#${jade})`}
        stroke="#1f6b40"
        strokeWidth="1"
      />
      {/* Pale stripe down the back, typical of jade shrimp */}
      <path d="M30 26 Q48 15 66 19 Q84 15 98 25" fill="none" stroke="#c9f5d6" strokeWidth="2" strokeLinecap="round" opacity="0.8" />

      {/* Eye on a short stalk */}
      <circle cx="97" cy="29" r="3.2" fill="#0f2418" />
      <circle cx="98" cy="28" r="1" fill="#fff" />
    </svg>
  )
}
