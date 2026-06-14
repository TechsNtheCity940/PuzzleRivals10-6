export const slidingPuzzleTileFrame = { frameWidth: 256, frameHeight: 256, margin: 0, spacing: 0 } as const;

export const slidingPuzzleTileSheets = {
  left: { key: 'e1_sliding_puzzle_sliding_tiles_left_master', path: 'slide_left.png' },
  right: { key: 'e1_sliding_puzzle_sliding_tiles_right_master', path: 'slide_right.png' },
  up: { key: 'e1_sliding_puzzle_sliding_tiles_up_master', path: 'slide_up.png' },
  down: { key: 'e1_sliding_puzzle_sliding_tiles_down_master', path: 'slide_down.png' },
} as const;


export function createSlidingPuzzleTileAnimations(scene: Phaser.Scene) {

  const directions = Object.keys(slidingPuzzleTileSheets) as Array<keyof typeof slidingPuzzleTileSheets>;

  const slugs = ['tile_01_top_left','tile_02_top_center','tile_03_top_right','tile_04_mid_left','tile_05_mid_center','tile_06_mid_right','tile_07_bottom_left','tile_08_bottom_center','tile_09_blank'] as const;

  const fps = 14;

  for (const direction of directions) {

    for (let i = 0; i < slugs.length; i += 1) {

      const key = `e1_sliding_puzzle_${slugs[i]}_slide_${direction}`;

      if (scene.anims.exists(key)) continue;

      scene.anims.create({

        key,

        frames: scene.anims.generateFrameNumbers(slidingPuzzleTileSheets[direction].key, { start: i * 8, end: i * 8 + 7 }),

        frameRate: fps,

        repeat: 0,

      });

    }

  }

}
