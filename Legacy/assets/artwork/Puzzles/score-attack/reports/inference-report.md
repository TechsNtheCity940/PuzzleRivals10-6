# Inference Report — Score Attack
**Generated:** 2026-04-10T07:18:41.742Z

## Asset Summary
- Total asset requirements: 7
- Explicitly mentioned in design doc: 2
- Inferred from puzzle type/mechanics: 5
- Sprite sheets planned: 3

## Inferred Assets
### Board Cell Sheet (`score-attack_req_board_cell`)
- Priority: high
- Reason: Inferred: board background cells needed

### Match Burst FX Sheet (`score-attack_req_match_burst`)
- Priority: critical
- Reason: Inferred: match clear requires burst FX

### Combo Chain FX Sheet (`score-attack_req_combo_fx`)
- Priority: high
- Reason: Inferred: cascading combos need escalating FX

### Board Danger Overlay (`score-attack_req_board_danger`)
- Priority: high
- Reason: Inferred: board fill mechanic requires danger state overlay

### Gem Fall Animation Sheet (`score-attack_req_fall_sheet`)
- Priority: medium
- Reason: Inferred: cascade requires motion blur on falling gems
