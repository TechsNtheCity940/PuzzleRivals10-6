import PuzzleTile from './PuzzleTile.js';
import { TILE_FAMILIES } from '../data/rules.js';

const COLORS = [
  { name: 'pink', hex: 0xff4fd8 },
  { name: 'cyan', hex: 0x4fd9ff },
  { name: 'mint', hex: 0x7ef0b1 },
  { name: 'amber', hex: 0xffbf57 }
];

export default class PuzzleBoard {
  constructor(scene, config = {}) {
    this.scene = scene;
    this.boardId = config.boardId ?? 'left';
    this.rows = config.rows ?? 6;
    this.cols = config.cols ?? 6;
    this.tileSize = config.tileSize ?? 54;
    this.gap = config.gap ?? 9;
    this.startX = config.startX ?? 96;
    this.startY = config.startY ?? 170;
    this.label = config.label ?? '';
    this.badge = config.badge ?? (this.boardId === 'left' ? 'P1' : 'P2');
    this.accent = config.accent ?? 0x4fd9ff;
    this.secondaryAccent = config.secondaryAccent ?? 0xff4fd8;
    this.tiles = [];
    this.selectedTiles = [];
    this.boardWidth = this.cols * this.tileSize + (this.cols - 1) * this.gap;
    this.boardHeight = this.rows * this.tileSize + (this.rows - 1) * this.gap;
    this.createBoardFrame();
    this.createBoard();
  }

  createBoardFrame() {
    const width = this.boardWidth + 80;
    const height = this.boardHeight + 102;
    const centerX = this.startX + this.boardWidth / 2;
    const centerY = this.startY + this.boardHeight / 2 + 4;
    this.centerX = centerX;
    this.centerY = centerY;
    this.topY = centerY - height / 2;

    this.backAura = this.scene.add.circle(centerX, centerY, width * 0.54, this.secondaryAccent, 0.05).setBlendMode(Phaser.BlendModes.ADD);
    this.innerBg = this.scene.add.image(centerX, centerY + 6, 'boardInnerBg').setDisplaySize(width - 96, height - 140).setAlpha(0.30);
    this.frameArt = this.scene.add.image(centerX, centerY, 'boardFrame').setDisplaySize(width, height).setAlpha(0.96);
    this.frameGlass = this.scene.add.rectangle(centerX, centerY + 3, width - 96, height - 140, 0x061122, 0.22).setStrokeStyle(1, this.accent, 0.22);
    this.topTab = this.scene.add.image(centerX, this.topY + 22, 'sideTag').setDisplaySize(164, 34).setAlpha(0.9).setTint(this.accent);
    this.topTabText = this.scene.add.text(centerX, this.topY + 22, this.label, { fontSize: '12px', color: '#eef8ff', fontStyle: 'bold' }).setOrigin(0.5);
    this.sideBadge = this.scene.add.image(centerX + width / 2 - 38, centerY - height / 2 + 44, 'sideTag').setDisplaySize(54, 86).setAlpha(0.85).setTint(this.secondaryAccent);
    this.sideBadgeText = this.scene.add.text(centerX + width / 2 - 38, centerY - height / 2 + 44, this.badge, { fontSize: '15px', color: '#eef8ff', fontStyle: 'bold' }).setOrigin(0.5);

    this.edgeNeonA = this.scene.add.rectangle(centerX - width / 2 + 24, centerY, 4, height - 94, this.secondaryAccent, 0.18).setBlendMode(Phaser.BlendModes.ADD);
    this.edgeNeonB = this.scene.add.rectangle(centerX + width / 2 - 24, centerY, 4, height - 94, this.accent, 0.18).setBlendMode(Phaser.BlendModes.ADD);
    [this.edgeNeonA, this.edgeNeonB].forEach(edge => {
      this.scene.tweens.add({ targets: edge, alpha: { from: 0.05, to: 0.22 }, duration: 160, yoyo: true, repeat: -1, repeatDelay: Phaser.Math.Between(400, 1200) });
    });

    this.hitArea = this.scene.add.rectangle(centerX, centerY, width - 70, height - 58, 0xffffff, 0.001).setInteractive({ useHandCursor: true });
    this.hitArea.on('pointerdown', () => this.scene.events.emit('boardActivated', this.boardId));
  }

  setActive(active) {
    this.backAura.setFillStyle(this.accent, active ? 0.09 : 0.04);
    this.frameArt.setAlpha(active ? 1 : 0.92);
    this.frameGlass.setStrokeStyle(1, this.accent, active ? 0.3 : 0.18);
  }

  tilePosition(row, col) {
    return { x: this.startX + col * (this.tileSize + this.gap) + this.tileSize / 2, y: this.startY + row * (this.tileSize + this.gap) + this.tileSize / 2 };
  }

  createBoard() {
    for (let row = 0; row < this.rows; row++) {
      this.tiles[row] = [];
      for (let col = 0; col < this.cols; col++) this.tiles[row][col] = this.makeRandomTile(row, col);
    }
  }

  makeRandomTile(row, col) {
    const color = Phaser.Utils.Array.GetRandom(COLORS);
    const family = Phaser.Utils.Array.GetRandom(TILE_FAMILIES);
    const value = Phaser.Math.Between(1, 5);
    const tileData = { row, col, color: color.name, displayColor: color.hex, family: family.id, iconKey: family.iconKey, badge: family.badge, value };
    const pos = this.tilePosition(row, col);
    const tile = new PuzzleTile(this.scene, pos.x, pos.y, this.tileSize, tileData);
    tile.shell.on('pointerdown', () => this.handleTileClick(tile));
    return tile;
  }

  handleTileClick(tile) {
    this.scene.events.emit('boardActivated', this.boardId);
    const alreadySelected = this.selectedTiles.includes(tile);
    if (this.scene.netClient && this.scene.netClient.connected && this.scene.controlledBoardId === this.boardId) {
      this.scene.netClient.tap(tile.data.row, tile.data.col);
      return;
    }
    if (alreadySelected) {
      this.selectedTiles = this.selectedTiles.filter(t => t !== tile);
      tile.setSelected(false);
    } else {
      this.selectedTiles.push(tile);
      tile.setSelected(true);
    }
    this.scene.events.emit('selectionChanged', { boardId: this.boardId, selectedTiles: this.selectedTiles });
  }

  clearSelection() { this.selectedTiles.forEach(t => t.setSelected(false)); this.selectedTiles = []; }

  refreshSelectedTiles() {
    const toReplace = [...this.selectedTiles];
    toReplace.forEach(tile => {
      const { row, col } = tile.data;
      tile.destroy();
      this.tiles[row][col] = this.makeRandomTile(row, col);
    });
    this.selectedTiles = [];
  }

  rerollRandomTiles(count = 3) {
    const used = new Set();
    for (let i = 0; i < count; i++) {
      let row, col, key;
      do {
        row = Phaser.Math.Between(0, this.rows - 1);
        col = Phaser.Math.Between(0, this.cols - 1);
        key = `${row}:${col}`;
      } while (used.has(key));
      used.add(key);
      const oldTile = this.tiles[row][col];
      if (this.selectedTiles.includes(oldTile)) this.selectedTiles = this.selectedTiles.filter(t => t !== oldTile);
      oldTile.destroy();
      this.tiles[row][col] = this.makeRandomTile(row, col);
      this.tiles[row][col].pulseInvalid();
    }
  }

  recolorFrame(color) {
    this.frameGlass.setStrokeStyle(1, color, 0.28);
    this.topTab.setTint(color);
    this.sideBadge.setTint(color);
    this.backAura.fillColor = color;
    this.edgeNeonA.fillColor = color;
    this.edgeNeonB.fillColor = color;
  }
}
