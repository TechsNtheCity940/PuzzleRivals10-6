export default class BootScene extends Phaser.Scene {
  constructor() {
    super('BootScene');
  }

  preload() {
    // preload future assets here
    // example:
    // this.load.image('tileBg', 'assets/images/tileBg.png');
  }

  create() {
    this.scene.start('MenuScene');
  }
}