const TILE_KEYS = { pink: 'tilePink', cyan: 'tileCyan', mint: 'tileMint', amber: 'tileAmber' };

export default class PuzzleTile {
  constructor(scene, x, y, size, data) {
    this.scene = scene;
    this.data = data;
    this.selected = false;
    this.size = size;
    this.baseX = x;
    this.baseY = y;
    this.container = scene.add.container(x, y);

    this.glow = scene.add.circle(0, 0, size * 0.64, data.displayColor, 0.13).setBlendMode(Phaser.BlendModes.ADD);
    this.shadow = scene.add.ellipse(0, size * 0.42, size * 0.88, 10, 0x000000, 0.28);
    this.bg = scene.add.image(0, 0, TILE_KEYS[data.color] || 'tileCyan').setDisplaySize(size + 10, size + 10);
    this.inner = scene.add.rectangle(0, 0, size * 0.72, size * 0.72, 0x061225, 0.58).setStrokeStyle(1, data.displayColor, 0.22);
    this.icon = scene.add.image(0, -4, data.iconKey).setDisplaySize(size * 0.36, size * 0.36).setBlendMode(Phaser.BlendModes.SCREEN);
    this.familyText = scene.add.text(0, -size * 0.23, data.badge, { fontSize: `${Math.floor(size * 0.11)}px`, color: '#a8d7ff', fontStyle: 'bold' }).setOrigin(0.5);
    this.valueChip = scene.add.rectangle(0, size * 0.28, size * 0.36, 14, 0x071020, 0.86).setStrokeStyle(1, data.displayColor, 0.68);
    this.valueText = scene.add.text(0, size * 0.28, `${data.value}`, { fontSize: `${Math.floor(size * 0.18)}px`, color: '#eff9ff', fontStyle: 'bold' }).setOrigin(0.5);
    this.scan = scene.add.rectangle(0, -size * 0.11, size * 0.50, 5, 0xffffff, 0.08);
    this.sparkA = scene.add.circle(size * 0.23, -size * 0.19, 2.3, 0xffffff, 0.7).setBlendMode(Phaser.BlendModes.ADD);
    this.sparkB = scene.add.circle(-size * 0.20, size * 0.03, 1.8, data.displayColor, 0.55).setBlendMode(Phaser.BlendModes.ADD);
    this.container.add([this.glow, this.shadow, this.bg, this.inner, this.icon, this.familyText, this.valueChip, this.valueText, this.scan, this.sparkA, this.sparkB]);

    this.shell = this.bg;
    this.bg.setInteractive({ useHandCursor: true });

    scene.tweens.add({ targets: [this.sparkA, this.sparkB], alpha: { from: 0.75, to: 0.18 }, duration: 820, yoyo: true, repeat: -1, delay: Phaser.Math.Between(0, 450) });
    scene.tweens.add({ targets: this.scan, x: { from: -size * 0.12, to: size * 0.12 }, duration: 900, yoyo: true, repeat: -1, ease: 'Sine.InOut' });
  }

  setSelected(value) {
    this.selected = value;
    this.glow.fillColor = value ? 0xffe67f : this.data.displayColor;
    this.glow.alpha = value ? 0.28 : 0.13;
    this.container.setScale(value ? 1.09 : 1);
    this.icon.setTint(value ? 0xfff5d9 : 0xffffff);
    this.valueChip.setStrokeStyle(1, value ? 0xffdf7a : this.data.displayColor, value ? 1 : 0.68);
  }

  pulseRule(color) {
    this.glow.fillColor = color;
    this.scene.tweens.add({ targets: this.glow, alpha: { from: 0.30, to: 0.12 }, duration: 250, ease: 'Sine.Out' });
  }

  pulseInvalid() {
    this.scene.tweens.add({ targets: this.container, x: { from: this.baseX - 4, to: this.baseX + 4 }, duration: 42, yoyo: true, repeat: 4, onComplete: () => { this.container.x = this.baseX; } });
    this.scene.tweens.add({ targets: this.glow, alpha: { from: 0.34, to: 0.12 }, duration: 240 });
  }

  burst(color) {
    const ring = this.scene.add.circle(this.baseX, this.baseY, this.size * 0.28, color, 0.22).setBlendMode(Phaser.BlendModes.ADD);
    this.scene.tweens.add({ targets: ring, radius: this.size * 0.82, alpha: 0, duration: 260, onComplete: () => ring.destroy() });
    for (let i = 0; i < 5; i++) {
      const dot = this.scene.add.circle(this.baseX, this.baseY, 2, color, 0.9).setBlendMode(Phaser.BlendModes.ADD);
      const angle = Phaser.Math.FloatBetween(0, Math.PI * 2);
      const dist = Phaser.Math.Between(14, 28);
      this.scene.tweens.add({ targets: dot, x: this.baseX + Math.cos(angle) * dist, y: this.baseY + Math.sin(angle) * dist, alpha: 0, duration: 260, onComplete: () => dot.destroy() });
    }
  }

  destroy() { this.container.destroy(true); }
}
