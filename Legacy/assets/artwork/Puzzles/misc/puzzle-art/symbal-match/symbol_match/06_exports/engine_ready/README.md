# Symbol Match Phaser Integration

Files:
- `symbol_match.animations.ts`: preload and animation registration helpers
- `SymbolMatchDemoScene.ts`: sample 4x4 reveal, hide, shuffle, and pair-match scene
- `phaser_loader_pack.json`: optional loader-pack metadata
- `phaser_spritesheets.json`: explicit sheet registry with frame sizes and animation definitions
- `aseprite_filmstrips.json`: Aseprite-style filmstrip metadata

Recommended back sheets:
- Use `tile_back_shimmer_06f_v01.png` during normal hidden-tile gameplay.
- Use `tile_back_focus_06f_v01.png` for hover, focus, or selected states.
