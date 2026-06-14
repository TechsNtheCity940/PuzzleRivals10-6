# Combo Rush
**Internal ID:** `combo_rush`  
**Mode Support:** `Both`

---

# 1. Puzzle Identity

## Core Fantasy
Combo Rush is the **momentum addiction mode** in Puzzle Rivals. Where Score Attack rewards strong decisions across the whole match, Combo Rush rewards the player who can keep a chain alive under pressure and turn a good opening into a relentless scoring avalanche.

This mode should feel like:
- riding a live wire
- keeping a streak alive by reflex and precision
- converting one solve into the next without dropping tempo
- panic when the chain is about to die
- euphoria when the combo meter catches fire

This is not a calm score optimizer. This is a **combo survival sprint**.

## Cognitive Skill Tested
Primary skill:
- **tempo chaining under pressure**

Secondary skills:
- rapid board scanning
- short-term planning
- path continuity recognition
- execution speed
- recovery judgment

## Why It Exists in Puzzle Rivals
Combo Rush gives the roster a high-adrenaline mode that is distinct from pure score racing. It creates emotional peaks that Score Attack intentionally does not. It is the mode for:
- streak players
- aggressive arcade grinders
- “one more try” repeat sessions
- streamable highlight moments

It should be one of the flashiest and most addictive modes in the game.

## What Makes It Unique
Combo Rush must be clearly different from all other puzzle types:
- unlike **Score Attack**, maintaining combo matters more than raw isolated solve value
- unlike **Clear Rush**, board emptying is not the main goal
- unlike **Color Hunt**, target selection is not constrained by a fixed hunt color
- unlike **RuleBreaker**, the core solve rules do not mutate
- unlike **Memory Flash / Glyph Rush**, there is no recall phase
- unlike **Number Crunch**, the puzzle is not arithmetic-led
- unlike **Link Lock / Circuit Clash / Pipe Pulse**, success is not about persistent path building

Combo Rush is the mode where players are rewarded for **continuous action** over discrete optimization.

---

# 2. Match Role

## Category
- combo management
- speed solve
- momentum race
- execution pressure

## Match Style
- timed combo race
- separate boards
- highest score wins, but combo multiplier is the core engine

## Recommended Session Length
Primary recommendation:
- **75 seconds**

Alternative variants:
- 60 seconds for very short-form mobile
- 90 seconds for ranked longer format

Recommended default:
- **75 seconds**

Why:
- long enough for multiple combo arcs
- short enough that dropping combo feels devastating and immediate
- ideal for mobile-first sessions

---

# 3. Core Objective

## Win Condition
A player wins by:
- having the highest final score at timer end, **or**
- reaching the emergency score cap first if one is enabled

## Lose Condition
A player loses by:
- trailing in score at timer end
- failing to sustain meaningful combo pressure while opponent snowballs

## Primary Goal
Maintain the combo chain as long as possible while converting it into escalating score multipliers.

## Secondary Goals
Advanced players optimize for:
- chaining solves with minimal downtime
- using medium solves to preserve rhythm rather than greedily stalling for huge solves
- protecting high multipliers
- recovering quickly after a combo break
- forcing momentum swings through speed and consistency

---

# 4. Board Layout

## Board Shape
- square grid

## Board Size
Recommended default:
- **6 x 7**

Alternative sizes:
- 6 x 6 for lower chaos
- 7 x 7 for elite/high-volume later versions

Recommended shipping size:
- **6 x 7**

Why:
- slightly taller than wide feels good on mobile
- enough density for chain opportunities
- not so large that players waste time scanning

## Cell / Tile Count
- 42 active cells

## Zones
Base board:
- main playfield only
- no blocked cells by default
- no specialty source/end zones
- no multi-stage board sections

Optional advanced variant:
- a rotating “hot lane” that gives extra combo time
- but not in the base spec

## Reserved HUD Space
Must preserve room for:
- top: timer + combo meter + multiplier
- side or top-adjacent: score comparison
- bottom: player cards / usernames / status
- optional opponent combo warning indicator

The board must remain unobstructed by:
- large banners
- giant mode title cards
- overgrown combo notifications

## Mobile Readability Notes
- minimum tile interaction size: **56 to 64 px**
- combo meter must be readable in under a glance
- score should be secondary to combo visibility in this mode
- high-intensity VFX must never hide future move opportunities
- combo warning effects should frame the board, not wash over it

---

# 5. Controls and Inputs

## Primary Input
- drag-chain selection through valid adjacent tiles

## Secondary Input
- release to confirm
- drag back across prior tile to retract chain
- optional tap-start then swipe continuation

## Input Rules
Players may:
- start on any valid tile
- chain across adjacent matching tiles
- confirm on release
- continue rapidly into next selection immediately after refill

This mode should emphasize **fluid hand movement**.

## Invalid Input Behavior
If invalid:
- chain breaks instantly
- no points
- brief rejection spark
- combo timer continues draining while the player recovers

That last part is critical. Invalid actions hurt more here than in Score Attack because the combo system does not wait politely for humans to get themselves together.

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by selecting valid connected groups of matching tiles.

Recommended base rule:
- **3 or more adjacent tiles of the same family or color**

The important difference is not the group rule itself, but the scoring economy:
- every successful solve extends or stabilizes combo
- every delay risks combo collapse
- solve cadence matters as much as solve quality

## Solve Sequence
1. player spots a valid chain
2. player drags through the chain
3. chain confirms on release
4. solve scores points
5. combo timer refreshes or extends
6. multiplier increases or stabilizes
7. refill occurs quickly
8. player must instantly identify next solve

## Partial Progress Rules
- partial chains do not persist
- current combo persists only if next valid solve occurs before combo timer expires
- partial hesitation is punished through combo decay, not through extra rule complexity

## Failure Cases
- invalid chain attempt
- pausing too long between solves
- greedily searching for larger solves and missing combo timer
- breaking rhythm during refill windows
- losing the multiplier by overthinking

## Skill Ceiling Behavior
Top players separate themselves by:
- solving continuously with near-zero dead space
- choosing “good enough now” over “perfect later” when combo is at risk
- spotting chain-preserving micro-solves instantly
- using larger solves when safe, not when greedy
- recovering after combo breaks without mental collapse

---

# 7. Scoring Model

## Base Points
Recommended base:
- 3-tile solve: **8 points**
- 4-tile solve: **14 points**
- 5-tile solve: **22 points**
- 6-tile solve: **32 points**
- 7+ tiles: **32 + 7 per extra tile**

These base values are intentionally slightly lower than Score Attack because the real engine is the multiplier.

## Combo Multiplier
Recommended multiplier ladder:
- combo 1 to 2: **x1.0**
- combo 3 to 4: **x1.2**
- combo 5 to 6: **x1.5**
- combo 7 to 8: **x1.8**
- combo 9 to 10: **x2.2**
- combo 11 to 14: **x2.6**
- combo 15+: **x3.0 max**

## Combo Timer Rules
- every valid solve refreshes combo timer
- larger solves add a little extra combo time
- smaller solves refresh less generously

Recommended combo timer:
- base window: **2.2 seconds**
- 4+ solve bonus: **+0.15 sec**
- 5+ solve bonus: **+0.25 sec**
- emergency low-combo grace on first chain only: **yes**

## Bonus Points
- perfect rhythm solve under rapid threshold: **+2**
- 5+ solve during combo 8 or higher: **+6**
- full refill cascade directly into another solve: **+5**
- combo milestone bonus every 5 chain count: **+10 / +15 / +20 scaling**

## Penalties
- combo timer expires: multiplier resets to x1
- invalid move: no points and slight loss of combo timer
- refresh misuse, if refresh exists: immediate combo decay hit

## Score Target to Win
Recommended score targets:
- 60 sec variant: **220**
- 75 sec variant: **320**
- 90 sec variant: **400**

Recommended default:
- **320 points in 75 seconds**

## Tie Rules
If tied:
1. highest combo reached wins
2. if tied, longest combo duration wins
3. if tied, sudden-death first valid solve wins

## Recommended Score Tuning
Expected 75-second outcomes:
- average player: **140 to 220**
- good player: **220 to 320**
- strong competitive player: **320 to 450+**

---

# 8. Pressure Systems

## Main Pressure Source
- combo decay timer

## Secondary Pressure Source
- board scan urgency

This is the core emotional loop:
- “I need a solve now”
- “I can’t drop this chain”
- “take the fast solve”
- “don’t get greedy”
- “oh no, I’m about to lose x2.6”

## Escalation Model
Pressure increases through:
- rising combo multiplier
- increasing emotional value of not breaking chain
- combo timer visual urgency
- stronger warning FX at high combo
- score swings becoming larger the longer the chain lives

## Comeback Potential
Comeback is strong in Combo Rush because:
- one massive chain can reverse a lead
- high multiplier windows can spike score quickly
- opponent combo breaks create huge openings

This mode should have one of the highest comeback potentials in the roster.

---

# 9. Multiplayer Design

## Head-to-Head Behavior
Recommended:
- separate boards
- mirrored initial board seed for fairness
- independent combo states

## Shared / Separate Boards
- separate boards
- mirrored or fairness-bucket refill logic recommended

## Interaction Between Players
Base Combo Rush should be:
- mostly indirect race pressure
- opponent combo milestones should visibly pulse your HUD
- optional advanced ranked mechanic: combo threshold sends tiny tempo disturbances

But the default mode should not depend on sabotage.

## 4-Player Adaptation
If adapted later:
- score leaderboard race
- visible combo ranking
- no direct griefing in standard version
- highlight current leader’s combo intensity

## Spectator Readability
Very high spectator value because:
- combo meters are easy to follow
- chains create obvious excitement
- breaks create dramatic momentum shifts
- score lead can swing fast

---

# 10. Puzzle Flow

## Match Start
- 3-2-1 countdown
- board visible before go
- first obvious solves available immediately
- combo timer begins only after first valid solve, not at frame zero

## Mid-Match Flow
- intense solve rhythm
- refill, scan, solve, refill loop
- multiplier growth becomes the emotional center
- players oscillate between confidence and panic

## Endgame Flow
Final 15 seconds:
- combo urgency intensified
- score meter pulses harder
- streak/high combo VFX sharpen
- player should feel forced to commit

## Overtime / Sudden Death
Not standard. Only tie-resolution tool.

---

# 11. Difficulty Progression

## Early Difficulty
- several obvious chain options
- combo starts are accessible
- players learn rhythm quickly

## Mid Difficulty
- more fragmented boards
- chain preservation requires deliberate scanning
- multiplier pressure increases

## Late Difficulty
- board state becomes harder to exploit cleanly
- combo sustain becomes the true test
- players may abandon optimal solves just to preserve chain

## Scaling Variables
- combo timer length
- tile family variety
- refill speed
- density of easy 3-tile solves
- frequency of 4+/5+ opportunities
- warning threshold intensity
- bonus window tuning

---

# 12. Animation Language

## Visual Identity
Combo Rush animation must feel:
- explosive
- escalating
- rhythmic
- addictive
- alive with momentum

## Motion Personality
- snappy
- hungry
- rising
- streaky
- electric
- urgent

## Required Feedback Types
- tile idle
- chain drag trail
- valid solve burst
- invalid reject
- combo increment flare
- multiplier ignite
- combo warning pulse
- combo break collapse
- endgame stress pulse
- win / lose resolution

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `combo_rush_tile_idle_glow.png`
  - purpose: tile idle energy
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `combo_rush_chain_drag_trail.png`
  - purpose: active chain trail while dragging
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 14
  - loop: yes while dragging

- `combo_rush_tile_select_pulse.png`
  - purpose: selection feedback
  - frame count: 6
  - frame size: 96x96
  - layout: 3x2
  - fps: 14
  - loop: no

- `combo_rush_match_burst.png`
  - purpose: successful solve burst
  - frame count: 10
  - frame size: 128x128
  - layout: 5x2
  - fps: 18
  - loop: no

- `combo_rush_combo_increment.png`
  - purpose: combo count increase effect
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 14
  - loop: no

- `combo_rush_multiplier_ignite.png`
  - purpose: multiplier threshold reached
  - frame count: 10
  - frame size: 320x160
  - layout: 5x2
  - fps: 14
  - loop: no

- `combo_rush_combo_warning.png`
  - purpose: combo timer near collapse
  - frame count: 8
  - frame size: 1024x256
  - layout: 4x2
  - fps: 10
  - loop: yes while warning active

- `combo_rush_combo_break.png`
  - purpose: combo lost visual collapse
  - frame count: 10
  - frame size: 512x256
  - layout: 5x2
  - fps: 16
  - loop: no

- `combo_rush_refill_surge.png`
  - purpose: refill with momentum accent
  - frame count: 6
  - frame size: 96x128
  - layout: 3x2
  - fps: 14
  - loop: no

- `combo_rush_score_meter_live.png`
  - purpose: score lane energy
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `combo_rush_combo_meter_live.png`
  - purpose: combo meter internal pulse
  - frame count: 8
  - frame size: 512x128
  - layout: 4x2
  - fps: 10
  - loop: yes

- `combo_rush_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `combo_rush_endgame_overdrive.png`
  - purpose: final 15 second urgency
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

- `combo_rush_win_burst.png`
  - purpose: victory payoff
  - frame count: 10
  - frame size: 640x640
  - layout: 5x2
  - fps: 14
  - loop: no

## Icon Atlas
- `combo_rush_icon_families.png`
  - purpose: tile family icons
  - frame count: 20
  - frame size: 64x64
  - layout: 5x4
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- edge current stronger than Score Attack
- subtle chain path shimmer
- refill surges with streak energy
- final phase board intensity increase

## Tile FX
- glowing chain line while dragging
- match burst stronger than Score Attack
- combo-linked glow accents
- invalid crackle brief but sharp

## Solve FX
- successful solve creates:
  - tile burst
  - combo number pulse
  - multiplier flare if threshold crossed
  - score popup

## Failure FX
- invalid drag breaks with red-magenta glitch spark
- combo timer edge pulse worsens if player hesitates
- actual combo break should feel painful but fast

## Pressure FX
- combo timer bar changes color as it shrinks
- board edge pulses when combo is in danger
- high combo causes subtle live neon intensity

## Scoring FX
- score popups should scale with combo multiplier
- combo threshold milestones deserve bigger confirms
- side score lane should surge on large chains

## Multiplayer FX
- opponent high combo should trigger:
  - small side alert
  - enemy lane flare
  - lead shift accent
- not enough to distract from board

---

# 15. UI / HUD Requirements

## Must Display
- score
- combo count
- combo timer
- multiplier
- timer
- player card
- username
- opponent score
- opponent combo threat indicator

## HUD Layout Rules
Recommended layout:
- top center: timer + combo meter + multiplier
- side lanes: score comparison
- bottom left/right: player cards
- small non-blocking combo notifications only

## Mobile Priority Rules
Always visible:
- combo timer
- multiplier
- score
- current board
- player identity

Can be reduced:
- verbose text
- decorative status copy
- secondary labels

---

# 16. Audio Direction

## Music Feel
- higher energy than Score Attack
- more driving pulse
- more urgency
- still clear enough for concentration

## SFX Categories
- select: bright, quick tap tone
- chain drag: low electric shimmer
- solve: pop + surge
- combo increase: rising tick / pulse
- multiplier ignite: bigger flare
- warning: tight nervous pulse
- combo break: sharp drop snap
- win: explosive but clean rise
- lose: abrupt energy collapse

## Sonic Identity
Combo Rush should sound:
- hotter
- streakier
- more unstable
- more rewarding during successful momentum
- more painful when dropped

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Score Attack
- Clear Rush

## How It Is Different
Compared to Score Attack:
- multiplier and combo preservation are the core economy
- not just “get more points,” but “do not stop scoring”

Compared to Clear Rush:
- combo maintenance matters more than board depletion

## What Must Never Happen
- must not turn into generic score race
- must not become a clear-board mode
- must not use too many static objectives
- must not let players pause comfortably between moves
- must not hide the combo timer importance

---

# 18. Technical Implementation Notes

## Phaser Notes
- combo timer UI must animate smoothly
- drag path trail needs low-latency feedback
- burst FX should be pooled
- refill timing should be snappy to preserve flow
- combo break sequence must be fast and readable

## Data Model Notes
Per player:
- score
- streak
- combo count
- combo multiplier
- combo timer remaining
- highest combo
- last solve timestamp
- invalid input count

Per tile:
- family
- color
- point class
- optional rarity

## Animation Hooks
Trigger on:
- drag start
- tile added to chain
- valid solve
- invalid solve
- combo increment
- multiplier threshold
- combo warning
- combo break
- endgame overdrive
- win / lose

## Performance Notes
- keep drag trail lightweight on mobile
- burst particles must be capped
- combo warning overlay should frame edges, not cover board
- high combo intensity should scale without full-screen clutter

---

# 19. Telemetry / Balancing Metrics

Track:
- average combo length
- highest combo reached
- combo break frequency
- average score per minute
- average time between solves
- invalid drag rate
- multiplier uptime
- comeback win rate
- quit rate after combo loss
- win rate by first big combo

---

# 20. Final Design Summary

## One-Paragraph Summary
Combo Rush is Puzzle Rivals’ high-adrenaline momentum mode. Players rapidly chain valid tile solves on a 6x7 board while a combo timer constantly threatens to reset their multiplier. The heart of the mode is not just scoring well, but scoring continuously. Small fast solves can preserve life, while larger solves push the multiplier into explosive territory. The result is a tense, streamable, highly replayable arcade duel built around rhythm, greed control, and chain survival.

## Production Priority
- **Very High**

## Dependencies
- tile idle and chain trail FX
- solve burst and combo increment FX
- combo timer UI
- multiplier threshold effects
- board edge pressure effects
- player card shimmer
- mirrored seed multiplayer support