# Simon Says Tile Sprite Package

This package converts the approved A3 diamond tile artwork into Phaser-ready uniform spritesheets.

## Core Deliverables
- Master animation sheet: `06_exports/png/custom_puzzle_pack_a3_simon_says_tiles_floor_tiles_sequence_glow_master_08f_v01.png`
- Per-color glow strips: cyan, purple, green, gold, red
- Idle tile set: `06_exports/png/custom_puzzle_pack_a3_simon_says_tiles_floor_tiles_idle_set_05f_v01.png`
- Phaser metadata JSON and TypeScript helper in `06_exports/engine_ready/phaser/`

## Phaser Notes
- Frame size: 256x256
- Sheet layout: 8 columns x 5 rows
- Master frame indices are row-major
- Recommended default frameRate: 12
- Recommended repeat: -1 for looping glow playback

## Animation Rows
- 0-7: cyan
- 8-15: purple
- 16-23: green
- 24-31: gold
- 32-39: red


## Industrial Square Family
- Master animation sheet: `06_exports/png/custom_puzzle_pack_a3_simon_says_tiles_industrial_floor_tiles_sequence_glow_master_08f_v01.png`
- Idle set: `06_exports/png/custom_puzzle_pack_a3_simon_says_tiles_industrial_floor_tiles_idle_set_09f_v01.png`
- Row order: red_cross, blue_ring, green_octagon, gold_frame, purple_diamond, cyan_clover, orange_dial, pink_square, white_hex
- Usage example: `06_exports/engine_ready/phaser/custom_puzzle_pack_a3_simon_says_tiles_phaser_usage_snippet_v01.ts`
