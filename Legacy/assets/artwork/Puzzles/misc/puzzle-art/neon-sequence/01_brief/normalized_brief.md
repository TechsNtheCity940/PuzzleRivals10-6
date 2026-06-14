# Normalized Brief

## normalized_summary
- Puzzle mode: color-sequence memory challenge on interchangeable 4x4, 6x6, and 8x8 boards.
- Visual target: premium neon arcade UI with top-down orthographic presentation and strong small-scale readability.
- Production target: Phaser-ready transparent sprite sheets with modular base tiles, board backplates, and separate FX overlays.
- Gameplay priority: clear color recognition, fast flash readability, and no embedded text or labels.

## resolved_inputs
- project_name: Phaser Puzzle Pack
- mode_name: Neon Sequence Recall
- genre: memory puzzle / sequence replication
- camera: top_down_orthographic
- theme: neon colored sci-fi
- engine_target: Phaser
- sprite_sheet_requirement: yes
- transparency_requirement: transparent backgrounds for all sprite sheets
- text_policy: no words
- board_sizes: 4x4, 6x6, 8x8
- difficulty_scaling: more colors, faster display, longer sequences

## assumptions
- Six tile colors are used as the baseline family so the pack can cover both 4-color and 6-color difficulty tiers.
- Boards are modular backgrounds while the engine places gameplay tiles above them.
- Effects are separated from base tiles so playback, selection, success, and failure can be layered independently in Phaser.
- Default frame size is 256x256 for gameplay tiles and overlays, 512x512 for board backplates, and 128x128 for UI icons.

## risk_flags
- none

## production_defaults
- export_format: PNG
- transparent_background: true
- animation_layout: one_row_strip
- readability_priority: web and mobile gameplay clarity first
- text_free_assets: true
- source_frame_size: 256x256 baseline

## recommended_asset_families
- board_environment: reusable backplates for 4x4, 6x6, and 8x8 boards
- placeable_pieces: colored light pads that receive player input and CPU playback
- fx_overlays: flash, selection, success, and failure strips
- objectives: countdown and urgency overlays
- ui: wordless icons for repeat, play, speed, timer, and memory cues

## downstream_handoff
Use the board backplates as scene foundations, render colored light tiles on top, and layer FX strips via additive or screen blending during playback. Keep all tile art top-down, centered, transparent, and text-free. Register FX strips as Phaser animations and treat board sheets as static frame atlases.
