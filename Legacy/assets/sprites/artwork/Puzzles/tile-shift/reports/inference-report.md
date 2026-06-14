# Inference Report — Tile Shift
**Generated:** 2026-04-10T07:19:23.691Z

## Asset Summary
- Total asset requirements: 9
- Explicitly mentioned in design doc: 2
- Inferred from puzzle type/mechanics: 7
- Sprite sheets planned: 7

## Inferred Assets
### Flow Overlay Sheet (`tile-shift_req_flow_overlay`)
- Priority: critical
- Reason: Inferred: flowing energy must have a looping overlay animation layer

### Pressure Buildup Overlay (`tile-shift_req_pressure_overlay`)
- Priority: high
- Reason: Inferred: pressure escalation requires a tint/overlay animation

### Burst FX Sheet (`tile-shift_req_burst_fx`)
- Priority: critical
- Reason: Inferred: pressure overflow must have a burst explosion animation

### Connection Pulse Indicator (`tile-shift_req_connection_pulse`)
- Priority: high
- Reason: Inferred: valid connection feedback needs a brief snap indicator

### Route Complete FX (`tile-shift_req_route_complete_fx`)
- Priority: high
- Reason: Inferred: route completion needs a celebratory glow flash

### Pressure Warning Overlay (`tile-shift_req_pressure_warning`)
- Priority: high
- Reason: Inferred: near-burst segments need a warning pulse loop

### Flow Glow Aura Sheet (`tile-shift_req_glow_aura`)
- Priority: medium
- Reason: Inferred: active flow channels should have ambient energy glow
