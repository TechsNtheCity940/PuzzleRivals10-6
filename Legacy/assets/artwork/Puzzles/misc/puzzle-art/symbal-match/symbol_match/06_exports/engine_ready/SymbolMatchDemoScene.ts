import Phaser from "phaser";
import { preloadSymbolMatchSpriteSheets, registerSymbolMatchAnimations } from "./symbol_match.animations";

const SYMBOLS = ["diamond", "star", "sun", "moon", "wave", "rune", "hex", "disc"];

export class SymbolMatchDemoScene extends Phaser.Scene {
  cards = [];
  openCards = [];

  constructor() {
    super("SymbolMatchDemoScene");
  }

  preload() {
    preloadSymbolMatchSpriteSheets(this);
  }

  create() {
    registerSymbolMatchAnimations(this);
    this.cameras.main.setBackgroundColor("#08111d");

    const deck = Phaser.Utils.Array.Shuffle([...SYMBOLS, ...SYMBOLS]);
    const startX = this.scale.width * 0.5 - 240;
    const startY = this.scale.height * 0.5 - 240;

    deck.forEach((symbol, index) => {
      const x = startX + (index % 4) * 160;
      const y = startY + Math.floor(index / 4) * 160;

      const back = this.add.sprite(x, y, "memory_lights_symbol_match_tile_back_shimmer_06f_v01", 0).setScale(0.55).play("tile_back_shimmer_loop");
      const face = this.add.sprite(x, y, `memory_lights_symbol_match_symbol_tile_${symbol}_reveal_06f_v01`, 5).setScale(0.55).setVisible(false);

      back.setInteractive({ useHandCursor: true });
      back.on("pointerover", () => back.play("tile_back_focus_loop"));
      back.on("pointerout", () => back.play("tile_back_shimmer_loop"));
      back.on("pointerdown", () => this.revealCard({ symbol, back, face, matched: false }));

      this.cards.push({ symbol, back, face, matched: false });
    });

    this.time.delayedCall(150, () => this.previewCards());
  }

  previewCards() {
    this.cards.forEach((card, index) => {
      this.time.delayedCall(index * 60, () => {
        card.back.setVisible(false);
        card.face.setVisible(true).play(`symbol_tile_${card.symbol}_reveal_play`);
      });
    });
    this.time.delayedCall(3000, () => this.hideAndShuffle());
  }

  hideAndShuffle() {
    Phaser.Utils.Array.Shuffle(this.cards);
    const startX = this.scale.width * 0.5 - 240;
    const startY = this.scale.height * 0.5 - 240;
    this.cards.forEach((card, index) => {
      card.face.setVisible(false);
      card.back.setVisible(true).play("tile_back_shimmer_loop");
      this.tweens.add({
        targets: [card.back, card.face],
        x: startX + (index % 4) * 160,
        y: startY + Math.floor(index / 4) * 160,
        duration: 260,
        ease: "Quad.Out"
      });
    });
  }

  revealCard(card) {
    if (card.matched || this.openCards.includes(card) || this.openCards.length >= 2) {
      return;
    }
    card.back.setVisible(false);
    card.face.setVisible(true).play(`symbol_tile_${card.symbol}_reveal_play`);
    this.openCards.push(card);
    if (this.openCards.length === 2) {
      this.time.delayedCall(500, () => this.checkPair());
    }
  }

  checkPair() {
    const [a, b] = this.openCards;
    if (!a || !b) {
      return;
    }
    if (a.symbol === b.symbol) {
      a.matched = true;
      b.matched = true;
      this.tweens.add({ targets: [a.face, b.face], scale: 0.62, yoyo: true, duration: 140 });
    } else {
      [a, b].forEach((card) => {
        card.face.setVisible(false);
        card.back.setVisible(true).play("tile_back_shimmer_loop");
      });
      this.cameras.main.shake(140, 0.004);
    }
    this.openCards = [];
  }
}
