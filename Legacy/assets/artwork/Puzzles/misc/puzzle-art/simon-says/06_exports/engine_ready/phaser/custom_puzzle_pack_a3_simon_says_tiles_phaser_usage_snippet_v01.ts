import Phaser from 'phaser';
import { simonSaysTileSheet, createSimonSaysTileAnimations } from './custom_puzzle_pack_a3_simon_says_tiles_phaser_helper_v01';
import { industrialSimonSaysTileSheet, createIndustrialSimonSaysTileAnimations } from './custom_puzzle_pack_a3_simon_says_tiles_industrial_floor_tiles_phaser_helper_v01';

export class SimonSaysDemoScene extends Phaser.Scene {
  preload() {
    this.load.spritesheet(simonSaysTileSheet.key, `assets/${simonSaysTileSheet.path}`, {
      frameWidth: simonSaysTileSheet.frameWidth,
      frameHeight: simonSaysTileSheet.frameHeight,
      margin: simonSaysTileSheet.margin,
      spacing: simonSaysTileSheet.spacing,
    });

    this.load.spritesheet(industrialSimonSaysTileSheet.key, `assets/${industrialSimonSaysTileSheet.path}`, {
      frameWidth: industrialSimonSaysTileSheet.frameWidth,
      frameHeight: industrialSimonSaysTileSheet.frameHeight,
      margin: industrialSimonSaysTileSheet.margin,
      spacing: industrialSimonSaysTileSheet.spacing,
    });
  }

  create() {
    createSimonSaysTileAnimations(this);
    createIndustrialSimonSaysTileAnimations(this);

    const diamond = this.add.sprite(280, 220, simonSaysTileSheet.key).play('a3_simon_says_tiles_cyan_glow');
    const industrial = this.add.sprite(620, 220, industrialSimonSaysTileSheet.key).play('a3_simon_says_tiles_blue_ring_glow');

    // Example Simon Says playback queue.
    const sequence = [
      { target: diamond, key: 'a3_simon_says_tiles_red_glow' },
      { target: industrial, key: 'a3_simon_says_tiles_purple_diamond_glow' },
      { target: diamond, key: 'a3_simon_says_tiles_green_glow' },
    ];

    let step = 0;
    this.time.addEvent({
      delay: 550,
      loop: true,
      callback: () => {
        const current = sequence[step % sequence.length];
        current.target.play(current.key, true);
        step += 1;
      },
    });
  }
}
