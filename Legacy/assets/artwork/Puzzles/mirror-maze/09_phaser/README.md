# Mirror Maze Phaser Integration

This folder contains a Phaser-ready export layer built from the Mirror Maze art package.

## Included
- `assets/`: normalized v03 sprite sheets with fixed frame sizes and transparent backgrounds
- `public/assets/`: static copies for browser-served previews
- `src/game/animations/generated/mirror-maze.animations.ts`: preload and animation registration helper
- `src/game/data/mirror-maze-sheetmaps.ts`: generated analysis manifest for runtime or tooling
- `reports/sprite-sheet-animation-report.md`: human-readable summary of the normalization pass
- `prototype.html`: offline-friendly playable fallback that runs without installing dependencies

## Recommended Runtime Choice
- Prefer `routing_angled_mirror_sheet_v02` over the older `routing_angled_mirror_sheet` key for runtime use.
- Prefer `beam_traveling_beam_v04`, `trigger_activator_v04`, `beam_blocked_impact_v04`, and `beam_prism_split_overlay_v04` for runtime effects.
- Keep the original mirror key only if you want both visual options available during implementation tests.

## Usage
```ts
import Phaser from 'phaser';
import {
  preloadMirrorMazeSpriteSheets,
  registerMirrorMazeAnimations,
} from './src/game/animations/generated/mirror-maze.animations';

export class MirrorMazeScene extends Phaser.Scene {
  preload() {
    preloadMirrorMazeSpriteSheets(this, 'assets');
  }

  create() {
    registerMirrorMazeAnimations(this);
  }
}
```

## Notes
- Static sheets are exposed as sprite sheets for convenient frame-based tile selection.
- Animated FX and source or target pieces register looping animations automatically.
- The normalized v03 sheets are safer for slicing than the earlier raw generator outputs, and the `v04` effects are the current preferred runtime replacements.
- In this environment, registry access blocked dependency install, so `prototype.html` is the reliable runnable preview while the Phaser scaffold remains the implementation target.
