export const memoryCardMismatchOverlaySheet = {
  key: 'd1_card_flip_memory_cards_mismatch_overlay_master',
  path: 'custom_puzzle_pack_d1_card_flip_memory_cards_mismatch_overlay_master_24x08f_v01.png',
  frameWidth: 256,
  frameHeight: 256,
  margin: 0,
  spacing: 0,
} as const;

export const memoryCardMismatchOverlayAnimations = [
  { key: 'd1_card_flip_card_01_mismatch_overlay', start: 0, end: 7, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_02_mismatch_overlay', start: 8, end: 15, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_03_mismatch_overlay', start: 16, end: 23, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_04_mismatch_overlay', start: 24, end: 31, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_05_mismatch_overlay', start: 32, end: 39, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_06_mismatch_overlay', start: 40, end: 47, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_07_mismatch_overlay', start: 48, end: 55, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_08_mismatch_overlay', start: 56, end: 63, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_09_mismatch_overlay', start: 64, end: 71, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_10_mismatch_overlay', start: 72, end: 79, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_11_mismatch_overlay', start: 80, end: 87, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_12_mismatch_overlay', start: 88, end: 95, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_13_mismatch_overlay', start: 96, end: 103, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_14_mismatch_overlay', start: 104, end: 111, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_15_mismatch_overlay', start: 112, end: 119, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_16_mismatch_overlay', start: 120, end: 127, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_17_mismatch_overlay', start: 128, end: 135, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_18_mismatch_overlay', start: 136, end: 143, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_19_mismatch_overlay', start: 144, end: 151, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_20_mismatch_overlay', start: 152, end: 159, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_21_mismatch_overlay', start: 160, end: 167, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_22_mismatch_overlay', start: 168, end: 175, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_23_mismatch_overlay', start: 176, end: 183, frameRate: 18, repeat: 0, hideOnComplete: true },
  { key: 'd1_card_flip_card_24_mismatch_overlay', start: 184, end: 191, frameRate: 18, repeat: 0, hideOnComplete: true },
] as const;

export function createMemoryCardMismatchOverlayAnimations(scene: Phaser.Scene) {
  for (const anim of memoryCardMismatchOverlayAnimations) {
    if (scene.anims.exists(anim.key)) continue;
    scene.anims.create({
      key: anim.key,
      frames: scene.anims.generateFrameNumbers(memoryCardMismatchOverlaySheet.key, { start: anim.start, end: anim.end }),
      frameRate: anim.frameRate,
      repeat: anim.repeat,
      hideOnComplete: anim.hideOnComplete,
    });
  }
}
