export default class PuzzleTile {
  constructor(scene, x, y, size, data) {
    this.scene = scene;
    this.data = data;
    this.selected = false;

    this.container = scene.add.container(x, y);

    this.bg = scene.add.rectangle(0, 0, size, size, data.displayColor)
      .setStrokeStyle(2, 0xffffff);

    this.label = scene.add.text(0, 0, `${data.shape}\n${data.value}`, {
      fontSize: '18px',
      color: '#ffffff',
      align: 'center'
    }).setOrigin(0.5);

    this.container.add([this.bg, this.label]);

    this.bg.setInteractive();
  }

  setSelected(value) {
    this.selected = value;
    this.bg.setStrokeStyle(4, value ? 0xffff00 : 0xffffff);
    this.container.setScale(value ? 1.08 : 1);
  }

  pulseInvalid() {
    this.scene.tweens.add({
      targets: this.container,
      angle: { from: -4, to: 4 },
      duration: 60,
      yoyo: true,
      repeat: 3,
      onComplete: () => {
        this.container.angle = 0;
      }
    });
  }

  destroy() {
    this.container.destroy(true);
  }
}