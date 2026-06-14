// Collapse Grid Phaser 3 asset loader + animation registration
// Drop this file into your Phaser project and call preloadCollapseGridAssets(this)
// inside preload(), then createCollapseGridAnimations(this) inside create().

export const CollapseGridAssets = {
  basePath: "assets/collapse_grid/",
  sheets: [
    { key: "collapse_grid_support_idle", url: "spritesheets/collapse_grid_support_idle.png", frameWidth: 96, frameHeight: 96, frames: 8, frameRate: 8, repeat: -1 },
    { key: "collapse_grid_weak_spot_pulse", url: "spritesheets/collapse_grid_weak_spot_pulse.png", frameWidth: 128, frameHeight: 128, frames: 8, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_crack_spread", url: "spritesheets/collapse_grid_crack_spread.png", frameWidth: 128, frameHeight: 128, frames: 8, frameRate: 12, repeat: 0 },
    { key: "collapse_grid_remove_tile", url: "spritesheets/collapse_grid_remove_tile.png", frameWidth: 128, frameHeight: 128, frames: 6, frameRate: 14, repeat: 0 },
    { key: "collapse_grid_shake_structure", url: "spritesheets/collapse_grid_shake_structure.png", frameWidth: 640, frameHeight: 640, frames: 8, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_debris_fall", url: "spritesheets/collapse_grid_debris_fall.png", frameWidth: 160, frameHeight: 160, frames: 10, frameRate: 16, repeat: 0 },
    { key: "collapse_grid_dust_plume", url: "spritesheets/collapse_grid_dust_plume.png", frameWidth: 256, frameHeight: 256, frames: 8, frameRate: 12, repeat: 0 },
    { key: "collapse_grid_controlled_chain", url: "spritesheets/collapse_grid_controlled_chain.png", frameWidth: 320, frameHeight: 160, frames: 10, frameRate: 14, repeat: 0 },
    { key: "collapse_grid_self_damage_warning", url: "spritesheets/collapse_grid_self_damage_warning.png", frameWidth: 256, frameHeight: 128, frames: 8, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_score_lane", url: "spritesheets/collapse_grid_score_lane.png", frameWidth: 128, frameHeight: 512, frames: 8, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_player_card_shimmer", url: "spritesheets/collapse_grid_player_card_shimmer.png", frameWidth: 512, frameHeight: 160, frames: 6, frameRate: 8, repeat: -1 },
    { key: "collapse_grid_endgame_pressure", url: "spritesheets/collapse_grid_endgame_pressure.png", frameWidth: 1024, frameHeight: 256, frames: 6, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_crack_tile", url: "spritesheets/collapse_grid_crack_tile.png", frameWidth: 128, frameHeight: 128, frames: 8, frameRate: 12, repeat: 0 },
    { key: "collapse_grid_weight_drop", url: "spritesheets/collapse_grid_weight_drop.png", frameWidth: 160, frameHeight: 160, frames: 8, frameRate: 14, repeat: 0 },
    { key: "collapse_grid_aftershock", url: "spritesheets/collapse_grid_aftershock.png", frameWidth: 640, frameHeight: 640, frames: 8, frameRate: 10, repeat: 0 },
    { key: "collapse_grid_dust_cloud", url: "spritesheets/collapse_grid_dust_cloud.png", frameWidth: 640, frameHeight: 640, frames: 6, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_reinforce", url: "spritesheets/collapse_grid_reinforce.png", frameWidth: 128, frameHeight: 128, frames: 6, frameRate: 12, repeat: 0 },
    { key: "collapse_grid_brace", url: "spritesheets/collapse_grid_brace.png", frameWidth: 256, frameHeight: 256, frames: 8, frameRate: 10, repeat: -1 },
    { key: "collapse_grid_survey", url: "spritesheets/collapse_grid_survey.png", frameWidth: 640, frameHeight: 640, frames: 6, frameRate: 10, repeat: 0 },
    { key: "collapse_grid_patch", url: "spritesheets/collapse_grid_patch.png", frameWidth: 128, frameHeight: 128, frames: 6, frameRate: 12, repeat: 0 }
  ],
  atlas: {
    key: "collapse_grid_support_atlas",
    url: "spritesheets/collapse_grid_support_atlas.png",
    frameWidth: 64,
    frameHeight: 64
  }
};

export function preloadCollapseGridAssets(scene, basePath = CollapseGridAssets.basePath) {
  for (const sheet of CollapseGridAssets.sheets) {
    scene.load.spritesheet(sheet.key, basePath + sheet.url, {
      frameWidth: sheet.frameWidth,
      frameHeight: sheet.frameHeight,
      endFrame: sheet.frames - 1
    });
  }

  scene.load.spritesheet(CollapseGridAssets.atlas.key, basePath + CollapseGridAssets.atlas.url, {
    frameWidth: CollapseGridAssets.atlas.frameWidth,
    frameHeight: CollapseGridAssets.atlas.frameHeight
  });
}

export function createCollapseGridAnimations(scene) {
  const anims = scene.anims;

if (!anims.exists("collapse_grid_support_idle")) {
  anims.create({
    key: "collapse_grid_support_idle",
    frames: anims.generateFrameNumbers("collapse_grid_support_idle", { start: 0, end: 7 }),
    frameRate: 8,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_weak_spot_pulse")) {
  anims.create({
    key: "collapse_grid_weak_spot_pulse",
    frames: anims.generateFrameNumbers("collapse_grid_weak_spot_pulse", { start: 0, end: 7 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_crack_spread")) {
  anims.create({
    key: "collapse_grid_crack_spread",
    frames: anims.generateFrameNumbers("collapse_grid_crack_spread", { start: 0, end: 7 }),
    frameRate: 12,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_remove_tile")) {
  anims.create({
    key: "collapse_grid_remove_tile",
    frames: anims.generateFrameNumbers("collapse_grid_remove_tile", { start: 0, end: 5 }),
    frameRate: 14,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_shake_structure")) {
  anims.create({
    key: "collapse_grid_shake_structure",
    frames: anims.generateFrameNumbers("collapse_grid_shake_structure", { start: 0, end: 7 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_debris_fall")) {
  anims.create({
    key: "collapse_grid_debris_fall",
    frames: anims.generateFrameNumbers("collapse_grid_debris_fall", { start: 0, end: 9 }),
    frameRate: 16,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_dust_plume")) {
  anims.create({
    key: "collapse_grid_dust_plume",
    frames: anims.generateFrameNumbers("collapse_grid_dust_plume", { start: 0, end: 7 }),
    frameRate: 12,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_controlled_chain")) {
  anims.create({
    key: "collapse_grid_controlled_chain",
    frames: anims.generateFrameNumbers("collapse_grid_controlled_chain", { start: 0, end: 9 }),
    frameRate: 14,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_self_damage_warning")) {
  anims.create({
    key: "collapse_grid_self_damage_warning",
    frames: anims.generateFrameNumbers("collapse_grid_self_damage_warning", { start: 0, end: 7 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_score_lane")) {
  anims.create({
    key: "collapse_grid_score_lane",
    frames: anims.generateFrameNumbers("collapse_grid_score_lane", { start: 0, end: 7 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_player_card_shimmer")) {
  anims.create({
    key: "collapse_grid_player_card_shimmer",
    frames: anims.generateFrameNumbers("collapse_grid_player_card_shimmer", { start: 0, end: 5 }),
    frameRate: 8,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_endgame_pressure")) {
  anims.create({
    key: "collapse_grid_endgame_pressure",
    frames: anims.generateFrameNumbers("collapse_grid_endgame_pressure", { start: 0, end: 5 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_crack_tile")) {
  anims.create({
    key: "collapse_grid_crack_tile",
    frames: anims.generateFrameNumbers("collapse_grid_crack_tile", { start: 0, end: 7 }),
    frameRate: 12,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_weight_drop")) {
  anims.create({
    key: "collapse_grid_weight_drop",
    frames: anims.generateFrameNumbers("collapse_grid_weight_drop", { start: 0, end: 7 }),
    frameRate: 14,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_aftershock")) {
  anims.create({
    key: "collapse_grid_aftershock",
    frames: anims.generateFrameNumbers("collapse_grid_aftershock", { start: 0, end: 7 }),
    frameRate: 10,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_dust_cloud")) {
  anims.create({
    key: "collapse_grid_dust_cloud",
    frames: anims.generateFrameNumbers("collapse_grid_dust_cloud", { start: 0, end: 5 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_reinforce")) {
  anims.create({
    key: "collapse_grid_reinforce",
    frames: anims.generateFrameNumbers("collapse_grid_reinforce", { start: 0, end: 5 }),
    frameRate: 12,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_brace")) {
  anims.create({
    key: "collapse_grid_brace",
    frames: anims.generateFrameNumbers("collapse_grid_brace", { start: 0, end: 7 }),
    frameRate: 10,
    repeat: -1,
    hideOnComplete: false
  });
}

if (!anims.exists("collapse_grid_survey")) {
  anims.create({
    key: "collapse_grid_survey",
    frames: anims.generateFrameNumbers("collapse_grid_survey", { start: 0, end: 5 }),
    frameRate: 10,
    repeat: 0,
    hideOnComplete: true
  });
}

if (!anims.exists("collapse_grid_patch")) {
  anims.create({
    key: "collapse_grid_patch",
    frames: anims.generateFrameNumbers("collapse_grid_patch", { start: 0, end: 5 }),
    frameRate: 12,
    repeat: 0,
    hideOnComplete: true
  });
}
}

export function playCollapseFx(scene, key, x, y, options = {}) {
  const sprite = scene.add.sprite(x, y, key);
  sprite.setDepth(options.depth ?? 100);
  sprite.setScale(options.scale ?? 1);
  sprite.setAlpha(options.alpha ?? 1);
  sprite.play(key);

  if (options.tint) sprite.setTint(options.tint);

  if (options.destroyOnComplete !== false) {
    sprite.once(Phaser.Animations.Events.ANIMATION_COMPLETE, () => sprite.destroy());
  }

  return sprite;
}

// Suggested tile-state frame indexes in collapse_grid_support_atlas.
export const CollapseGridTileFrames = {
  support_cyan: 0,
  support_magenta: 1,
  support_gold: 2,
  support_green: 3,
  support_stone: 4,
  support_cracked_cyan: 5,
  support_cracked_magenta: 6,
  support_reinforced: 7,
  objective_core: 8,
  objective_core_damaged: 9,
  lock_blue: 10,
  lock_red: 11,
  weight_1t: 12,
  weight_2t: 13,
  anchor_gold: 14,
  blocked_cell: 15,
  debris_blue: 16,
  debris_pink: 17,
  debris_gold: 18,
  dust_patch: 19,
  brace_marker: 20,
  danger_marker: 21,
  survey_marker: 22,
  empty_shadow: 23
};
