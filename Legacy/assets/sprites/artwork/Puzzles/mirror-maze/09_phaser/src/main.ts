import Phaser from 'phaser';
import { MirrorMazePreviewScene } from './game/scenes/MirrorMazePreviewScene';

const container = document.getElementById('game');

if (!container) {
  throw new Error('Missing #game container');
}

new Phaser.Game({
  type: Phaser.AUTO,
  parent: container,
  backgroundColor: '#04060a',
  scale: {
    mode: Phaser.Scale.RESIZE,
    autoCenter: Phaser.Scale.CENTER_BOTH,
    width: 1280,
    height: 840,
  },
  render: {
    antialias: true,
    pixelArt: false,
  },
  scene: [MirrorMazePreviewScene],
});
