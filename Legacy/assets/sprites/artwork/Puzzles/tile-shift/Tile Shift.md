# Tile Shift
**Internal ID:** `tile_shift`  
**Mode Support:** `Both`

---

# 1. Puzzle Identity

## Core Fantasy
Tile Shift is Puzzle Rivals’ **board-manipulation puzzle**. Instead of primarily clearing, routing, or matching what is already available, players actively **reposition entire rows, columns, or tile groups** to force the board into solvable states.

This mode should feel like:
- sliding a complex machine into alignment
- rearranging a locked puzzle box
- making the board obey your will
- forcing opportunity into existence instead of waiting for it
- committing to a structural move that changes everything

The emotional center is:
- “one shift can fix this whole board”
- “if I move this row, I ruin that column”
- “I can build a huge setup if I shift in the right order”
- “this puzzle is about arranging potential, not reacting to luck”

Tile Shift must not feel like a generic match mode with a gimmick slapped on top. The shift mechanic is the identity.

## Cognitive Skill Tested
Primary skill:
- **spatial board manipulation**

Secondary skills:
- pattern construction
- transformation prediction
- order-of-operations planning
- setup recognition
- board compression and expansion awareness

## Why It Exists in Puzzle Rivals
Tile Shift gives the roster a true **manipulation-first puzzle**. It fills a lane that is distinct from:
- Score Attack, which is about spotting value
- Combo Rush, which is about sustaining momentum
- Clear Rush, which is about cleanup
- Link Lock / Circuit Clash / Pipe Pulse, which are routing puzzles
- Mirror Maze, which is an optics sequence puzzle

Tile Shift asks a different question:
- “How do I reorganize the board itself so the right solves become possible?”

That makes it essential for puzzle variety.

## What Makes It Unique
Tile Shift must be clearly different from all nearby modes:
- unlike **Score Attack**, opportunities are often created rather than discovered
- unlike **Combo Rush**, board manipulation matters more than solving cadence
- unlike **Clear Rush**, the board is not simply being cleaned
- unlike **Spatial Spin**, the emphasis is whole-line or structural shifting, not rotational geometry
- unlike **RuleBreaker**, the board rules do not mutate constantly
- unlike **Number Crunch**, the challenge is spatial, not arithmetic

Tile Shift is about **rearranging the grid to manufacture advantage**.

---

# 2. Match Role

## Category
- board manipulation
- setup puzzle
- spatial planning
- transformation race

## Match Style
Recommended defaults:
- BvB setup race
- separate boards
- objective based on score, setups, and major created clears

Best identity:
- **shift-and-solve race**

That means the loop is:
1. shift board structure
2. create better solves
3. cash them in
4. repeat

## Recommended Session Length
Primary recommendation:
- **90 seconds**

Alternative:
- 75 seconds for short casual
- 100 seconds for ranked extended form

Recommended default:
- **90 seconds**

Why:
- enough time for layered manipulation
- enough time for big setup payoffs
- still fast enough for mobile play

---

# 3. Core Objective

## Win Condition
A player wins by:
- earning the highest score by timer end
- or reaching the score cap first if cap mode is enabled

## Lose Condition
A player loses by:
- creating fewer meaningful setups than the opponent
- wasting shifts on low-impact board moves
- repeatedly destroying future opportunities with sloppy reordering

## Primary Goal
Use shifts to create high-value board states, then solve them efficiently.

## Secondary Goals
Advanced players optimize for:
- building multi-solve setups
- preserving strong future shift opportunities
- minimizing wasted shift actions
- aligning multiple scoring lanes at once
- planning two or three structural moves ahead

---

# 4. Board Layout

## Board Shape
- square puzzle grid

## Board Size
Recommended default:
- **7 x 7**

Alternative sizes:
- 6 x 6 for easier onboarding
- 8 x 8 for ranked advanced variants

Recommended shipping size:
- **7 x 7**

## Cell / Tile Count
- 49 active cells

## Board Composition
The board should contain:
- standard colored / family tiles
- optional locked tiles in advanced variants
- optional anchor tiles that resist shifting
- optional dead cells in harder variants
- optional multiplier cells in event variants

## Required Structure Rule
A standard Tile Shift board must include:
- at least **3 meaningful shift opportunities**
- at least **1 tempting low-value shift that damages future setup potential**
- at least **1 route to create a large solve through row/column manipulation**
- at least **1 board state where two-step planning beats immediate cash-out**
- at least **1 choke alignment where multiple rows or columns can be improved in different ways**

This prevents the mode from becoming “slide one row, match three, done.”

## Shift Types
Recommended default mechanic:
- player may shift:
  - one full row left or right
  - one full column up or down

Optional advanced variants:
- wraparound shifts
- limited shift charges
- locked row segments
- anchor cells that do not move

### Recommended default behavior
- **wraparound shifts enabled**

Why:
- much richer strategic manipulation
- more readable and satisfying board transformation
- stronger identity than simple sliding-without-wrap

## Zones
Base board:
- unified playfield
- no explicit special zones required
- optional advanced variants can include:
  - anchor corner
  - locked strip
  - bonus cells
  - dead cell blockers

## Reserved HUD Space
Must preserve room for:
- top center: timer + score + current streak/setup state
- side lane: score comparison or setup meter
- bottom left/right: player cards and usernames
- optional small shift availability indicator

## Mobile Readability Notes
- shift arrows must be obvious
- selected row/column highlight must be clear
- board must not become cluttered by too many guidance overlays
- row and column boundaries must be visually readable
- wraparound destination preview should be clean and brief

---

# 5. Controls and Inputs

## Primary Input
Recommended default:
- tap a row or column selector, then swipe or tap arrow to shift

Alternative:
- direct swipe on row/column edge handles

Best default for mobile:
- **row/column selection + directional swipe/tap**

This gives better control and avoids accidental shifts.

## Secondary Input
- optional quick undo in casual mode only
- optional hold to preview resulting alignment before committing
- optional edge arrow buttons for accessibility

## Input Rules
Players may:
- select a full row or full column
- shift it by one unit per action
- chain multiple shifts quickly
- then perform solve actions on the new board state

## Invalid Input Behavior
If invalid:
- locked row/column or anchored zone rejects
- brief feedback animation
- no shift occurs
- no major punishment besides time loss

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by shifting rows and columns to create valid scoring groups, then clearing those groups.

Recommended base rule:
- match **3 or more adjacent same-color or same-family tiles**
- but those matches are often not present until the player reshapes the board

This is the critical distinction:
- Score Attack = find
- Tile Shift = create

## Solve Sequence
1. player scans current board
2. identifies a promising row or column shift
3. shifts structure
4. re-evaluates resulting board
5. either solves immediately or performs another shift
6. cashes in the created match
7. refill occurs if board clears
8. next setup cycle begins

## Partial Progress Rules
- shifts permanently change the board state
- no partial shift persistence beyond committed action
- created opportunities remain until used or destroyed by further shifts
- setup progress is structural, not combo-timer-based

## Failure Cases
- taking small immediate match instead of stronger future setup
- shifting a row that destroys two strong columns
- creating fragmentation instead of alignment
- over-shifting without cashing in
- wasting actions on cosmetic changes with no meaningful board improvement

## Skill Ceiling Behavior
Top players separate themselves by:
- seeing 2-step and 3-step setups
- identifying whether row or column movement has higher value
- preserving board flexibility
- cashing in at the right moment
- recognizing when a “good” shift is actually inferior to a later one

---

# 7. Scoring Model

## Base Points
Recommended:
- 3-tile clear: **10 points**
- 4-tile clear: **18 points**
- 5-tile clear: **28 points**
- 6+ clear: **40+ scaling**

## Setup Bonus
This mode needs a distinct scoring identity.

Recommended setup bonuses:
- created-from-shift clear bonus: **+5**
- two-shift engineered clear bonus: **+10**
- multi-lane setup bonus: **+12**
- board-state optimization bonus for large created alignment: **+15**

## Bonus Points
- back-to-back engineered solves: **+8**
- one shift that enables two valid groups: **+12**
- full-line clear created by structural shift: **+18**
- no-waste shift streak: **+6**

## Penalties
- no direct penalty for shifting
- optional ranked variant:
  - “wasted shift” threshold after repeated non-productive moves slightly reduces efficiency bonus eligibility
- avoid hard negative score in base mode

## Score Target to Win
Recommended targets:
- 75-second match: **220**
- 90-second match: **300**
- 100-second match: **340**

Recommended default:
- **300 points in 90 seconds**

## Tie Rules
1. more high-value engineered clears wins
2. if tied, fewer wasted shifts wins
3. if tied, largest single created clear wins

## Recommended Score Tuning
Expected 90-second results:
- average player: **160 to 230**
- good player: **230 to 320**
- strong player: **320 to 450+**

---

# 8. Pressure Systems

## Main Pressure Source
- shift efficiency pressure

## Secondary Pressure Source
- timer pressure

The player should feel:
- “every shift must matter”
- “I cannot just wiggle rows around forever”
- “the opponent is cashing in while I’m still setting up”
- “do I take the decent solve now or build something bigger?”

## Escalation Model
Pressure rises through:
- shrinking time window
- score gap visibility
- board states becoming more complex after repeated refill cycles
- increasing emotional cost of wasted manipulation

## Comeback Potential
Comeback is strong if tuned well because:
- one beautiful engineered board state can produce a huge score swing
- opponent can waste time on bad restructuring
- strong planning can reverse a raw speed deficit

---

# 9. Multiplayer Design

## BvB Behavior
- separate boards
- mirrored seed fairness recommended
- players compete on structural board mastery

## Expanded Modes
If adapted later:
- leaderboard race
- separate boards
- minimal sabotage in default spec

## Shared / Separate Boards
- separate boards only

## Interaction Between Players
Default:
- indirect race only
- score and setup quality pressure

Optional advanced variant:
- major engineered solve sends one temporary anchor tile to opponent
- but this should not be part of the default mode

## Spectator Readability
Strong if presented well:
- row/column shifts are easy to see
- big structural solves are satisfying
- setup payoffs are dramatic
- viewers can understand that the board is being “reshaped”

---

# 10. Puzzle Flow

## Match Start
- countdown
- board visible
- several modest shift opportunities available
- no giant instant solves at spawn

## Mid-Match Flow
- scan board
- shift row/column
- reshape opportunity space
- solve created match
- refill
- repeat

## Endgame Flow
Final 20 seconds:
- players lean toward practical rather than greedy setups
- quick, reliable structural payoffs become more valuable than perfect engineering
- score gap pressure becomes intense

## Overtime / Sudden Death
Only for exact tie states:
- next high-value engineered clear wins

---

# 11. Difficulty Progression

## Early Difficulty
- obvious one-step shifts exist
- setup logic readable
- multiple safe options

## Mid Difficulty
- stronger two-step planning required
- false “good-looking” shifts appear
- refill states create more ambiguous boards

## Late Difficulty
- best opportunities require layered thinking
- board may become more fragmented
- player must choose between safe and optimal restructuring

## Scaling Variables
- tile family count
- row/column shift complexity
- anchor tile frequency
- dead cell frequency
- wraparound importance
- refill behavior
- setup bonus strength

---

# 12. Animation Language

## Visual Identity
Tile Shift should feel:
- mechanical
- structural
- smooth
- satisfying
- board-bending
- premium and clean

## Motion Personality
- sliding
- aligning
- snapping
- settling
- reorganizing
- less explosive than Combo Rush
- more transformational than Score Attack

## Required Feedback Types
- idle tile life
- selected row/column highlight
- shift preview
- shift motion
- shift settle snap
- created solve burst
- failed / low-value shift feedback
- setup bonus flare
- score lane update
- win / lose payoff

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `tile_shift_tile_idle.png`
  - purpose: idle tile life
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `tile_shift_row_select.png`
  - purpose: selected row highlight
  - frame count: 6
  - frame size: 640x96
  - layout: 3x2
  - fps: 10
  - loop: short loop while selected

- `tile_shift_column_select.png`
  - purpose: selected column highlight
  - frame count: 6
  - frame size: 96x640
  - layout: 3x2
  - fps: 10
  - loop: short loop while selected

- `tile_shift_shift_motion.png`
  - purpose: row/column move effect
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 14
  - loop: no

- `tile_shift_shift_settle.png`
  - purpose: board settle snap after shift
  - frame count: 6
  - frame size: 256x128
  - layout: 3x2
  - fps: 12
  - loop: no

- `tile_shift_engineered_clear.png`
  - purpose: clear created by successful manipulation
  - frame count: 10
  - frame size: 128x128
  - layout: 5x2
  - fps: 16
  - loop: no

- `tile_shift_setup_bonus.png`
  - purpose: setup reward indicator
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `tile_shift_low_value_feedback.png`
  - purpose: weak or poor shift feedback
  - frame count: 6
  - frame size: 192x96
  - layout: 3x2
  - fps: 12
  - loop: no

- `tile_shift_score_lane.png`
  - purpose: score / setup meter
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `tile_shift_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `tile_shift_endgame_focus.png`
  - purpose: final phase urgency framing
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

- `tile_shift_win_burst.png`
  - purpose: victory flourish
  - frame count: 10
  - frame size: 640x640
  - layout: 5x2
  - fps: 14
  - loop: no

## Tile Atlas
- `tile_shift_tile_atlas.png`
  - purpose: colored/family tiles, anchor tiles, dead cells, multiplier cells, shift handles
  - frame count: 20+
  - frame size: 64x64
  - layout: flexible atlas
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- subtle frame current
- row/column selection shimmer
- smooth board movement trails
- settle snap accents

## Tile FX
- idle glow
- shift movement streak
- alignment snap
- engineered clear burst

## Solve FX
- created clears should feel smarter and more intentional than normal match bursts
- setup bonus effects should emphasize “you built this”

## Failure FX
- weak shift feedback should be clear but not punitive
- invalid anchored shift rejection if applicable
- avoid loud punishment visuals

## Pressure FX
- endgame urgency frame pulse
- high-value setup lane glow
- score deficit pressure if desired

## Scoring FX
- setup bonus pop
- engineered clear score popup
- score lane surge
- large created alignment milestone flare

## Multiplayer FX
- opponent major setup pulse
- lead shift highlight
- big engineered solve alert

---

# 15. UI / HUD Requirements

## Must Display
- timer
- score
- setup bonus state or indicator
- optional streak
- player card
- username
- selected row/column clarity
- opponent score comparison

## HUD Layout Rules
- top center: timer + score + setup state
- side lane: score or setup lane
- bottom: player cards
- selected row/column overlay must not obscure actual tile identities

## Mobile Priority Rules
Always visible:
- timer
- score
- board
- shift controls
- player identity

Can be reduced:
- long bonus labels
- secondary flavor UI
- extended explanation text

---

# 16. Audio Direction

## Music Feel
- slick
- structural
- energetic
- controlled
- premium arcade puzzle energy

## SFX Categories
- row select
- column select
- shift move
- settle snap
- engineered clear
- setup bonus
- weak shift
- score gain
- endgame urgency
- win / lose

## Sonic Identity
Tile Shift should sound:
- mechanical but satisfying
- less panicked than Combo Rush
- less machine-systems-heavy than Pipe Pulse
- more “board transformation” than “explosion reward”

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Score Attack
- Clear Rush
- Spatial Spin

## How It Is Different
Compared to Score Attack:
- opportunities are created through shifts, not mainly discovered in place

Compared to Clear Rush:
- board reorganization matters more than simple removal

Compared to Spatial Spin:
- Tile Shift is row/column structural manipulation, not object rotation as the primary mechanic

## What Must Never Happen
- must not become a normal match mode with occasional row wiggle
- must not make shifting feel cosmetic
- must not reward random shifting
- must not make every solve instantly available without structural setup
- must not overcomplicate the board with too many special tiles in the base version

---

# 18. Technical Implementation Notes

## Phaser Notes
- row and column shift animation must be buttery smooth
- wraparound motion must be visually clear
- selection highlighting must be distinct and lightweight
- engineered clear detection should know when a match was created by recent shift actions
- touch input must avoid accidental row/column misselection

## Data Model Notes
Per player:
- score
- setup bonus count
- engineered clear count
- wasted shifts
- timer state
- largest engineered solve

Per board:
- tile grid
- row/column state
- anchor tile positions
- dead cell positions
- shift history for bonus detection

## Animation Hooks
Trigger on:
- row select
- column select
- shift begin
- shift settle
- engineered clear
- setup bonus
- weak shift
- score lane surge
- endgame urgency
- win / loss

## Performance Notes
- moving entire rows/columns should be optimized
- avoid expensive redraw logic when shifting
- use pooled shift trail effects
- keep overlays subtle to preserve tile readability

---

# 19. Telemetry / Balancing Metrics

Track:
- average engineered clear size
- shifts per match
- wasted shift count
- score per minute
- average time between shift and solve
- setup bonus frequency
- comeback win rate
- win rate by first large engineered solve
- abandon rate on harder boards

---

# 20. Final Design Summary

## One-Paragraph Summary
Tile Shift is Puzzle Rivals’ board-manipulation mode. Players shift full rows and columns on a 7x7 grid to manufacture high-value matches that do not naturally exist on the board. Success depends on spatial planning, setup recognition, and knowing when to cash in a created board state versus continue building a stronger one. The mode should feel smooth, structural, and highly intentional, rewarding players who can reshape the board into opportunity faster than their opponent.

## Production Priority
- **High**

## Dependencies
- tile atlas
- row/column selection and shift FX
- engineered clear and setup bonus FX
- score/setup lane assets
- player card shimmer
- fairness-seeded manipulation board generation