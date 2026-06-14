export const industrialSimonSaysTileSheet = {
  key: 'a3_simon_says_tiles_industrial_floor_tiles_glow_master',
  path: 'custom_puzzle_pack_a3_simon_says_tiles_industrial_floor_tiles_sequence_glow_master_08f_v01.png',
  frameWidth: 256,
  frameHeight: 256,
  margin: 0,
  spacing: 0,
} as const;

export const industrialSimonSaysTileAnimations = [
  { key: 'a3_simon_says_tiles_red_cross_glow', start: 0, end: 7, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_blue_ring_glow', start: 8, end: 15, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_green_octagon_glow', start: 16, end: 23, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_gold_frame_glow', start: 24, end: 31, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_purple_diamond_glow', start: 32, end: 39, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_cyan_clover_glow', start: 40, end: 47, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_orange_dial_glow', start: 48, end: 55, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_pink_square_glow', start: 56, end: 63, frameRate: 12, repeat: -1 },
  { key: 'a3_simon_says_tiles_white_hex_glow', start: 64, end: 71, frameRate: 12, repeat: -1 },
] as const;

export function createIndustrialSimonSaysTileAnimations(scene: Phaser.Scene) {
  for (const anim of industrialSimonSaysTileAnimations) {
    if (scene.anims.exists(anim.key)) continue;
    scene.anims.create({
      key: anim.key,
      frames: scene.anims.generateFrameNumbers(industrialSimonSaysTileSheet.key, { start: anim.start, end: anim.end }),
      frameRate: anim.frameRate,
      repeat: anim.repeat,
    });
  }
}