import Phaser from "phaser";
import { preloadColorSequenceSpriteSheets, registerColorSequenceAnimations } from "./color_sequence.animations";

const PAD_COLORS = ["red", "yellow", "green", "cyan", "blue", "purple"];
const ORB_COLORS = ["red", "blue", "green", "yellow", "purple", "orange"];

export class ColorSequenceDemoScene extends Phaser.Scene {
  pads = [];
  cursorIndex = 0;
  inputLocked = true;
  sequence = [0, 2, 4, 1, 5];

  constructor() {
    super("ColorSequenceDemoScene");
  }

  preload() {
    preloadColorSequenceSpriteSheets(this);
  }

  create() {
    registerColorSequenceAnimations(this);
    this.cameras.main.setBackgroundColor("#08111d");

    const cx = this.scale.width * 0.5;
    const cy = this.scale.height * 0.52;
    const positions = [
      [cx, cy - 220],
      [cx - 220, cy],
      [cx, cy + 220],
      [cx + 220, cy],
      [cx - 340, cy + 220],
      [cx + 340, cy + 220],
    ];

    this.pads = PAD_COLORS.map((color, index) => {
      const [x, y] = positions[index];
      const sprite = this.add.sprite(x, y, `memory_lights_color_sequence_input_pad_${color}_pulse_08f_v01`, 0).setScale(0.72);
      sprite.play(`input_pad_${color}_pulse_loop`);
      return sprite;
    });

    ORB_COLORS.forEach((color, index) => {
      this.add
        .sprite(120 + index * 110, 88, `memory_lights_color_sequence_signal_orb_${color}_glow_soft_08f_v01`, 0)
        .setScale(0.32)
        .play(`signal_orb_${color}_glow_soft_loop`);
    });

    this.input.keyboard?.on("keydown", (event) => {
      const map = { ArrowUp: 0, ArrowLeft: 1, ArrowDown: 2, ArrowRight: 3, KeyQ: 4, KeyE: 5 };
      const choice = map[event.code];
      if (choice === undefined || this.inputLocked) {
        return;
      }
      this.flashPad(choice, choice === this.sequence[this.cursorIndex]);
      if (choice === this.sequence[this.cursorIndex]) {
        this.cursorIndex += 1;
        if (this.cursorIndex >= this.sequence.length) {
          this.cursorIndex = 0;
          this.time.delayedCall(350, () => void this.playSequence());
        }
      } else {
        this.cursorIndex = 0;
        this.cameras.main.shake(160, 0.005);
        this.time.delayedCall(450, () => void this.playSequence());
      }
    });

    this.playSequence();
  }

  flashPad(index, success) {
    const sprite = this.pads[index];
    this.tweens.add({
      targets: sprite,
      scaleX: success ? 0.82 : 0.66,
      scaleY: success ? 0.82 : 0.66,
      yoyo: true,
      duration: 120,
    });
  }

  async playSequence() {
    this.inputLocked = true;
    for (const step of this.sequence) {
      await new Promise((resolve) => {
        this.flashPad(step, true);
        this.time.delayedCall(420, resolve);
      });
    }
    this.inputLocked = false;
  }
}
