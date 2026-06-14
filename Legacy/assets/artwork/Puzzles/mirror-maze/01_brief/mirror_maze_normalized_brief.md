# Mirror Maze Art Workup

## normalized_summary
- Mirror Maze is a top-down neon puzzle game about redirecting beams of light with angled mirrors to reach targets.
- The artwork package should support modular puzzle objects, beam routing, readable activations, blocked-beam impact states, and engine-ready animation strips.
- Visual direction is bright, premium, high-contrast sci-fi neon with clean silhouettes and strong readability on a puzzle board.
- Production output should separate static puzzle pieces from animated beam, source, target, activator, and impact FX.

## resolved_inputs
- project_name: Mirror Maze
- mode_name: Main Puzzle
- genre: top-down light-routing puzzle
- camera: top-down
- visual_theme: bright neon energy puzzle
- core_mechanics: mirrors deflect beams at 90 degrees, prisms split beams, sources emit beams, targets receive beams, activators trigger secondary beams, blockers stop beams
- required_assets: traveling beams, angled mirrors, prisms, start source, end target, activators, blocked-beam impact FX
- output_need: artwork, spritesheets, animations, full workup

## assumptions
- Use orthographic top-down camera for all gameplay pieces and FX.
- Use transparent PNG output with one-row animation strips and centered assets.
- Use 256x256 as the default cell or frame size for puzzle objects and 1024x256 for four-frame beam strips where needed.
- Treat the end object as a target receiver rather than a literal second source.
- Separate beam body segments from impact and activation FX so the game can compose them in-engine.

## risk_flags
- The brief does not specify final in-game tile size, so this package is authored at high-resolution source scale for later downscaling.
- Beam traversal can be implemented either as animated straight segments, tileable strips, or additive FX overlays; this package prepares art for all three but engine integration will choose the final combination.

## production_defaults
- camera: orthographic_top_down
- source_frame_size: 256x256
- beam_strip_size: 1024x256
- export_format: PNG
- transparent_background: true
- animation_layout: one_row_strip
- readability_priority: puzzle clarity first
- text_policy: text_free

## recommended_asset_families
- beam_fx: traveling beam core, pulse overlays, blocked impact bursts, prism split bursts
- routing_pieces: mirrors, prisms, blockers
- source_target: beam emitter and target receiver states
- trigger_system: activators and secondary-source ignition effects
- ui_icons: optional board-legend icons for sources, targets, mirrors, prisms, blockers, activators

## downstream_handoff
Use this brief as the source of truth for style lock, prompt pack, manifests, generation queue, and QC. Keep all gameplay objects centered, top-down, transparent, and readable against dark puzzle boards. Separate static puzzle pieces from animated energy overlays, and keep beams, impacts, and activator states generator-ready as horizontal strips.
