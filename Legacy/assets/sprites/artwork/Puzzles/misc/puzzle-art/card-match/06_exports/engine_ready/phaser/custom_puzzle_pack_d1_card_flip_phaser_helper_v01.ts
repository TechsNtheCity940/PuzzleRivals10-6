export const memoryCardFlipSheet = {
  key: 'd1_card_flip_memory_cards_flip_open_master',
  path: 'custom_puzzle_pack_d1_card_flip_memory_cards_flip_open_master_24x08f_v01.png',
  frameWidth: 256,
  frameHeight: 256,
  margin: 0,
  spacing: 0,
} as const;

export const memoryCardFlipAnimations = [
  { key: 'd1_card_flip_card_01_flip_open', start: 0, end: 7, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_02_flip_open', start: 8, end: 15, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_03_flip_open', start: 16, end: 23, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_04_flip_open', start: 24, end: 31, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_05_flip_open', start: 32, end: 39, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_06_flip_open', start: 40, end: 47, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_07_flip_open', start: 48, end: 55, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_08_flip_open', start: 56, end: 63, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_09_flip_open', start: 64, end: 71, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_10_flip_open', start: 72, end: 79, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_11_flip_open', start: 80, end: 87, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_12_flip_open', start: 88, end: 95, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_13_flip_open', start: 96, end: 103, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_14_flip_open', start: 104, end: 111, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_15_flip_open', start: 112, end: 119, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_16_flip_open', start: 120, end: 127, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_17_flip_open', start: 128, end: 135, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_18_flip_open', start: 136, end: 143, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_19_flip_open', start: 144, end: 151, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_20_flip_open', start: 152, end: 159, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_21_flip_open', start: 160, end: 167, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_22_flip_open', start: 168, end: 175, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_23_flip_open', start: 176, end: 183, frameRate: 14, repeat: 0 },
  { key: 'd1_card_flip_card_24_flip_open', start: 184, end: 191, frameRate: 14, repeat: 0 },
] as const;

export function createMemoryCardFlipAnimations(scene: Phaser.Scene) {
  for (const anim of memoryCardFlipAnimations) {
    if (scene.anims.exists(anim.key)) continue;
    scene.anims.create({
      key: anim.key,
      frames: scene.anims.generateFrameNumbers(memoryCardFlipSheet.key, { start: anim.start, end: anim.end }),
      frameRate: anim.frameRate,
      repeat: anim.repeat,
    });
  }
}

export function playMemoryCardFlipClose(sprite: Phaser.GameObjects.Sprite, scene: Phaser.Scene, cardIndex: number) {
  const start = (cardIndex - 1) * 8;
  const frames = scene.anims.generateFrameNumbers(memoryCardFlipSheet.key, { start, end: start + 7 }).reverse();
  scene.anims.create({ key: `d1_card_flip_card_${String(cardIndex).padStart(2, '0')}_flip_close_temp`, frames, frameRate: 14, repeat: 0 });
  sprite.play(`d1_card_flip_card_${String(cardIndex).padStart(2, '0')}_flip_close_temp`);
}
