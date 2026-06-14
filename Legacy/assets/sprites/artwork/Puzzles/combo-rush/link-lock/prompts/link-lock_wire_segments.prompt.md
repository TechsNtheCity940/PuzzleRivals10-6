# Art Prompt: `link-lock_wire_segments`
**Category:** board-tile

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: link-lock_wire_segments
**File:** `link-lock_wire_segments.png`
**Puzzle:** Link Lock (circuit)
**Category:** board-tile
**Phaser Usage:** frame per wire orientation × signal state

## Layout
7 columns × 3 rows = 21 frames at 128×128px each. Total sheet: 896×384px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Logic pathway engineering: clean node-to-node wire traces, logic gate clarity, signal pulse flow. Activation is bright and directional. Error states flash red with a circuit-break aesthetic.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Precise circuit-trace lines. Low = dim gray. High = bright cyan. Error = red flash overlay. Transparent base for board integration.

## Color Logic
Base: #0a0d1a–#111830 (dark navy). Grid lines: #1e2a4a (subtle). Interactive states: cyan edge (#00e5ff at 40–80% opacity). Blocked tiles: #1a1a2e.

## Animation Content
This sheet contains the following animation regions:
- **link-lock_wire_signal** — frames 14–20 (24fps, loop) — Signal traveling pulse

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 7×3 sprite sheet PNG (896×384px). Each frame is 128×128px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 896×384px
Grid: 7 columns × 3 rows
Frame size: 128×128px per frame
Total frames: 21
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
- node pair colors/shapes must be unmistakablelinks must be thick enough to read on phonecompleted vs incomplete vs invalid links must be visually distinctroute cells must not look clutteredoverlapping path ambiguity must be impossible
- node pair colors/shapes must be unmistakable
- node pair colors/shapes must be unmistakable
- clean outer silhouette readable at 64px
- no detail that disappears below 128px
- no decorative filigree on gameplay pieces
- Mobile Readability Notes
- node pair colors/shapes must be unmistakablelinks must be thick enough to read on phonecompleted vs incomplete vs invalid links must be visually distinctroute cells must not look clutteredoverlapping path ambiguity must be impossible
- node pair colors/shapes must be unmistakable
- Signal state must be instantly legible. High-contrast difference between low/high signal required.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, player characters, gameplay pieces on top of tiles

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('link-lock_wire_segments', '.../link-lock_wire_segments.png', { frameWidth: 128, frameHeight: 128 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 896×384px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 21 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 64px