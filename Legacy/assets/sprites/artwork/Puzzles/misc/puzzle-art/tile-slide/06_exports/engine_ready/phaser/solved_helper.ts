export const slidingPuzzleSolvedOverlaySheet = {
  key: 'e1_sliding_puzzle_sliding_tiles_solved_overlay_master',
  path: 'solved_fx.png',
  frameWidth: 256,
  frameHeight: 256,
  margin: 0,
  spacing: 0,
} as const;

export const slidingPuzzleSolvedOverlayAnimations = [
  { key: 'e1_sliding_puzzle_tile_01_top_left_solved_overlay', start: 0, end: 7, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_02_top_center_solved_overlay', start: 8, end: 15, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_03_top_right_solved_overlay', start: 16, end: 23, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_04_mid_left_solved_overlay', start: 24, end: 31, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_05_mid_center_solved_overlay', start: 32, end: 39, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_06_mid_right_solved_overlay', start: 40, end: 47, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_07_bottom_left_solved_overlay', start: 48, end: 55, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_08_bottom_center_solved_overlay', start: 56, end: 63, frameRate: 16, repeat: 0, hideOnComplete: true },
  { key: 'e1_sliding_puzzle_tile_09_blank_solved_overlay', start: 64, end: 71, frameRate: 16, repeat: 0, hideOnComplete: true },
] as const;

export function createSlidingPuzzleSolvedOverlayAnimations(scene: Phaser.Scene) {
  for (const anim of slidingPuzzleSolvedOverlayAnimations) {
    if (scene.anims.exists(anim.key)) continue;
    scene.anims.create({
      key: anim.key,
      frames: scene.anims.generateFrameNumbers(slidingPuzzleSolvedOverlaySheet.key, { start: anim.start, end: anim.end }),
      frameRate: anim.frameRate,
      repeat: anim.repeat,
      hideOnComplete: anim.hideOnComplete,
    });
  }
}
