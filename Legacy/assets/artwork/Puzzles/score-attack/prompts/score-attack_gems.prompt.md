# Art Prompt: `score-attack_gems`
**Category:** gameplay-piece

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: score-attack_gems
**File:** `score-attack_gems.png`
**Puzzle:** Score Attack (match)
**Category:** gameplay-piece
**Phaser Usage:** spritesheet per color variant; individual animations per state

## Layout
8 columns × 6 rows = 48 frames at 96×96px each. Total sheet: 768×576px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Reactive cascade engine: gem-bright saturated pieces, explosive chain reactions, punchy clear FX. Each piece type has strong color identity. Combo bursts are high-contrast. Board danger expressed through darkening overlay.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Each row = 1 color variant (cyan, magenta, amber, green, red, neutral). Strong gem silhouette with faceted surface. Bright inner glow per color. Match flash = white-burst then color explosion. Selected state = gem enlarges slightly with ring outline.

## Color Logic
Idle: muted version of primary color. Active/flowing: full cyan (#00e5ff). Charged/pressurized: cyan shifts toward amber (#ffc400). Burst/danger: amber/orange (#ff6d00) → white flash.

## Animation Content
This sheet contains the following animation regions:
- **score-attack_gem_idle** — frames 0–3 (6fps, loop) — Gem idle shimmer
- **score-attack_gem_select** — frames 4–7 (18fps, once) — Gem selected pop
- **score-attack_gem_match** — frames 8–13 (24fps, once) — Gem match flash
- **score-attack_gem_clear** — frames 14–21 (24fps, once) — Gem clear burst
- **score-attack_gem_fall** — frames 22–25 (14fps, loop) — Gem falling motion

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 8×6 sprite sheet PNG (768×576px). Each frame is 96×96px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 768×576px
Grid: 8 columns × 6 rows
Frame size: 96×96px per frame
Total frames: 48
Frame spacing: 0px
Sheet margin: 0px

CRITICAL LAYOUT RULES:
- All frames must be exactly 96×96px — no exceptions.
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
Particles: minimal — do not exceed

MUST:
- Crisp, clean silhouettes readable at 48px (half render size)
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
- Each animation region (score-attack_gem_idle, score-attack_gem_select, score-attack_gem_match, score-attack_gem_clear, score-attack_gem_fall) must be visually distinguishable. Do not make idle and active look nearly identical.

---

## Readability Rules

- gameplay state must be readable from tile color/shape alone
- flow direction must be readable without tooltips
- high contrast against dark board backgrounds required
- no clutter within tile bounds
- Mobile Readability Notes
- minimum tile tap target: 56 px visual target, 64 px interaction-safe targetboard should occupy roughly 55% to 65% of vertical mobile view in portrait-adapted modetile symbols must remain readable without text labelsscore indicators must be glanceableno VFX should cover more than 20% of the board at once in standard play
- minimum tile tap target: 56 px visual target, 64 px interaction-safe target
- minimum tile tap target: 56 px visual target, 64 px interaction-safe target
- clean outer silhouette readable at 64px
- no detail that disappears below 128px
- no decorative filigree on gameplay pieces
- Mobile Readability Notes
- minimum tile tap target: 56 px visual target, 64 px interaction-safe targetboard should occupy roughly 55% to 65% of vertical mobile view in portrait-adapted modetile symbols must remain readable without text labelsscore indicators must be glanceableno VFX should cover more than 20% of the board at once in standard play
- minimum tile tap target: 56 px visual target, 64 px interaction-safe target
- Color variants must remain distinguishable even at small sizes (64px). No two variants similar in hue.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, background scenery, board grid visible behind pieces

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('score-attack_gems', '.../score-attack_gems.png', { frameWidth: 96, frameHeight: 96 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 768×576px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 48 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 48px