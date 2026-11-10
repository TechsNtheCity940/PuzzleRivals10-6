# Maze Rush
**Internal ID:** `maze_rush`  
**Mode Support:** `Both`  
**Supported Formats:**  
- **BvB Tactical Maze Duel**
- **4 Player Ranked Memory Maze FFA**

---

# 1. Puzzle Identity

## Core Fantasy
Maze Rush is Puzzle Rivals’ **large-scale navigation mode**. Players must route through a substantial maze, hit sequential checkpoints, and ultimately reach the exit while under time pressure.

Maze Rush is not a tiny route toy. It must feel like:
- real navigation
- route commitment
- recovery from wrong turns
- memorizing large structures
- identifying safe versus dangerous branches
- choosing whether to commit or reroute

## Core Split by Mode

### 4 Player FFA Identity
In 4-player FFA, Maze Rush becomes a **memory-navigation race**:
- all players are shown the full maze briefly at the start
- then visibility collapses to a small local vision radius
- players must remember the route structure
- a beacon indicates the general direction of the next target
- success depends on efficient maze memory, path judgment, and fast correction after mistakes

### BvB Identity
In BvB, Maze Rush becomes a **tactical trap-and-navigation duel**:
- both players navigate large mazes
- switch tiles alter route states
- hazard corridors punish careless pathing
- checkpoint progress can trigger tactical attacks
- success depends on route planning plus tactical adaptation

## Cognitive Skill Tested
Primary skill:
- **pathfinding under pressure**

Secondary skills vary by mode:

### 4 Player FFA
- short-term spatial memory
- route recall
- direction estimation
- efficient exploration under fog

### BvB
- route planning
- trap avoidance
- switch-state understanding
- checkpoint timing
- hazard management

## Why It Exists in Puzzle Rivals
Maze Rush gives Puzzle Rivals a true large-format navigation puzzle with distinct variants:
- a **memory-and-direction FFA**
- a **tactical hazard duel in BvB**

This makes it much stronger and more unique than a basic maze runner.

## What Makes It Unique
Maze Rush must be different from:
- **Link Lock**: not node-link puzzle solving
- **Mirror Maze**: not reflection-first
- **Pipe Pulse / Circuit Clash**: not conduit construction
- **Score Attack / Combo Rush / Clear Rush**: not cluster-clearing
- **Memory Flash**: not pure memorization without navigation pressure

Maze Rush is about **moving through space intelligently** under layered pressure.

---

# 2. Match Role

## Category
- pathfinding
- navigation race
- spatial memory
- tactical route management

## Match Style

### 4 Player FFA
- hidden-information checkpoint race
- partial-visibility navigation
- memory-driven progression
- first to deeper progress or final exit wins

### BvB
- tactical route duel
- hazard-aware pathing
- checkpoint-triggered pressure
- speed plus disruption management

## Recommended Session Length

### 4 Player FFA
- **110 seconds**

### BvB
- **100 seconds**

Why:
- FFA needs time for memory failure, correction, and long-form navigation
- BvB needs time for tactical hazard interaction without becoming sluggish

---

# 3. Core Objective

## Universal Objective
Navigate from start through multiple checkpoints and ultimately to the final exit.

## Win Condition

### 4 Player FFA
A player wins by:
- reaching the final exit first, **or**
- being farthest in checkpoint progression when time expires

### BvB
A player wins by:
- reaching the final exit first, **or**
- having greater checkpoint progress / maze score when time expires

## Lose Condition
A player loses by:
- falling behind in checkpoint stage
- wasting too much time in false paths
- failing to recover from route mistakes
- in BvB, mishandling hazard corridors and switch-state routes

## Primary Goal
Progress efficiently from:
1. Start
2. Checkpoint A
3. Checkpoint B
4. Checkpoint C
5. Final Exit

## Secondary Goals
- minimize wrong turns
- minimize dead-end exploration
- preserve route confidence under pressure
- in BvB, weaponize checkpoint timing and hazard awareness
- in FFA, use memory better than the field

---

# 4. Board Layout

## Board Shape
- large grid-based maze with walls, corridors, checkpoints, and optional special cells

## Board Size

### Recommended Default
- **17 x 17**

### Advanced / Ranked Variant
- **19 x 19**

Recommended shipping default:
- **17 x 17**

This is large enough to matter and large enough that memory actually becomes relevant.

## Cell / Tile Count
- 289 grid units in a 17x17 maze
- mixture of:
  - corridor cells
  - wall cells
  - checkpoint nodes
  - exit node
  - optional switch tiles
  - optional hazard corridors

## Required Structure Rule
A standard Maze Rush board must include:
- at least **3 major route decision layers**
- at least **2 false-path clusters**
- at least **1 meaningful reroute opportunity**
- at least **3 checkpoints total** before final completion

That means the player is not solving one short path. They are solving:
1. start to checkpoint A
2. checkpoint A to checkpoint B
3. checkpoint B to checkpoint C
4. checkpoint C to final exit

## FFA Visibility Rule
At match start:
- full maze visible for **3 to 5 seconds**

Then:
- vision collapses to local reveal only
- player can only see roughly **5 to 6 spaces around current position**
- everything beyond that is dimmed or obscured
- next checkpoint or exit is indicated by a **beacon direction cue**, not a full revealed path

## BvB Maze Structure Additions
BvB mazes may include:
- **switch tiles**
- **hazard corridors**
- **checkpoint-triggered attacks**
- **branch locks or timed route changes** if desired in advanced variants

## Zones

### Universal Zones
- Start Node
- Checkpoint A
- Checkpoint B
- Checkpoint C
- Final Exit

### BvB Additional Zones
- switch tiles
- hazard corridors
- optional gate paths

### FFA Additional Zones
- no heavy tactical hazards by default
- instead use visibility limitation and memory dependence as the main challenge

## Reserved HUD Space
Must preserve space for:
- top center: timer + current objective
- side or top-adjacent: progress meter
- bottom left/right: player cards and usernames
- optional mini-progress indicator showing current checkpoint stage

## Mobile Readability Notes
- corridor versus wall readability is mandatory
- partial-vision mask must be clean and readable
- beacon must clearly show general direction, not exact solution
- route line must remain visible under fog
- checkpoint markers must be unmistakable when within vision radius

---

# 5. Controls and Inputs

## Primary Input
- drag to trace or move along valid corridors

## Secondary Input
- lift finger to stop
- optional quick backtrack control
- optional tap-hold for local route preview if accessibility mode is enabled

## Input Rules
Players may:
- move only through open connected corridor cells
- begin from current active node / checkpoint
- continue route through visible or remembered path
- cannot cross walls
- must physically reach each checkpoint in order

## Invalid Input Behavior
If invalid:
- wall or blocked corridor flashes
- movement stops at last valid node
- short reject spark
- no hard reset unless the design explicitly wants segment reset in a harder variant

---

# 6. Solving Rules

## Universal Solve Structure
Players solve the maze in stages:
1. Start to checkpoint A
2. Checkpoint A to checkpoint B
3. Checkpoint B to checkpoint C
4. Checkpoint C to final exit

Each checkpoint permanently saves progress.

## 4 Player FFA Solve Rules
- full maze is shown briefly at match start
- after dimming, only local vision remains
- player must remember overall structure
- beacon indicates target direction
- wrong branches cost major time
- no heavy direct sabotage between players in default FFA

## BvB Solve Rules
- players navigate their maze while adapting to:
  - switch tiles
  - hazard corridors
  - checkpoint-triggered attacks
- some branches may become better or worse depending on switch activation
- hazards may slow, damage progress tempo, or create reroute pressure

## Partial Progress Rules
- checkpoint reached = permanent save
- route between checkpoints is not saved unless target is reached
- wrong turn recovery is part of the challenge

## Failure Cases
- dead-end commitment
- forgetting layout in FFA
- over-relying on beacon instead of memory
- hitting hazard corridors in BvB
- toggling wrong switch path in BvB
- losing time to reroutes

## Skill Ceiling Behavior
Top players separate themselves by:
- chunking route memory in FFA
- recognizing maze motif structure fast
- making fewer exploratory mistakes
- in BvB, understanding when to trigger switches or avoid hazards
- managing tactical checkpoint pacing under pressure

---

# 7. Scoring Model

## Base Progress Points
- Checkpoint A: **40**
- Checkpoint B: **55**
- Checkpoint C: **70**
- Final Exit: **100**

## Efficiency Bonuses
- low backtrack segment: **+10**
- no dead-end entry in segment: **+15**
- fast checkpoint capture: **+8 to +20**
- perfect full maze completion: **+30**

## BvB Tactical Bonuses
- safe hazard bypass: **+8**
- optimal switch usage: **+10**
- checkpoint attack timing bonus: **+12**

## FFA Memory Bonuses
- first checkpoint reached without dead-end entry: **+12**
- low-vision segment with no wrong branch: **+15**
- beacon-efficient route completion: **+10**

## Penalties
- major dead-end entry: time loss
- repeated route reset: optional small score tax in ranked
- wrong hazard path in BvB: time + tempo penalty

## Score Target to Win

### BvB
Recommended default:
- **300**

### 4 Player FFA
Recommended:
- finishing order matters more than raw score, but score still tracks efficiency
- use checkpoint progress as primary ranking and score as tie-break layer

## Tie Rules
1. farther checkpoint stage wins
2. if tied, faster latest checkpoint split wins
3. if tied, fewer dead-end entries wins

---

# 8. Pressure Systems

## Main Pressure Source

### 4 Player FFA
- memory decay under low visibility

### BvB
- hazard/switch route pressure

## Secondary Pressure Source
- match timer

## FFA Escalation
- visibility restriction creates growing uncertainty
- remembered routes become fuzzier as time and stress rise
- deeper checkpoints produce longer blind navigation chains

## BvB Escalation
- later checkpoints require more switch/hazard decisions
- opponent progress pressures tactical risk-taking
- checkpoint attacks can distort comfortable pacing

## Comeback Potential
- moderate in FFA through clean memory execution
- moderate to high in BvB through smart tactical route recovery and opponent mistakes

---

# 9. Multiplayer Design

## 4 Player FFA Behavior
- full maze shown briefly at start
- then local-vision only
- all players race same checkpoint structure
- no direct sabotage in default spec
- placement determined by progress depth and completion timing

## BvB Behavior
- tactical duel version
- switch tiles, hazard corridors, and checkpoint-triggered attacks active
- players pressure each other through route tempo and attack timing

## Shared / Separate Boards
- separate boards in both modes
- mirrored or equivalent complexity seed for fairness

## Spectator Readability
FFA:
- readable if beacon, progress stage, and visibility mask are well-presented

BvB:
- very readable if switch/hazard states are clear and checkpoint attacks are visually distinct

---

# 10. Puzzle Flow

## Match Start
- countdown
- full maze revealed
- players can study global structure briefly

## FFA Early Transition
- maze dims after the reveal period
- local vision radius activates
- beacon points toward first target

## Mid-Match Flow
- move
- commit
- reroute if necessary
- checkpoint lock
- next checkpoint activates
- repeat

## Endgame Flow
- final checkpoint to exit is the most stressful segment
- in FFA, memory pressure peaks
- in BvB, tactical route hazards peak

## Overtime / Sudden Death
Only for exact tie states:
- next checkpoint reached wins

---

# 11. Difficulty Progression

## Early Difficulty
- maze readable in reveal phase
- first checkpoint relatively accessible
- low initial trick density

## Mid Difficulty
- route depth increases
- false branches matter more
- in FFA, memory stress begins dominating
- in BvB, switch/hazard interplay becomes meaningful

## Late Difficulty
- longest segments occur later
- dead-end punishment becomes more costly
- exit route should be tense, not trivial

## Scaling Variables
- maze size
- false-path density
- dead-end depth
- checkpoint spacing
- switch tile complexity
- hazard corridor density
- vision radius in FFA
- reveal time at start

---

# 12. Animation Language

## Visual Identity
Maze Rush should feel:
- electric
- structural
- navigational
- tense
- spatially alive

## Motion Personality
- tracing
- pulsing
- scanning
- committing
- rerouting
- not explosive for the sake of it

## Required Feedback Types
- maze idle energy
- active checkpoint pulse
- route trace line
- beacon pulse
- low-vision edge glow
- wall reject spark
- dead-end warning
- switch activation
- hazard corridor warning
- checkpoint capture
- exit completion

---

# 13. Sprite Sheet Requirements

## Universal Maze Sheets
- `maze_rush_board_idle.png`
  - purpose: ambient maze frame energy
  - frame count: 8
  - frame size: 640x640
  - layout: 4x2
  - fps: 8
  - loop: yes

- `maze_rush_checkpoint_pulse.png`
  - purpose: active checkpoint pulse
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 10
  - loop: yes

- `maze_rush_route_trace.png`
  - purpose: animated route line
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 14
  - loop: yes while tracing

- `maze_rush_wall_reject.png`
  - purpose: wall collision feedback
  - frame count: 6
  - frame size: 96x96
  - layout: 3x2
  - fps: 16
  - loop: no

- `maze_rush_deadend_warning.png`
  - purpose: dead-end pulse
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 10
  - loop: short loop or one-shot

- `maze_rush_checkpoint_capture.png`
  - purpose: checkpoint reached burst
  - frame count: 10
  - frame size: 160x160
  - layout: 5x2
  - fps: 14
  - loop: no

- `maze_rush_progress_meter_fill.png`
  - purpose: progress lane meter
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `maze_rush_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `maze_rush_exit_completion.png`
  - purpose: final exit completion burst
  - frame count: 10
  - frame size: 640x640
  - layout: 5x2
  - fps: 14
  - loop: no

## 4 Player FFA Specific Sheets
- `maze_rush_fog_of_memory.png`
  - purpose: visibility mask / dimming effect
  - frame count: 6
  - frame size: 640x640
  - layout: 3x2
  - fps: 8
  - loop: yes

- `maze_rush_beacon_pulse.png`
  - purpose: direction beacon for next objective
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 10
  - loop: yes

- `maze_rush_reveal_to_dim_transition.png`
  - purpose: start-of-match full-light to dimmed transition
  - frame count: 8
  - frame size: 1024x256
  - layout: 4x2
  - fps: 10
  - loop: no

## BvB Specific Sheets
- `maze_rush_switch_activate.png`
  - purpose: switch tile activation
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `maze_rush_hazard_corridor.png`
  - purpose: hazard corridor live animation
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 10
  - loop: yes

- `maze_rush_checkpoint_attack.png`
  - purpose: checkpoint-triggered attack FX
  - frame count: 10
  - frame size: 640x640
  - layout: 5x2
  - fps: 14
  - loop: no

## Node Atlas
- `maze_rush_node_atlas.png`
  - purpose: walls, corridors, checkpoints, start, exit, switch tiles, hazard corridors
  - frame count: 24+
  - frame size: 64x64
  - layout: flexible atlas
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- subtle maze frame current
- corridor shimmer
- checkpoint pulse
- progress meter surges

## 4 Player FFA FX
- fog mask / dim state
- beacon pulse for target direction
- local visibility halo around player
- reveal-to-dim transition

## BvB FX
- switch activation pulse
- hazard corridor live warning
- checkpoint attack impact effects

## Failure FX
- wall reject spark
- dead-end pulse
- reroute flash

## Scoring FX
- checkpoint gain pop
- progress lane surge
- final exit burst

## Multiplayer FX
FFA:
- placement progress clarity
- no heavy sabotage visuals

BvB:
- checkpoint-triggered opponent pressure FX
- route-disruption feedback
- lead change highlight

---

# 15. UI / HUD Requirements

## Must Display
- timer
- current checkpoint objective
- progress stage
- player cards
- usernames
- optional mini-progress indicator

### FFA must also display
- beacon direction
- visibility state clearly
- placement or race rank

### BvB must also display
- switch/hazard status cues
- checkpoint attack readiness or trigger feedback

## HUD Layout Rules
- top center: timer + current objective
- side or top-adjacent: progress meter
- bottom left/right: player cards and usernames
- optional mini-progress indicator showing current checkpoint stage

## Mobile Priority Rules
Always visible:
- active objective
- timer
- checkpoint stage
- player position area
- route visibility mask / beacon in FFA
- hazard cues in BvB

---

# 16. Audio Direction

## Music Feel
- tense
- searching
- forward-moving
- suspenseful in FFA
- sharper and more tactical in BvB

## SFX Categories
- route trace
- wall reject
- checkpoint pulse
- beacon ping
- reveal-to-dim sweep
- switch activate
- hazard warning
- checkpoint attack
- exit completion
- win / lose

## Sonic Identity
FFA should sound:
- mysterious
- memory-driven
- quietly tense

BvB should sound:
- sharper
- more tactical
- more aggressive

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Link Lock
- Mirror Maze
- Pipe Pulse
- Circuit Clash
- Memory Flash

## How It Is Different
Compared to Memory Flash:
- memory is used in service of navigation, not standalone recall

Compared to Pipe Pulse / Circuit Clash:
- traversal and checkpoint routing are primary, not conduit construction

Compared to Mirror Maze:
- large-route navigation is primary, not reflective puzzle logic

## What Must Never Happen
- must not become a tiny short maze
- must not be solvable in a handful of taps
- must not hide complexity behind fake size
- must not let the beacon trivialize the route
- must not let BvB hazards overwhelm the maze itself

---

# 18. Technical Implementation Notes

## Phaser Notes
- route tracing must remain smooth on mobile
- local-vision masking in FFA must be performant
- beacon direction should be readable but not exact-path revealing
- switch/hazard states in BvB must be visually obvious
- checkpoint progression should be cleanly state-driven

## Data Model Notes
Per player:
- current checkpoint stage
- score
- dead-end count
- reroute count
- full maze completion count

FFA-specific:
- visibility radius
- beacon target direction
- reveal phase state

BvB-specific:
- switch states
- hazard corridor states
- checkpoint attack trigger states

## Animation Hooks
Trigger on:
- reveal phase start/end
- route trace
- wall reject
- dead-end entry
- beacon pulse
- checkpoint activation
- switch activation
- hazard corridor entry
- checkpoint attack trigger
- exit completion
- win / loss

## Performance Notes
- fog-of-war / visibility mask must be optimized
- route line should be cheap to update
- hazard corridor animations should be pooled
- large maze clarity is more important than decorative excess

---

# 19. Telemetry / Balancing Metrics

Track:
- average checkpoint stage reached
- average dead-end entries
- average reroutes
- average full maze completions
- FFA memory error rate after reveal
- beacon dependence rate
- BvB switch misuse rate
- hazard corridor entry rate
- win rate by first checkpoint lead
- comeback rate

---

# 20. Final Design Summary

## One-Paragraph Summary
Maze Rush is Puzzle Rivals’ large-scale navigation mode with two distinct variants. In 4-player FFA, players briefly study the full maze before visibility collapses, forcing them to navigate through memory with only a small local vision radius and a beacon indicating the general direction of the next target. In BvB, players face a more tactical version featuring switch tiles, hazard corridors, and checkpoint-triggered attacks. In both formats, players must traverse a substantial maze through multiple checkpoints before reaching the final exit, making Maze Rush a true long-form pathfinding puzzle rather than a trivial short route mode.

## Production Priority
- **Very High**

## Dependencies
- large maze node atlas
- fog-of-memory FX
- beacon FX
- switch / hazard FX
- checkpoint pulse and capture FX
- progress lane assets
- player card shimmer
- mirrored complexity maze generator