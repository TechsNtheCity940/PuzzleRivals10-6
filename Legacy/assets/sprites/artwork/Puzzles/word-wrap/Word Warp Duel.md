# Word Warp Duel
**Internal ID:** `word_warp_duel`  
**Mode Support:** `Both`

---

# 1. Puzzle Identity

## Core Fantasy
Word Warp Duel is Puzzle Rivals’ **competitive word-forging battle mode**. Players scan a live letter grid, trace valid words, and convert language skill into score, tempo, and tactical attacks. The better the word, the stronger the board pressure they can apply.

This mode should feel like:
- pulling words out of chaos faster than your opponent
- spotting hidden language patterns under pressure
- chaining clever words into a mounting offensive
- turning vocabulary into combat energy
- balancing fast easy words against higher-quality, rarer, more valuable plays

The emotional center is:
- “there’s a longer word in here if I can see it”
- “do I cash this short word now or hunt a bigger one?”
- “that rare letter makes this worth a lot more”
- “I can’t just spam junk words if I want the stronger attacks”
- “one great word can flip this whole duel”

This mode must not feel like a boring classroom word search with neon paint thrown on top. That would be an insult to both education and entertainment, which humans somehow manage to do together all the time.

## Cognitive Skill Tested
Primary skill:
- **rapid lexical pattern recognition**

Secondary skills:
- spelling
- vocabulary depth
- board scanning
- path optimization
- rare-letter prioritization
- tempo management
- risk versus reward judgment

## Why It Exists in Puzzle Rivals
Word Warp Duel gives the roster a true **language-based competitive puzzle** that reaches beyond the standard action-puzzle audience. It broadens the game’s appeal while still fitting Puzzle Rivals’ identity:
- fast
- competitive
- readable
- score-driven
- tactically expressive

It also creates a very different type of mastery:
- not route planning
- not shape fitting
- not reflection logic
- but word construction under battle pressure

## What Makes It Unique
Word Warp Duel must be clearly different from every nearby mode:
- unlike **Score Attack**, opportunity is linguistic, not purely visual pattern value
- unlike **Combo Rush**, speed matters, but word quality matters just as much
- unlike **Riddle Relay / Trivia Blitz / Vocabulary Duel**, the challenge is live grid construction, not question-answer recall
- unlike **Glyph Rush**, symbols are letters with semantic weight
- unlike **Memory Flash**, success is not recall-first, but active lexical extraction
- unlike standard word games, attacks are powered by **word quality**, not mindless spam

Word Warp Duel is about **finding, tracing, and weaponizing words**.

---

# 2. Match Role

## Category
- word puzzle
- lexical battle
- speed spelling
- tactical language duel

## Match Style
Recommended defaults:
- BvB tactical word duel
- optional later “Both” support via ranked or asynchronous ladder variants

Best identity:
- **live word-building battle race**

## Recommended Session Length

### BvB
- **90 seconds**

### Optional expanded ranked / event variant
- **100 seconds**

Recommended default:
- **90 seconds**

Why:
- enough time for multiple momentum swings
- enough time for word-quality decisions to matter
- short enough to stay intense on mobile

---

# 3. Core Objective

## Win Condition
A player wins by:
- having the highest score when time expires, **or**
- reaching the score cap first in capped variants

## Lose Condition
A player loses by:
- trailing in score
- relying on weak spam while opponent lands stronger words
- failing to manage board corruption and tactical attacks
- missing rare-word and combo opportunities

## Primary Goal
Build and submit valid words from the live letter grid faster and more efficiently than the opponent.

## Secondary Goals
Advanced players optimize for:
- longer words
- rarer letters
- stronger word-quality ratings
- rapid submission chains
- efficient pathing through dense boards
- timing attacks after high-value lexical plays

---

# 4. Board Layout

## Board Shape
- square live letter grid

## Board Size
Recommended default:
- **6 x 6**

Alternative sizes:
- 5 x 5 for casual / beginner variant
- 7 x 7 for advanced event variant

Recommended shipping size:
- **6 x 6**

Why:
- dense enough to support strong vocabulary play
- readable on mobile
- large enough for long paths without becoming visual soup

## Cell / Tile Count
- 36 active letter cells

## Required Structure Rule
A standard Word Warp Duel board must include:
- at least **2 paths to medium-length words**
- at least **1 high-value rare-letter opportunity**
- at least **1 temptation toward short spam words**
- at least **1 route to a stronger chained follow-up**
- at least **1 possibility for creative longer-word extraction**
- at least **1 corrupted or disrupted board recovery possibility in battle conditions**

This prevents the mode from becoming:
- “tap CAT, tap DOG, pretend it’s deep”

## Letter Distribution Rules
The board should use a tuned letter bag:
- common vowels present enough for playability
- consonant distribution balanced for word density
- rare letters appear intentionally, not too often
- enough structure to reward vocabulary depth without creating constant dead boards

## Tile Types
- Standard letters
- Rare-value letters
- Corrupted / junk letters
- Muted letters
- Temporarily hidden letters under Fog Script
- optional charged letters in advanced variants

## Reserved HUD Space
Must preserve room for:
- top center: timer + score + current chain bonus
- side or top-adjacent: submitted word panel or recent word feed
- bottom left/right: player cards and usernames
- optional attack / defense charge lane
- optional current traced word display

## Mobile Readability Notes
- letters must be bold and unmistakable
- trace path must remain visible without obscuring letters
- corrupted, muted, and hidden states must be immediately distinguishable
- the currently traced word must be large and clear
- recent submissions should not cover the board

---

# 5. Controls and Inputs

## Primary Input
- drag across adjacent letters to form a word path

## Secondary Input
- release to submit
- drag backward over previous letter to undo path step
- optional tap-submit button in accessibility mode

Recommended default:
- **drag-to-trace word path**

## Input Rules
Players may:
- begin on any active valid letter tile
- drag through adjacent letters
- not reuse the same tile twice in one word unless a special rule allows it in a variant
- release to submit
- submit only if the result is a valid dictionary word meeting minimum length

## Word Validity Rules
Recommended default:
- minimum length: **3**
- dictionary validation required
- profanity / banned word filtering if product requires it

## Invalid Input Behavior
If invalid:
- trace line flashes briefly
- word display rejects
- no score awarded
- board remains unchanged
- time lost naturally

---

# 6. Solving Rules

## How the Puzzle Is Solved
Players solve by tracing valid words from adjacent letters on the board and submitting them for score.

## Solve Sequence
1. scan the board for promising letter clusters
2. trace a valid adjacent path
3. current word preview updates live
4. release to submit
5. dictionary checks validity
6. score awarded if valid
7. letters resolve according to board rules
8. player seeks next word immediately

## Board Resolution Rules
Recommended base behavior:
- submitted letters are consumed and replaced
- refill drops or populates from above
- board remains alive and continuously changing

Alternative variant:
- letters persist and only certain modes consume them

Recommended default:
- **consumed and refilled**, because it adds motion, freshness, and tactical board churn

## Partial Progress Rules
- current trace exists only while dragging
- chain bonus persists across rapid valid submissions
- board disruption from attacks creates temporary pressure states

## Failure Cases
- seeing only short obvious words
- ignoring rare-letter potential
- tracing inefficient words while opponent builds strong combos
- wasting time on invalid paths
- allowing corruption and muting to choke good board sectors

## Skill Ceiling Behavior
Top players separate themselves by:
- spotting hidden longer words quickly
- extracting value from rare letters
- chaining good submissions with little downtime
- recognizing high-quality words beyond raw length
- balancing score speed with tactical attack generation
- recovering from corrupted or obscured tiles without panic

---

# 7. Scoring Model

## Base Points by Word Length
Recommended:
- 3 letters: **6 points**
- 4 letters: **10 points**
- 5 letters: **15 points**
- 6 letters: **21 points**
- 7 letters: **28 points**
- 8+ letters: **36+ scaling**

## Rare Letter Bonus
Recommended bonus values:
- each rare letter included: **+3 to +6**
- premium rare combination bonus: **+8**
- rare letters should improve word quality score, not just raw points

## Word Quality Rating
Attacks should be powered by **word quality**, not pure spam count.

Recommended quality components:
- length
- rare-letter value
- dictionary rarity / sophistication band
- no-repeat-word freshness bonus
- efficient rapid submission timing

### Word Quality Charge Model
- short common word = low attack charge
- longer word = medium charge
- long + rare + fast = high charge
- repeating low-value spam should produce sharply reduced charge

## Combo Bonus
Recommended:
- valid words submitted within rapid succession window build chain bonus
- combo timer window: **2.5 seconds**
- 2-word chain: **+4**
- 3-word chain: **+7**
- 4+ chain: **+10 and stronger attack charge gain**

## Penalties
- invalid word: no score
- repeated ultra-common spam words: reduced quality charge
- optional ranked rule: repeated low-quality spam reduces future attack charge efficiency temporarily

## Score Target to Win
Recommended BvB target:
- **240 to 320**

Recommended default:
- **280 points in 90 seconds**

## Tie Rules
1. higher word quality average wins
2. if tied, highest single-word score wins
3. if tied, longer final valid word wins

## Recommended Score Tuning
Expected 90-second results:
- average player: **120 to 190**
- good player: **190 to 290**
- strong player: **290 to 420+**

---

# 8. Pressure Systems

## Main Pressure Source
- lexical tempo pressure

## Secondary Pressure Source
- board disruption pressure

This is the real stress:
- “I need a word now”
- “that row just got shuffled”
- “my vowel got corrupted”
- “I can either submit this short word or hunt a much stronger one”
- “I need quality, not just button-mashing literacy theater”

## Escalation Model
Pressure rises through:
- increasing attack frequency
- more corrupted / muted / hidden tiles
- stronger score gap visibility
- endgame urgency rewarding faster submission decisions

## Comeback Potential
Comeback is strong because:
- one excellent long word with rare letters can spike score and attack charge
- a player can recover from disruption through superior word vision
- spam-heavy opponent can collapse if their board quality falls off

---

# 9. Multiplayer Design

## BvB Behavior
- separate live letter boards
- mirrored or fairness-seeded letter bag recommended
- same match timer
- tactical attacks and defenses enabled

## Expanded Modes
If expanded later:
- ranked ladder versions
- asynchronous challenge format
- tournament objective variants

## Shared / Separate Boards
- separate boards only

## Attack Design
Using your concepts, refined:

### Attacks
- **Vowel Drain**
  - replaces one enemy vowel with junk or a low-value corrupted tile

- **Shuffle Hex**
  - reorders one enemy row, forcing board rescan

- **Mute Tile**
  - disables one tile temporarily so it cannot be used in a word path

- **Fog Script**
  - briefly hides some enemy letters, leaving them uncertain or obscured

## Defense Design

### Defenses
- **Dictionary Pulse**
  - highlights one or more possible word paths briefly

- **Letter Cleanse**
  - restores corrupted or junked letters

- **Lock Word**
  - preserves current chain bonus through a short gap or disruption window

- **Quick Ink**
  - speeds trace responsiveness and submission flow briefly

## Interaction Philosophy
Attacks should:
- distort language visibility
- break tempo
- reduce clarity
- make the board harder to parse

They should **not** make the board unreadable garbage. Frustration is not depth, despite how often games seem to confuse the two.

## Spectator Readability
Excellent:
- traced words are readable
- long words are exciting
- rare-letter plays are dramatic
- corruption and fog are obvious
- word quality creates visible hype moments

---

# 10. Puzzle Flow

## Match Start
- countdown
- board fully visible
- first word opportunities available immediately
- no opening disruption

## Mid-Match Flow
- trace word
- submit
- letters pop and refill
- chain bonus rises
- attacks/defenses trigger
- board reshapes continuously

## Endgame Flow
Final 20 seconds:
- players choose between safe short words and high-value risky searches
- attacks become more decisive
- board disruption management matters heavily
- score swings can accelerate through strong quality plays

## Overtime / Sudden Death
Only for exact tie state:
- next valid high-quality word wins
- if both submit simultaneously, higher score word wins

---

# 11. Difficulty Progression

## Early Difficulty
- obvious medium words available
- low disruption
- healthy vowel distribution
- clear trace readability

## Mid Difficulty
- letter board becomes more volatile
- attacks begin reshaping high-value areas
- players must choose better between speed and quality

## Late Difficulty
- board disruption and timer pressure both matter
- high-value long words become more decisive
- poor board hygiene or over-spam gets punished naturally

## Scaling Variables
- board size
- vowel/consonant distribution
- rare letter density
- corruption frequency
- attack charge generation
- combo timer length
- refill behavior
- dictionary strictness / word rarity weighting

---

# 12. Animation Language

## Visual Identity
Word Warp Duel should feel:
- inky
- arcane-tech
- sharp
- clever
- fast
- premium and readable

## Motion Personality
- writing
- tracing
- popping
- streaking
- glitching
- flowing into score rails

## Required Feedback Types
- letter idle life
- trace line
- valid submission pop
- invalid reject
- ink trail word submission
- rare-word dramatic burst
- corrupted letter hiss/glitch
- board disruption cues
- combo chain flare
- win / lose payoff

---

# 13. Sprite Sheet Requirements

## Core Sprite Sheets

- `word_warp_letter_idle.png`
  - purpose: idle letter tile shimmer
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 8
  - loop: yes

- `word_warp_trace_ink.png`
  - purpose: live traced word path / ink trail
  - frame count: 8
  - frame size: 128x128
  - layout: 4x2
  - fps: 12
  - loop: yes while tracing

- `word_warp_valid_submit.png`
  - purpose: valid word submission pop
  - frame count: 8
  - frame size: 160x160
  - layout: 4x2
  - fps: 14
  - loop: no

- `word_warp_invalid_reject.png`
  - purpose: invalid word/path rejection
  - frame count: 6
  - frame size: 128x128
  - layout: 3x2
  - fps: 14
  - loop: no

- `word_warp_letter_score_fly.png`
  - purpose: letters popping and flying into score bar
  - frame count: 10
  - frame size: 192x96
  - layout: 5x2
  - fps: 16
  - loop: no

- `word_warp_ink_submit_trail.png`
  - purpose: submitted words creating ink trails
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `word_warp_rare_word_burst.png`
  - purpose: dramatic rare-word activation burst
  - frame count: 10
  - frame size: 256x256
  - layout: 5x2
  - fps: 14
  - loop: no

- `word_warp_combo_chain.png`
  - purpose: rapid-submission chain flare
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `word_warp_score_lane.png`
  - purpose: score / word quality meter
  - frame count: 8
  - frame size: 128x512
  - layout: 2x4
  - fps: 10
  - loop: yes

- `word_warp_player_card_shimmer.png`
  - purpose: player card shimmer
  - frame count: 6
  - frame size: 512x160
  - layout: 3x2
  - fps: 8
  - loop: yes

- `word_warp_endgame_pressure.png`
  - purpose: endgame urgency framing
  - frame count: 6
  - frame size: 1024x256
  - layout: 3x2
  - fps: 10
  - loop: yes

## Attack / Defense FX Sheets

- `word_warp_vowel_drain.png`
  - purpose: vowel corruption / replacement attack
  - frame count: 8
  - frame size: 160x160
  - layout: 4x2
  - fps: 14
  - loop: no

- `word_warp_shuffle_hex.png`
  - purpose: row reorder attack effect
  - frame count: 8
  - frame size: 640x128
  - layout: 4x2
  - fps: 14
  - loop: no

- `word_warp_mute_tile.png`
  - purpose: disabled tile effect
  - frame count: 8
  - frame size: 96x96
  - layout: 4x2
  - fps: 10
  - loop: yes while active

- `word_warp_fog_script.png`
  - purpose: hidden-letter fog overlay
  - frame count: 6
  - frame size: 640x640
  - layout: 3x2
  - fps: 10
  - loop: yes while active

- `word_warp_dictionary_pulse.png`
  - purpose: highlight possible word path
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 12
  - loop: no

- `word_warp_letter_cleanse.png`
  - purpose: restore corrupted tiles
  - frame count: 6
  - frame size: 160x160
  - layout: 3x2
  - fps: 12
  - loop: no

- `word_warp_lock_word.png`
  - purpose: preserve current chain bonus
  - frame count: 6
  - frame size: 256x128
  - layout: 3x2
  - fps: 12
  - loop: no

- `word_warp_quick_ink.png`
  - purpose: speed boost / handwriting acceleration effect
  - frame count: 8
  - frame size: 256x128
  - layout: 4x2
  - fps: 12
  - loop: yes while active

## Letter Atlas
- `word_warp_letter_atlas.png`
  - purpose: letters, rare-letter states, corrupted tiles, muted tiles, junk glyphs
  - frame count: 30+
  - frame size: 64x64
  - layout: flexible atlas
  - fps: static atlas
  - loop: n/a

---

# 14. VFX Requirements

## Board FX
- subtle ink-tech shimmer
- active trace path glow
- score lane surges
- row shuffle feedback
- fog obscuration clarity

## Letter FX
- letter pop on use
- rare letter glint
- corrupted hiss and glitch
- mute tile dim/lock pulse

## Solve FX
- submitted words should feel authored, not merely removed
- words should create visible ink trails
- higher-quality words deserve stronger identity than short spam

## Failure FX
- invalid word reject flash
- dead trace fade
- no giant punishment visuals

## Pressure FX
- corruption spread cues
- fog script overlay
- endgame urgency framing
- quality meter surge for strong words

## Scoring FX
- letters fly into score bar
- rare letters spike brighter
- combo chain flares
- high-quality word popups

## Multiplayer FX
- enemy attack cast visibility
- lead shift highlight
- strong-word milestone pulse
- defense activation cues

---

# 15. UI / HUD Requirements

## Must Display
- timer
- score
- current chain bonus
- current traced word
- recent submitted word or small history feed
- attack quality / charge
- player card
- username

## HUD Layout Rules
- top center: timer + score + chain bonus
- side or upper side: recent words / quality meter
- bottom: player cards
- active traced word should appear clearly without covering the grid

## Mobile Priority Rules
Always visible:
- timer
- score
- current traced word
- board
- player identity
- attack / defense readiness

Can be reduced:
- larger history feed
- flavor text
- verbose dictionary messages

---

# 16. Audio Direction

## Music Feel
- fast
- clever
- stylish
- slightly arcane-digital
- energetic without becoming mush

## SFX Categories
- trace start
- trace move
- submit
- invalid reject
- rare word burst
- corruption hiss
- row shuffle
- defense cast
- score gain
- win / lose

## Sonic Identity
Word Warp Duel should sound:
- literate, sharp, and alive
- more inky and intelligent than most of the roster
- less explosive than Combo Rush
- less mechanical than Pipe Pulse
- less mystical than Mirror Maze

---

# 17. Anti-Overlap Check

## Closest Puzzle Comparisons
- Vocabulary Duel
- Glyph Rush
- Trivia Blitz
- Riddle Relay

## How It Is Different
Compared to Vocabulary Duel:
- Word Warp Duel is live board extraction and battle tempo, not vocabulary challenge prompt solving

Compared to Glyph Rush:
- letters have dictionary meaning and word structure, not just symbolic recognition

Compared to Trivia / Riddle modes:
- this is active board construction, not question answering

## What Must Never Happen
- must not become low-quality word spam
- must not reward short words too generously
- must not make corruption states unreadable
- must not let rare-letter scoring dominate so hard that basic play feels pointless
- must not feel like a static word search

---

# 18. Technical Implementation Notes

## Phaser Notes
- live trace line must feel smooth and immediate
- dictionary validation needs to be fast
- consumed letter refill should be readable and satisfying
- letter state overlays must remain clear on mobile
- recent word and current trace UI should update with minimal latency

## Data Model Notes
Per player:
- score
- current chain
- word quality charge
- submitted word history
- invalid word count
- rare-letter usage
- attack/defense cooldown states

Per board:
- letter grid
- corruption states
- mute states
- fog states
- refill bag / distribution state

## Animation Hooks
Trigger on:
- trace start
- trace continue
- valid submit
- invalid reject
- letter consume
- refill
- rare word
- corruption apply
- cleanse
- shuffle row
- combo chain
- endgame urgency
- win / loss

## Performance Notes
- dictionary checks must not stall input
- trace path rendering must be lightweight
- board refill and score-fly animations should be pooled where possible
- fog and corruption overlays must stay readable without excessive alpha stacking

---

# 19. Telemetry / Balancing Metrics

Track:
- average word length
- average word quality score
- rare-letter usage rate
- invalid word rate
- combo chain frequency
- attack generation by word quality band
- spam short-word frequency
- comeback rate
- win rate by early rare-word lead
- quit rate under corruption-heavy pressure

---

# 20. Final Design Summary

## One-Paragraph Summary
Word Warp Duel is Puzzle Rivals’ live lexical battle mode. Players trace and submit words from a 6x6 letter grid, scoring based on word length, rare-letter value, and rapid high-quality submissions. The board constantly shifts through consumption, refill, corruption, muting, and visibility disruption, while tactical attacks and defenses reward strong language play rather than mindless spam. It should feel fast, stylish, and surprisingly competitive, with vocabulary depth and board-reading speed working together to drive score and combat pressure.

## Production Priority
- **High**

## Dependencies
- letter atlas
- trace / submit / score-fly FX
- corruption / mute / fog FX
- attack and defense visual package
- score / quality lane assets
- player card shimmer
- fast dictionary validation pipeline
- fairness-seeded letter bag generation