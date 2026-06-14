export default class MenuScene extends Phaser.Scene {
  constructor() { super('MenuScene'); }
  create() {
    this.add.rectangle(640, 360, 1280, 720, 0x020612, 1);
    this.add.image(640, 360, 'menuBg').setDisplaySize(1280, 720).setAlpha(0.94);
    this.add.rectangle(640, 360, 1280, 720, 0x05091a, 0.45);
    this.add.image(640, 114, 'headerBar').setDisplaySize(800, 210).setAlpha(0.82);
    this.add.image(640, 94, 'voltaseEmblem').setDisplaySize(70, 60).setAlpha(0.95);
    this.add.text(640, 150, 'RULEBREAKER VERSUS', { fontSize: '42px', color: '#eef8ff', fontStyle: 'bold' }).setOrigin(0.5);
    this.add.text(640, 194, 'Mobile-first duel prototype // Season 1 style pass', { fontSize: '18px', color: '#f0b9ef' }).setOrigin(0.5);
    this.add.image(640, 398, 'themePanel').setDisplaySize(540, 330).setAlpha(0.92);
    const button = this.add.image(640, 610, 'sideTag').setDisplaySize(260, 54).setInteractive({ useHandCursor: true });
    button.setTint(0x5bd8ff);
    this.add.text(640, 610, 'TAP TO ENTER THE ARENA', { fontSize: '22px', color: '#eef8ff', fontStyle: 'bold' }).setOrigin(0.5);
    this.add.text(640, 660, 'If socket.io is running on localhost:3000, matchmaking starts automatically. Otherwise it falls back to local duel mode.', { fontSize: '15px', color: '#d9e9ff', align: 'center', wordWrap: { width: 880 } }).setOrigin(0.5);
    button.on('pointerdown', () => this.scene.start('RulebreakerScene'));
  }
}
