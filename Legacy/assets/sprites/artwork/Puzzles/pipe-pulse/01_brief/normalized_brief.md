# Normalized Brief

## normalized_summary
- Pipe Pulse is a neon-colored top-down flow puzzle focused on rotating modular pipe pieces to connect a pump to a reservoir.
- The art package should support a complete gameplay set: board tiles, straight/corner/T/4-way pipes, pumps, reservoirs, broken pipe hazards, flow overlays, burst FX, and small UI support icons.
- Production priority is gameplay readability first, with bold cyber-neon materials and clean modular silhouettes that hold up at puzzle-board scale.
- Downstream outputs should be sprite-sheet friendly, text-free, transparent where appropriate, and ready for Phaser-style slicing assumptions.

## resolved_inputs
- project_name: Pipe Pulse
- genre: flow puzzle
- core_mechanic: rotate pipe pieces to route water from one end to the other
- required_piece_types: board tiles, corner pieces, straight pieces, T pieces, 4-way pieces, reservoirs, pumps
- requested_hazards: broken pipes, burst events
- requested_animation: flow animation, burst animation
- visual_theme: neon colored
- output_mode: full workup plus generated artwork

## assumptions
- Use orthographic top-down camera because the game is puzzle-forward and connection readability matters more than depth.
- Use crisp premium pixel-art style with neon sci-fi plating rather than painterly rendering.
- Treat exported gameplay assets as transparent PNG sheets with one-row animation strips for moving elements.
- Keep base modules and flow FX separated so the game can layer them if needed.
- Include a small UI icon strip because the user asked for a full workup.

## risk_flags
- Exact frame slicing coordinates are not guaranteed by image generation alone and may need one cleanup pass if the engine requires strict atlas coordinates.
- Rotational variants are represented visually in the generated sheets, but engine-ready rotated-state extraction may still need a production slicing pass.

## production_defaults
- camera: orthographic_top_down
- source_frame_size: 256x256
- export_format: PNG
- transparent_background: true
- animation_layout: one_row_strip
- text_policy: text_free
- readability_priority: small_scale_board_readability

## recommended_asset_families
- board_environment: puzzle grid tiles, sockets, and energy board foundations
- gameplay_pieces: pipe modules, pumps, reservoirs, and rotation-friendly shapes
- hazards: broken pipes and damaged modules
- fx_overlays: flowing water, pressure pulses, leaks, and burst events
- ui_icons: rotate, reroute, warning, burst, and goal status symbols

## downstream_handoff
Generate a cohesive neon puzzle asset batch from this brief. Keep silhouettes modular, centered, and readable. Separate base pieces from animated flow and burst FX wherever possible. Use transparent backgrounds, Phaser-friendly sprite-sheet assumptions, and consistent neon cyan-magenta-lime palette language across the entire package.
