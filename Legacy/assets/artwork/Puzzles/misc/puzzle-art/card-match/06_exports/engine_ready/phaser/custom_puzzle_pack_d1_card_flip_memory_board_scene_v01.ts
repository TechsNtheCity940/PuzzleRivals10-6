import Phaser from 'phaser';

import {
  memoryCardFlipSheet,
  createMemoryCardFlipAnimations,
  playMemoryCardFlipClose,
} from './custom_puzzle_pack_d1_card_flip_phaser_helper_v01';
import {
  memoryCardMatchOverlaySheet,
  createMemoryCardMatchOverlayAnimations,
} from './custom_puzzle_pack_d1_card_flip_match_overlay_phaser_helper_v01';
import {
  memoryCardMismatchOverlaySheet,
  createMemoryCardMismatchOverlayAnimations,
} from './custom_puzzle_pack_d1_card_flip_mismatch_overlay_phaser_helper_v01';

type CardActor = {
  pairId: number;
  cardIndex: number;
  faceUp: boolean;
  matched: boolean;
  base: Phaser.GameObjects.Sprite;
  overlay: Phaser.GameObjects.Sprite;
};

export class MemoryBoardScene extends Phaser.Scene {
  private cards: CardActor[] = [];
  private openCards: CardActor[] = [];
  private inputLocked = false;
  private remainingMs = 90_000;
  private timerText!: Phaser.GameObjects.Text;
  private pairsText!: Phaser.GameObjects.Text;
  private matchedPairs = 0;

  constructor() {
    super('MemoryBoardScene');
  }

  preload() {
    this.load.spritesheet(memoryCardFlipSheet.key, `assets/${memoryCardFlipSheet.path}`, {
      frameWidth: memoryCardFlipSheet.frameWidth,
      frameHeight: memoryCardFlipSheet.frameHeight,
      margin: memoryCardFlipSheet.margin,
      spacing: memoryCardFlipSheet.spacing,
    });

    this.load.spritesheet(memoryCardMatchOverlaySheet.key, `assets/${memoryCardMatchOverlaySheet.path}`, {
      frameWidth: memoryCardMatchOverlaySheet.frameWidth,
      frameHeight: memoryCardMatchOverlaySheet.frameHeight,
      margin: memoryCardMatchOverlaySheet.margin,
      spacing: memoryCardMatchOverlaySheet.spacing,
    });

    this.load.spritesheet(memoryCardMismatchOverlaySheet.key, `assets/${memoryCardMismatchOverlaySheet.path}`, {
      frameWidth: memoryCardMismatchOverlaySheet.frameWidth,
      frameHeight: memoryCardMismatchOverlaySheet.frameHeight,
      margin: memoryCardMismatchOverlaySheet.margin,
      spacing: memoryCardMismatchOverlaySheet.spacing,
    });
  }

  create() {
    this.cameras.main.setBackgroundColor('#070a12');
    createMemoryCardFlipAnimations(this);
    createMemoryCardMatchOverlayAnimations(this);
    createMemoryCardMismatchOverlayAnimations(this);

    this.timerText = this.add.text(28, 20, '01:30', {
      fontFamily: 'Arial',
      fontSize: '28px',
      color: '#f1f5ff',
    });
    this.pairsText = this.add.text(260, 20, 'Pairs 0 / 18', {
      fontFamily: 'Arial',
      fontSize: '28px',
      color: '#f1f5ff',
    });

    const deck: Array<{ pairId: number; cardIndex: number }> = [];
    for (let pairId = 1; pairId <= 18; pairId += 1) {
      deck.push({ pairId, cardIndex: pairId });
      deck.push({ pairId, cardIndex: pairId });
    }
    Phaser.Utils.Array.Shuffle(deck);

    const startX = 120;
    const startY = 150;
    const stepX = 180;
    const stepY = 132;
    const scale = 0.48;

    deck.forEach((entry, i) => {
      const col = i % 6;
      const row = Math.floor(i / 6);
      const x = startX + col * stepX;
      const y = startY + row * stepY;

      const base = this.add.sprite(x, y, memoryCardFlipSheet.key, 0).setScale(scale).setInteractive({ useHandCursor: true });
      const overlay = this.add
        .sprite(x, y, memoryCardMatchOverlaySheet.key, 0)
        .setScale(scale)
        .setVisible(false)
        .setDepth(2);

      const actor: CardActor = {
        pairId: entry.pairId,
        cardIndex: entry.cardIndex,
        faceUp: false,
        matched: false,
        base,
        overlay,
      };

      base.on(Phaser.Input.Events.GAMEOBJECT_POINTER_DOWN, () => this.onCardPressed(actor));
      this.cards.push(actor);
    });

    this.time.addEvent({
      delay: 1000,
      loop: true,
      callback: () => this.tickTimer(),
    });
  }

  private tickTimer() {
    if (this.remainingMs <= 0 || this.matchedPairs >= 18) return;
    this.remainingMs = Math.max(0, this.remainingMs - 1000);
    const totalSeconds = Math.ceil(this.remainingMs / 1000);
    const mm = String(Math.floor(totalSeconds / 60)).padStart(2, '0');
    const ss = String(totalSeconds % 60).padStart(2, '0');
    this.timerText.setText(`${mm}:${ss}`);
    if (this.remainingMs === 0) {
      this.inputLocked = true;
      this.pairsText.setText('Time up');
    }
  }

  private onCardPressed(card: CardActor) {
    if (this.inputLocked || card.faceUp || card.matched || this.openCards.length >= 2) return;

    card.base.play(`d1_card_flip_card_${String(card.cardIndex).padStart(2, '0')}_flip_open`, true);
    card.faceUp = true;
    this.openCards.push(card);

    if (this.openCards.length === 2) {
      this.inputLocked = true;
      this.time.delayedCall(420, () => this.resolvePair());
    }
  }

  private resolvePair() {
    const [a, b] = this.openCards;
    if (!a || !b) {
      this.inputLocked = false;
      return;
    }

    if (a.pairId === b.pairId) {
      a.matched = true;
      b.matched = true;
      this.playOverlay(a, 'match');
      this.playOverlay(b, 'match');
      this.matchedPairs += 1;
      this.pairsText.setText(`Pairs ${this.matchedPairs} / 18`);
      if (this.matchedPairs >= 18) {
        this.pairsText.setText('Cleared');
      }
      this.openCards = [];
      this.inputLocked = false;
      return;
    }

    this.playOverlay(a, 'mismatch');
    this.playOverlay(b, 'mismatch');
    this.cameras.main.shake(130, 0.0035);
    this.time.delayedCall(560, () => {
      playMemoryCardFlipClose(a.base, this, a.cardIndex);
      playMemoryCardFlipClose(b.base, this, b.cardIndex);
      a.faceUp = false;
      b.faceUp = false;
      this.openCards = [];
      this.inputLocked = false;
    });
  }

  private playOverlay(card: CardActor, type: 'match' | 'mismatch') {
    const textureKey = type === 'match' ? memoryCardMatchOverlaySheet.key : memoryCardMismatchOverlaySheet.key;
    card.overlay.setTexture(textureKey);
    card.overlay.setVisible(true);
    card.overlay.play(`d1_card_flip_card_${String(card.cardIndex).padStart(2, '0')}_${type}_overlay`, true);
    card.overlay.once(Phaser.Animations.Events.ANIMATION_COMPLETE, () => {
      card.overlay.setVisible(false);
    });
  }
}
