import rules from '../data/rules.js';

export default class RuleManager {
  constructor(scene) {
    this.scene = scene;
    this.rules = rules;
    this.currentRule = null;
    this.lastShiftTime = 0;
    this.inverseLawActive = false;
  }

  getRandomRule() {
    let next = Phaser.Utils.Array.GetRandom(this.rules);
    if (this.currentRule && next.id === this.currentRule.id && this.rules.length > 1) {
      next = Phaser.Utils.Array.GetRandom(this.rules.filter(r => r.id !== this.currentRule.id));
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

    const inverse = this.inverseLawActive;
    switch (rule.id) {
      case 'color': {
        const same = tiles.length === 3 && tiles.every(t => t.data.color === tiles[0].data.color);
        const allDifferent = tiles.length === 3 && new Set(tiles.map(t => t.data.color)).size === 3;
        return inverse ? allDifferent : same;
      }
      case 'family': {
        const same = tiles.length === 3 && tiles.every(t => t.data.family === tiles[0].data.family);
        const allDifferent = tiles.length === 3 && new Set(tiles.map(t => t.data.family)).size === 3;
        return inverse ? allDifferent : same;
      }
      case 'sum': {
        const sum = tiles.reduce((acc, t) => acc + t.data.value, 0);
        return inverse ? (sum === 9 || sum === 11) : sum === 10;
      }
      case 'adjacency':
        return inverse ? this.validateNonAdjacency(tiles) : this.validateAdjacency(tiles);
      case 'mirror':
        return inverse ? this.validateOffMirror(tiles, board) : this.validateMirror(tiles, board);
      default:
        return false;
    }
  }

  validateAdjacency(tiles) {
    if (tiles.length < 3) return false;
    for (let i = 1; i < tiles.length; i++) {
      const prev = tiles[i - 1].data, curr = tiles[i].data;
      if (Math.abs(prev.row - curr.row) + Math.abs(prev.col - curr.col) !== 1) return false;
    }
    return true;
  }

  validateNonAdjacency(tiles) {
    if (tiles.length < 3) return false;
    for (let i = 1; i < tiles.length; i++) {
      const prev = tiles[i - 1].data, curr = tiles[i].data;
      if (Math.abs(prev.row - curr.row) + Math.abs(prev.col - curr.col) === 1) return false;
    }
    return true;
  }

  validateMirror(tiles, board) {
    if (tiles.length !== 2) return false;
    const [a, b] = tiles;
    const mirroredCol = board.cols - 1 - a.data.col;
    return a.data.row === b.data.row && b.data.col === mirroredCol;
  }

  validateOffMirror(tiles, board) {
    if (tiles.length !== 2) return false;
    const [a, b] = tiles;
    const mirroredCol = board.cols - 1 - a.data.col;
    return !(a.data.row === b.data.row && b.data.col === mirroredCol);
  }
}
