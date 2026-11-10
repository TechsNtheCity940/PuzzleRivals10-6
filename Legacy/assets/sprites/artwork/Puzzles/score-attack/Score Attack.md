# Score Attack
**Internal ID:** `score_attack`  
**Mode Support:** `Both`

---

# 1. Puzzle Identity

## Core Fantasy
Score Attack is the purest “arcade competition” mode in Puzzle Rivals. It is about rapidly spotting the **highest-value available solves** on a constantly refreshable board and deciding whether to take the safe points now or gamble for a more lucrative setup a moment later.

This mode should feel like:
- a score chase
- a momentum race
- a high-speed board-reading duel
- a “greed versus efficiency” puzzle

It is not about survival, trivia, memory, or route construction. It is about reading opportunity density faster than the other player and converting it into points with minimal waste.

## Cognitive Skill Tested
Primary skill:
- **high-speed prioritization**

Secondary skills:
- pattern recognition
- short-horizon planning
- risk / reward judgment
- score optimization under time pressure

## Why It Exists in Puzzle Rivals
Score Attack is the clean benchmark mode for the whole game. It establishes the franchise’s core arcade tension without requiring a niche rule set. It is the easiest mode to understand from a spectator perspective and one of the strongest anchor modes for ranked play, onboarding, and balance testing.

## What Makes It Unique
Score Attack must be different from every other mode in these ways:
- unlike **Combo Rush**, score is primary and combo is only supportive, not the entire economy
- unlike **Clear Rush**, clearing the board is not the main goal
- unlike **Color Hunt**, there is no restricted target color objective
- unlike **Tile Shift**, the challenge is not board rearrangement
- unlike **Number Crunch**, solving is not arithmetic-first
- unlike **Link Lock / Circuit Clash / Pipe Pulse**, the player is not constructing persistent routes
- unlike **Memory Flash / Glyph Rush**, the challenge is not recall
- unlike **RuleBreaker**, the rules do not mutate mid-match

Score Attack must feel like the most “classic competitive puzzle sprint” mode in the roster.

---

# 2. Match Role

## Category
- speed solve
- score race
- board evaluation
- efficiency competition

## Match Style
- timed score race
- separate boards or mirrored seed boards
- first to score cap **or** highest score at timer end

## Recommended Session Length
Primary recommendation:
- **90 seconds**

Alternative ranked variants:
- **60 seconds** for short-form fast matchmaking
- **120 seconds** for deeper competitive play

Recommended default:
- **90 seconds**

That is long enough for meaningful momentum shifts, but short enough to remain intense and replayable on mobile.

---

# 3. Core Objective

## Win Condition
A player wins by:
- reaching the score target first, **or**
- having the highest score when the match timer expires

## Lose Condition
A player loses by:
- trailing in score when the timer expires, **or**
- allowing the opponent to hit the score cap first

## Primary Goal
Continuously identify and execute the **highest-value available matches** faster than the opponent.

## Secondary Goals
Advanced players optimize for:
- higher-value patterns over easy low-value matches
- chain-friendly board states
- minimal dead time between moves
- safe board refresh timing
- streak preservation
- opponent tempo disruption through outpacing

---

# 4. Board Layout

## Board Shape
- square grid

## Board Size
Recommended default:
- **7 x 7**

Alternative sizes:
- 6 x 6 for casual / lower complexity
- 8 x 8 for elite ranked / later event variants

Recommended shipping size:
- **7 x 7**

## Cell / Tile Count
- 49 active cells

## Zones
Standard board has:
- main playfield only
- no blocked zones by default
- no reserved source/end nodes
- no persistent hazard cells in the base mode

Optional advanced event variant:
- center bonus tiles
- multiplier edge tiles
- hot zone rotation

But the default Score Attack spec should avoid gimmick zones.

## Reserved HUD Space
Must preserve visible space for:
- top center: match title and timer
- left/right side: score lane or score meter
- bottom left/right: player cards and usernames
- bottom center: short ability / streak / mode hint strip if needed

The board must never be obstructed by:
- large rule cards
- oversized score popups
- giant mode banners
- repeated branding

## Mobile Readability Notes
- minimum tile tap target: **56 px visual target**, **64 px interaction-safe target**
- board should occupy roughly **55% to 65%** of vertical mobile view in portrait-adapted mode
- tile symbols must remain readable without text labels
- score indicators must be glanceable
- no VFX should cover more than 20% of the board at once in standard play

---

# 5. Controls and Inputs

## Primary Input
- tap-select pattern
- swipe-chain select
- drag through tiles to form a valid match path

Recommended default:
- **swipe-chain select**

This makes the mode feel fast and continuous.

## Secondary Input
- release to confirm
- tap single tile to start chain
- optional cancel by dragging backward
- optional quick refresh button if refresh mechanic is implemented

## Input Rules
Players may:
- start on any valid tile
- drag across adjacent qualifying tiles
- complete a valid scoring pattern
- release to confirm the solve

If the game supports simple match groups rather than path chaining:
- tap one tile group and auto-resolve if valid

Recommended solve logic:
- **adjacent tile-chain grouping**, because it makes higher-value patterns feel skillful

## Invalid Input Behavior
If invalid:
- line breaks
- selection glows red briefly
- light rejection sound
- no score awarded
- selection resets quickly

Invalid behavior must be fast and crisp, not punishingly slow.

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by identifying clusters or chains of compatible tiles and selecting them to score points.

Recommended base rule:
- select **3 or more connected tiles** of the same family / color / symbol class

To keep Score Attack broad and readable:
- tiles should be grouped by a single obvious property in the base mode
- the puzzle is not about complicated special rules
- value comes from **size, rarity, and speed**, not from memorizing exceptions

## Solve Sequence
1. player scans board
2. player spots high-value cluster or chain
3. player traces or selects the group
4. valid group confirms
5. matched tiles burst and clear
6. score is awarded
7. replacement tiles fall or refill
8. player immediately searches next scoring opportunity

## Partial Progress Rules
- partial selection does not persist
- unfinished selections vanish on release if invalid
- no long-hold puzzle state
- no saved chains between turns

## Failure Cases
- selecting fewer than minimum tile count
- selecting non-adjacent tiles
- breaking color/family consistency
- hesitating too long and losing tempo
- repeatedly taking low-value solves while opponent secures better ones

## Skill Ceiling Behavior
Experts distinguish themselves by:
- spotting 4+ and 5+ groups instantly
- preferring high-value chain structures
- predicting refill patterns
- preserving streaks
- minimizing idle time between solves
- recognizing when a smaller solve unlocks a better follow-up versus when greed wastes time

---

# 7. Scoring Model

## Base Points
Recommended base:
- 3-tile solve: **10 points**
- 4-tile solve: **18 points**
- 5-tile solve: **28 points**
- 6-tile solve: **40 points**
- 7+ tiles: **40 + 8 per extra tile**

This keeps small solves meaningful while strongly rewarding better board reading.

## Bonus Points
Additional modifiers:
- quick solve bonus: **+2 to +8**
- streak bonus: **+3 / +5 / +8 scaling**
- rare tile / premium symbol included: **+5**
- full-board chain collapse caused: **+10**
- back-to-back 5+ solves: **+12 momentum bonus**

## Penalties
Recommended penalties:
- invalid chain: no points, brief input freeze of **150 ms**
- wasted refresh: **-5 points** if refresh exists
- over-reliance on tiny 3-tile chains: no direct penalty, but poor efficiency loses naturally

Avoid harsh negative scoring in the base mode. Score Attack should reward better play rather than feel punitive.

## Score Target to Win
Recommended targets:
- 60-second match: **180 points**
- 90-second match: **260 points**
- 120-second match: **340 points**

Recommended default:
- **260 points in 90 seconds**

## Tie Rules
If timer ends in tie:
1. highest current streak wins
2. if still tied, highest largest single solve wins
3. if still tied, sudden-death next valid solve wins

## Recommended Score Tuning
Target score pacing:
- average player: **140 to 190** in 90 seconds
- good player: **190 to 250**
- strong competitive player: **250 to 320+**

That means the 260-point cap is reachable but not trivial.

---

# 8. Pressure Systems

## Main Pressure Source
- time pressure

## Secondary Pressure Source
- opportunity cost pressure

The real tension comes from:
- “do I take this 3-tile solve now?”
- “do I lose tempo hunting a 5-tile solve?”
- “is the board about to dry up?”
- “am I falling behind on score while looking for perfection?”

## Escalation Model
Pressure ramps through:
- increasing urgency as timer drops
- score delta visibility
- streak value becoming more important
- endgame warning pulses
- faster board refresh feel in final 20 seconds

Optional late-match escalation:
- final 20 seconds = all solves gain a slight urgency FX and bonus multiplier potential

## Comeback Potential
Comebacks should come from:
- landing several 5+ solves in sequence
- preserving streak while opponent fumbles
- late-game bonus windows
- high-value collapse opportunities

Score Attack should always have comeback tension until the final moments.

---

# 9. Multiplayer Design

## Head-to-Head Behavior
Best format:
- separate boards, mirrored seed randomness for fairness
- same tile generation logic per match seed
- each player solves independently
- winner determined by score pace

## Shared / Separate Boards
Recommended:
- **separate boards**
- mirrored initial seed where possible
- synchronized refill RNG buckets if fairness is critical

## Interaction Between Players
Base Score Attack should have:
- no heavy sabotage
- pressure comes from race, not harassment

Optional advanced ranked variant:
- score threshold abilities
- tempo steal effects
- meter burst effects

But the default mode should remain clean and competitive.

## 4-Player Adaptation
If ever expanded:
- score leaderboard race
- same time limit
- individual boards
- top score wins
- no direct sabotage in 4-player casual
- limited ranked disruption only if balance supports it

## Spectator Readability
Very strong spectator mode because:
- bigger solves are obvious
- scores are easy to compare
- lead shifts are easy to understand
- bursts and refill moments look exciting

---

# 10. Puzzle Flow

## Match Start
- short 3-2-1 countdown
- board visible before countdown ends
- timer starts on “go”
- first solves should be available immediately

## Mid-Match Flow
- rapid scanning
- repeated solve-confirm-refill loop
- occasional streak spike moments
- score lead changes visible in side meters

## Endgame Flow
Final 20 seconds:
- timer urgency FX
- scoreboard glows stronger
- solve bursts slightly sharper
- comeback opportunity emphasized

## Overtime / Sudden Death
Not standard. Only used for exact tie state.

---

# 11. Difficulty Progression

## Early Difficulty
- stable, readable board
- multiple obvious 3-tile options
- easy onboarding into rhythm

## Mid Difficulty
- more fragmented board states
- players must prioritize better paths
- streaks become meaningful

## Late Difficulty
- fewer obvious premium solves
- higher reliance on board reading speed
- pressure from timer and score delta

## Scaling Variables
Potential tuning levers:
- tile variety count
- refill speed
- amount of rare tiles
- frequency of 4+/5+ opportunities
- refresh availability
- endgame bonus tuning

---

# 12. Animation Language

## Visual Identity
Score Attack’s animation language should feel:
- fast
- rewarding
- precise
- score-hungry
- crisp and energetic

## Motion Personality
- snappy
- efficient
- competitive
- celebratory on good reads
- never mystical or sleepy

## Required Feedback Types
- tile idle glow
- hover / select pulse
- valid match burst
- invalid rejection crackle
- score gain streak trail
- chain refill cascade
- endgame urgency pulse
- score meter threshold flare
- win burst / lose dim

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `score_attack_tile_idle_glow.png`
  - purpose: live tile energy
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `score_attack_tile_select.png`
  - purpose: selection highlight
  - frame count: 6
  - frame size: 96x96
  - layout: 3x2
  - fps: 14
  - loop: no

- `score_attack_match_burst.png`
  - purpose: successful tile burst
  - frame count: 10
  - frame size: 128x128
  - layout: 5x2
  - fps: 18
  - loop: no

- `score_attack_invalid_crackle.png`
  - purpose: invalid selection rejection
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 16
  - loop: no

- `score_attack_chain_trail.png`
  - purpose: drag path energy trail
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 14
  - loop: yes while dragging

- `score_attack_refill_drop_fx.png`
  - purpose: refill / tile drop accent
  - frame count: 6
  - frame size: 96x128
  - layout: 3x2
  - fps: 14
  - loop: no

- `score_attack_score_meter_fill.png`
  - purpose: score meter internal energy
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `score_attack_score_gain_pop.png`
  - purpose: score popup burst
  - frame count: 8
  - frame size: 192x96
  - layout: 4x2
  - fps: 16
  - loop: no

- `score_attack_streak_flare.png`
  - purpose: streak threshold feedback
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 14
  - loop: no

- `score_attack_endgame_urgency.png`
  - purpose: final 20 seconds urgency overlay
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

- `score_attack_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `score_attack_win_burst.png`
  - purpose: victory flare
  - frame count: 10
  - frame size: 640x640
  - layout: 5x2
  - fps: 14
  - loop: no

## Icon Atlas
- `score_attack_icon_families.png`
  - purpose: tile families used in this mode
  - frame count: 20
  - frame size: 64x64
  - layout: 5x4
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- soft board edge live current
- occasional corner spark
- subtle scanline sweep
- refill drop energy lines

## Tile FX
- idle glow pulse
- select ring
- burst pop on valid solve
- crackle reject on invalid solve

## Solve FX
- bright ring burst at matched tiles
- short score popup
- mini streak trail if solve is large enough

## Failure FX
- red-magenta rejection spark
- short input jitter
- immediate reset

## Pressure FX
- timer pulse at 20 seconds
- score lane glow intensifies if trailing or leading strongly
- final 10-second urgency flickers

## Scoring FX
- vertical score lane fill pulse
- milestone flare every 50 points
- larger solve = larger score pop

## Multiplayer FX
- side score lane pulse when opponent scores big
- lead change flash
- final overtake highlight

---

# 15. UI / HUD Requirements

## Must Display
- player score
- opponent score
- timer
- streak
- player card + username
- mode title
- match lead / deficit feeling through score meter
- optional refresh button if used

## HUD Layout Rules
- top center: title + timer only
- side lanes: vertical score meters
- bottom player lanes: player cards + username + streak info
- no large blocking banners over the board

## Mobile Priority Rules
Always visible on phone:
- timer
- both scores
- current streak
- player identity
- main board

Can be minimized:
- secondary flavor text
- extra mode branding
- verbose notifications

---

# 16. Audio Direction

## Music Feel
- clean arcade adrenaline
- quick-thinking
- bright, kinetic, not oppressive

## SFX Categories
- select: sharp digital click
- valid: satisfying pop + tiny energy ring
- invalid: short glitch snap
- streak: rising flare
- score gain: upward tone
- endgame: urgent pulse
- win: bright triumphant bloom
- lose: controlled drop

## Sonic Identity
Score Attack should sound:
- efficient
- score-focused
- upbeat
- energetic
- less chaotic than Combo Rush
- less cerebral than Number Crunch
- less mystical than Glyph Rush

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Combo Rush
- Clear Rush
- Color Hunt

## How It Is Different
Compared to Combo Rush:
- combos are supportive, not the full economy

Compared to Clear Rush:
- clearing board is not the main objective

Compared to Color Hunt:
- the puzzle is not about hunting one target color under restriction

## What Must Never Happen
- must not become a combo-only mode
- must not turn into a clear-board mode
- must not require special knowledge rules
- must not introduce too many conditional modifiers
- must not bury the score race beneath gimmicks

---

# 18. Technical Implementation Notes

## Phaser Notes
- board state should support rapid refill
- input buffering should be tight
- tile bursts should be pooled if possible for performance
- side score meters should update smoothly, not step harshly

## Data Model Notes
Per player:
- score
- streak
- timer state
- board seed
- solve history
- largest solve
- invalid input count

Per tile:
- family
- color
- point class
- rarity flag if applicable

## Animation Hooks
Trigger points:
- select start
- selection expand
- valid solve confirm
- invalid release
- refill complete
- score threshold crossed
- lead change
- final 20 seconds
- win / loss state

## Performance Notes
- avoid too many simultaneous burst particles on mobile
- keep solve VFX localized
- side meter animation should be lightweight
- no full-board opaque overlays in base mode

---

# 19. Telemetry / Balancing Metrics

Track:
- average score per match
- average score per minute
- average solve size
- average time between solves
- invalid input rate
- streak frequency
- comeback win rate
- early quit rate
- lead change count per match
- largest solve frequency
- win rate by first-score lead

---

# 20. Final Design Summary

## One-Paragraph Summary
Score Attack is Puzzle Rivals’ foundational arcade score race mode. Players rapidly scan a 7x7 board for the highest-value valid tile groups, chain solves as fast as possible, and convert speed plus judgment into a growing score lead. The mode emphasizes efficient board reading, fast decision-making, and score optimization under a strict time limit. Its VFX language is sharp, clean, and rewarding, with strong solve confirmation, clear score lane feedback, and minimal rule clutter.

## Production Priority
- **Very High**

## Dependencies
- tile family icon atlas
- tile idle / burst / invalid FX sheets
- score meter animation set
- player card shimmer
- board ambient frame effects
- timer / endgame urgency UI
- multiplayer mirrored seed support