# Mirror Maze Artwork Package

This package contains a full artwork workup for the `Mirror Maze` puzzle game, including the original v01 batch and a v02 refinement and support pass.

## What Is Included
- normalized brief and production assumptions
- style lock and prompt pack
- asset, prompt, animation, and queue manifests
- generated raw artwork
- selected artwork with deterministic filenames
- export PNGs
- engine-ready PNGs with a first-pass transparency cleanup
- normalized v03 sheets for cleaner slicing
- Phaser-ready metadata, sheet maps, and animation helpers
- QC checklist

## Generated Asset Set
- angled mirror variants: 4-orientation sheet
- prism variants: 4-variant sheet
- blocker variants: 4-variant sheet
- source emitter: 4-frame loop
- target receiver: 4-frame loop
- traveling beam: 4-frame loop
- activator: 6-frame loop
- blocked beam impact: 6-frame loop
- refined angled mirror sheet: v02 spacing pass
- board tile variants: 4-piece support sheet
- beam socket variants: 4-piece support sheet
- prism split overlay: 6-frame loop
- UI icon sheet: 7-icon support sheet

## Folder Notes
- `04_generated/raw`: untouched generated outputs
- `04_generated/selected`: renamed production picks
- `06_exports/png`: export copies of the selected files
- `06_exports/engine_ready`: first-pass cutout versions with alpha added for engine use

## Engine Import Notes
- Treat mirrors, prisms, and blockers as board pieces with manual slicing by visual cell.
- Treat the animated strips as horizontal strips and slice them by frame count from `animation_manifest.json`.
- Recommended starting fps:
  - source emitter: 10
  - target receiver: 10
  - traveling beam: 14
  - activator: 12
  - blocked impact: 16
- Keep beam body, impact, and activator FX on separate layers so you can composite them in-game.

## QC Note
The generator produced flattened backgrounds on the first pass. `engine_ready` includes an automated edge-based transparency cleanup pass. For shipping use, a manual polish pass is still recommended to tighten masks and frame boundaries.

## v02 Additions
- Added a refined mirror sheet intended to be easier to slice cleanly than the original v01 layout.
- Added board support art so the puzzle pieces have matching floor and socket context.
- Added a compact prism split overlay for layered beam-splitting feedback.
- Added a readable HUD icon sheet for source, target, mirror, prism, blocker, activator, and beam energy.

## v03 Additions
- Added normalized fixed-frame sheets under `05_sheets/` for safer manual and engine slicing.
- Added per-asset Phaser sheet maps under `07_manifests/phaser/`.
- Added a generated Phaser helper and runtime data package under `09_phaser/`.
- Added a report summarizing the metadata assumptions and recommended manual follow-up.

## v04 Additions
- Added targeted replacement passes for the traveling beam, activator, blocked impact, and prism split overlay.
- Promoted those `v04` effects as the preferred runtime FX set in the Phaser preview scaffold.
- Expanded the package into a preview-ready Phaser mini project with `package.json`, `index.html`, and a board showcase scene.
