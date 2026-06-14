# Art Prompt: `maze-rush_board_tiles`
**Category:** board-tile

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: maze-rush_board_tiles
**File:** `maze-rush_board_tiles.png`
**Puzzle:** Maze Rush (maze)
**Category:** board-tile
**Phaser Usage:** individual frame index per tile type drawn to board grid

## Layout
8 columns × 5 rows = 40 frames at 128×128px each. Total sheet: 1024×640px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Tactical arena navigation: muted base geometry with bright traversal beacons. Fog-of-war creates mystery. Route clarity is paramount. Checkpoints pulse softly. Directional cues are always visible.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Floor tiles: dark slate base with subtle grid texture. Tactical/arena feel. Wall tiles: heavier navy-black, strong edge contrast. Checkpoints: glowing cyan marker on floor. Hazard tiles: amber warning / red-active state.

## Color Logic
Base: #0a0d1a–#111830 (dark navy). Grid lines: #1e2a4a (subtle). Interactive states: cyan edge (#00e5ff at 40–80% opacity). Blocked tiles: #1a1a2e.

## Animation Content
This sheet contains the following animation regions:
- **maze-rush_checkpoint_pulse** — frames 16–19 (8fps, loop) — Checkpoint idle pulse
- **maze-rush_hazard_warning** — frames 24–27 (12fps, loop) — Hazard warning cycle
- **maze-rush_hazard_active** — frames 28–31 (18fps, loop) — Hazard active state

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 8×5 sprite sheet PNG (1024×640px). Each frame is 128×128px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 1024×640px
Grid: 8 columns × 5 rows
Frame size: 128×128px per frame
Total frames: 40
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
- Each animation region (maze-rush_checkpoint_pulse, maze-rush_hazard_warning, maze-rush_hazard_active) must be visually distinguishable. Do not make idle and active look nearly identical.

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
- Wall/floor distinction must be clear at 128px and 64px. Hazard state must be unmistakable without relying on color alone (shape + intensity differ).

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, player characters, gameplay pieces on top of tiles

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('maze-rush_board_tiles', '.../maze-rush_board_tiles.png', { frameWidth: 128, frameHeight: 128 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 1024×640px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 40 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 64px