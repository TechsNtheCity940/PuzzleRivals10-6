import PuzzleBoard from '../objects/PuzzleBoard.js';
import RuleManager from '../systems/RuleManager.js';
import ScoreManager from '../systems/ScoreManager.js';
import AttackDefenseManager from '../systems/AttackDefenseManager.js';

export default class RulebreakerScene extends Phaser.Scene {
  constructor() {
    super('RulebreakerScene');
  }

  create() {
    this.scoreCap = 100;

    this.board = new PuzzleBoard(this);
    this.ruleManager = new RuleManager(this);
    this.scoreManager = new ScoreManager();
    this.attackDefenseManager = new AttackDefenseManager(this);

    this.createUI();

    this.events.on('selectionChanged', this.onSelectionChanged, this);

    this.shiftToNewRule();

    this.scheduleNextRuleShift();
  }

  createUI() {
    this.scoreText = this.add.text(950, 100, 'Score: 0', {
      fontSize: '28px',
      color: '#ffffff'
    });

    this.streakText = this.add.text(950, 140, 'Streak: 0', {
      fontSize: '24px',
      color: '#ffdd57'
    });

    this.ruleText = this.add.text(950, 220, 'Rule:', {
      fontSize: '30px',
      color: '#8be9fd',
      wordWrap: { width: 260 }
    });

    this.feedbackText = this.add.text(950, 340, '', {
      fontSize: '24px',
      color: '#ffffff',
      wordWrap: { width: 260 }
    });

    this.warningFlash = this.add.rectangle(640, 360, 1280, 720, 0xff3355, 0)
      .setDepth(100);
  }

  onSelectionChanged(selectedTiles) {
    if (selectedTiles.length < 2) return;

    const isValid = this.ruleManager.validateSelection(selectedTiles, this.board);

    if (isValid) {
      const adaptBonus = this.ruleManager.isWithinAdaptWindow();
      const points = this.scoreManager.addValidSolve(adaptBonus);
      this.attackDefenseManager.addCharge(1);

      this.feedbackText.setText(`Valid! +${points}`);
      this.scoreText.setText(`Score: ${this.scoreManager.score}`);
      this.streakText.setText(`Streak: ${this.scoreManager.streak}`);

      this.animateValidSolve(selectedTiles);

      this.time.delayedCall(250, () => {
        this.board.refreshSelectedTiles();
      });

      if (this.scoreManager.score >= this.scoreCap) {
        this.endGame();
      }
    } else {
      if (selectedTiles.length >= 3) {
        this.scoreManager.breakStreak();
        this.streakText.setText(`Streak: ${this.scoreManager.streak}`);
        this.feedbackText.setText('Invalid solve');

        selectedTiles.forEach(tile => tile.pulseInvalid());

        this.time.delayedCall(300, () => {
          this.board.clearSelection();
        });
      }
    }
  }

  shiftToNewRule() {
    const newRule = this.ruleManager.shiftRule();
    this.ruleText.setText(`Rule:\n${newRule.name}\n\n${newRule.description}`);
    this.showRuleBanner(newRule.name);
    this.recolorBoardForRule(newRule.id);
  }

  scheduleNextRuleShift() {
    const delay = Phaser.Math.Between(15000, 25000);

    this.ruleShiftTimer = this.time.delayedCall(delay, () => {
      this.showShiftWarning();
      this.time.delayedCall(1200, () => {
        this.shiftToNewRule();
        this.scheduleNextRuleShift();
      });
    });
  }

  showShiftWarning() {
    this.tweens.add({
      targets: this.warningFlash,
      alpha: 0.22,
      duration: 200,
      yoyo: true,
      repeat: 3
    });
  }

  showRuleBanner(text) {
    const banner = this.add.rectangle(640, 80, 700, 70, 0x1f4068, 0.95).setDepth(200);
    const label = this.add.text(640, 80, `RULE SHIFT: ${text}`, {
      fontSize: '30px',
      color: '#ffffff'
    }).setOrigin(0.5).setDepth(201);

    banner.x = 1500;
    label.x = 1500;

    this.tweens.add({
      targets: [banner, label],
      x: 640,
      duration: 350,
      ease: 'Back.Out',
      onComplete: () => {
        this.time.delayedCall(1200, () => {
          this.tweens.add({
            targets: [banner, label],
            x: -400,
            duration: 300,
            onComplete: () => {
              banner.destroy();
              label.destroy();
            }
          });
        });
      }
    });
  }

  recolorBoardForRule(ruleId) {
    let tint = 0xffffff;

    switch (ruleId) {
      case 'color':
        tint = 0xff6b6b;
        break;
      case 'shape':
        tint = 0x6bcBff;
        break;
      case 'sum':
        tint = 0xf7d794;
        break;
      case 'adjacency':
        tint = 0x78e08f;
        break;
      case 'mirror':
        tint = 0xc56cf0;
        break;
    }

    for (let row of this.board.tiles) {
      for (let tile of row) {
        this.tweens.add({
          targets: tile.container,
          scaleX: 1.02,
          scaleY: 1.02,
          duration: 120,
          yoyo: true
        });
        tile.bg.setStrokeStyle(2, tint);
      }
    }
  }

  animateValidSolve(selectedTiles) {
    selectedTiles.forEach(tile => {
      this.tweens.add({
        targets: tile.container,
        alpha: 0.3,
        scale: 1.2,
        duration: 180,
        yoyo: true
      });
    });
  }

  showClarityOverlay(rule) {
    const panel = this.add.rectangle(640, 360, 540, 240, 0x000000, 0.85).setDepth(300);
    const text = this.add.text(640, 360, `${rule.name}\n\n${rule.description}`, {
      fontSize: '30px',
      color: '#ffffff',
      align: 'center',
      wordWrap: { width: 460 }
    }).setOrigin(0.5).setDepth(301);

    this.time.delayedCall(1500, () => {
      panel.destroy();
      text.destroy();
    });
  }

  endGame() {
    if (this.ruleShiftTimer) this.ruleShiftTimer.remove();

    this.add.rectangle(640, 360, 1280, 720, 0x000000, 0.75).setDepth(500);
    this.add.text(640, 320, 'You Win!', {
      fontSize: '56px',
      color: '#ffffff'
    }).setOrigin(0.5).setDepth(501);

    this.add.text(640, 400, `Final Score: ${this.scoreManager.score}`, {
      fontSize: '32px',
      color: '#8be9fd'
    }).setOrigin(0.5).setDepth(501);

    this.input.once('pointerdown', () => {
      this.scene.start('MenuScene');
    });
  }
}