# Art Prompt: `shard-stack_zone_clear_fx`
**Category:** destruction-burst

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: shard-stack_zone_clear_fx
**File:** `shard-stack_zone_clear_fx.png`
**Puzzle:** Shard Stack (shape-packing)
**Category:** destruction-burst
**Phaser Usage:** played once centered on cleared zone, scaled to fit zone size

## Layout
7 columns × 1 rows = 7 frames at 384×384px each. Total sheet: 2688×384px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Precision geometry: clean-edged block forms with subtle inner glow on valid placement. Strong edge contrast for piece differentiation. Snap feedback is crisp and immediate. Area completion is rewarding and visible.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Large burst covering completed zone area. Bright white flash → cyan spread → fade. Geometric angular shards fly outward. Premium, crisp, celebratory.

## Color Logic
Start: cyan-white core (#ffffff). Expand: cyan (#00e5ff) → amber (#ffc400) → orange (#ff6d00). End: alpha fade to transparent. No sustained smoke.

## Animation Content
This sheet contains the following animation regions:
- **shard-stack_zone_clear** — frames 0–6 (18fps, once) — Zone clear burst

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 7×1 sprite sheet PNG (2688×384px). Each frame is 384×384px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 2688×384px
Grid: 7 columns × 1 rows
Frame size: 384×384px per frame
Total frames: 7
Frame spacing: 0px
Sheet margin: 0px

CRITICAL LAYOUT RULES:
- All frames must be exactly 384×384px — no exceptions.
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
- Crisp, clean silhouettes readable at 192px (half render size)
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
- Must not persist longer than 0.5s. Board must be readable immediately after.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, slow burn effects, sustained smoke trails across frames

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('shard-stack_zone_clear_fx', '.../shard-stack_zone_clear_fx.png', { frameWidth: 384, frameHeight: 384 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 2688×384px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 7 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 192px