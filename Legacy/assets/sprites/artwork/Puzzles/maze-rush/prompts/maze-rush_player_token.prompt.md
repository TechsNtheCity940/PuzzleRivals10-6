# Art Prompt: `maze-rush_player_token`
**Category:** gameplay-piece

---

# SPRITE SHEET PRODUCTION BRIEF

## Sheet: maze-rush_player_token
**File:** `maze-rush_player_token.png`
**Puzzle:** Maze Rush (maze)
**Category:** gameplay-piece
**Phaser Usage:** animated player avatar, centered on current tile position

## Layout
8 columns × 4 rows = 30 frames at 80×80px each. Total sheet: 640×320px.

## Visual Style
Game: Premium competitive arcade puzzle battler.
Visual language: Tactical arena navigation: muted base geometry with bright traversal beacons. Fog-of-war creates mystery. Route clarity is paramount. Checkpoints pulse softly. Directional cues are always visible.

Base aesthetic: dark navy/black-blue backgrounds (#0a0d1a to #111830), electric cyan (#00e5ff) as primary energy, magenta/violet (#e040fb) as secondary accent, amber/gold (#ffc400) ONLY for high-impact states. Glossy hard-surface material finish. Controlled bloom. No muddy rendering.

Sheet-specific style: Premium stylized character token. Compact silhouette (fits in 80×80 on 128×128 tile). Energy trail element on movement frames. Cyan energy core visible on idle.

## Color Logic
Idle: muted version of primary color. Active/flowing: full cyan (#00e5ff). Charged/pressurized: cyan shifts toward amber (#ffc400). Burst/danger: amber/orange (#ff6d00) → white flash.

## Animation Content
This sheet contains the following animation regions:
- **maze-rush_player_idle** — frames 0–7 (8fps, loop) — Player idle bob
- **maze-rush_player_move** — frames 8–15 (14fps, loop) — Player run cycle
- **maze-rush_player_stun** — frames 16–23 (12fps, once) — Player stun spin
- **maze-rush_player_checkpoint** — frames 24–27 (12fps, once) — Player checkpoint reaction
- **maze-rush_player_exit** — frames 28–29 (8fps, hold-last) — Player exit celebration

Each animation region must read as a coherent temporal sequence. Frame-to-frame motion must be smooth and consistent.

## Art Direction
Produce a 8×4 sprite sheet PNG (640×320px). Each frame is 80×80px with 0px spacing and 0px outer margin. Transparent background: YES — REQUIRED.

This sprite sheet is for direct use in a Phaser game scene. All frames must be pixel-perfect, consistently scaled, and immediately usable as game assets. No concept art. No painterly finish. No perspective distortion. No cropped FX. Production sprite sheet only.

---

## Layout Instructions

Sheet dimensions: 640×320px
Grid: 8 columns × 4 rows
Frame size: 80×80px per frame
Total frames: 30
Frame spacing: 0px
Sheet margin: 0px

CRITICAL LAYOUT RULES:
- All frames must be exactly 80×80px — no exceptions.
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
- Crisp, clean silhouettes readable at 40px (half render size)
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
- Each animation region (maze-rush_player_idle, maze-rush_player_move, maze-rush_player_stun, maze-rush_player_checkpoint, maze-rush_player_exit) must be visually distinguishable. Do not make idle and active look nearly identical.

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
- Must be immediately distinguishable from all tile types. Unique silhouette.

---

## Negative Prompt

> EXCLUDE: painterly look, soft brushstrokes, watercolor, oil paint texture, perspective distortion, fisheye, cropped effects outside frame bounds, inconsistent scale between frames, uneven spacing or misaligned grid, complex background scenery, text labels in frames, UI chrome mixed into sprite frames, photo-realistic rendering, muddy colors, low contrast, excessive particle clutter, decorative flourishes on gameplay pieces, blurry silhouettes, transparent holes in opaque backgrounds, background scenery, board grid visible behind pieces

---

## Production Notes

PRODUCTION QUALITY STANDARD: This asset must be ready for immediate use in a premium competitive puzzle game shipping to mobile and desktop.

PHASER INTEGRATION: The sheet will be loaded with `this.load.spritesheet('maze-rush_player_token', '.../maze-rush_player_token.png', { frameWidth: 80, frameHeight: 80 })`. Frame indices must match the animation definitions exactly.

DELIVERY: Single PNG file, 640×320px, transparent background (PNG-24 with alpha). No JPG. No embedded metadata. No color profile embedding.

HANDOFF CHECK:
  ✓ All 30 frames present and correctly ordered
  ✓ Transparent background on all frames
  ✓ No aliasing or rough edges
  ✓ Consistent scale across frames
  ✓ Effects contained within frame bounds
  ✓ Silhouette readable at 40px