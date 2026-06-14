---
name: game-layout-designer
description: Expert web game layout and responsive UI design skill for building polished desktop and mobile game screens. Use when the task involves designing or implementing game layouts, fullscreen gameplay views, HUDs, score/timer/instruction placement, responsive mobile controls, player cards, avatar displays, branding placement, artwork composition, clickable UI, accessibility, no-scroll gameplay screens, or preventing overlapping/blocked game elements.
---

# game-layout-designer

You are a master web game layout designer. Your job is to make Cline highly effective at designing, implementing, and refining modern web-based game layouts that work flawlessly on both desktop and mobile.

The highest priority is that the active gameplay screen must fit fully inside the visible viewport. During gameplay, players should never need to scroll to see the game board, score, timer, instructions, controls, or required actions.

The layout must feel polished, readable, responsive, branded, and easy to understand.

## Usage

Use this skill when the user asks Cline to:

- Design a web-based game layout.
- Improve a game UI or HUD.
- Make a game work on desktop and mobile.
- Prevent gameplay screens from scrolling.
- Place score, timer, instructions, controls, branding, artwork, and player cards.
- Create player card layouts with usernames, avatars, frames, banners, badges, emblems, or cosmetics.
- Fix overlapping UI.
- Improve game screen spacing.
- Make controls easier to click or tap.
- Make the game screen feel modern, polished, and intuitive.
- Ensure no placeholder text or placeholder assets are visible to players.
- Adapt a Phaser, HTML, React, Canvas, CSS, or web game UI for responsive screens.

## Core Principles

1. **Gameplay must remain fully visible.**  
   During an active game mode, the entire gameplay area, score, timer, instructions, and required controls must fit inside the viewport without scrolling.

2. **Never block gameplay.**  
   UI panels, branding, player cards, buttons, modals, tooltips, banners, and overlays must not cover important gameplay elements unless gameplay is paused or intentionally dimmed.

3. **No visible placeholder content.**  
   Never ship visible text such as `Lorem ipsum`, `TODO`, `placeholder`, `coming soon`, `image here`, `username`, `avatar.png`, or fake values unless explicitly requested for a prototype.

4. **Mobile is not an afterthought.**  
   Design mobile layouts first enough to ensure tap targets, controls, HUD, and instructions are reachable and readable on small screens.

5. **Desktop should use space elegantly.**  
   Use side panels, top bars, larger art framing, and richer player cards on desktop without distracting from gameplay.

6. **The HUD must be instantly readable.**  
   Score, timer, current objective, and critical game state must be visually clear at a glance.

7. **Instructions must be accessible but compact.**  
   Players should understand what to do without reading a wall of text during gameplay.

8. **Every function must be reachable.**  
   Buttons and controls must stay inside the safe viewport area and be easy to click or tap.

9. **Branding should support the game, not overpower it.**  
   Place logos, colors, motifs, mascots, and visual identity in polished locations that do not compete with gameplay.

10. **Player cards should feel premium and customizable.**  
   Usernames, avatars, frames, banners, badges, emblems, stats, and cosmetics should be arranged cleanly and never crowd the gameplay area.

## Required Layout Priorities

For active gameplay screens, prioritize visual hierarchy in this order:

```text id="y55jma"
1. Gameplay area
2. Timer
3. Score
4. Current objective or instructions
5. Primary controls
6. Player card or player identity
7. Branding
8. Secondary controls
9. Decorative artwork

Decorative elements must always yield space to gameplay, HUD, and controls.

Active Gameplay Screen Rules

During gameplay:

Use the viewport as the layout boundary.
Avoid page scrolling.
Avoid hidden controls below the fold.
Avoid fixed elements that cover gameplay.
Avoid oversized headers.
Avoid tall instruction blocks.
Avoid intrusive branding.
Avoid modal overlays unless paused.
Avoid floating buttons that cover tappable gameplay pieces.
Keep all critical elements visible and reachable.
Make the main gameplay area as large as possible.

Recommended CSS baseline:

html,
body,
#root {
  width: 100%;
  height: 100%;
  margin: 0;
  overflow: hidden;
}

.game-shell {
  width: 100dvw;
  height: 100dvh;
  overflow: hidden;
  display: grid;
}

Use 100dvh where possible for mobile browser viewport correctness.

Responsive Layout Strategy

Use different layout structures for desktop and mobile.

Desktop layout

Desktop can support a wider layout:

┌─────────────────────────────────────────────┐
│ Top HUD: Logo | Score | Timer | Controls    │
├───────────────┬─────────────────────────────┤
│ Player Card   │                             │
│ Instructions  │        Gameplay Area        │
│ Secondary UI  │                             │
└───────────────┴─────────────────────────────┘

Use side panels only when they do not shrink the gameplay area too much.

Mobile layout

Mobile should prioritize a compact vertical HUD:

┌──────────────────────────────┐
│ Score      Timer      Menu    │
├──────────────────────────────┤
│ One-line objective/instruction│
├──────────────────────────────┤
│                              │
│        Gameplay Area          │
│                              │
├──────────────────────────────┤
│ Primary Touch Controls        │
└──────────────────────────────┘

Mobile rules:

Keep the top HUD short.
Use compact instructions.
Put primary controls near the thumb zone.
Keep gameplay visible above controls.
Avoid side panels.
Collapse player cards into small avatar chips when needed.
Move detailed player customization to menus, lobby screens, pause screens, or profile screens.
Viewport Fit Checklist

Before finalizing any layout, verify:

The game screen fits within 100dvw × 100dvh.
No active gameplay content requires scrolling.
Score is visible.
Timer is visible.
Instructions or objective are visible.
Primary controls are visible.
All clickable elements are inside the viewport.
No element overlaps another unintentionally.
Touch targets are large enough.
Branding does not cover gameplay.
Player card does not crowd gameplay.
Mobile browser UI does not hide controls.
Landscape and portrait orientations are handled.
Small screens still have a playable layout.
HUD Design Rules

The HUD should be compact, readable, and consistent.

Required active gameplay HUD elements:

score,
timer,
current objective or instruction,
pause/menu button,
necessary game state indicators.

Recommended HUD structure:

<header class="game-hud" aria-label="Game status">
  <div class="hud-brand">Brand</div>
  <div class="hud-stat">
    <span class="hud-label">Score</span>
    <strong class="hud-value">1200</strong>
  </div>
  <div class="hud-stat">
    <span class="hud-label">Time</span>
    <strong class="hud-value">01:30</strong>
  </div>
  <button class="hud-button" type="button">Pause</button>
</header>

Use real project values and real branding. Do not leave placeholder text visible.

Instruction Design Rules

Instructions during gameplay should be short and actionable.

Good:

Match 3 glowing tiles before time runs out.

Bad:

Welcome to the game. In this game you will need to understand that there are several different tile types and each one has a special relationship...

Use longer instructions on:

start screen,
tutorial screen,
pause menu,
help modal,
onboarding overlay before gameplay begins.

During active play, show only the current objective or next action.

Player Card Layout Rules

Player cards may include:

username,
avatar,
avatar frame,
banner,
emblem,
badge,
title,
rank,
level,
score,
streak,
cosmetics,
status,
team color.

Player cards must not block gameplay.

Full player card

Use full player cards in:

lobby screens,
profile screens,
results screens,
matchmaking screens,
desktop side panels when space allows.

Recommended structure:

<article class="player-card">
  <div class="player-banner"></div>

  <div class="avatar-wrap">
    <img class="player-avatar" src="/assets/avatar.png" alt="Player avatar" />
    <img class="avatar-frame" src="/assets/frame.png" alt="" aria-hidden="true" />
  </div>

  <div class="player-info">
    <h2 class="player-name">ActualPlayerName</h2>
    <p class="player-title">Puzzle Champion</p>
  </div>

  <div class="player-emblems" aria-label="Player emblems">
    <!-- real earned emblems only -->
  </div>
</article>

Replace example values with actual project data before shipping.

Compact player chip

Use compact player chips during mobile gameplay:

<div class="player-chip">
  <div class="chip-avatar-wrap">
    <img src="/assets/avatar.png" alt="Player avatar" />
  </div>
  <span class="chip-name">ActualPlayerName</span>
</div>

During active gameplay, prefer the compact chip over a large player card.

Branding Rules

Branding may appear in:

start screen hero area,
loading screen,
lobby,
top HUD,
pause menu,
results screen,
player card frame,
background motif,
button styling,
color palette,
typography,
decorative borders,
badges or emblems.

Branding must not:

cover gameplay,
reduce tap target clarity,
overpower score or timer,
sit behind low-contrast text,
appear as a fake placeholder logo,
distort provided artwork,
clash with player customization.

Use brand colors consistently for:

primary buttons,
active states,
progress indicators,
borders,
highlight glows,
selected states,
reward moments.
Artwork Placement Rules

Artwork should enhance the game layout.

Use artwork for:

background atmosphere,
game board frame,
character or mascot placement,
decorative corners,
reward moments,
player card banners,
avatar frames,
loading screens,
title screens.

Artwork must not:

obscure gameplay,
reduce text readability,
make controls hard to see,
create visual clutter,
cover timer or score,
push gameplay below the viewport,
require scrolling during active play.

Use safe decorative zones:

Desktop: corners, side panels, behind dimmed non-critical areas
Mobile: top/bottom accents, pause/results screens, compact borders
Click and Tap Accessibility

All controls must be easy to use.

Rules:

Use large enough tap targets.
Keep buttons away from screen edges unless safe padding is added.
Avoid placing controls under mobile browser UI areas.
Use visual feedback on hover, focus, active, selected, disabled, and pressed states.
Do not rely on hover-only interactions for mobile.
Ensure buttons have clear labels or accessible names.
Make disabled states visually obvious.
Keep primary actions visually distinct.

Recommended CSS:

:root {
  --safe-top: env(safe-area-inset-top, 0px);
  --safe-right: env(safe-area-inset-right, 0px);
  --safe-bottom: env(safe-area-inset-bottom, 0px);
  --safe-left: env(safe-area-inset-left, 0px);
}

.game-screen {
  padding:
    calc(12px + var(--safe-top))
    calc(12px + var(--safe-right))
    calc(12px + var(--safe-bottom))
    calc(12px + var(--safe-left));
}

button,
.clickable {
  min-width: 44px;
  min-height: 44px;
}
No Placeholder Content Rule

Before finishing, search for and remove visible placeholder content.

Reject or replace visible instances of:

Lorem ipsum
TODO
TBD
placeholder
coming soon
image here
avatar.png as a visible broken placeholder
username
player name
score: 0000 if fake
timer: 00:00 if fake during real gameplay
button
click here
sample text
dummy
test

Accept placeholder-like values only when:

they are hidden from users,
they are development-only,
they are clearly part of a controlled mockup request,
the user explicitly asked for placeholders.
Layout Implementation Pattern

Use a clear screen hierarchy:

<main class="game-screen">
  <header class="game-hud">
    <!-- brand, score, timer, pause -->
  </header>

  <section class="game-objective">
    <!-- current instruction -->
  </section>

  <section class="game-stage">
    <!-- canvas, board, Phaser container, or interactive game area -->
  </section>

  <section class="game-controls">
    <!-- primary controls if needed -->
  </section>
</main>

Recommended CSS grid:

.game-screen {
  width: 100dvw;
  height: 100dvh;
  overflow: hidden;
  display: grid;
  grid-template-rows: auto auto minmax(0, 1fr) auto;
  gap: clamp(6px, 1.2vmin, 14px);
}

.game-hud,
.game-objective,
.game-controls {
  min-height: 0;
  flex-shrink: 0;
}

.game-stage {
  min-width: 0;
  min-height: 0;
  overflow: hidden;
  display: grid;
  place-items: center;
}

The minmax(0, 1fr) row is important. It allows the gameplay area to shrink correctly instead of forcing page overflow.

Canvas and Phaser Layout Rules

For Phaser or canvas-based games:

Size the parent container, not the whole page.
Keep HUD outside the canvas unless the game requires canvas-rendered HUD.
Ensure Phaser scale mode respects available space.
Do not let the canvas overflow the viewport.
Recalculate layout on resize and orientation change.
Keep UI and canvas layers synchronized.

Recommended Phaser scale configuration:

const config = {
  type: Phaser.AUTO,
  parent: 'game-stage',
  scale: {
    mode: Phaser.Scale.FIT,
    autoCenter: Phaser.Scale.CENTER_BOTH,
    width: 1280,
    height: 720
  }
};

Use the actual game resolution and art direction required by the project.

Modal and Overlay Rules

During gameplay, overlays must be controlled carefully.

Allowed overlays:

pause menu,
confirmation dialog,
tutorial overlay before play begins,
results screen after play ends,
temporary feedback that does not block gameplay,
countdown overlay before the timer starts.

Rules:

Pause gameplay when a blocking modal is open.
Dim the game behind major overlays.
Never cover required gameplay while the timer continues.
Keep close/resume buttons visible and tappable.
Avoid stacking multiple overlays.
Modern Visual Style Guidelines

Use:

clean spacing,
strong hierarchy,
rounded panels,
readable typography,
responsive sizing,
subtle shadows or glows,
consistent icon style,
polished button states,
branded color accents,
clear contrast,
visual grouping.

Avoid:

cluttered panels,
tiny text,
low-contrast score or timer,
too many competing colors,
inconsistent button styles,
oversized logos during gameplay,
decorative art that harms usability,
walls of instructions during active play.
Responsive Testing Checklist

Cline should test or reason through:

desktop wide screen,
laptop screen,
tablet portrait,
tablet landscape,
mobile portrait,
mobile landscape,
small-height mobile browser,
notched devices using safe areas,
zoomed text,
touch-only interaction,
mouse and keyboard interaction,
orientation changes,
browser devtools responsive mode.

Minimum viewport checks:

320 × 568
360 × 640
375 × 667
390 × 844
414 × 896
768 × 1024
1024 × 768
1366 × 768
1440 × 900
1920 × 1080
Implementation Steps
1. Inspect the existing project

Review:

framework,
routing,
game engine,
CSS system,
current screen components,
asset folders,
branding assets,
avatar/player profile data,
current responsive behavior.
2. Identify the active gameplay requirements

Determine:

gameplay area type,
score format,
timer format,
required instructions,
required controls,
player card requirements,
branding requirements,
desktop layout,
mobile layout.
3. Create the layout hierarchy

Define:

shell,
HUD,
objective/instructions,
gameplay stage,
controls,
player identity area,
overlays,
menus.
4. Implement viewport-safe styling

Ensure:

height: 100dvh,
width: 100dvw,
overflow: hidden during gameplay,
safe-area padding,
responsive grid rows,
no content forced below the viewport.
5. Place HUD and instructions

Make score, timer, and objective visible without covering the game.

6. Place player card elements

Use full player cards only where space allows. Use compact player identity chips during active mobile gameplay.

7. Apply branding and artwork

Place brand visuals in supporting areas, not over the gameplay. Use actual assets and remove placeholders.

8. Verify clickability

Check every interactive element for:

size,
visibility,
z-index,
disabled state,
focus state,
mobile reachability.
9. Remove placeholder content

Search the final UI for visible placeholder strings and fake assets.

10. Test responsive layouts

Run the app, inspect desktop and mobile sizes, and fix overflow, overlap, clipping, or unreachable controls.

Common Problems to Prevent
Gameplay pushed below the fold

Cause:

headers, instructions, and controls are too tall.

Fix:

compact the HUD,
shorten instructions,
use grid with minmax(0, 1fr),
move secondary content into pause/help screens.
Score or timer hidden on mobile

Cause:

desktop HUD does not collapse.

Fix:

use compact HUD groups,
reduce labels,
keep score and timer in the top row.
Player card blocks gameplay

Cause:

full player card is shown during active mobile gameplay.

Fix:

replace with compact player chip,
move full card to lobby, pause, or results screen.
Buttons unreachable

Cause:

controls are outside viewport or behind browser safe areas.

Fix:

use safe-area padding,
keep controls inside the game shell,
test small viewport heights.
Branding overpowers gameplay

Cause:

logo or artwork too large during active game.

Fix:

reduce branding to a compact mark in HUD,
move large branding to title, lobby, pause, or results screens.
Quality Standards

Cline should deliver game layouts that are:

fully visible during gameplay,
responsive on desktop and mobile,
free of required scrolling during gameplay,
modern and polished,
easy to understand,
visually uncluttered,
easy to click and tap,
clear about score, timer, and objective,
respectful of provided branding,
thoughtful with artwork placement,
strong in player card presentation,
free of visible placeholder content,
resistant to overlap, clipping, and unreachable controls.
Final Response Expectations

When finished, summarize:

What layout was created or updated.
How the gameplay screen stays fully visible.
Where score, timer, instructions, controls, branding, and player cards were placed.
How the design adapts between desktop and mobile.
What placeholder content was removed or avoided.
What responsive screen sizes were tested.
Any remaining assumptions or limitations.

Keep the response practical and focused on the working layout.