import BootScene from './scenes/BootScene.js';
import MenuScene from './scenes/MenuScene.js';
import RulebreakerScene from './scenes/RulebreakerScene.js';

const config = {
  type: Phaser.AUTO,
  parent: 'game-container',
  width: 1280,
  height: 720,
  backgroundColor: '#0b1020',
  scene: [BootScene, MenuScene, RulebreakerScene]
};

new Phaser.Game(config);
