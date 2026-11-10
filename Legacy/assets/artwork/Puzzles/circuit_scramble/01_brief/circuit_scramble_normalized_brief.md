# Circuit Scramble Normalized Brief

## normalized_summary
- Circuit Scramble is a premium 2D web puzzle strategy game using an exact orthographic top-down board-game camera.
- The batch target is a clean, game-ready emissive overlay sprite sheet for a high-tech laboratory tabletop presentation with blueprint drafting cues and small-scale readability.
- Visual language is technical blue and white energy over a transparent background, designed to tint cleanly in-engine for different player colors.
- The generated sheet must stay text-free, evenly spaced, centered per frame, and consistent in scale, lighting, and materials-free emissive treatment.

## resolved_inputs
- project_name: Circuit Scramble
- mode_name: Circuit Scramble
- platform: web
- genre: competitive sci-fi puzzle strategy
- camera: exact orthographic top-down
- environment_theme: high-tech laboratory tabletop with blueprint drafting aesthetic
- palette: technical blue and white, deep navy base panels, cyan circuitry
- rendering_language: crisp vector-like edges, polished sci-fi tabletop readability
- output_request: clean uniform sprite sheet with transparent background
- overlay_request: emissive-only overlay sheet
- overlay_list:
  - straight_flow
  - elbow_flow
  - t_junction_flow
  - cross_flow
  - splitter_branch_effect
  - booster_surge
  - gate_activation_sparkle
  - trap_redirect_flash
  - hijack_siphon_arc
  - power_core_charge_pulse
  - central_node_radial_burst
- frame_or_tile_size: 256x256 source per tile or frame

## assumptions
- This batch focuses on the emissive-only FX overlay family because that is the concrete generation scope in the request.
- Each listed overlay is treated as one centered tile on a single transparent sprite sheet rather than as a multi-frame animation strip.
- The board-material language informs shape design and glow behavior, but the overlay sheet itself contains no base materials, panels, or metal.
- Safe internal padding is preserved inside each 256x256 cell for downstream sheet slicing.

## risk_flags
- Some requested overlay names imply motion states; this first pass treats them as static key overlays rather than full animated strips.

## production_defaults
- camera: orthographic_top_down
- rendering_approach: clean vector-like sci-fi emissive FX
- source_frame_size: 256x256
- export_format: PNG
- transparent_background: true
- sheet_layout: uniform grid
- readability_priority: small-scale gameplay clarity first
- text_policy: text_free

## recommended_asset_families
- fx_overlays: emissive-only gameplay states and board-energy routing overlays
- static_modules: future base tiles, nodes, gates, and board modules that can sit under the overlays
- ui_icons: future tintable puzzle-state icons using the same energy language

## downstream_handoff
Generate a single transparent overlay sheet using one 256x256 cell per overlay, keeping all elements centered, evenly spaced, and materials-free. Preserve consistent cyan-white emissive language, crisp silhouettes, and top-down technical drafting geometry. Save the raw generation, selected output, export copy, prompt manifest, asset manifest, animation manifest, generation queue, and QC notes under the Circuit Scramble package.
