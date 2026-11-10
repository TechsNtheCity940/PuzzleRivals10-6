# Circuit Scramble UI Kit Brief

## normalized_summary
- Circuit Scramble needs a premium 2D web-game UI kit sprite sheet in the same exact orthographic top-down blueprint-tabletop style as the core art batch.
- The sheet is a transparent 4x3 atlas with twelve centered UI assets, one per 256x256 cell, intended for direct slicing.
- Visual language must stay technical blue and white with deep navy panel accents, white ceramic and brushed alloy module cues, etched grid details, cyan circuitry glow, and clean vector-like edges.
- The batch must remain text-free, logo-free, and readable at small size with uniform scale, lighting, and proportions across all cells.

## resolved_inputs
- project_name: Circuit Scramble
- mode_name: Circuit Scramble
- platform: web
- camera: exact orthographic top-down
- visual_theme: high-tech laboratory tabletop with blueprint drafting aesthetic
- palette: technical blue and white with deep navy base panels and glowing cyan circuitry
- output_request: clean uniform game-ready sprite sheet with transparent background
- grid_layout: 4 columns by 3 rows
- source_frame_size: 256x256
- ui_kit_assets:
  - panel_corner_frame_piece
  - panel_edge_segment
  - score_badge
  - turn_indicator_ring
  - wire_icon
  - logic_gate_icon
  - trap_icon
  - hijack_icon
  - splitter_icon
  - booster_icon
  - battery_icon
  - central_node_icon

## assumptions
- This sheet is treated as a static icon-and-frame atlas rather than an animated UI strip.
- Transparent padding is preserved inside each cell so the assets can be used on variable UI layouts.
- Panel assets are modular pieces for later composition, while the other entries are standalone icons or badges.

## risk_flags
- none

## production_defaults
- camera: orthographic_top_down
- rendering_approach: premium blueprint tabletop UI modules
- export_format: PNG
- transparent_background: true
- readability_priority: small-scale gameplay clarity first
- text_policy: text_free

## downstream_handoff
Generate a 4x3 transparent UI sheet with one centered asset per 256x256 cell. Keep all assets in the same Circuit Scramble material and lighting language, with crisp technical edges, cyan emissive traces, and readable silhouettes. Preserve exact top-down presentation and avoid perspective drift, text, or decorative clutter.
