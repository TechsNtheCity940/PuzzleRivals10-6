# Pipe Pulse
**Internal ID:** `pipe_rush`  
**Mode Support:** `4 Player Ranked`

---

# 1. Puzzle Identity

## Core Fantasy
Pipe Pulse is Puzzle Rivals’ **live-flow pressure puzzle**. Players rotate, route, and stabilize a network of futuristic conduits while active energy pulses through the system in real time. The puzzle is not just about making a valid path eventually. It is about making the **right path fast enough** before pressure builds, routes fail, or better opportunities are lost.

This mode should feel like:
- emergency system routing
- pressure building through a damaged neon conduit network
- racing to complete flow lines before overload
- juggling multiple partial routes at once
- reading and repairing an unstable machine under load

Pipe Pulse should feel more like:
- **reactive systems engineering**
than:
- simple maze drawing
- static tile matching
- leisurely pipe toy gameplay

## Cognitive Skill Tested
Primary skill:
- **dynamic route repair under time pressure**

Secondary skills:
- spatial rotation
- forward flow prediction
- priority management
- pressure triage
- live board scanning

## Why It Exists in Puzzle Rivals
Pipe Pulse gives the roster a true **real-time flow-routing mode**. It is one of the strongest “machine puzzle” identities in the game and sits far away from the match/clear/score family.

It fills these design needs:
- real-time urgency
- mechanical systems fantasy
- visible flow motion
- route stabilization as the core skill
- strong spectator appeal because the board visibly “works” or “fails”

## What Makes It Unique
Pipe Pulse must be clearly different from:
- **Maze Rush**: Maze Rush is human route navigation through a maze. Pipe Pulse is machine route construction and repair.
- **Circuit Clash**: Circuit Clash should be logic-structured and cleaner. Pipe Pulse is more kinetic, unstable, and pressure-driven.
- **Link Lock**: Link Lock is about pattern linking, not live flow pressure.
- **Tile Shift / Spatial Spin**: those emphasize board manipulation; Pipe Pulse emphasizes **network completion under active pressure**.
- **Score Attack / Combo Rush / Clear Rush / Color Hunt**: those are solve-and-clear modes, while Pipe Pulse is a **maintain-and-route system**.

Pipe Pulse should never feel like “Maze Rush but with pipes.” It is about **flow logic, pressure, and conduit repair**.

---

# 2. Match Role

## Category
- real-time routing
- spatial rotation
- systems pressure
- network repair race

## Match Style
- 4-player ranked
- separate boards
- simultaneous play
- leaderboard determined by routed flow, objective completion, and failure avoidance

Best identity:
- **round-based live pressure routing race**

## Recommended Session Length
Primary recommendation:
- **105 seconds**

Alternative variants:
- 90 seconds for quicker ranked queues
- 120 seconds for full competitive format

Recommended default:
- **105 seconds**

Why:
- enough time for multiple full route problems
- enough time for escalating system pressure
- short enough to remain intense on mobile

---

# 3. Core Objective

## Win Condition
Players rank by total performance score built from:
- successful source-to-target completions
- pressure relief events
- sustained flow uptime
- special route objectives completed
- overload avoidance

Recommended default:
- highest total system score at timer end wins

## Lose Condition
A player falls behind by:
- letting pressure burst segments repeatedly
- failing to complete routes in time
- wasting too many rotations on bad placements
- stabilizing too little flow versus other players

There may also be a soft “critical failure” state:
- repeated overloads reduce scoring efficiency dramatically
- but full elimination should be avoided in the standard mode

## Primary Goal
Route energy from one or more sources to one or more destination nodes before pressure overload causes failures.

## Secondary Goals
Advanced players optimize for:
- shorter and safer flow routes
- multi-route stabilization
- pressure relief efficiency
- simultaneous path support
- smart prioritization of the most dangerous leaks / overloads first

---

# 4. Board Layout

## Board Shape
- square tile grid with fixed conduit cells

## Board Size
Recommended default:
- **8 x 8**

Alternative sizes:
- 7 x 7 for easier entry mode
- 9 x 9 for elite or event variants

Recommended shipping size:
- **8 x 8**

## Cell / Tile Count
- 64 cells

## Board Composition
The board should contain:
- standard conduit tiles
- corner turns
- straight segments
- T-junctions
- cross junctions
- blocked / dead cells
- source nodes
- destination nodes
- unstable / pressure-sensitive segments
- locked or frozen segments in advanced variants

## Required Structure Rule
A standard Pipe Pulse board must include:
- at least **2 active source nodes**
- at least **2 destination nodes**
- at least **1 alternate routing opportunity**
- at least **1 pressure-sensitive failure zone**
- at least **1 route that can be completed quickly but inefficiently**
- at least **1 longer route with better payoff**

That ensures the board is not a trivial single-line pipe puzzle.

## Zone Types
- **Source Nodes**
- **Destination Nodes**
- **Neutral Conduit Field**
- **Pressure Zones**
- **Burst-Risk Segments**
- **Locked / Frozen Tiles** in harder variants
- optional **Bonus Flow Nodes**

## Reserved HUD Space
Must preserve space for:
- top center: timer + current system state
- left/right or top-adjacent: pressure meter and rank position
- bottom: player card / username
- optional objective strip
- optional upcoming hazard warning lane

## Mobile Readability Notes
- pipe silhouettes must be instantly readable
- flow color must remain visible through the conduit center
- pressure zones must be visibly distinct from standard pipes
- rotating a tile must feel obvious and immediate
- route completion feedback must not hide nearby tiles
- over-bright VFX must not obscure the topology of the network

---

# 5. Controls and Inputs

## Primary Input
- tap to rotate pipe tile clockwise

## Secondary Input
- optional long-press to rotate counterclockwise
- optional tap-hold to preview connection glow
- optional drag across several eligible tiles for rapid maintenance in advanced accessibility mode only

Recommended default:
- **single-tap clockwise rotation**

This is simple, fast, and mobile-friendly.

## Input Rules
Players may:
- tap rotatable pipe tiles
- change orientation one step per tap
- attempt to align conduits to complete live routes
- interact while flow is active

## Invalid Input Behavior
If invalid:
- tile rotates if rotatable, but may create bad routing
- if tile is locked, brief reject animation
- if segment is overloaded, it may spark or resist

Unlike match puzzles, “invalid” in Pipe Pulse often means **suboptimal or dangerous**, not mechanically forbidden.

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by rotating pipe tiles to create valid flow routes from sources to destinations before pressure causes failures.

### Base loop
1. source activates
2. energy starts moving through partial network
3. player rotates tiles to guide flow
4. correct route reaches destination
5. system stabilizes and awards points
6. next pressure problem or route objective appears

## Solve Sequence
1. identify active source and intended destination
2. locate broken or incomplete route segments
3. rotate pipe pieces to align path
4. manage pressure buildup while routing
5. deliver flow to destination
6. secure or maintain route if secondary flow remains active
7. transition to next active objective

## Partial Progress Rules
- partially correct paths matter
- active flow may move through incomplete systems until blocked
- partial progress can buy time if it relieves pressure
- completed path segments may remain useful for future routing

## Failure Cases
- route dead-ends into closed segment
- flow enters burst-risk corridor too long
- pressure reaches overload threshold
- player focuses on low-danger section while critical area fails
- unnecessary rotations destabilize a good path

## Skill Ceiling Behavior
Top players separate themselves by:
- reading the network topology instantly
- fixing the most dangerous route problem first
- preparing future branch support before it becomes urgent
- recognizing efficient versus emergency routes
- managing multiple simultaneous live problems

---

# 7. Scoring Model

## Base Points
Recommended:
- destination successfully reached: **40 points**
- major pressure relief event: **20 points**
- multi-branch stabilization: **15 points**
- full-board system stabilization window: **35 points**

## Bonus Points
- fast completion bonus: **+8 to +20**
- longer efficient route bonus: **+12**
- simultaneous dual-route support: **+15**
- no-burst streak: **+10**
- unstable segment rescued at last moment: **+8**

## Penalties
- burst / overload event: **-10 to -20 score equivalent**
- repeated misrouting: indirect penalty through time loss
- locked segment hit repeatedly: no direct score loss, but wasted tempo
- critical failure chain: scoring multiplier reduced briefly

## Score Target to Win
Because this is 4-player ranked, score target is less useful than leaderboard placement.

Recommended expected score range for 105 seconds:
- average: **180 to 260**
- good: **260 to 360**
- strong: **360 to 480+**

## Tie Rules
1. fewer overloads wins
2. more completed destinations wins
3. faster final major completion wins

## Recommended Score Tuning
Pipe Pulse score must reward:
- stable routing
- pressure response
- good prioritization
more than:
- random frantic tapping

---

# 8. Pressure Systems

## Main Pressure Source
- live flow pressure buildup

## Secondary Pressure Source
- multiple active route demands

This is the heart of the mode. The player should constantly feel:
- “this line is about to burst”
- “I can fix this, but then the other source will become unstable”
- “do I take the quick ugly route or hold for the better one?”

## Escalation Model
Pressure rises through:
- more simultaneous live sources
- faster pressure buildup
- more locked segments
- more burst-risk corridors
- denser decision layers later in the match

## Comeback Potential
Comeback is moderate to high because:
- a player can stabilize multiple routes quickly
- one meltdown can tank a leader
- strong pressure handling can produce score spikes fast

This keeps the mode dramatic without making it random.

---

# 9. Multiplayer Design

## 4-Player Ranked Behavior
- all players have separate boards
- equivalent complexity seed family
- leaderboard updates live
- players compete on routing efficiency and system stability

## Interaction Between Players
Recommended default:
- indirect race with light ranked pressure events

Possible ranked interactions:
- major stabilization milestone sends a minor hazard pulse to others
- top-pressure clear may accelerate rival source activation
- overload may create a small self-only penalty, not a direct grief tool

Important:
- do not overdo sabotage
- the mode should stay primarily about solving your own system better than everyone else

## Spectator Readability
Very strong:
- visible live flow
- obvious burst failures
- clear route completions
- clear leaderboard momentum swings

---

# 10. Puzzle Flow

## Match Start
- board loads with stable initial network
- first source activates
- one or two early routing problems appear
- players begin immediately

## Mid-Match Flow
- sources activate
- players rotate to complete lines
- pressure spikes
- destination completion awards relief and score
- new route tasks emerge

## Endgame Flow
Final 20 seconds:
- source intensity rises
- pressure windows tighten
- unstable segments appear more often
- leaderboard pressure becomes more visible

## Overtime / Sudden Death
Not standard in 4-player ranked.
Placement resolves by:
- score
- then stability tiebreakers

---

# 11. Difficulty Progression

## Early Difficulty
- single active source
- obvious repair route
- low pressure speed
- limited unstable tiles

## Mid Difficulty
- two active source routes possible
- burst-risk corridors matter
- longer route planning required
- inefficient choices begin to hurt

## Late Difficulty
- overlapping route demands
- faster pressure buildup
- more locked or hazard segments
- higher reward for elegant routing
- greater punishment for panic solving

## Scaling Variables
- number of active sources
- pressure buildup rate
- locked tile frequency
- unstable segment density
- route length options
- number of simultaneous destination demands

---

# 12. Animation Language

## Visual Identity
Pipe Pulse should feel:
- electrical
- mechanical
- alive
- unstable
- high-voltage
- premium arcade sci-fi

## Motion Personality
- pulsing
- surging
- sparking
- pressurizing
- stabilizing
- bursting

## Required Feedback Types
- idle conduit energy
- flow movement
- pipe rotation feedback
- connection lock
- pressure buildup
- overload warning
- burst event
- stabilization pulse
- destination reached
- leaderboard surge
- win / placement resolution

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `pipe_pulse_pipe_idle.png`
  - purpose: idle conduit life
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `pipe_pulse_flow_travel.png`
  - purpose: energy moving through pipe center
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 12
  - loop: yes while active

- `pipe_pulse_rotate_feedback.png`
  - purpose: tile rotation confirmation
  - frame count: 6
  - frame size: 96x96
  - layout: 3x2
  - fps: 14
  - loop: no

- `pipe_pulse_connection_lock.png`
  - purpose: route segment successfully aligned
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 14
  - loop: no

- `pipe_pulse_pressure_buildup.png`
  - purpose: unstable pipe pressure ramp
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 10
  - loop: yes

- `pipe_pulse_overload_warning.png`
  - purpose: imminent burst warning
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 10
  - loop: yes while critical

- `pipe_pulse_burst_event.png`
  - purpose: pipe burst / failure effect
  - frame count: 10
  - frame size: 160x160
  - layout: 5x2
  - fps: 16
  - loop: no

- `pipe_pulse_stabilize.png`
  - purpose: successful pressure relief
  - frame count: 8
  - frame size: 160x160
  - layout: 4x2
  - fps: 14
  - loop: no

- `pipe_pulse_destination_reached.png`
  - purpose: completed source-to-target delivery
  - frame count: 10
  - frame size: 256x256
  - layout: 5x2
  - fps: 14
  - loop: no

- `pipe_pulse_score_lane.png`
  - purpose: vertical score / stability meter
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `pipe_pulse_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `pipe_pulse_endgame_overload.png`
  - purpose: endgame pressure frame effect
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

- `pipe_pulse_rank_gain_flash.png`
  - purpose: placement/rank movement highlight
  - frame count: 6
  - frame size: 256x128
  - layout: 3x2
  - fps: 12
  - loop: no

## Tile / Node Atlas
- `pipe_pulse_node_atlas.png`
  - purpose: straight pipes, corner pipes, T-junctions, crosses, source nodes, destination nodes, blocked cells, hazard corridors, locked tiles
  - frame count: 24+
  - frame size: 64x64
  - layout: flexible atlas
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- subtle board frame current
- active source node pulse
- live flow through pipes
- route completion glow
- leaderboard placement pulse

## Tile FX
- rotation snap
- conduit lock confirmation
- pressure shimmer in unstable sections
- burst crack and spark on failure

## Solve FX
- destination reached should feel satisfying and systemic
- not like “matched candy”
- should feel like a machine successfully coming online

## Failure FX
- overload warning
- burst spark and smoke-like neon dissipation
- route interruption flash

## Pressure FX
- escalating conduit color intensity
- warning pulses near unstable segments
- endgame frame overload effect

## Scoring FX
- destination completion pop
- pressure relief gain pop
- side score/stability lane pulse

## Multiplayer FX
- rank gain flash
- rival surge notice
- milestone stabilization alert
- keep these readable but not overwhelming

---

# 15. UI / HUD Requirements

## Must Display
- timer
- score
- current rank / placement
- pressure meter or system stability meter
- active source / destination status
- player card
- username
- optional hazard warning strip

## HUD Layout Rules
- top center: timer + system state
- side: score / stability / placement lane
- bottom: player card
- board center must remain fully readable

## Mobile Priority Rules
Always visible:
- timer
- pressure / stability
- score
- placement
- player identity
- route network

Can be minimized:
- secondary objective text
- verbose milestone messages
- extra decorative alerts

---

# 16. Audio Direction

## Music Feel
- urgent
- mechanical
- high-voltage
- reactive
- more system-failure tension than Maze Rush

## SFX Categories
- pipe rotate
- connection lock
- flow travel
- pressure buildup
- warning
- burst
- stabilize
- destination reached
- rank gain
- win / placement result

## Sonic Identity
Pipe Pulse should sound:
- alive
- industrial sci-fi
- electrically unstable
- very reactive
- more machine-like than any other puzzle mode

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Circuit Clash
- Maze Rush
- Link Lock
- Spatial Spin

## How It Is Different
Compared to Circuit Clash:
- Pipe Pulse is more live, unstable, and pressure-driven; Circuit Clash should be cleaner and more logic-forward

Compared to Maze Rush:
- Maze Rush is human navigation through space; Pipe Pulse is machine route construction and live-flow management

Compared to Link Lock:
- Link Lock is connection pattern solving, not active pressure routing

## What Must Never Happen
- must not become a calm static pipe toy
- must not become a maze in disguise
- must not rely on giant opaque FX that hide topology
- must not reward frantic random rotation over intelligent pressure management
- must not have only one trivial route per board

---

# 18. Technical Implementation Notes

## Phaser Notes
- pipe rotation must feel instant
- active flow animation must be performant
- conduit center flow should remain readable over all pipe types
- burst-risk segments must be clearly state-driven
- route completion feedback should be strong but local

## Data Model Notes
Per player:
- score
- current rank
- completed destination count
- burst count
- stability state
- average pressure relief
- timer state

Per board:
- tile network
- active source nodes
- active destination nodes
- pressure states
- locked segment states
- hazard corridor states

## Animation Hooks
Trigger on:
- rotate input
- conduit alignment
- source activation
- flow begin
- pressure threshold
- overload warning
- burst
- stabilize
- destination reached
- placement change
- endgame overload

## Performance Notes
- flow animation should be tile-efficient
- warnings should use pooled effects
- board readability is more important than raw particle count
- rank flashes should be light-touch in 4-player environments

---

# 19. Telemetry / Balancing Metrics

Track:
- completed destinations per match
- burst count per match
- pressure relief frequency
- rotation count per completion
- stability uptime
- win rate by early stabilization lead
- comeback rate
- average score by rank placement
- misrotation / panic rotation rate

---

# 20. Final Design Summary

## One-Paragraph Summary
Pipe Pulse is Puzzle Rivals’ live-flow routing mode for 4-player ranked play. Players rotate conduit tiles on an 8x8 board to deliver active energy from sources to destinations while managing pressure buildup, unstable segments, and overload risks. The puzzle rewards fast spatial rotation, pressure triage, and route efficiency, with a strong mechanical sci-fi identity built around flowing neon energy and unstable system repair. It should feel tense, reactive, and systemic, not like a simple static pipe toy.

## Production Priority
- **Very High**

## Dependencies
- conduit atlas
- flow travel FX
- pressure / overload FX
- stabilization and destination completion FX
- rank / score lane assets
- player card shimmer
- fairness-seeded ranked board generation