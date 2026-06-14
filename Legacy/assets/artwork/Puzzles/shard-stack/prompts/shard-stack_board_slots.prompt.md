# Art Prompt: `shard-stack_board_slots`
**Category:** board-tile

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: shard-stack_board_slots
**File:** `shard-stack_board_slots.png`
**Puzzle:** Shard Stack (shape-packing)
**Category:** board-tile
**Phaser Usage:** frame index per slot state drawn to grid

## Layout
6 columns × 1 rows = 6 frames at 96×96px each. Total sheet: 576×96px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Precision geometry: clean-edged block forms with subtle inner glow on valid placement. Strong edge contrast for piece differentiation. Snap feedback is crisp and immediate. Area completion is rewarding and visible.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Clean geometric slot recess. Dark inset, subtle grid lines. Hover state = subtle cyan edge. Valid target = brighter edge. Invalid = red edge.

## Color Logic
Base: #0a0d1a–#111830 (dark navy). Grid lines: #1e2a4a (subtle). Interactive states: cyan edge (#00e5ff at 40–80% opacity). Blocked tiles: #1a1a2e.

## Art Direction
Produce a 6×1 sprite sheet PNG (576×96px). Each frame is 96×96px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 576×96px
Grid: 6 columns × 1 rows
Frame size: 96×96px per frame
Total frames: 6
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
Particles: moderate — do not exceed

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

---

## Readability Rules

- gameplay state must be readable from tile color/shape alone
- flow direction must be readable without tooltips
- high contrast against dark board backgrounds required
- no clutter within tile bounds
- Mobile Readability Notes
- cells must be large enough to show shard silhouettes clearlydrag ghost must be readablerotation preview must be instantillegal placement state must be obviousqueue pieces must remain visible and distinctboard cannot be overloaded with decorative crystal clutter
- cells must be large enough to show shard silhouettes clearly
- cells must be large enough to show shard silhouettes clearly
- clean outer silhouette readable at 64px
- no detail that disappears below 128px
- no decorative filigree on gameplay pieces
- Mobile Readability Notes
- cells must be large enough to show shard silhouettes clearlydrag ghost must be readablerotation preview must be instantillegal placement state must be obviousqueue pieces must remain visible and distinctboard cannot be overloaded with decorative crystal clutter
- cells must be large enough to show shard silhouettes clearly
- Slot boundaries must be clearly visible to guide placement decisions.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, player characters, gameplay pieces on top of tiles

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('shard-stack_board_slots', '.../shard-stack_board_slots.png', { frameWidth: 96, frameHeight: 96 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 576×96px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 6 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 48px