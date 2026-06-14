# Art Prompt: `tile-shift_burst_fx`
**Category:** destruction-burst

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: tile-shift_burst_fx
**File:** `tile-shift_burst_fx.png`
**Puzzle:** Tile Shift (sliding)
**Category:** destruction-burst
**Phaser Usage:** played once at pipe burst location, centered on tile

## Layout
8 columns × 2 rows = 14 frames at 192×192px each. Total sheet: 1536×384px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Smooth spatial precision: tile edge clarity is critical. Slide trails are brief and directional. Goal tile distinguished by strong accent color. Path completion has a satisfying lock-in flash.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Energetic decompression explosion. Starts cyan core, expands to orange/amber shockwave, fragments of pipe debris scatter, ends with white flash then alpha fade. Frames slightly larger than pipe tile (192px) to allow FX overflow. No excessive particle clouds — contained destructive burst.

## Color Logic
Start: cyan-white core (#ffffff). Expand: cyan (#00e5ff) → amber (#ffc400) → orange (#ff6d00). End: alpha fade to transparent. No sustained smoke.

## Animation Content
This sheet contains the following animation regions:
- **tile-shift_pipe_burst_fx** — frames 0–13 (18fps, once) — Pipe pressure burst explosion

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 8×2 sprite sheet PNG (1536×384px). Each frame is 192×192px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 1536×384px
Grid: 8 columns × 2 rows
Frame size: 192×192px per frame
Total frames: 14
Frame spacing: 0px
Sheet margin: 0px

CRITICAL LAYOUT RULES:
- All frames must be exactly 192×192px — no exceptions.
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
- Crisp, clean silhouettes readable at 96px (half render size)
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
- Burst must be visually dramatic enough to communicate failure. Must not persist long enough to obscure neighboring tiles during gameplay.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, slow burn effects, sustained smoke trails across frames

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('tile-shift_burst_fx', '.../tile-shift_burst_fx.png', { frameWidth: 192, frameHeight: 192 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 1536×384px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 14 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 96px