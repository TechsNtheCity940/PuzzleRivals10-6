# Color Sequence Phaser Integration

Files:
- `color_sequence.animations.ts`: preload and animation registration helpers
- `ColorSequenceDemoScene.ts`: playable sample scene with a sequence playback loop and directional input mapping
- `phaser_loader_pack.json`: optional loader-pack metadata
- `phaser_spritesheets.json`: explicit sheet registry with frame sizes and animation definitions
- `aseprite_filmstrips.json`: Aseprite-style filmstrip metadata for tools that expect tag-based frame data

Recommended orb sheets:
- Use `*_glow_soft_08f_v01.png` during normal gameplay.
- Use `*_glow_08f_v01.png` only for stronger emphasis moments such as rewards or combo pop.

Input mapping in demo scene:
- `ArrowUp`, `ArrowLeft`, `ArrowDown`, `ArrowRight`
- `Q` for the fifth color slot
- `E` for the sixth color slot
