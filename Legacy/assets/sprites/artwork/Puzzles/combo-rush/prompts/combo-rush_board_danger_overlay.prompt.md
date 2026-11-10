# Art Prompt: `combo-rush_board_danger_overlay`
**Category:** environment-overlay

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: combo-rush_board_danger_overlay
**File:** `combo-rush_board_danger_overlay.png`
**Puzzle:** Combo Rush (match)
**Category:** environment-overlay
**Phaser Usage:** looped overlay applied to board edges when danger threshold reached

## Layout
6 columns × 1 rows = 6 frames at 128×128px each. Total sheet: 768×128px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Reactive cascade engine: gem-bright saturated pieces, explosive chain reactions, punchy clear FX. Each piece type has strong color identity. Combo bursts are high-contrast. Board danger expressed through darkening overlay.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Dark red vignette that pulses inward. Edge-only, transparent center. Unsettling but not blinding.

## Color Logic
Darkening overlay using near-black (#030508 at variable alpha). Edge tinting: dark red (#4a0000) for danger. Transparent center always.

## Animation Content
This sheet contains the following animation regions:
- **combo-rush_board_danger_loop** — frames 0–5 (8fps, loop) — Board danger edge pulse

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 6×1 sprite sheet PNG (768×128px). Each frame is 128×128px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 768×128px
Grid: 6 columns × 1 rows
Frame size: 128×128px per frame
Total frames: 6
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
- minimum tile interaction size: 56 to 64 pxcombo meter must be readable in under a glancescore should be secondary to combo visibility in this modehigh-intensity VFX must never hide future move opportunitiescombo warning effects should frame the board, not wash over it
- minimum tile interaction size: 56 to 64 px
- minimum tile interaction size: 56 to 64 px
- clean outer silhouette readable at 64px
- no detail that disappears below 128px
- no decorative filigree on gameplay pieces
- Mobile Readability Notes
- minimum tile interaction size: 56 to 64 pxcombo meter must be readable in under a glancescore should be secondary to combo visibility in this modehigh-intensity VFX must never hide future move opportunitiescombo warning effects should frame the board, not wash over it
- minimum tile interaction size: 56 to 64 px
- Center of board must remain fully playable — no center tinting.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, gameplay pieces mixed into overlay, opaque center tinting

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('combo-rush_board_danger_overlay', '.../combo-rush_board_danger_overlay.png', { frameWidth: 128, frameHeight: 128 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 768×128px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 6 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 64px