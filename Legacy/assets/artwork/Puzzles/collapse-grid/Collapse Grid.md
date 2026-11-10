# Collapse Grid
**Internal ID:** `collapse_grid`  
**Mode Support:** `Both`

---

# 1. Puzzle Identity

## Core Fantasy
Collapse Grid is Puzzle Rivals’ **structural stability puzzle**. Players remove or weaken tiles from a stacked support field while trying to preserve their own board’s critical objectives and, indirectly or directly, destabilize the opponent.

This is not a physics sandbox with unpredictable chaos. It should feel like:
- engineering around imminent collapse
- pulling the right support at the right time
- creating controlled failures
- preserving key structures while letting expendable sections fall
- reading weak points, load paths, and chain reactions

The emotional center is:
- “if I remove this, that side drops but my objective survives”
- “one more crack and their tower section folds”
- “I need a controlled collapse, not a disaster”
- “that support looks harmless, but it’s carrying half the board”
- “I can win by precision, not by random destruction”

It should feel tense, clever, and highly readable.

## Cognitive Skill Tested
Primary skill:
- **structural cause-and-effect prediction**

Secondary skills:
- weak-point detection
- chain reaction planning
- spatial stability reasoning
- risk management
- tactical timing
- damage containment

## Why It Exists in Puzzle Rivals
Collapse Grid gives the roster a true **stability and collapse puzzle**, which is different from:
- route puzzles
- match puzzles
- queue placement puzzles
- word or trivia puzzles

It fills the niche of:
- simulated physical tension
- support logic
- controlled demolition
- tactical sabotage
- board damage management

This mode should feel uniquely satisfying because the player is not just “solving.” They are **managing structural consequences**.

## What Makes It Unique
Collapse Grid must be clearly distinct from nearby modes:
- unlike **Clear Rush**, removal is dangerous and context-sensitive
- unlike **Shard Stack**, pieces are not the main challenge; supports and stability are
- unlike **Tile Shift**, the board is not reorganized structurally by line movement
- unlike **Pipe Pulse / Circuit Clash**, the challenge is not network routing
- unlike **Maze Rush / Mirror Maze**, there is no traversal or beam logic
- unlike **Score Attack**, more removal is not automatically better

Collapse Grid is about **safe destruction and controlled instability**.

---

# 2. Match Role

## Category
- structural puzzle
- support logic
- controlled demolition
- tactical board destabilization

## Match Style
Recommended defaults:
- BvB tactical collapse duel
- separate boards
- score or survival-timer hybrid win logic

Best identity:
- **controlled collapse battle**

That means:
- players remove supports
- chain collapses award points
- self-damage is dangerous
- stability management matters as much as offense

## Recommended Session Length

### BvB
- **95 seconds**

### Optional survival-focused variant
- **105 seconds**

Recommended default:
- **95 seconds**

Why:
- enough time for multiple structural scenarios
- enough time for attacks/defenses to matter
- short enough that collapse tension stays sharp

---

# 3. Core Objective

## Win Condition
A player wins by:
- having the highest score when time expires, **or**
- surviving longer if the mode uses a survival threshold, **or**
- causing the opponent’s objective structure to fail before their own does

Recommended default:
- **score first, survival second**

## Lose Condition
A player loses by:
- taking too much structural self-damage
- allowing protected objective pieces to collapse
- trailing in score while failing to engineer controlled collapses

## Primary Goal
Remove or weaken support tiles to create profitable chain collapses without compromising your own protected objective structure.

## Secondary Goals
Advanced players optimize for:
- creating controlled chain collapses
- preserving key supports while sacrificing expendable structures
- targeting high-leverage enemy weak points via attacks
- timing defensive stabilization at the last possible moment
- avoiding low-value removals that expose your own collapse zones

---

# 4. Board Layout

## Board Shape
- vertical-leaning structural grid
- board should visually read like a stacked support frame, scaffold, or crystal support lattice

## Board Size
Recommended default:
- **8 columns x 10 rows**

Alternative sizes:
- 7 x 9 for easier mode
- 8 x 12 for advanced endurance variant

Recommended shipping size:
- **8 x 10**

Why:
- enough verticality for real collapse drama
- enough width for multiple support branches
- still readable on mobile

## Cell / Tile Count
- 80 cells

## Board Composition
The board should contain:
- standard support tiles
- reinforced support tiles
- fragile cracked tiles
- empty gaps
- weighted blocks
- protected objective nodes / anchor structures
- optional decorative top mass / stack elements
- optional unstable hanging segments

## Required Structure Rule
A standard Collapse Grid board must include:
- at least **1 protected objective structure**
- at least **3 meaningful support chains**
- at least **2 obvious-looking weak points where only one is actually safe**
- at least **1 controlled-collapse opportunity**
- at least **1 chain reaction route**
- at least **1 false move that looks good but threatens self-damage**
- at least **1 recoverable instability state**

This prevents the mode from becoming “click cracked tile, watch stuff fall, call it game design.”

## Zone Types
- **Objective Zone**
- **Primary Support Columns**
- **Load-Bearing Cross Supports**
- **Expendable Debris Region**
- **Heavy Mass / Weight Zone**
- **Weak-Point Region**
- **Danger / Dust Zone** in advanced variants

## Reserved HUD Space
Must preserve room for:
- top center: timer + score + board stability state
- side or top-adjacent: structural health / danger meter
- bottom left/right: player cards and usernames
- optional attack / defense charge lane
- optional objective integrity icon

## Mobile Readability Notes
- support state must be visible instantly
- protected objective pieces must stand out clearly
- cracks, reinforcements, and weight tiles must be unmistakable
- board shake and dust must never hide critical weak points
- collapse direction and result need to be readable at a glance

---

# 5. Controls and Inputs

## Primary Input
Recommended default:
- tap support tile to remove, weaken, or interact depending on state

## Secondary Input
- tap defense ability button, then tile target
- optional long-press for stability preview in accessibility mode

## Input Rules
Players may:
- target removable support tiles
- crack or remove tiles depending on board rules
- use defenses on eligible structural tiles
- trigger attacks through earned charge

## Removal Rules
Recommended base:
- standard support tile: removed in one action
- reinforced support tile: requires cracking first or more effort
- cracked tile: easier to remove and more dangerous structurally
- some objective anchors: cannot be directly removed

## Invalid Input Behavior
If invalid:
- protected anchor rejects
- already unsupported empty cell rejects
- brace-protected tile ignores removal temporarily
- quick feedback only

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by identifying which supports can be safely removed to cause profitable collapses without destroying their own protected objective structure.

The puzzle is about:
- understanding support dependency
- triggering collapses intentionally
- reading what parts of the board are expendable

## Solve Sequence
1. scan structure for weak points
2. identify safe removal candidate
3. evaluate whether objective support remains intact
4. remove or weaken support
5. board recalculates stability
6. unsupported sections fall or collapse
7. score awarded for controlled removals and collapses
8. reassess board stability
9. continue or repair as needed

## Core Support Logic
The mode should use **fake-physics support rules**, not full simulation chaos.

Recommended support logic:
- a structural segment remains stable if connected to sufficient support anchors beneath or beside it
- disconnected heavy mass falls
- cracked supports count as weakened and may fail under aftershock or weight pressure
- board collapse should be deterministic and legible

## Partial Progress Rules
- cracks persist
- reinforcements persist until broken
- controlled damage can set up later collapses
- self-instability can be recovered if caught early

## Failure Cases
- removing a key support that looked expendable
- ignoring weak supports under heavy tiles
- focusing too much on enemy pressure and letting your own board destabilize
- creating a chain collapse that destroys your protected structure
- failing to repair or brace at the right time

## Skill Ceiling Behavior
Top players separate themselves by:
- reading true support chains instantly
- recognizing sacrificial regions
- engineering multi-step collapses
- understanding when to damage now versus stabilize first
- weaponizing enemy weak-point pressure without overcommitting

---

# 7. Scoring Model

## Base Points
Using your foundations:

- successful removal: **+10**
- controlled chain collapse: **+20**

## Expanded Scoring Recommendations
- multi-segment controlled collapse: **+28**
- heavy block safely dropped without self-damage: **+15**
- enemy-pressure-assisted collapse setup: **+10**
- precision removal that leaves objective safe by minimal margin: **+8**
- perfect stabilization recovery before collapse: **+10**

## Penalties
Using your foundations:

- points lost for your own board damage

Recommended tuning:
- self-damage to non-objective structure: **-5**
- objective support damage: **-12**
- protected objective collapse or severe failure: **-25 to -40**
- avoid immediate instant-loss unless using a hardcore variant

## Score Target to Win
Recommended BvB target:
- **220 to 300**

Recommended default:
- **260 points in 95 seconds**

## Tie Rules
1. higher objective integrity wins
2. if tied, fewer self-collapse incidents wins
3. if tied, bigger single controlled collapse wins

## Recommended Score Tuning
Expected 95-second outcomes:
- average player: **120 to 190**
- good player: **190 to 280**
- strong player: **280 to 380+**

---

# 8. Pressure Systems

## Main Pressure Source
- structural instability pressure

## Secondary Pressure Source
- enemy disruption

The player should constantly feel:
- “my board is safe for now, but barely”
- “their weak point is one crack away”
- “if I greed this removal, I might lose my own objective”
- “I need to stabilize before I cash in another collapse”

## Escalation Model
Pressure rises through:
- more cracked supports
- more heavy blocks or load zones
- rising attack frequency
- shrinking recovery windows
- higher endgame value on controlled collapses

## Comeback Potential
Comeback is strong because:
- one well-timed chain collapse can swing score heavily
- a leading player can throw by destabilizing their own objective
- defensive play can recover from brink-of-collapse situations

This mode should be volatile, but not random.

---

# 9. Multiplayer Design

## BvB Behavior
- separate boards
- mirrored or equivalent support layout fairness recommended
- attacks and defenses matter significantly
- players race on score while threatening each other’s structural stability indirectly

## Shared / Separate Boards
- separate boards only

## Attack Design
Using your concepts, refined:

### Attacks
- **Crack Tile**
  - weakens one enemy support tile, making it more vulnerable

- **Weight Drop**
  - places a heavy block onto enemy structure, increasing local support demand

- **Aftershock**
  - shakes enemy board, testing weakened structures and possibly triggering collapses

- **Dust Cloud**
  - obscures weak spots and immediate support state briefly

## Defense Design

### Defenses
- **Reinforce**
  - strengthen one support tile

- **Brace**
  - prevent collapse for 3 seconds in a protected local area or board-wide minimal stability window, depending on tuning

- **Survey**
  - highlight weak structural areas

- **Patch**
  - repair a cracked tile back to stable state

## Interaction Philosophy
Attacks should:
- create structural inconvenience
- expose weak planning
- punish unstable boards

They should **not** turn the mode into random grief physics nonsense.

## Spectator Readability
Very strong:
- weak points are readable
- cracks and collapses are dramatic
- shaking and debris are satisfying
- self-damage versus controlled collapse is easy to understand

---

# 10. Puzzle Flow

## Match Start
- countdown
- board begins stable
- objective structure clearly highlighted
- several potential removal candidates visible
- no immediate catastrophic weakness at spawn

## Mid-Match Flow
- remove support
- assess stability
- chain collapse if safe
- recover or reinforce
- pressure enemy with attacks
- board becomes more fragile and more valuable over time

## Endgame Flow
Final 20 seconds:
- instability intensifies
- attacks become more dangerous
- players must balance greed against survival
- one major mistake can decide the match

## Overtime / Sudden Death
Only for exact tie:
- next controlled collapse or higher objective integrity after brief overtime wins

---

# 11. Difficulty Progression

## Early Difficulty
- support chains are readable
- weak points obvious
- controlled removals fairly safe

## Mid Difficulty
- more ambiguous support dependencies
- cracked tiles appear
- attacks start reshaping board confidence

## Late Difficulty
- tighter support margins
- heavier load interactions
- more chained instability
- greed is punished harder
- stabilizing becomes as important as scoring

## Scaling Variables
- support density
- crack frequency
- heavy block frequency
- objective fragility
- attack charge rate
- collapse chain reward strength
- dust/visibility disruption frequency

---

# 12. Animation Language

## Visual Identity
Collapse Grid should feel:
- structural
- brittle
- pressurized
- cracking
- dusty
- heavy but controlled

## Motion Personality
- trembling
- fracturing
- dropping
- bracing
- settling
- collapsing

## Required Feedback Types
- idle structural tension
- weak support pulse
- crack spread
- tile removal
- board shake
- debris fall
- dust plume
- reinforcement glow
- brace field
- controlled collapse reward
- self-damage warning
- win / lose payoff

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `collapse_grid_support_idle.png`
  - purpose: support tile idle tension / subtle shimmer
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `collapse_grid_weak_spot_pulse.png`
  - purpose: unstable support warning
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 10
  - loop: yes while unstable

- `collapse_grid_crack_spread.png`
  - purpose: crack line propagation effect
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `collapse_grid_remove_tile.png`
  - purpose: support removal feedback
  - frame count: 6
  - frame size: 128x128
  - layout: 3x2
  - fps: 14
  - loop: no

- `collapse_grid_shake_structure.png`
  - purpose: board shake / instability pulse
  - frame count: 8
  - frame size: 640x640
  - layout: 4x2
  - fps: 10
  - loop: short loop during instability

- `collapse_grid_debris_fall.png`
  - purpose: falling debris / chunk collapse
  - frame count: 10
  - frame size: 160x160
  - layout: 5x2
  - fps: 16
  - loop: no

- `collapse_grid_dust_plume.png`
  - purpose: collapse dust cloud
  - frame count: 8
  - frame size: 256x256
  - layout: 4x2
  - fps: 12
  - loop: no

- `collapse_grid_controlled_chain.png`
  - purpose: controlled collapse scoring flourish
  - frame count: 10
  - frame size: 320x160
  - layout: 5x2
  - fps: 14
  - loop: no

- `collapse_grid_self_damage_warning.png`
  - purpose: own board danger alert
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 10
  - loop: yes while critical

- `collapse_grid_score_lane.png`
  - purpose: score / stability / danger lane
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `collapse_grid_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `collapse_grid_endgame_pressure.png`
  - purpose: final phase instability framing
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

## Attack / Defense FX Sheets

- `collapse_grid_crack_tile.png`
  - purpose: enemy tile crack attack
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `collapse_grid_weight_drop.png`
  - purpose: heavy block impact onto enemy structure
  - frame count: 8
  - frame size: 160x160
  - layout: 4x2
  - fps: 14
  - loop: no

- `collapse_grid_aftershock.png`
  - purpose: board-wide shake attack
  - frame count: 8
  - frame size: 640x640
  - layout: 4x2
  - fps: 10
  - loop: no

- `collapse_grid_dust_cloud.png`
  - purpose: weak spot obscuring effect
  - frame count: 6
  - frame size: 640x640
  - layout: 3x2
  - fps: 10
  - loop: yes while active

- `collapse_grid_reinforce.png`
  - purpose: support strengthening effect
  - frame count: 6
  - frame size: 128x128
  - layout: 3x2
  - fps: 12
  - loop: no

- `collapse_grid_brace.png`
  - purpose: temporary collapse prevention field
  - frame count: 8
  - frame size: 256x256
  - layout: 4x2
  - fps: 10
  - loop: yes while active

- `collapse_grid_survey.png`
  - purpose: weak area highlight scan
  - frame count: 6
  - frame size: 640x640
  - layout: 3x2
  - fps: 10
  - loop: no

- `collapse_grid_patch.png`
  - purpose: cracked tile repair
  - frame count: 6
  - frame size: 128x128
  - layout: 3x2
  - fps: 12
  - loop: no

## Tile / Support Atlas
- `collapse_grid_support_atlas.png`
  - purpose: standard supports, reinforced supports, cracked supports, heavy blocks, objective anchors, blocked cells, debris fragments
  - frame count: 24+
  - frame size: 64x64
  - layout: flexible atlas
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- subtle structural tension shimmer
- weak-point pulse
- board shake during instability
- progress/stability lane surges

## Support FX
- crack spread lines
- support removal burst
- reinforcement sheen
- patch restoration effect

## Collapse FX
- debris dropping
- dust plume
- controlled-collapse reward flare
- self-damage warning if protected structure is at risk

## Failure FX
- objective danger flashing
- self-collapse damage cues
- weak-point obscuration from Dust Cloud
- no unreadable full-screen mess

## Pressure FX
- escalating instability framing
- danger lane pulsing
- endgame tremor emphasis
- aftershock screen shake

## Scoring FX
- successful removal popup
- controlled chain collapse popup
- stability-preserved bonus cue
- score/stability lane pulse

## Multiplayer FX
- enemy attack cast visibility
- lead shift highlight
- major collapse milestone pulse
- defense activation clarity

---

# 15. UI / HUD Requirements

## Must Display
- timer
- score
- structural health / danger meter
- protected objective integrity
- attack charge
- defense availability
- player card
- username

## HUD Layout Rules
- top center: timer + score + objective integrity
- side: danger/stability lane and charge state
- bottom: player cards
- board center must stay readable, especially around weak points

## Mobile Priority Rules
Always visible:
- timer
- score
- structural health
- objective integrity
- board
- player identity

Can be reduced:
- verbose damage text
- extra decorative labels
- large secondary notifications

---

# 16. Audio Direction

## Music Feel
- tense
- heavy
- brittle
- tactical
- more grounded than mystical
- more structural than explosive

## SFX Categories
- tile removal
- crack spread
- shake
- debris fall
- dust plume
- reinforce
- patch
- aftershock
- score gain
- win / lose

## Sonic Identity
Collapse Grid should sound:
- weighty
- brittle
- pressurized
- physically threatening without requiring real physics chaos
- less flashy than Combo Rush
- less mechanical-energy than Pipe Pulse
- more dangerous than Clear Rush

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Clear Rush
- Shard Stack
- Tile Shift

## How It Is Different
Compared to Clear Rush:
- removals are dangerous and structurally consequential, not just beneficial cleanup

Compared to Shard Stack:
- the challenge is structural support management, not queue-based placement

Compared to Tile Shift:
- board manipulation is secondary; support removal and collapse logic are primary

## What Must Never Happen
- must not become fake physics chaos with no legibility
- must not reward random destruction
- must not let collapses feel arbitrary
- must not make self-damage too forgiving
- must not bury weak-point readability under dust and shake effects

---

# 18. Technical Implementation Notes

## Phaser Notes
- use deterministic support rules, not full real-time physics
- collapse logic should be easy to recalculate
- cracked and reinforced states need clear overlays
- shake and dust must be readable and performant on mobile
- controlled chain detection must distinguish skillful collapse from self-damage

## Data Model Notes
Per player:
- score
- structural integrity
- objective integrity
- controlled collapse count
- self-damage count
- attack charge
- defense cooldown states

Per board:
- support tile states
- crack states
- reinforced states
- heavy block positions
- protected objective nodes
- stability graph / support evaluation
- dust cloud state
- brace state

## Animation Hooks
Trigger on:
- support removal
- crack application
- crack spread
- instability threshold
- board shake
- debris fall
- dust plume
- reinforce
- patch
- aftershock
- controlled collapse
- self-damage
- endgame pressure
- win / loss

## Performance Notes
- stability calculation should remain deterministic and lightweight
- dust and debris FX should be pooled
- avoid expensive per-frame pseudo-physics
- prioritize clear support-state readability over spectacle

---

# 19. Telemetry / Balancing Metrics

Track:
- average score
- successful removals per match
- controlled chain collapse count
- self-damage frequency
- objective integrity loss rate
- attack usage rate
- defense usage rate
- comeback rate
- win rate by first major collapse
- average board health at match end

---

# 20. Final Design Summary

## One-Paragraph Summary
Collapse Grid is Puzzle Rivals’ structural stability battle mode. Players remove or weaken support tiles from an 8x10 support lattice, trying to trigger controlled collapses for score while protecting their own critical objective structure from chain failure. The mode combines deterministic support logic, tactical attacks, and defensive stabilization into a tense puzzle where every removal has consequences. It should feel weighty, readable, and dangerously satisfying, rewarding players who understand structural causality instead of just smashing weak points and hoping the universe is feeling generous.

## Production Priority
- **High**

## Dependencies
- support atlas
- crack / debris / dust / shake FX
- attack and defense FX package
- score / stability / integrity HUD assets
- player card shimmer
- deterministic support-evaluation system