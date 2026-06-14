# Batch Brief

Project slug: memory_lights
Mode slug: color_sequence

Assumptions:
- Camera is orthographic top-down because this is a puzzle memory mode.
- Phaser target uses fixed 256x256 frames for all animated assets.
- The uploaded 8-frame button pulse strip is the visual source of truth for pad timing.
- The uploaded orb set provides the base orb family; tuned glow loops are rebuilt procedurally in soft and full intensity variants.
- Assets are text-free and exported with transparent backgrounds for direct Phaser use.
