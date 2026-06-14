# D1 Card Flip Sprite Package

This package converts the uploaded card sheet into transparent Phaser-ready card flip strips.

## Core Deliverables
- Back card static: `06_exports/png/custom_puzzle_pack_d1_card_flip_memory_cards_back_static_v01.png`
- Clean face set: `06_exports/png/custom_puzzle_pack_d1_card_flip_memory_cards_faces_clean_set_24f_v01.png`
- Master flip sheet: `06_exports/png/custom_puzzle_pack_d1_card_flip_memory_cards_flip_open_master_24x08f_v01.png`
- Per-card flip strips: `06_exports/png/custom_puzzle_pack_d1_card_flip_memory_cards_card_XX_flip_open_08f_v01.png`
- Phaser metadata and helper: `06_exports/engine_ready/phaser/`

## Phaser Notes
- Frame size: 256x256
- Layout: 24 rows x 8 columns on the master sheet
- Frame rate: 14
- Flip-close: reverse the same 8-frame range


## Card FX
- Match overlay master: `06_exports/png/custom_puzzle_pack_d1_card_flip_memory_cards_match_overlay_master_24x08f_v01.png`
- Mismatch overlay master: `06_exports/png/custom_puzzle_pack_d1_card_flip_memory_cards_mismatch_overlay_master_24x08f_v01.png`
- All overlays are transparent one-shot strips designed to sit above the face-up card sprite.
