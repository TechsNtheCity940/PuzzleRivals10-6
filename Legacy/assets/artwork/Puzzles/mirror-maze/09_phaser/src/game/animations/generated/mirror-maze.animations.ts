import Phaser from 'phaser';

export const mirrorMazeSpriteSheets = [
  { key: 'routing_angled_mirror_sheet', path: 'mirror_maze_main_puzzle_routing_angled_mirror_sheet_idle_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: false },
  { key: 'routing_prism_sheet', path: 'mirror_maze_main_puzzle_routing_prism_sheet_idle_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: false },
  { key: 'source_source_emitter', path: 'mirror_maze_main_puzzle_source_source_emitter_pulse_04f_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: true },
  { key: 'target_target_receiver', path: 'mirror_maze_main_puzzle_target_target_receiver_charge_04f_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: true },
  { key: 'beam_traveling_beam', path: 'mirror_maze_main_puzzle_beam_traveling_beam_flow_04f_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: true },
  { key: 'trigger_activator', path: 'mirror_maze_main_puzzle_trigger_activator_loop_06f_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 6, animated: true },
  { key: 'beam_blocked_impact', path: 'mirror_maze_main_puzzle_beam_blocked_impact_loop_06f_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 6, animated: true },
  { key: 'routing_blocker_sheet', path: 'mirror_maze_main_puzzle_routing_blocker_sheet_idle_v01_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: false },
  { key: 'routing_angled_mirror_sheet_v02', path: 'mirror_maze_main_puzzle_routing_angled_mirror_sheet_idle_v02_v03_sheet.png', frameWidth: 384, frameHeight: 384, frameCount: 4, animated: false },
  { key: 'board_board_tile_sheet', path: 'mirror_maze_main_puzzle_board_board_tile_sheet_idle_v01_v03_sheet.png', frameWidth: 384, frameHeight: 384, frameCount: 4, animated: false },
  { key: 'board_beam_socket_sheet', path: 'mirror_maze_main_puzzle_board_beam_socket_sheet_idle_v01_v03_sheet.png', frameWidth: 384, frameHeight: 384, frameCount: 4, animated: false },
  { key: 'beam_prism_split_overlay', path: 'mirror_maze_main_puzzle_beam_prism_split_overlay_loop_06f_v01_v03_sheet.png', frameWidth: 341, frameHeight: 341, frameCount: 6, animated: true },
  { key: 'ui_puzzle_icon_sheet', path: 'mirror_maze_main_puzzle_ui_puzzle_icon_sheet_idle_v01_v03_sheet.png', frameWidth: 292, frameHeight: 292, frameCount: 7, animated: false },
  { key: 'beam_traveling_beam_v04', path: 'mirror_maze_main_puzzle_beam_traveling_beam_flow_04f_v04_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 4, animated: true },
  { key: 'trigger_activator_v04', path: 'mirror_maze_main_puzzle_trigger_activator_loop_06f_v04_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 6, animated: true },
  { key: 'beam_blocked_impact_v04', path: 'mirror_maze_main_puzzle_beam_blocked_impact_loop_06f_v04_v03_sheet.png', frameWidth: 256, frameHeight: 256, frameCount: 6, animated: true },
  { key: 'beam_prism_split_overlay_v04', path: 'mirror_maze_main_puzzle_beam_prism_split_overlay_loop_06f_v04_v03_sheet.png', frameWidth: 341, frameHeight: 341, frameCount: 6, animated: true },
] as const;

export function preloadMirrorMazeSpriteSheets(scene: Phaser.Scene, assetBaseUrl = 'assets') {
  scene.load.spritesheet('routing_angled_mirror_sheet', assetBaseUrl + '/mirror_maze_main_puzzle_routing_angled_mirror_sheet_idle_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('routing_prism_sheet', assetBaseUrl + '/mirror_maze_main_puzzle_routing_prism_sheet_idle_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('source_source_emitter', assetBaseUrl + '/mirror_maze_main_puzzle_source_source_emitter_pulse_04f_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('target_target_receiver', assetBaseUrl + '/mirror_maze_main_puzzle_target_target_receiver_charge_04f_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('beam_traveling_beam', assetBaseUrl + '/mirror_maze_main_puzzle_beam_traveling_beam_flow_04f_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('trigger_activator', assetBaseUrl + '/mirror_maze_main_puzzle_trigger_activator_loop_06f_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('beam_blocked_impact', assetBaseUrl + '/mirror_maze_main_puzzle_beam_blocked_impact_loop_06f_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('routing_blocker_sheet', assetBaseUrl + '/mirror_maze_main_puzzle_routing_blocker_sheet_idle_v01_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('routing_angled_mirror_sheet_v02', assetBaseUrl + '/mirror_maze_main_puzzle_routing_angled_mirror_sheet_idle_v02_v03_sheet.png', { frameWidth: 384, frameHeight: 384 });
  scene.load.spritesheet('board_board_tile_sheet', assetBaseUrl + '/mirror_maze_main_puzzle_board_board_tile_sheet_idle_v01_v03_sheet.png', { frameWidth: 384, frameHeight: 384 });
  scene.load.spritesheet('board_beam_socket_sheet', assetBaseUrl + '/mirror_maze_main_puzzle_board_beam_socket_sheet_idle_v01_v03_sheet.png', { frameWidth: 384, frameHeight: 384 });
  scene.load.spritesheet('beam_prism_split_overlay', assetBaseUrl + '/mirror_maze_main_puzzle_beam_prism_split_overlay_loop_06f_v01_v03_sheet.png', { frameWidth: 341, frameHeight: 341 });
  scene.load.spritesheet('ui_puzzle_icon_sheet', assetBaseUrl + '/mirror_maze_main_puzzle_ui_puzzle_icon_sheet_idle_v01_v03_sheet.png', { frameWidth: 292, frameHeight: 292 });
  scene.load.spritesheet('beam_traveling_beam_v04', assetBaseUrl + '/mirror_maze_main_puzzle_beam_traveling_beam_flow_04f_v04_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('trigger_activator_v04', assetBaseUrl + '/mirror_maze_main_puzzle_trigger_activator_loop_06f_v04_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('beam_blocked_impact_v04', assetBaseUrl + '/mirror_maze_main_puzzle_beam_blocked_impact_loop_06f_v04_v03_sheet.png', { frameWidth: 256, frameHeight: 256 });
  scene.load.spritesheet('beam_prism_split_overlay_v04', assetBaseUrl + '/mirror_maze_main_puzzle_beam_prism_split_overlay_loop_06f_v04_v03_sheet.png', { frameWidth: 341, frameHeight: 341 });
}

export function registerMirrorMazeAnimations(scene: Phaser.Scene) {
  if (!scene.anims.exists('source_source_emitter_loop')) {
    scene.anims.create({
      key: 'source_source_emitter_loop',
      frames: scene.anims.generateFrameNumbers('source_source_emitter', { start: 0, end: 3 }),
      frameRate: 10,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('target_target_receiver_loop')) {
    scene.anims.create({
      key: 'target_target_receiver_loop',
      frames: scene.anims.generateFrameNumbers('target_target_receiver', { start: 0, end: 3 }),
      frameRate: 10,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('beam_traveling_beam_loop')) {
    scene.anims.create({
      key: 'beam_traveling_beam_loop',
      frames: scene.anims.generateFrameNumbers('beam_traveling_beam', { start: 0, end: 3 }),
      frameRate: 14,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('trigger_activator_loop')) {
    scene.anims.create({
      key: 'trigger_activator_loop',
      frames: scene.anims.generateFrameNumbers('trigger_activator', { start: 0, end: 5 }),
      frameRate: 12,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('beam_blocked_impact_loop')) {
    scene.anims.create({
      key: 'beam_blocked_impact_loop',
      frames: scene.anims.generateFrameNumbers('beam_blocked_impact', { start: 0, end: 5 }),
      frameRate: 16,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('beam_prism_split_overlay_loop')) {
    scene.anims.create({
      key: 'beam_prism_split_overlay_loop',
      frames: scene.anims.generateFrameNumbers('beam_prism_split_overlay', { start: 0, end: 5 }),
      frameRate: 14,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('beam_traveling_beam_v04_loop')) {
    scene.anims.create({
      key: 'beam_traveling_beam_v04_loop',
      frames: scene.anims.generateFrameNumbers('beam_traveling_beam_v04', { start: 0, end: 3 }),
      frameRate: 14,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('trigger_activator_v04_loop')) {
    scene.anims.create({
      key: 'trigger_activator_v04_loop',
      frames: scene.anims.generateFrameNumbers('trigger_activator_v04', { start: 0, end: 5 }),
      frameRate: 12,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('beam_blocked_impact_v04_loop')) {
    scene.anims.create({
      key: 'beam_blocked_impact_v04_loop',
      frames: scene.anims.generateFrameNumbers('beam_blocked_impact_v04', { start: 0, end: 5 }),
      frameRate: 16,
      repeat: -1,
    });
  }
  if (!scene.anims.exists('beam_prism_split_overlay_v04_loop')) {
    scene.anims.create({
      key: 'beam_prism_split_overlay_v04_loop',
      frames: scene.anims.generateFrameNumbers('beam_prism_split_overlay_v04', { start: 0, end: 5 }),
      frameRate: 14,
      repeat: -1,
    });
  }
}
