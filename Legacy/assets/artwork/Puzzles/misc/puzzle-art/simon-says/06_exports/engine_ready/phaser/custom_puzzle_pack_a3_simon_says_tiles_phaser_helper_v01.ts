export const simonSaysTileSheet = {
  key: 'a3_simon_says_tiles_floor_tiles_glow_master',
  path: 'custom_puzzle_pack_a3_simon_says_tiles_floor_tiles_sequence_glow_master_08f_v01.png',
  frameWidth: 256,
  frameHeight: 256,
  margin: 0,
  spacing: 0,
} as const;

export const simonSaysTileAnimations = [
  { key: 'a3_simon_says_tiles_cyan_glow', start: 0, end: 7, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_purple_glow', start: 8, end: 15, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_green_glow', start: 16, end: 23, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_gold_glow', start: 24, end: 31, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_red_glow', start: 32, end: 39, frameRate: 12, repeat: -1 },
] as const;

export function createSimonSaysTileAnimations(scene: Phaser.Scene) {
  for (const anim of simonSaysTileAnimations) {
    if (scene.anims.exists(anim.key)) continue;
    scene.anims.create({
      key: anim.key,
      frames: scene.anims.generateFrameNumbers(simonSaysTileSheet.key, { start: anim.start, end: anim.end }),
      frameRate: anim.frameRate,
      repeat: anim.repeat,
    });
  }
}
