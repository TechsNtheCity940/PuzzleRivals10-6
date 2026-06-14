export default class BootScene extends Phaser.Scene {
  constructor() { super('BootScene'); }
  preload() {
    this.load.image('menuBg', 'assets/images/menu_bg.png');
    this.load.image('boardsBg', 'assets/images/boards_bg.png');
    this.load.image('themePanel', 'assets/images/theme_panel.png');
    this.load.image('headerBar', 'assets/ui/header_bar.png');
    this.load.image('boardFrame', 'assets/ui/board_frame.png');
    this.load.image('boardInnerBg', 'assets/ui/board_inner_bg.png');
    this.load.image('sideTag', 'assets/ui/side_tag.png');
    this.load.image('scoreModule', 'assets/ui/score_module.png');
    this.load.image('hudStrip', 'assets/ui/hud_strip.png');
    this.load.image('ruleBanner', 'assets/ui/rule_banner.png');
    this.load.image('voltaseEmblem', 'assets/ui/voltase_emblem.png');
    this.load.image('tilePink', 'assets/ui/tile_pink.png');
    this.load.image('tileCyan', 'assets/ui/tile_cyan.png');
    this.load.image('tileMint', 'assets/ui/tile_mint.png');
    this.load.image('tileAmber', 'assets/ui/tile_amber.png');
    this.load.image('familyPulse', 'assets/ui/icon_circle.png');
    this.load.image('familyNova', 'assets/ui/icon_square.png');
    this.load.image('familyFlux', 'assets/ui/icon_triangle.png');
    this.load.image('familyVector', 'assets/ui/icon_diamond.png');
    this.load.image('familyPrism', 'assets/ui/icon_circle.png');
  }
  create() { this.scene.start('MenuScene'); }
}
