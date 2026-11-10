# Art Prompt: `tile-shift_board_tiles`
**Category:** board-tile

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: tile-shift_board_tiles
**File:** `tile-shift_board_tiles.png`
**Puzzle:** Tile Shift (sliding)
**Category:** board-tile
**Phaser Usage:** this.load.spritesheet() in scene preload; individual frames drawn per cell

## Layout
6 columns × 3 rows = 18 frames at 128×128px each. Total sheet: 768×384px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Smooth spatial precision: tile edge clarity is critical. Slide trails are brief and directional. Goal tile distinguished by strong accent color. Path completion has a satisfying lock-in flash.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Row 0: empty cell variants (default, hover, selected). Row 1: source node states (unpressurized, active, pulsing, overloaded). Row 2: destination node states (unfulfilled, receiving, satisfied, overflow). All cells: 128×128px, transparent bg, dark navy base, cyan accent energy.

## Color Logic
Base: #0a0d1a–#111830 (dark navy). Grid lines: #1e2a4a (subtle). Interactive states: cyan edge (#00e5ff at 40–80% opacity). Blocked tiles: #1a1a2e.

## Art Direction
Produce a 6×3 sprite sheet PNG (768×384px). Each frame is 128×128px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 768×384px
Grid: 6 columns × 3 rows
Frame size: 128×128px per frame
Total frames: 18
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
- shift arrows must be obviousselected row/column highlight must be clearboard must not become cluttered by too many guidance overlaysrow and column boundaries must be visually readablewraparound destination preview should be clean and brief
- shift arrows must be obvious
- shift arrows must be obvious
- clean outer silhouette readable at 64px
- no detail that disappears below 128px
- no decorative filigree on gameplay pieces
- Mobile Readability Notes
- shift arrows must be obviousselected row/column highlight must be clearboard must not become cluttered by too many guidance overlaysrow and column boundaries must be visually readablewraparound destination preview should be clean and brief
- shift arrows must be obvious
- Source/destination nodes must be immediately distinguishable from connectors. Node aperture must be clearly directional.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, player characters, gameplay pieces on top of tiles

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('tile-shift_board_tiles', '.../tile-shift_board_tiles.png', { frameWidth: 128, frameHeight: 128 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 768×384px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 18 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 64px