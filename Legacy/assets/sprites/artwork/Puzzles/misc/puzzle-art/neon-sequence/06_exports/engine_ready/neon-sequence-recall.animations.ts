import Phaser from 'phaser';

export const neonSequenceRecallAssetManifest = {
  board_sizes_static: { key: 'board_sizes_static', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_board_environment_board_sizes_static_03f_v03.png', frameWidth: 512, frameHeight: 512 },
  light_tiles_static: { key: 'light_tiles_static', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_placeable_pieces_light_tiles_static_06f_v03.png', frameWidth: 256, frameHeight: 256 },
  flash_overlay_loop: { key: 'flash_overlay_loop', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_fx_overlays_flash_overlay_loop_08f_v03.png', frameWidth: 256, frameHeight: 256 },
  selection_ring_loop: { key: 'selection_ring_loop', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_fx_overlays_selection_ring_loop_08f_v03.png', frameWidth: 256, frameHeight: 256 },
  confirm_burst_oneshot: { key: 'confirm_burst_oneshot', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_fx_overlays_confirm_burst_oneshot_08f_v03.png', frameWidth: 256, frameHeight: 256 },
  error_burst_oneshot: { key: 'error_burst_oneshot', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_fx_overlays_error_burst_oneshot_08f_v03.png', frameWidth: 256, frameHeight: 256 },
  countdown_ring_loop: { key: 'countdown_ring_loop', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_objectives_countdown_ring_loop_08f_v03.png', frameWidth: 256, frameHeight: 256 },
  ui_icons_static: { key: 'ui_icons_static', path: 'assets/phaser_puzzle_pack/neon_sequence_recall/phaser_puzzle_pack_neon_sequence_recall_ui_ui_icons_static_06f_v03.png', frameWidth: 128, frameHeight: 128 },
} as const;

export function preloadNeonSequenceRecallAssets(scene: Phaser.Scene) {
  Object.values(neonSequenceRecallAssetManifest).forEach((asset) => {
    scene.load.spritesheet(asset.key, asset.path, {
      frameWidth: asset.frameWidth,
      frameHeight: asset.frameHeight,
    });
  });
}

export function registerNeonSequenceRecallAnimations(scene: Phaser.Scene) {
  const animations = [
    { key: 'flash_overlay_loop', asset: 'flash_overlay_loop', frameRate: 12, repeat: -1, end: 7 },
    { key: 'selection_ring_loop', asset: 'selection_ring_loop', frameRate: 10, repeat: -1, end: 7 },
    { key: 'confirm_burst_oneshot', asset: 'confirm_burst_oneshot', frameRate: 16, repeat: 0, end: 7 },
    { key: 'error_burst_oneshot', asset: 'error_burst_oneshot', frameRate: 18, repeat: 0, end: 7 },
    { key: 'countdown_ring_loop', asset: 'countdown_ring_loop', frameRate: 8, repeat: -1, end: 7 },
  ];

  animations.forEach((entry) => {
    if (scene.anims.exists(entry.key)) return;
    scene.anims.create({
      key: entry.key,
      frames: scene.anims.generateFrameNumbers(entry.asset, { start: 0, end: entry.end }),
      frameRate: entry.frameRate,
      repeat: entry.repeat,
    });
  });
}

export const neonSequenceRecallFrameSets = {
  boardSizes: { fourByFour: 0, sixBySix: 1, eightByEight: 2 },
  lightTiles: { cyan: 0, magenta: 1, lime: 2, amber: 3, violet: 4, coral: 5 },
  uiIcons: { play: 0, repeat: 1, speed: 2, sequence: 3, memory: 4, timer: 5 },
} as const;
