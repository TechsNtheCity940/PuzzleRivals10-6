const GLOW_VARIANT = "soft";

function orbSheet(color) {
  return `memory_lights_color_sequence_signal_orb_${color}_glow_${GLOW_VARIANT}_08f_v01`;
}

export function preloadColorSequenceSpriteSheets(scene) {
  const base = "memory_lights/color_sequence/06_exports/png";
  const sheets = [
    "memory_lights_color_sequence_input_pad_red_pulse_08f_v01",
    "memory_lights_color_sequence_input_pad_yellow_pulse_08f_v01",
    "memory_lights_color_sequence_input_pad_green_pulse_08f_v01",
    "memory_lights_color_sequence_input_pad_cyan_pulse_08f_v01",
    "memory_lights_color_sequence_input_pad_blue_pulse_08f_v01",
    "memory_lights_color_sequence_input_pad_purple_pulse_08f_v01",
    orbSheet("red"),
    orbSheet("blue"),
    orbSheet("green"),
    orbSheet("yellow"),
    orbSheet("purple"),
    orbSheet("orange")
  ];

  sheets.forEach((key) => {
    scene.load.spritesheet(key, `${base}/${key}.png`, {
      frameWidth: 256,
      frameHeight: 256
    });
  });
}

export function registerColorSequenceAnimations(scene) {
  const definitions = [
    ["input_pad_red_pulse_loop", "memory_lights_color_sequence_input_pad_red_pulse_08f_v01", 14],
    ["input_pad_yellow_pulse_loop", "memory_lights_color_sequence_input_pad_yellow_pulse_08f_v01", 14],
    ["input_pad_green_pulse_loop", "memory_lights_color_sequence_input_pad_green_pulse_08f_v01", 14],
    ["input_pad_cyan_pulse_loop", "memory_lights_color_sequence_input_pad_cyan_pulse_08f_v01", 14],
    ["input_pad_blue_pulse_loop", "memory_lights_color_sequence_input_pad_blue_pulse_08f_v01", 14],
    ["input_pad_purple_pulse_loop", "memory_lights_color_sequence_input_pad_purple_pulse_08f_v01", 14],
    ["signal_orb_red_glow_soft_loop", orbSheet("red"), 9],
    ["signal_orb_blue_glow_soft_loop", orbSheet("blue"), 9],
    ["signal_orb_green_glow_soft_loop", orbSheet("green"), 9],
    ["signal_orb_yellow_glow_soft_loop", orbSheet("yellow"), 9],
    ["signal_orb_purple_glow_soft_loop", orbSheet("purple"), 9],
    ["signal_orb_orange_glow_soft_loop", orbSheet("orange"), 9]
  ];

  definitions.forEach(([animKey, sheetKey, frameRate]) => {
    if (scene.anims.exists(animKey)) {
      return;
    }

    scene.anims.create({
      key: animKey,
      frames: scene.anims.generateFrameNumbers(sheetKey, { start: 0, end: 7 }),
      frameRate,
      repeat: -1
    });
  });
}
