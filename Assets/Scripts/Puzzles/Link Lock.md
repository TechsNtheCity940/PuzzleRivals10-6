# Link Lock
**Internal ID:** `link_lock`  
**Mode Support:** `Both`

---

# 1. Puzzle Identity

## Core Fantasy
Link Lock is Puzzle Rivals’ **constraint-connection puzzle**. Players must connect specific nodes under strict linking rules, limited space, and evolving lock conditions. It is not about navigating a maze, not about keeping flow alive under pressure, and not about broad score farming. It is about building the **correct connection pattern** under tight structural rules.

This mode should feel like:
- threading exact links through a constrained board
- solving a knot without crossing lines
- unlocking paired systems in the right order
- creating clean, elegant node networks
- seeing the whole board as one connected logic diagram

The emotional center is:
- “these pairs must connect cleanly”
- “if I route this here, I block the rest”
- “I need to think ahead before committing”
- “one sloppy link ruins the board”

## Cognitive Skill Tested
Primary skill:
- **constraint-based connection planning**

Secondary skills:
- spatial foresight
- route reservation
- adjacency planning
- order-of-operations reasoning
- board compression awareness

## Why It Exists in Puzzle Rivals
Link Lock gives the roster a true **network-constraint puzzle**. It sits in a different lane from:
- Maze Rush, which is about traversing through space
- Pipe Pulse, which is about live flow stabilization
- Circuit Clash, which is about logic sequencing and powered systems

Link Lock is about:
- connecting the right endpoints
- not crossing or blocking future connections
- using limited board space efficiently

It is one of the cleanest “think before you commit” modes in the lineup.

## What Makes It Unique
Link Lock must be different from nearby modes:
- unlike **Maze Rush**, the player is not a traveler moving through corridors
- unlike **Pipe Pulse**, there is no live pressure flow system
- unlike **Circuit Clash**, there are no electrical objective chains or relays as the primary identity
- unlike **Tile Shift**, the puzzle is not about rearranging the whole board
- unlike **Score Attack / Combo Rush / Clear Rush**, it is not about clearing clusters
- unlike **Mirror Maze**, the challenge is not reflection logic
- unlike **Spatial Spin**, rotation is not the main feature

Link Lock is specifically about **reserved-space route planning between locked node pairs**.

---

# 2. Match Role

## Category
- node connection
- spatial planning
- constraint logic
- board reservation puzzle

## Match Style
Recommended defaults:
- BvB exact-link duel
- 4-player ranked pattern-completion race if expanded later

Best identity:
- **objective-based pair-link race**

## Recommended Session Length

### BvB
- **85 seconds**

### Optional expanded ranked variant
- **95 seconds**

Recommended default:
- **85 seconds**

Why:
- enough time for real planning
- short enough to maintain pressure
- keeps the mode cerebral without becoming sluggish

---

# 3. Core Objective

## Win Condition
A player wins by:
- completing more valid locked link objectives than the opponent
- or finishing the full board objective first

## Lose Condition
A player loses by:
- completing fewer valid connections
- blocking themselves into unsalvageable layouts
- failing to route key pairs in time
- wasting too many actions on inefficient pathing

## Primary Goal
Connect required node pairs using valid link paths without violating board constraints.

## Secondary Goals
Advanced players optimize for:
- shortest clean links
- correct order of pair completion
- preserving future route space
- solving the board without self-blocking
- recognizing impossible configurations early and recovering

---

# 4. Board Layout

## Board Shape
- square node grid with connection cells between anchor points

## Board Size
Recommended default:
- **8 x 8**

Alternative sizes:
- 7 x 7 for easier casual format
- 9 x 9 for elite/event variants

Recommended shipping size:
- **8 x 8**

## Cell / Tile Count
- 64 cells total
- mixture of:
  - anchor nodes
  - open route cells
  - blocked cells
  - lock cells
  - optional gate cells in advanced variants

## Required Structure Rule
A standard Link Lock board must include:
- at least **3 required node pairs**
- at least **1 pair that tempts the shortest path but blocks future solutions**
- at least **1 pair that requires routing around a reserved corridor**
- at least **1 meaningful order-of-operations choice**
- at least **1 partial board choke point**
- at least **1 clean full-board solution that rewards foresight**

This prevents the mode from becoming “draw three lines and done.”

## Node Types
- **Locked Pair Nodes**
- **Neutral Route Cells**
- **Blocked Cells**
- **Reserved Corridor Cells** in some variants
- **Priority Link Nodes** in advanced variants
- optional **One-Way Link Cells** in ranked variants only

## Zones
- Upper cluster
- lower cluster
- central choke routes
- side bypass corridors
- optional reserved gate areas

These do not need explicit visual borders, but board generation should create meaningful structural regions.

## Reserved HUD Space
Must preserve room for:
- top center: timer + current pair objective count
- side lane: progress / score meter
- bottom left/right: player cards and usernames
- optional small “remaining pairs” strip

## Mobile Readability Notes
- node pair colors/shapes must be unmistakable
- links must be thick enough to read on phone
- completed vs incomplete vs invalid links must be visually distinct
- route cells must not look cluttered
- overlapping path ambiguity must be impossible

---

# 5. Controls and Inputs

## Primary Input
- drag from one node to trace a path to its matching node

## Secondary Input
- lift finger to commit
- optional tap on completed link to inspect
- optional undo button for accessibility / casual modes only

## Input Rules
Players may:
- start from a valid unlocked or active node
- trace through open cells
- connect only to the matching pair node
- cannot cross existing completed links unless rules explicitly allow bridges in special variants
- cannot pass through blocked cells
- must obey any reserved corridor restrictions

## Invalid Input Behavior
If invalid:
- path line glows red briefly
- trace snaps back or disappears
- no completion awarded
- board remains unchanged

The punishment should be clarity and time loss, not visual chaos.

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by drawing valid, non-overlapping paths between required matching node pairs.

Recommended base rule:
- all required pairs must be connected
- paths cannot overlap
- paths cannot pass through blocked cells
- some pairs should be solved in a smarter order than others

## Solve Sequence
1. player scans the node layout
2. identifies likely safe pair order
3. begins tracing the first path
4. commits a valid connection
5. board space updates visually
6. player routes remaining pairs around the new reserved path
7. objective completes when all required pairs are linked validly

## Partial Progress Rules
- completed valid links persist
- incomplete traces do not persist
- a badly chosen early completed link may make later pairs harder or impossible

This is central to the puzzle’s identity.

## Failure Cases
- taking the obvious shortest path too early and blocking later links
- routing one pair through a critical corridor needed for two future pairs
- creating isolated regions
- failing to recognize choke points
- solving pairs in a bad order

## Skill Ceiling Behavior
Experts separate themselves by:
- reading full-board route pressure before drawing
- spotting trap-shortcuts
- solving in the correct pair order
- minimizing wasted motion
- recognizing impossible board states early

---

# 7. Scoring Model

## Base Points
Recommended:
- first valid pair linked: **20 points**
- second valid pair linked: **25 points**
- third valid pair linked: **30 points**
- full board completion bonus: **60 points**

This creates momentum toward full solve without making partial solves worthless.

## Bonus Points
- shortest valid path bonus: **+5**
- no-invalid-trace bonus: **+10**
- perfect solve order bonus: **+12**
- full-board completion with zero blocked-future mistakes: **+20**
- fast complete board solve: **+15**

## Penalties
- invalid trace: no score, light time loss
- optional ranked penalty: if player commits a link then must hard-reset board, lose **5 to 10 points equivalent**
- no large punishment spam in the default mode

## Score Target to Win
Recommended BvB target:
- **220 to 280**

Recommended default:
- **250**

This assumes one or more full pair-board objectives plus efficiency bonuses.

## Tie Rules
1. more full pair sets completed wins
2. if tied, fewer invalid traces wins
3. if tied, faster final pair completion wins

## Recommended Score Tuning
Expected 85-second outcomes:
- average player: several partial board solves, maybe one full
- good player: one full solve plus strong second-board progress
- strong player: repeated clean full-board completions with high efficiency

---

# 8. Pressure Systems

## Main Pressure Source
- self-blocking route pressure

## Secondary Pressure Source
- timer pressure

This is the real stress:
- “if I draw this now, do I ruin the board later?”
- “I need to commit, but I need the right commit”
- “the opponent is finishing pairs while I’m second-guessing”

## Escalation Model
Pressure rises through:
- remaining route space shrinking
- fewer clean corridors left
- more expensive consequences for wrong commitments
- endgame clock pressure on unresolved final pairs

## Comeback Potential
Comeback is moderate:
- a trailing player can recover with a cleaner full-board solve
- an opponent can self-block and waste major time
- elegant route order can reverse momentum

The mode should feel skill-driven, not swingy for cheap reasons.

---

# 9. Multiplayer Design

## BvB Behavior
- separate boards
- mirrored or fairness-seeded node layouts
- same number of pair objectives
- score + completion pace determine winner

## Expanded 4-Player Ranked Behavior
If expanded later:
- separate boards
- live completion leaderboard
- same difficulty class boards
- no direct sabotage by default

## Shared / Separate Boards
- separate boards only

Shared-board Link Lock would become unreadable and frustrating fast. Humans do love forcing bad shared-board ideas into elegant puzzles, but we do not have to join them.

## Interaction Between Players
Default:
- indirect race only
- visible opponent pair completion pace
- milestone pulses for major completions

Optional advanced interaction:
- completing full board may add one blocker to opponent preview queue
- only if rigorously balanced, and not in base mode

## Spectator Readability
Strong if UI is clear:
- pairs are visible
- completed links are easy to understand
- self-blocking is dramatic
- full board completion is satisfying to watch

---

# 10. Puzzle Flow

## Match Start
- countdown
- node pairs visible immediately
- player begins route planning before first draw

## Mid-Match Flow
- connect one pair
- reserve board space
- route next pair
- preserve future corridors
- complete full objective
- next board objective appears

## Endgame Flow
Final 20 seconds:
- unresolved pair pressure spikes
- players may choose safer but longer paths instead of elegant shortest routes
- full-board completion becomes the decisive swing

## Overtime / Sudden Death
Only for exact tie states:
- next full pair set completion wins
- if impossible in time, next valid pair completion wins

---

# 11. Difficulty Progression

## Early Difficulty
- clearer route lanes
- simpler pair relationships
- fewer choke points

## Mid Difficulty
- more pair interdependence
- more false-shortcut temptation
- higher chance of self-blocking

## Late Difficulty
- tighter corridor allocation
- more order-sensitive solving
- more punishing choke points
- fewer forgiving bypass routes

## Scaling Variables
- number of pairs
- blocked cell density
- choke point density
- route space openness
- trap shortcut frequency
- order sensitivity
- reserved corridor usage

---

# 12. Animation Language

## Visual Identity
Link Lock should feel:
- elegant
- precise
- clean
- connected
- tactically constrained

## Motion Personality
- tracing
- locking
- snapping
- pulsing
- tightening
- not explosive, not frantic

## Required Feedback Types
- node idle pulse
- active trace line
- valid lock completion
- invalid trace reject
- pair completion
- full-board solve flourish
- progress lane update
- endgame urgency
- win / lose resolution

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `link_lock_node_idle.png`
  - purpose: idle node life
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `link_lock_trace_line.png`
  - purpose: live path trace segment
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 12
  - loop: yes while tracing

- `link_lock_trace_corner.png`
  - purpose: corner turn path trace
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 12
  - loop: yes while tracing

- `link_lock_invalid_trace.png`
  - purpose: invalid path rejection
  - frame count: 6
  - frame size: 128x128
  - layout: 3x2
  - fps: 14
  - loop: no

- `link_lock_pair_complete.png`
  - purpose: single pair completion
  - frame count: 8
  - frame size: 160x160
  - layout: 4x2
  - fps: 12
  - loop: no

- `link_lock_lock_seal.png`
  - purpose: completed path “sealed” into board
  - frame count: 6
  - frame size: 160x160
  - layout: 3x2
  - fps: 12
  - loop: no

- `link_lock_full_board_complete.png`
  - purpose: full objective completion flourish
  - frame count: 10
  - frame size: 320x160
  - layout: 5x2
  - fps: 14
  - loop: no

- `link_lock_progress_lane.png`
  - purpose: score/progress lane
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `link_lock_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `link_lock_endgame_tension.png`
  - purpose: endgame urgency framing
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

## Node Atlas
- `link_lock_node_atlas.png`
  - purpose: pair nodes, blockers, reserved corridors, neutral route cells, objective markers
  - frame count: 20+
  - frame size: 64x64
  - layout: flexible atlas
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- subtle board grid life
- reserved corridor glow
- completed path seal effect
- progress lane surges

## Node FX
- pair idle pulse
- active node emphasis
- pair completion burst
- invalid route reject flicker

## Solve FX
- valid path completion should feel satisfying and exact
- full-board completion should feel elegant and clean, not explosive chaos

## Failure FX
- invalid path glow
- blocked-corridor reject
- no giant punishment visuals

## Pressure FX
- shrinking-space tension via edge accents
- endgame framing pulse
- unresolved-pair urgency in final moments

## Scoring FX
- pair completion pop
- score lane pulse
- full solve milestone flash

## Multiplayer FX
- opponent full-board completion pulse
- lead shift highlight
- progress differential cue

---

# 15. UI / HUD Requirements

## Must Display
- timer
- score
- completed pairs
- remaining pairs
- player card
- username
- opponent completion state
- optional efficiency cue

## HUD Layout Rules
- top center: timer + remaining pair count
- side lane: score / progress
- bottom: player cards
- board center must remain open for path readability

## Mobile Priority Rules
Always visible:
- timer
- score
- pair progress
- board nodes
- player identity

Can be minimized:
- long status text
- verbose efficiency labels
- decorative extras

---

# 16. Audio Direction

## Music Feel
- focused
- measured
- tactical
- low-chaos
- premium and sharp

## SFX Categories
- path trace
- invalid route
- pair completion
- full-board completion
- score gain
- endgame urgency
- win / lose

## Sonic Identity
Link Lock should sound:
- elegant
- geometric
- deliberate
- more restrained than Combo Rush or Pipe Pulse
- more “smart lock puzzle” than “electrical drama”

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Circuit Clash
- Maze Rush
- Spatial Spin

## How It Is Different
Compared to Circuit Clash:
- Link Lock is about pair connection constraints, not powered logic sequencing

Compared to Maze Rush:
- Link Lock is about constructing connection paths between anchors, not traversing the board as a moving agent

Compared to Spatial Spin:
- rotation is not the identity; route reservation is

## What Must Never Happen
- must not become a wire version of Circuit Clash
- must not become trivial one-stroke line drawing
- must not allow cluttered overlapping route readability
- must not overcomplicate with too many gimmick tile rules in the base mode
- must not make order-of-operations irrelevant

---

# 18. Technical Implementation Notes

## Phaser Notes
- path trace must render smoothly
- completed links should remain clearly visible without overwhelming the board
- invalid traces must resolve instantly
- sealed paths should be stateful and readable
- touch input must be forgiving but accurate on mobile

## Data Model Notes
Per player:
- score
- completed pairs
- remaining pairs
- invalid trace count
- full-board completion count
- solve time
- efficiency rating

Per board:
- pair node definitions
- blocked cells
- reserved corridor states
- completed link states
- objective completion state

## Animation Hooks
Trigger on:
- trace start
- trace turn
- invalid trace
- pair completion
- seal path
- full board completion
- progress lane surge
- endgame urgency
- win / loss

## Performance Notes
- keep path rendering lightweight
- use pooled path visual segments where possible
- ensure completed links remain crisp on mobile
- avoid excess particle clutter

---

# 19. Telemetry / Balancing Metrics

Track:
- average pairs completed
- full-board completion rate
- invalid trace rate
- average solve time per pair
- self-block rate
- comeback rate
- win rate by first pair lead
- average score
- abandon rate on hard layouts

---

# 20. Final Design Summary

## One-Paragraph Summary
Link Lock is Puzzle Rivals’ constraint-connection puzzle mode. Players connect matching node pairs on an 8x8 board using clean, non-overlapping paths while preserving space for future links. The challenge comes from route reservation, order of operations, and avoiding self-blocking mistakes. Unlike Circuit Clash, it is not about electrical logic; unlike Maze Rush, it is not about traversal. It is a precise, elegant spatial planning duel built around clean connectivity under competitive time pressure.

## Production Priority
- **High**

## Dependencies
- node atlas
- trace line / corner FX
- pair completion and seal FX
- progress lane assets
- player card shimmer
- fairness-seeded connection board generator