import PuzzleBoard from '../objects/PuzzleBoard.js';
import RuleManager from '../systems/RuleManager.js';
import ScoreManager from '../systems/ScoreManager.js';
import AttackDefenseManager from '../systems/AttackDefenseManager.js';
import NetClient from '../net/NetClient.js';

const THEME = {
  bg: 0x020612,
  cyan: '#67e9ff', cyanInt: 0x67e9ff,
  magenta: '#ff4fd8', magentaInt: 0xff4fd8,
  violet: '#8f69ff', violetInt: 0x8f69ff,
  amber: '#ffbf57', amberInt: 0xffbf57,
  mint: '#7ef0b1', mintInt: 0x7ef0b1,
  white: '#eef8ff'
};

export default class RulebreakerScene extends Phaser.Scene {
  constructor() { super('RulebreakerScene'); }

  create() {
    this.scoreCap = 100;
    this.activeBoardId = 'left';
    this.controlledBoardId = 'left';
    this.gameOver = false;

    this.drawBackground();
    this.createHeader();

    this.players = {
      left: this.createPlayerState('left', { name: 'PLAYER ONE', username: '@voltase_one', boardLabel: 'P1 GRID', boardX: 40, uiX: 32, accent: THEME.cyan, accentInt: THEME.cyanInt, secondary: THEME.magentaInt, badgeText: 'P1', meterX: 614 }),
      right: this.createPlayerState('right', { name: 'PLAYER TWO', username: '@voltase_two', boardLabel: 'P2 GRID', boardX: 688, uiX: 680, accent: THEME.magenta, accentInt: THEME.magentaInt, secondary: THEME.amberInt, badgeText: 'P2', meterX: 666 })
    };

    this.createFooter();
    this.bindEvents();
    this.initializePlayers();
    this.setActiveBoard('left');

    this.netClient = new NetClient(this);
    this.netClient.connect();

    this.time.addEvent({ delay: 150, loop: true, callback: () => this.tickAutoSystems(150) });
  }

  drawBackground() {
    this.add.rectangle(640, 360, 1280, 720, THEME.bg, 1);
    this.add.image(640, 360, 'boardsBg').setDisplaySize(1280, 720).setAlpha(0.92);
    this.add.rectangle(640, 360, 1280, 720, 0x02040d, 0.48);
    const grid = this.add.graphics();
    grid.lineStyle(1, THEME.cyanInt, 0.045);
    for (let x = 0; x <= 1280; x += 48) grid.lineBetween(x, 0, x, 720);
    for (let y = 0; y <= 720; y += 48) grid.lineBetween(0, y, 1280, y);
    const scan = this.add.graphics();
    for (let y = 0; y < 720; y += 4) { scan.fillStyle(0xffffff, y % 8 === 0 ? 0.018 : 0.008); scan.fillRect(0, y, 1280, 2); }
    scan.setBlendMode(Phaser.BlendModes.SCREEN).setAlpha(0.46);
    this.scanAlpha = scan;
    this.tweens.add({ targets: scan, alpha: { from: 0.34, to: 0.52 }, duration: 1800, yoyo: true, repeat: -1 });
    this.sparks = this.add.particles(0, 0, 'voltaseEmblem', { x: { min: 40, max: 1240 }, y: { min: 40, max: 700 }, scale: { start: 0.014, end: 0 }, speed: { min: 4, max: 18 }, lifespan: 1200, frequency: 220, alpha: { start: 0.18, end: 0 }, tint: [THEME.cyanInt, THEME.magentaInt, THEME.amberInt] });
  }

  createHeader() {
    this.add.image(640, 58, 'headerBar').setDisplaySize(600, 108).setAlpha(0.58);
    this.add.image(530, 58, 'voltaseEmblem').setDisplaySize(34, 30).setAlpha(0.92);
    this.add.image(750, 58, 'voltaseEmblem').setDisplaySize(34, 30).setAlpha(0.92).setFlipX(true);
    this.add.text(640, 44, 'NEON RIVALS', { fontSize: '28px', color: THEME.white, fontStyle: 'bold' }).setOrigin(0.5);
    this.add.text(640, 74, 'RULEBREAKER VERSUS', { fontSize: '16px', color: '#f4bae7', fontStyle: 'bold' }).setOrigin(0.5);
    this.netText = this.add.text(640, 96, 'LOCAL DUEL MODE', { fontSize: '11px', color: '#d7eeff', fontStyle: 'bold' }).setOrigin(0.5);
    const lightning = this.add.rectangle(640, 28, 310, 3, THEME.cyanInt, 0.65).setBlendMode(Phaser.BlendModes.ADD);
    this.tweens.add({ targets: lightning, x: { from: 540, to: 740 }, alpha: { from: 0.2, to: 0.9 }, duration: 520, yoyo: true, repeat: -1, ease: 'Sine.InOut' });
  }

  createFooter() {
    this.add.image(640, 704, 'hudStrip').setDisplaySize(860, 28).setAlpha(0.58);
    this.add.text(640, 704, 'Attacks and defenses are auto-triggered by skillful solves once their cooldowns are ready. Multiplayer uses socket.io on localhost:3000.', { fontSize: '11px', color: '#d3e4ff' }).setOrigin(0.5);
  }

  createPlayerState(id, cfg) {
    const board = new PuzzleBoard(this, { boardId: id, startX: cfg.boardX + 46, startY: 176, tileSize: 54, gap: 9, label: cfg.boardLabel, badge: cfg.badgeText, accent: cfg.accentInt, secondaryAccent: cfg.secondary });
    const meterBg = this.add.rectangle(cfg.meterX, 372, 26, 350, 0x081322, 0.78).setStrokeStyle(1, cfg.accentInt, 0.25);
    const meterFill = this.add.rectangle(cfg.meterX, 547, 16, 0, cfg.accentInt, 0.92).setOrigin(0.5, 1).setBlendMode(Phaser.BlendModes.ADD);
    const meterGlow = this.add.rectangle(cfg.meterX, 372, 34, 350, cfg.accentInt, 0.05).setBlendMode(Phaser.BlendModes.ADD);
    const meterScore = this.add.text(cfg.meterX, 570, '0', { fontSize: '18px', color: '#eef8ff', fontStyle: 'bold' }).setOrigin(0.5);
    const cardX = cfg.uiX + 278;
    const panel = this.add.image(cardX, 629, 'scoreModule').setDisplaySize(560, 94).setAlpha(0.95);
    const userCardX = id === 'left' ? cfg.uiX + 72 : cfg.uiX + 486;
    const playerCard = this.add.image(userCardX, 629, 'themePanel').setDisplaySize(112, 72).setAlpha(0.84);
    const emblem = this.add.image(userCardX, 629, 'voltaseEmblem').setDisplaySize(36, 32).setAlpha(0.92);
    const nameText = this.add.text(id === 'left' ? cfg.uiX + 144 : cfg.uiX + 32, 608, cfg.name, { fontSize: '22px', color: THEME.white, fontStyle: 'bold' });
    const userText = this.add.text(id === 'left' ? cfg.uiX + 144 : cfg.uiX + 32, 632, cfg.username, { fontSize: '14px', color: cfg.accent, fontStyle: 'bold' });
    const ruleText = this.add.text(id === 'left' ? cfg.uiX + 144 : cfg.uiX + 32, 652, 'RULE // ---', { fontSize: '13px', color: '#bff0ff', fontStyle: 'bold', wordWrap: { width: 320 } });
    const feedbackText = this.add.text(id === 'left' ? cfg.uiX + 144 : cfg.uiX + 32, 671, 'Awaiting first solve.', { fontSize: '12px', color: '#dceaff', wordWrap: { width: 320 } });
    const statusText = this.add.text(cardX, 690, '', { fontSize: '11px', color: '#ffbfe8' }).setOrigin(0.5);

    const ruleChip = this.add.image(board.centerX, 116, 'sideTag').setDisplaySize(240, 46).setAlpha(0.0);
    const ruleChipTitle = this.add.text(board.centerX, 108, '', { fontSize: '11px', color: '#e7f7ff', fontStyle: 'bold' }).setOrigin(0.5).setAlpha(0);
    const ruleChipText = this.add.text(board.centerX, 122, '', { fontSize: '16px', color: '#ffffff', fontStyle: 'bold' }).setOrigin(0.5).setAlpha(0);

    const fogOverlay = this.add.rectangle(board.centerX, board.centerY, 562, 438, cfg.accentInt, 0).setDepth(800).setBlendMode(Phaser.BlendModes.MULTIPLY);
    const warningFlash = this.add.rectangle(board.centerX, board.centerY, 562, 438, 0xff3355, 0).setDepth(900);

    return { id, name: cfg.name, username: cfg.username, accent: cfg.accent, accentInt: cfg.accentInt, secondary: cfg.secondary, board, panel, playerCard, meterFill, meterScore, meterGlow, nameText, userText, ruleText, feedbackText, statusText, ruleChip, ruleChipTitle, ruleChipText, fogOverlay, warningFlash, ruleManager: new RuleManager(this), scoreManager: new ScoreManager(), attackDefenseManager: new AttackDefenseManager(this), firstSolveAfterShift: true, autoCooldown: 0, ruleShiftTimer: null };
  }

  bindEvents() {
    this.events.on('boardActivated', (boardId) => this.setActiveBoard(boardId));
    this.events.on('selectionChanged', (payload) => this.onSelectionChanged(payload));
  }

  initializePlayers() {
    Object.values(this.players).forEach(player => { this.shiftToNewRule(player); this.scheduleNextRuleShift(player); this.updateMeters(player); });
  }

  setActiveBoard(boardId) {
    this.activeBoardId = boardId;
    Object.values(this.players).forEach(player => player.board.setActive(player.id === boardId));
  }

  getOpponent(player) { return this.players[player.id === 'left' ? 'right' : 'left']; }

  onSelectionChanged({ boardId, selectedTiles }) {
    if (this.gameOver) return;
    const player = this.players[boardId];
    this.setActiveBoard(boardId);
    if (selectedTiles.length < 2) return;
    const valid = player.ruleManager.validateSelection(selectedTiles, player.board);
    if (valid) {
      const adaptBonus = player.ruleManager.isWithinAdaptWindow() || player.attackDefenseManager.adaptBoostReady;
      const skillBonus = this.computeSkillBonus(player, selectedTiles);
      const points = player.scoreManager.addValidSolve(adaptBonus, player.attackDefenseManager.adaptBoostReady, skillBonus);
      player.attackDefenseManager.addCharge(skillBonus >= 6 ? 3 : skillBonus >= 3 ? 2 : 1);
      player.attackDefenseManager.adaptBoostReady = false;
      player.feedbackText.setText(`Valid solve // +${points}`);
      player.statusText.setText(player.firstSolveAfterShift ? 'Fast adaptation bonus landed.' : 'Grid stabilized.');
      this.animateValidSolve(player, selectedTiles);
      this.time.delayedCall(220, () => player.board.refreshSelectedTiles());
      player.firstSolveAfterShift = false;
      this.updateMeters(player);
      this.tryAutoAbility(player, skillBonus);
      if (player.scoreManager.score >= this.scoreCap) this.endGame(player);
      return;
    }
    if (selectedTiles.length >= 3 || (player.ruleManager.currentRule.id === 'mirror' && selectedTiles.length >= 2)) {
      player.scoreManager.breakStreak();
      player.feedbackText.setText('Invalid solve. The rule rejected your move.');
      player.statusText.setText('');
      selectedTiles.forEach(tile => tile.pulseInvalid());
      this.showInvalidCrackle(player);
      this.time.delayedCall(260, () => player.board.clearSelection());
      this.updateMeters(player);
    }
  }

  computeSkillBonus(player, tiles) {
    const rule = player.ruleManager.currentRule.id;
    if (rule === 'sum') return Math.max(1, tiles.length - 2) * 3;
    if (rule === 'adjacency') return 4;
    if (rule === 'mirror') return 5;
    if (rule === 'family') return 3;
    return 2;
  }

  tryAutoAbility(player, skillBonus) {
    if (player.autoCooldown > 0) return;
    const enemy = this.getOpponent(player);
    const adm = player.attackDefenseManager;
    const useDefense = player.scoreManager.streak >= 3 && adm.defenseCharge >= 24;
    if (skillBonus >= 5 && adm.canUseAttack(24, 'forcedShift')) {
      adm.spend('forcedShift', 'attack', 24, 6500);
      if (!this.consumeNullifyIfNeeded(enemy, player, 'forcedShift')) {
        enemy.ruleManager.shiftRule();
        this.shiftToExistingRule(enemy, enemy.ruleManager.currentRule, true);
        this.resetRuleTimer(enemy);
        this.attackScreenFX(enemy, enemy.accentInt, 'FORCED SHIFT');
      }
      player.feedbackText.setText('Auto-cast // Forced Shift');
      player.autoCooldown = 1800;
    } else if (skillBonus >= 4 && adm.canUseAttack(20, 'chaosPulse')) {
      adm.spend('chaosPulse', 'attack', 20, 5200);
      if (!this.consumeNullifyIfNeeded(enemy, player, 'chaosPulse')) {
        enemy.board.rerollRandomTiles(3);
        this.attackScreenFX(enemy, 0xffbf57, 'CHAOS PULSE');
      }
      player.feedbackText.setText('Auto-cast // Chaos Pulse');
      player.autoCooldown = 1800;
    } else if (useDefense && adm.canUseDefense(18, 'clarity')) {
      adm.spend('clarity', 'defense', 18, 5000);
      this.showClarityOverlay(player.ruleManager.currentRule);
      player.feedbackText.setText('Auto-cast // Clarity');
      player.autoCooldown = 1600;
    }
    this.updateMeters(player);
  }

  consumeNullifyIfNeeded(defender, attacker, type) {
    if (!defender.attackDefenseManager.nullifyReady) return false;
    defender.attackDefenseManager.nullifyReady = false;
    defender.statusText.setText(`Nullify canceled ${attacker.name}'s ${type}.`);
    this.updateMeters(defender);
    return true;
  }

  updateRuleText(player) {
    const rule = player.ruleManager.currentRule;
    const inverseTag = player.ruleManager.inverseLawActive ? ' // INVERSE' : '';
    player.ruleText.setText(`RULE // ${rule.name}${inverseTag}`);
    player.ruleText.setColor(Phaser.Display.Color.IntegerToColor(rule.themeColor).rgba);
  }

  shiftToNewRule(player) { this.shiftToExistingRule(player, player.ruleManager.shiftRule(), false); }

  shiftToExistingRule(player, rule, forced = false) {
    player.firstSolveAfterShift = true;
    this.updateRuleText(player);
    this.showRuleChip(player, forced ? 'INCOMING OVERRIDE' : 'UPCOMING RULE', `${rule.short} // ${rule.name.toUpperCase()}`, rule.themeColor);
    this.recolorBoardForRule(player, rule.themeColor);
    player.feedbackText.setText(rule.description);
  }

  showRuleChip(player, title, text, color) {
    player.ruleChip.setTint(color);
    player.ruleChipTitle.setText(title);
    player.ruleChipText.setText(text);
    const startX = player.board.centerX + (player.id === 'left' ? -90 : 90);
    player.ruleChip.x = startX; player.ruleChipTitle.x = startX; player.ruleChipText.x = startX;
    player.ruleChip.y = 116; player.ruleChipTitle.y = 108; player.ruleChipText.y = 122;
    [player.ruleChip, player.ruleChipTitle, player.ruleChipText].forEach(el => el.setAlpha(1));
    this.tweens.add({ targets: [player.ruleChip, player.ruleChipTitle, player.ruleChipText], x: player.board.centerX, duration: 240, ease: 'Sine.Out', onComplete: () => {
      this.time.delayedCall(700, () => {
        this.tweens.add({ targets: [player.ruleChip, player.ruleChipTitle, player.ruleChipText], alpha: 0, scaleX: 1.06, scaleY: 1.06, duration: 320 });
      });
    }});
  }

  scheduleNextRuleShift(player) {
    const delay = Phaser.Math.Between(15000, 25000);
    player.ruleShiftTimer = this.time.delayedCall(delay, () => {
      if (this.gameOver) return;
      this.showShiftWarning(player);
      this.time.delayedCall(920, () => { this.shiftToNewRule(player); this.scheduleNextRuleShift(player); });
    });
  }

  resetRuleTimer(player) { if (player.ruleShiftTimer) player.ruleShiftTimer.remove(); this.scheduleNextRuleShift(player); }

  showShiftWarning(player) {
    player.statusText.setText('Warning // rule shift incoming.');
    this.tweens.add({ targets: player.warningFlash, alpha: 0.18, duration: 120, yoyo: true, repeat: 4 });
  }

  recolorBoardForRule(player, color) {
    player.board.recolorFrame(color);
    player.playerCard.setTint(color);
    player.meterGlow.fillColor = color;
    for (const row of player.board.tiles) for (const tile of row) tile.pulseRule(color);
  }

  animateValidSolve(player, selectedTiles) {
    const color = player.ruleManager.currentRule.themeColor;
    selectedTiles.forEach(tile => { this.tweens.add({ targets: tile.container, alpha: 0.42, scale: 1.16, duration: 170, yoyo: true }); tile.burst(color); });
    this.attackScreenFX(player, color, 'MATCH BURST', 0.05);
  }

  showInvalidCrackle(player) {
    const fx = this.add.rectangle(player.board.centerX, player.board.centerY, 572, 430, player.accentInt, 0).setDepth(999);
    this.tweens.add({ targets: fx, alpha: 0.07, duration: 50, yoyo: true, repeat: 2, onComplete: () => fx.destroy() });
  }

  attackScreenFX(player, color, label, strength = 0.09) {
    const wipe = this.add.rectangle(player.board.centerX, player.board.centerY, 572, 430, color, 0).setBlendMode(Phaser.BlendModes.ADD).setDepth(1000);
    const tag = this.add.text(player.board.centerX, player.board.centerY - 6, label, { fontSize: '24px', color: '#ffffff', fontStyle: 'bold' }).setOrigin(0.5).setDepth(1001);
    this.tweens.add({ targets: wipe, alpha: strength, duration: 120, yoyo: true, onComplete: () => wipe.destroy() });
    this.tweens.add({ targets: tag, alpha: { from: 1, to: 0 }, y: tag.y - 18, duration: 420, onComplete: () => tag.destroy() });
  }

  showClarityOverlay(rule) {
    const panel = this.add.image(640, 360, 'scoreModule').setDisplaySize(620, 180).setDepth(300).setTint(rule.themeColor);
    const text = this.add.text(640, 360, `${rule.name}\n${rule.description}`, { fontSize: '26px', color: THEME.white, align: 'center', wordWrap: { width: 460 } }).setOrigin(0.5).setDepth(302);
    this.time.delayedCall(1350, () => { panel.destroy(); text.destroy(); });
  }

  updateMeters(player) {
    player.meterFill.height = 3.2 * player.scoreManager.score;
    player.meterScore.setText(`${player.scoreManager.score}`);
    player.nameText.setText(player.name);
    player.userText.setText(player.username);
    player.statusText.setText(player.statusText.text || '');
  }

  tickAutoSystems(ms) {
    if (this.gameOver) return;
    Object.values(this.players).forEach(player => {
      player.attackDefenseManager.tick(ms);
      player.autoCooldown = Math.max(0, player.autoCooldown - ms);
      const adm = player.attackDefenseManager;
      if (player.scoreManager.streak >= 4 && adm.canUseDefense(20, 'nullify') && !adm.nullifyReady) {
        adm.spend('nullify', 'defense', 20, 7000);
        adm.nullifyReady = true;
        player.statusText.setText('Auto-cast // Nullify armed.');
      }
      this.updateMeters(player);
    });
  }

  endGame(winner) {
    this.gameOver = true;
    Object.values(this.players).forEach(player => player.ruleShiftTimer && player.ruleShiftTimer.remove());
    this.add.rectangle(640, 360, 1280, 720, 0x000000, 0.78).setDepth(500);
    this.add.image(640, 346, 'headerBar').setDisplaySize(760, 220).setDepth(501).setTint(winner.accentInt);
    this.add.image(640, 292, 'voltaseEmblem').setDisplaySize(78, 68).setDepth(503).setAlpha(0.95);
    this.add.text(640, 350, `${winner.name} WINS`, { fontSize: '48px', color: THEME.white, fontStyle: 'bold' }).setOrigin(0.5).setDepth(503);
    this.add.text(640, 402, `${this.players.left.scoreManager.score}  //  ${this.players.right.scoreManager.score}`, { fontSize: '24px', color: THEME.cyan }).setOrigin(0.5).setDepth(503);
    this.add.text(640, 450, 'Tap anywhere to return to menu.', { fontSize: '20px', color: '#dceaff' }).setOrigin(0.5).setDepth(503);
    this.input.once('pointerdown', () => this.scene.start('MenuScene'));
  }

  onNetReady(payload) {
    this.netText.setText(`ONLINE MATCH // ${payload.side.toUpperCase()}`);
    this.controlledBoardId = payload.side === 'left' ? 'left' : 'right';
    this.activeBoardId = this.controlledBoardId;
    this.setActiveBoard(this.controlledBoardId);
  }

  onNetDisconnected() {
    this.netText.setText('LOCAL DUEL MODE');
  }

  applyNetState(state) {
    // Scaffold hook. The socket layer is wired for room sync, but this visual pass still falls back locally if the server is absent.
    this.netText.setText(`ONLINE MATCH // ${state.roomId || 'SYNCED'}`);
  }
}
