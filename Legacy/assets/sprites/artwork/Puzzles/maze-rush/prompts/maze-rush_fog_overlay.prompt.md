# Art Prompt: `maze-rush_fog_overlay`
**Category:** environment-overlay

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: maze-rush_fog_overlay
**File:** `maze-rush_fog_overlay.png`
**Puzzle:** Maze Rush (maze)
**Category:** environment-overlay
**Phaser Usage:** layered over unexplored tiles; fade out on approach

## Layout
8 columns × 1 rows = 8 frames at 128×128px each. Total sheet: 1024×128px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Tactical arena navigation: muted base geometry with bright traversal beacons. Fog-of-war creates mystery. Route clarity is paramount. Checkpoints pulse softly. Directional cues are always visible.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Deep navy/dark smoke texture with subtle drift animation. Semi-transparent at edges. Dense at center. Premium competitive feel — not cartoonish fog, more like arena sensor blackout.

## Color Logic
Darkening overlay using near-black (#030508 at variable alpha). Edge tinting: dark red (#4a0000) for danger. Transparent center always.

## Animation Content
This sheet contains the following animation regions:
- **maze-rush_fog_drift** — frames 0–7 (6fps, loop) — Fog of war slow drift

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 8×1 sprite sheet PNG (1024×128px). Each frame is 128×128px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 1024×128px
Grid: 8 columns × 1 rows
Frame size: 128×128px per frame
Total frames: 8
Frame spacing: 0px
Sheet margin: 0px

CRITICAL LAYOUT RULES:
- All frames must be exactly 128×128px — no exceptions.
- Frames must fill the grid left-to-right, top-to-bottom in exact order.
- No extra whitespace rows. No partial rows unless intentional (remaining frames must be transparent).
- Effect bounds must not overflow frame edges — crop hard at frame boundary.
- Scale must be identical across all frames (no zoom drift between frames).
- Sprites must be centered within their frame.

---

## Style Instructions

Finish: glossy hard-surface with controlled bloom
Contrast: high
Bloom: controlled — keep controlled and readable
Particles: moderate — do not exceed

MUST:
- Crisp, clean silhouettes readable at 64px (half render size)
- Consistent material language across all frames
- Transparent background on every frame (PNG alpha)
- Premium polished finish — no rough edges, no aliasing

MUST NOT:
- Painterly or soft-rendered look
- Background scenes or ground planes
- Perspective distortion
- Lighting that changes angle between frames
- Details that disappear at 50% scale

---

## Consistency Rules

- All frames must use the same base geometry and proportions.
- State changes (idle → active → burst) must feel like evolution of the same object, not different objects.
- Color accents must remain consistent per state: idle = low brightness, active = full brightness, charged = amber addition, burst = white flash.
- If this sheet is part of a set (e.g., pipe-flow pieces), all pieces must share the same pipe diameter, material, and edge style.

---

## Readability Rules

- gameplay state must be readable from tile color/shape alone
- flow direction must be readable without tooltips
- high contrast against dark board backgrounds required
- no clutter within tile bounds
- Mobile Readability Notes
- corridor versus wall readability is mandatorypartial-vision mask must be clean and readablebeacon must clearly show general direction, not exact solutionroute line must remain visible under fogcheckpoint markers must be unmistakable when within vision radius
- corridor versus wall readability is mandatory
- corridor versus wall readability is mandatory
- clean outer silhouette readable at 64px
- no detail that disappears below 128px
- no decorative filigree on gameplay pieces
- Mobile Readability Notes
- corridor versus wall readability is mandatorypartial-vision mask must be clean and readablebeacon must clearly show general direction, not exact solutionroute line must remain visible under fogcheckpoint markers must be unmistakable when within vision radius
- corridor versus wall readability is mandatory
- Must preserve enough opacity to be effective, but not 100% opaque to hint at wall shapes.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, gameplay pieces mixed into overlay, opaque center tinting

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('maze-rush_fog_overlay', '.../maze-rush_fog_overlay.png', { frameWidth: 128, frameHeight: 128 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 1024×128px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 8 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 64px