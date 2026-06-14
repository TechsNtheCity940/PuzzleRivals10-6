const BACK_VARIANT = "shimmer";

function symbolSheet(name) {
  return `memory_lights_symbol_match_symbol_tile_${name}_reveal_06f_v01`;
}

function backSheet() {
  return `memory_lights_symbol_match_tile_back_${BACK_VARIANT}_06f_v01`;
}

export function preloadSymbolMatchSpriteSheets(scene) {
  const base = "memory_lights/symbol_match/06_exports/png";
  const sheets = [
    symbolSheet("diamond"),
    symbolSheet("star"),
    symbolSheet("sun"),
    symbolSheet("moon"),
    symbolSheet("wave"),
    symbolSheet("rune"),
    symbolSheet("hex"),
    symbolSheet("disc"),
    backSheet()
  ];

  sheets.forEach((key) => {
    scene.load.spritesheet(key, `${base}/${key}.png`, {
      frameWidth: 256,
      frameHeight: 256
    });
  });
}

export function registerSymbolMatchAnimations(scene) {
  const reveals = ["diamond", "star", "sun", "moon", "wave", "rune", "hex", "disc"];
  reveals.forEach((name) => {
    const key = `symbol_tile_${name}_reveal_play`;
    if (!scene.anims.exists(key)) {
      scene.anims.create({
        key,
        frames: scene.anims.generateFrameNumbers(symbolSheet(name), { start: 0, end: 5 }),
        frameRate: 12,
        repeat: 0
      });
    }
  });

  [["tile_back_shimmer_loop", "shimmer", 8], ["tile_back_focus_loop", "focus", 10]].forEach(([animKey, variant, frameRate]) => {
    const sheetKey = `memory_lights_symbol_match_tile_back_${variant}_06f_v01`;
    if (!scene.anims.exists(animKey)) {
      scene.anims.create({
        key: animKey,
        frames: scene.anims.generateFrameNumbers(sheetKey, { start: 0, end: 5 }),
        frameRate,
        repeat: -1
      });
    }
  });
}
