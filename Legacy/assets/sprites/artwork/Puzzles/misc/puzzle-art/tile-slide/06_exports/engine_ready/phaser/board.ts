import Phaser from 'phaser';

import {
  slidingPuzzleTileFrame,
  slidingPuzzleTileSheets,
  createSlidingPuzzleTileAnimations,
} from './custom_puzzle_pack_e1_sliding_puzzle_phaser_helper_v01';
import {
  slidingPuzzleSolvedOverlaySheet,
  createSlidingPuzzleSolvedOverlayAnimations,
} from './custom_puzzle_pack_e1_sliding_puzzle_solved_overlay_phaser_helper_v01';

type TileSlug =
  | 'tile_01_top_left'
  | 'tile_02_top_center'
  | 'tile_03_top_right'
  | 'tile_04_mid_left'
  | 'tile_05_mid_center'
  | 'tile_06_mid_right'
  | 'tile_07_bottom_left'
  | 'tile_08_bottom_center'
  | 'tile_09_blank';

type TileActor = {
  slug: TileSlug;
  boardIndex: number;
  sprite: Phaser.GameObjects.Sprite;
  overlay: Phaser.GameObjects.Sprite;
};

const SOLVED_ORDER: TileSlug[] = [
  'tile_01_top_left',
  'tile_02_top_center',
  'tile_03_top_right',
  'tile_04_mid_left',
  'tile_05_mid_center',
  'tile_06_mid_right',
  'tile_07_bottom_left',
  'tile_08_bottom_center',
  'tile_09_blank',
];

export class SlidingPuzzleBoardScene extends Phaser.Scene {
  private tiles = new Map<TileSlug, TileActor>();
  private board: TileSlug[] = [];
  private moveCount = 0;
  private inputLocked = false;
  private movesText!: Phaser.GameObjects.Text;
  private statusText!: Phaser.GameObjects.Text;
  private readonly originX = 250;
  private readonly originY = 130;
  private readonly cell = 170;
  private readonly scale = 0.62;

  constructor() {
    super('SlidingPuzzleBoardScene');
  }

  preload() {
    const directions = Object.keys(slidingPuzzleTileSheets) as Array<keyof typeof slidingPuzzleTileSheets>;
    for (const direction of directions) {
      const sheet = slidingPuzzleTileSheets[direction];
      this.load.spritesheet(sheet.key, `assets/${sheet.path}`, {
        frameWidth: slidingPuzzleTileFrame.frameWidth,
        frameHeight: slidingPuzzleTileFrame.frameHeight,
        margin: slidingPuzzleTileFrame.margin,
        spacing: slidingPuzzleTileFrame.spacing,
      });
    }

    this.load.spritesheet(slidingPuzzleSolvedOverlaySheet.key, `assets/${slidingPuzzleSolvedOverlaySheet.path}`, {
      frameWidth: slidingPuzzleSolvedOverlaySheet.frameWidth,
      frameHeight: slidingPuzzleSolvedOverlaySheet.frameHeight,
      margin: slidingPuzzleSolvedOverlaySheet.margin,
      spacing: slidingPuzzleSolvedOverlaySheet.spacing,
    });
  }

  create() {
    this.cameras.main.setBackgroundColor('#070c16');
    createSlidingPuzzleTileAnimations(this);
    createSlidingPuzzleSolvedOverlayAnimations(this);

    this.movesText = this.add.text(34, 24, 'Moves 0', {
      fontFamily: 'Arial',
      fontSize: '28px',
      color: '#eef4ff',
    });
    this.statusText = this.add.text(230, 24, 'Slide tiles to rebuild the bomb', {
      fontFamily: 'Arial',
      fontSize: '24px',
      color: '#c1cde2',
    });

    this.board = this.makeShuffledBoard();
    for (let boardIndex = 0; boardIndex < this.board.length; boardIndex += 1) {
      const slug = this.board[boardIndex];
      const { x, y } = this.indexToPosition(boardIndex);
      const sprite = this.add
        .sprite(x, y, this.textureForSlug(slug), this.staticFrameForSlug(slug))
        .setScale(this.scale)
        .setInteractive({ useHandCursor: true });
      const overlay = this.add
        .sprite(x, y, slidingPuzzleSolvedOverlaySheet.key, 0)
        .setScale(this.scale)
        .setVisible(false)
        .setDepth(2);

      const actor: TileActor = { slug, boardIndex, sprite, overlay };
      sprite.on(Phaser.Input.Events.GAMEOBJECT_POINTER_DOWN, () => this.tryMove(actor.slug));
      this.tiles.set(slug, actor);
      if (slug === 'tile_09_blank') {
        sprite.setAlpha(0.6);
      }
    }
  }

  private makeShuffledBoard(): TileSlug[] {
    const board = [...SOLVED_ORDER];
    let blankIndex = board.indexOf('tile_09_blank');
    const neighbors = (index: number) => {
      const row = Math.floor(index / 3);
      const col = index % 3;
      const out: number[] = [];
      if (col > 0) out.push(index - 1);
      if (col < 2) out.push(index + 1);
      if (row > 0) out.push(index - 3);
      if (row < 2) out.push(index + 3);
      return out;
    };

    for (let i = 0; i < 40; i += 1) {
      const options = neighbors(blankIndex);
      const swapIndex = Phaser.Utils.Array.GetRandom(options);
      [board[blankIndex], board[swapIndex]] = [board[swapIndex], board[blankIndex]];
      blankIndex = swapIndex;
    }
    if (board.every((slug, i) => slug === SOLVED_ORDER[i])) {
      [board[8], board[7]] = [board[7], board[8]];
    }
    return board;
  }

  private tryMove(slug: TileSlug) {
    if (this.inputLocked || slug === 'tile_09_blank') return;
    const tileIndex = this.board.indexOf(slug);
    const blankIndex = this.board.indexOf('tile_09_blank');
    if (!this.isAdjacent(tileIndex, blankIndex)) return;

    this.inputLocked = true;
    const direction = this.directionFromBlank(tileIndex, blankIndex);
    const actor = this.tiles.get(slug);
    const blank = this.tiles.get('tile_09_blank');
    if (!actor || !blank) {
      this.inputLocked = false;
      return;
    }

    actor.sprite.play(`e1_sliding_puzzle_${slug}_slide_${direction}`, true);
    const target = this.indexToPosition(blankIndex);
    this.tweens.add({
      targets: [actor.sprite, actor.overlay],
      x: target.x,
      y: target.y,
      duration: Math.round((8 / 14) * 1000),
      ease: 'Quad.Out',
      onComplete: () => {
        actor.boardIndex = blankIndex;
        blank.boardIndex = tileIndex;
        [this.board[tileIndex], this.board[blankIndex]] = [this.board[blankIndex], this.board[tileIndex]];
        const blankPos = this.indexToPosition(tileIndex);
        blank.sprite.setPosition(blankPos.x, blankPos.y);
        blank.overlay.setPosition(blankPos.x, blankPos.y);
        this.moveCount += 1;
        this.movesText.setText(`Moves ${this.moveCount}`);
        this.inputLocked = false;
        if (this.isSolved()) this.onSolved();
      },
    });
  }

  private onSolved() {
    this.inputLocked = true;
    this.statusText.setText('Bomb image restored');
    this.cameras.main.flash(220, 255, 220, 120);
    for (const slug of SOLVED_ORDER) {
      const actor = this.tiles.get(slug);
      if (!actor) continue;
      actor.overlay.setVisible(true);
      actor.overlay.play(`e1_sliding_puzzle_${slug}_solved_overlay`, true);
      actor.overlay.once(Phaser.Animations.Events.ANIMATION_COMPLETE, () => {
        actor.overlay.setVisible(false);
      });
    }
  }

  private isSolved() {
    return this.board.every((slug, index) => slug === SOLVED_ORDER[index]);
  }

  private textureForSlug(slug: TileSlug) {
    if (slug === 'tile_09_blank') return slidingPuzzleTileSheets.left.key;
    return slidingPuzzleTileSheets.left.key;
  }

  private staticFrameForSlug(slug: TileSlug) {
    return SOLVED_ORDER.indexOf(slug) * 8 + 7;
  }

  private directionFromBlank(tileIndex: number, blankIndex: number): 'left' | 'right' | 'up' | 'down' {
    const tileRow = Math.floor(tileIndex / 3);
    const tileCol = tileIndex % 3;
    const blankRow = Math.floor(blankIndex / 3);
    const blankCol = blankIndex % 3;
    if (blankCol < tileCol) return 'left';
    if (blankCol > tileCol) return 'right';
    if (blankRow < tileRow) return 'up';
    return 'down';
  }

  private isAdjacent(a: number, b: number) {
    const rowA = Math.floor(a / 3);
    const colA = a % 3;
    const rowB = Math.floor(b / 3);
    const colB = b % 3;
    return Math.abs(rowA - rowB) + Math.abs(colA - colB) === 1;
  }

  private indexToPosition(index: number) {
    const row = Math.floor(index / 3);
    const col = index % 3;
    return {
      x: this.originX + col * this.cell,
      y: this.originY + row * this.cell,
    };
  }
}
