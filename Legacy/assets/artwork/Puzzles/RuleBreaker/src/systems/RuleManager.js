import rules from '../data/rules.js';

export default class RuleManager {
  constructor(scene) {
    this.scene = scene;
    this.rules = rules;
    this.currentRule = null;
    this.lastShiftTime = 0;
  }

  getRandomRule() {
    let next = Phaser.Utils.Array.GetRandom(this.rules);

    if (this.currentRule && next.id === this.currentRule.id && this.rules.length > 1) {
      const filtered = this.rules.filter(r => r.id !== this.currentRule.id);
      next = Phaser.Utils.Array.GetRandom(filtered);
    }

    return next;
  }

  shiftRule() {
    this.currentRule = this.getRandomRule();
    this.lastShiftTime = this.scene.time.now;
    return this.currentRule;
  }

  isWithinAdaptWindow() {
    return this.scene.time.now - this.lastShiftTime <= 3000;
  }

  validateSelection(selectedTiles, board) {
    const tiles = selectedTiles;
    const rule = this.currentRule;

    if (!rule || tiles.length === 0) return false;

    switch (rule.id) {
      case 'color':
        return tiles.length === 3 && tiles.every(t => t.data.color === tiles[0].data.color);

      case 'shape':
        return tiles.length === 3 && tiles.every(t => t.data.shape === tiles[0].data.shape);

      case 'sum':
        return tiles.reduce((sum, t) => sum + t.data.value, 0) === 10;

      case 'adjacency':
        return this.validateAdjacency(tiles);

      case 'mirror':
        return this.validateMirror(tiles, board);

      default:
        return false;
    }
  }

  validateAdjacency(tiles) {
    if (tiles.length < 3) return false;

    for (let i = 1; i < tiles.length; i++) {
      const prev = tiles[i - 1].data;
      const curr = tiles[i].data;

      const rowDiff = Math.abs(prev.row - curr.row);
      const colDiff = Math.abs(prev.col - curr.col);

      if (rowDiff + colDiff !== 1) {
        return false;
      }
    }

    return true;
  }

  validateMirror(tiles, board) {
    if (tiles.length !== 2) return false;

    const [a, b] = tiles;
    const mirroredCol = board.cols - 1 - a.data.col;
    return a.data.row === b.data.row && b.data.col === mirroredCol;
  }
}