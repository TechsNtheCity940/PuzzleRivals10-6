export default class MenuScene extends Phaser.Scene {
  constructor() {
    super('MenuScene');
  }

  create() {
    this.add.text(640, 220, 'Puzzle Rivals', {
      fontSize: '48px',
      color: '#ffffff'
    }).setOrigin(0.5);

    this.add.text(640, 300, 'Rulebreaker Prototype', {
      fontSize: '28px',
      color: '#8be9fd'
    }).setOrigin(0.5);

    const startText = this.add.text(640, 420, 'Click to Start', {
      fontSize: '32px',
      color: '#f8f8f2',
      backgroundColor: '#22304a',
      padding: { x: 20, y: 10 }
    }).setOrigin(0.5).setInteractive();

    startText.on('pointerdown', () => {
      this.scene.start('RulebreakerScene');
    });
  }
}