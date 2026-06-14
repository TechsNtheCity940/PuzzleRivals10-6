import PuzzleTile from './PuzzleTile.js';

const COLORS = [
  { name: 'red', hex: 0xe74c3c },
  { name: 'blue', hex: 0x3498db },
  { name: 'green', hex: 0x2ecc71 },
  { name: 'yellow', hex: 0xf1c40f }
];

const SHAPES = ['○', '□', '△', '◇'];

export default class PuzzleBoard {
  constructor(scene, rows = 6, cols = 6, tileSize = 80) {
    this.scene = scene;
    this.rows = rows;
    this.cols = cols;
    this.tileSize = tileSize;
    this.tiles = [];
    this.selectedTiles = [];

    this.startX = 340;
    this.startY = 120;

    this.createBoard();
  }

  createBoard() {
    for (let row = 0; row < this.rows; row++) {
      this.tiles[row] = [];

      for (let col = 0; col < this.cols; col++) {
        const color = Phaser.Utils.Array.GetRandom(COLORS);
        const shape = Phaser.Utils.Array.GetRandom(SHAPES);
        const value = Phaser.Math.Between(1, 5);

        const tileData = {
          row,
          col,
          color: color.name,
          displayColor: color.hex,
          shape,
          value
        };

        const x = this.startX + col * this.tileSize;
        const y = this.startY + row * this.tileSize;

        const tile = new PuzzleTile(this.scene, x, y, this.tileSize - 8, tileData);
        tile.bg.on('pointerdown', () => this.handleTileClick(tile));

        this.tiles[row][col] = tile;
      }
    }
  }

  handleTileClick(tile) {
    const alreadySelected = this.selectedTiles.includes(tile);

    if (alreadySelected) {
      this.selectedTiles = this.selectedTiles.filter(t => t !== tile);
      tile.setSelected(false);
    } else {
      this.selectedTiles.push(tile);
      tile.setSelected(true);
    }

    this.scene.events.emit('selectionChanged', this.selectedTiles);
  }

  clearSelection() {
    this.selectedTiles.forEach(tile => tile.setSelected(false));
    this.selectedTiles = [];
  }

  getTileAt(row, col) {
    if (row < 0 || row >= this.rows || col < 0 || col >= this.cols) return null;
    return this.tiles[row][col];
  }

  refreshSelectedTiles() {
    this.selectedTiles.forEach(tile => {
      tile.destroy();

      const color = Phaser.Utils.Array.GetRandom(COLORS);
      const shape = Phaser.Utils.Array.GetRandom(SHAPES);
      const value = Phaser.Math.Between(1, 5);

      const tileData = {
        row: tile.data.row,
        col: tile.data.col,
        color: color.name,
        displayColor: color.hex,
        shape,
        value
      };

      const x = this.startX + tile.data.col * this.tileSize;
      const y = this.startY + tile.data.row * this.tileSize;

      const newTile = new PuzzleTile(this.scene, x, y, this.tileSize - 8, tileData);
      newTile.bg.on('pointerdown', () => this.handleTileClick(newTile));

      this.tiles[tile.data.row][tile.data.col] = newTile;
    });

    this.selectedTiles = [];
  }
}