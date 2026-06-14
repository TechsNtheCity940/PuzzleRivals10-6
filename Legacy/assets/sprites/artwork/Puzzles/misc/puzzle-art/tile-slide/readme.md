# E1 Sliding Puzzle Sprite Package

This package converts the uploaded bomb puzzle artwork into transparent Phaser-ready sliding-tile strips.

## Core Deliverables
- Static tile set: `06_exports/png/tile_set.png`
- Directional master sheets: one each for left, right, up, and down in `06_exports/png/`
- Per-tile directional strips for all 9 tiles
- Phaser metadata and helper in `06_exports/engine_ready/phaser/`

## Phaser Notes
- Frame size: 256x256
- Each per-tile strip: 8 frames
- Recommended frame rate: 14
- Convention: place the animated sprite on the destination cell and play the matching direction strip so the piece arrives into that slot


## Solved FX
- Solved overlay master: `06_exports/png/solved_fx.png`
- Solved overlays are transparent one-shot pulses intended to play when the board is completed.


## Short Filenames
- Per-tile slide exports now use short sequenced names like `tile_slide_up01.png`, `tile_slide_left04.png`, and `tile_slide_down09.png`.

- Short export names now include `tile_set.png`, `slide_left.png`, `slide_right.png`, `slide_up.png`, `slide_down.png`, `solved_fx.png`, and `solved01.png` through `solved09.png`.
