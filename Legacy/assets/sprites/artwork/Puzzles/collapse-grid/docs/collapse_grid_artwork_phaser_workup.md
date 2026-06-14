# Collapse Grid — Full Artwork & Phaser Animation Workup

## 1. Art Direction

**Core read:** controlled demolition puzzle, not chaotic physics. The board should look like a stacked support lattice under pressure: stone, glass, metal braces, crystal energy seams, dust, and heavy blocks.

**Visual keywords:** brittle, structural, pressurized, tactical, readable, heavy, cracked, braced, dusty.

**Primary readability rule:** the player must understand support state before spectacle. Dust, shake, debris, and glow are secondary layers and should never hide weak points for longer than the design intentionally allows.

### Color Language

| State / Function | Main Color | Meaning |
|---|---:|---|
| Standard support | Cyan / blue | Stable, interactable support |
| Weak / cracked | Magenta / hot pink | Risk, damage, future collapse |
| Reinforced / protected | Gold | Strengthened, braced, valuable |
| Repair / survey | Green | Defensive clarity / restoration |
| Objective danger | Red + gold | Critical threat |
| Heavy / weight | Slate / charcoal | Load pressure |
| Dust / collapse | Tan / amber | Obscuration and impact |

### Shape Language

- **Support tiles:** chunky beveled squares with internal seams.
- **Reinforced supports:** same base silhouette with gold cross-bracing or metal bands.
- **Cracked supports:** visible dark fractures plus pink/cyan highlight traces.
- **Weighted blocks:** darker square chunks, labeled with weight values like `1T` or `2T`.
- **Objective nodes:** crown/anchor-core language; should feel sacred and fragile.
- **Collapse FX:** debris falls downward, dust expands outward, score chain arrows travel horizontally or along collapse path.

---

## 2. Production Asset Package

The generated package contains:

```text
collapse_grid_artwork_phaser_workup/
  collapse_grid_asset_manifest.json
  docs/collapse_grid_artwork_phaser_workup.md
  phaser/collapseGridAnimations.js
  spritesheets/
    collapse_grid_support_atlas.png
    collapse_grid_support_idle.png
    collapse_grid_weak_spot_pulse.png
    collapse_grid_crack_spread.png
    collapse_grid_remove_tile.png
    collapse_grid_shake_structure.png
    collapse_grid_debris_fall.png
    collapse_grid_dust_plume.png
    collapse_grid_controlled_chain.png
    collapse_grid_self_damage_warning.png
    collapse_grid_score_lane.png
    collapse_grid_player_card_shimmer.png
    collapse_grid_endgame_pressure.png
    collapse_grid_crack_tile.png
    collapse_grid_weight_drop.png
    collapse_grid_aftershock.png
    collapse_grid_dust_cloud.png
    collapse_grid_reinforce.png
    collapse_grid_brace.png
    collapse_grid_survey.png
    collapse_grid_patch.png
```

All animation sheets use **uniform frame cells**, transparent backgrounds, and predictable frame ordering.

---

## 3. Uniform Sprite Sheet Specification

| Asset | Frame Size | Layout | Frames | FPS | Loop | Usage |
|---|---:|---:|---:|---:|---|---|
| `collapse_grid_support_idle.png` | 96×96 | 4×2 | 8 | 8 | Yes | Subtle support tension shimmer |
| `collapse_grid_weak_spot_pulse.png` | 128×128 | 4×2 | 8 | 10 | Yes | Unstable support warning |
| `collapse_grid_crack_spread.png` | 128×128 | 4×2 | 8 | 12 | No | Crack propagation |
| `collapse_grid_remove_tile.png` | 128×128 | 3×2 | 6 | 14 | No | Support removal feedback |
| `collapse_grid_shake_structure.png` | 640×640 | 4×2 | 8 | 10 | Short loop | Board instability overlay |
| `collapse_grid_debris_fall.png` | 160×160 | 5×2 | 10 | 16 | No | Falling chunks |
| `collapse_grid_dust_plume.png` | 256×256 | 4×2 | 8 | 12 | No | Collapse dust cloud |
| `collapse_grid_controlled_chain.png` | 320×160 | 5×2 | 10 | 14 | No | Scoring flourish |
| `collapse_grid_self_damage_warning.png` | 256×128 | 4×2 | 8 | 10 | Yes | Own-board danger alert |
| `collapse_grid_score_lane.png` | 128×512 | 2×4 | 8 | 10 | Yes | Stability/danger lane pulse |
| `collapse_grid_player_card_shimmer.png` | 512×160 | 3×2 | 6 | 8 | Yes | Player card shimmer |
| `collapse_grid_endgame_pressure.png` | 1024×256 | 3×2 | 6 | 10 | Yes | Final phase framing |
| `collapse_grid_crack_tile.png` | 128×128 | 4×2 | 8 | 12 | No | Attack: crack enemy tile |
| `collapse_grid_weight_drop.png` | 160×160 | 4×2 | 8 | 14 | No | Attack: weight drop |
| `collapse_grid_aftershock.png` | 640×640 | 4×2 | 8 | 10 | No | Attack: board-wide tremor |
| `collapse_grid_dust_cloud.png` | 640×640 | 3×2 | 6 | 10 | Yes | Attack: temporary obscuration |
| `collapse_grid_reinforce.png` | 128×128 | 3×2 | 6 | 12 | No | Defense: strengthen support |
| `collapse_grid_brace.png` | 256×256 | 4×2 | 8 | 10 | Yes | Defense: temporary brace field |
| `collapse_grid_survey.png` | 640×640 | 3×2 | 6 | 10 | No | Defense: weak-point scan |
| `collapse_grid_patch.png` | 128×128 | 3×2 | 6 | 12 | No | Defense: repair cracked tile |

---

## 4. Static Tile Atlas

`collapse_grid_support_atlas.png`

- Frame size: **64×64**
- Layout: **8×3**
- Frames: **24**
- Use for deterministic board state rendering.
- Keep animations as overlay sprites so tile state remains stable and readable.

### Atlas Frame Map

| Index | Key |
|---:|---|
| 0 | `support_cyan` |
| 1 | `support_magenta` |
| 2 | `support_gold` |
| 3 | `support_green` |
| 4 | `support_stone` |
| 5 | `support_cracked_cyan` |
| 6 | `support_cracked_magenta` |
| 7 | `support_reinforced` |
| 8 | `objective_core` |
| 9 | `objective_core_damaged` |
| 10 | `lock_blue` |
| 11 | `lock_red` |
| 12 | `weight_1t` |
| 13 | `weight_2t` |
| 14 | `anchor_gold` |
| 15 | `blocked_cell` |
| 16 | `debris_blue` |
| 17 | `debris_pink` |
| 18 | `debris_gold` |
| 19 | `dust_patch` |
| 20 | `brace_marker` |
| 21 | `danger_marker` |
| 22 | `survey_marker` |
| 23 | `empty_shadow` |

---

## 5. Phaser Animation Hooks

Use these game-event hooks:

| Game Event | Tile/State Action | Animation |
|---|---|---|
| `tile_idle` | Stable tile waits under load | `collapse_grid_support_idle` |
| `tile_becomes_weak` | Support enters unsafe state | `collapse_grid_weak_spot_pulse` |
| `tile_cracked` | Crack applied or spread | `collapse_grid_crack_spread` |
| `tile_removed` | Player removes support | `collapse_grid_remove_tile` |
| `instability_threshold` | Board crosses warning threshold | `collapse_grid_shake_structure` |
| `segment_falls` | Unsupported section collapses | `collapse_grid_debris_fall` + `collapse_grid_dust_plume` |
| `controlled_chain_scored` | Skillful collapse detected | `collapse_grid_controlled_chain` |
| `objective_at_risk` | Protected objective threatened | `collapse_grid_self_damage_warning` |
| `endgame_20s` | Final pressure phase starts | `collapse_grid_endgame_pressure` |
| `attack_crack_tile` | Enemy crack attack lands | `collapse_grid_crack_tile` |
| `attack_weight_drop` | Enemy weight lands | `collapse_grid_weight_drop` |
| `attack_aftershock` | Enemy tremor attack | `collapse_grid_aftershock` |
| `attack_dust_cloud` | Enemy obscuration | `collapse_grid_dust_cloud` |
| `defense_reinforce` | Player strengthens support | `collapse_grid_reinforce` |
| `defense_brace` | Temporary collapse prevention | `collapse_grid_brace` |
| `defense_survey` | Weak areas highlighted | `collapse_grid_survey` |
| `defense_patch` | Cracked support repaired | `collapse_grid_patch` |

---

## 6. Layering Order

Recommended Phaser depth order:

| Depth | Layer |
|---:|---|
| 0 | Board backing / frame |
| 10 | Empty cells / shadows |
| 20 | Static supports from `collapse_grid_support_atlas` |
| 30 | Heavy blocks / objective nodes |
| 40 | State overlays: cracks, reinforcement, brace markers |
| 50 | Tile interaction FX |
| 60 | Debris fall |
| 70 | Dust plume / dust cloud |
| 80 | Survey scan / aftershock |
| 90 | Score and chain flourish |
| 100 | Warning / endgame pressure |
| 200 | HUD / player cards |

---

## 7. Phaser Usage

### Basic preload/create

```js
import {
  preloadCollapseGridAssets,
  createCollapseGridAnimations,
  playCollapseFx,
  CollapseGridTileFrames
} from "./collapseGridAnimations.js";

export default class CollapseGridScene extends Phaser.Scene {
  preload() {
    preloadCollapseGridAssets(this, "assets/collapse_grid/");
  }

  create() {
    createCollapseGridAnimations(this);

    const tile = this.add.sprite(160, 240, "collapse_grid_support_atlas", CollapseGridTileFrames.support_cyan);
    tile.setDisplaySize(64, 64);

    playCollapseFx(this, "collapse_grid_support_idle", tile.x, tile.y, {
      depth: 50,
      destroyOnComplete: false
    });
  }
}
```

### Suggested helpers

- Pool `dust_plume`, `debris_fall`, `aftershock`, and `shake_structure`.
- Use camera shake for feel, but keep it low: `this.cameras.main.shake(120, 0.004)`.
- Use the deterministic support graph for actual collapse outcomes. Animations should only visualize state changes.

---

## 8. Implementation Notes

### Tile states

Each board cell should render one static atlas frame plus zero or more overlays:

```ts
type CollapseGridCell = {
  x: number;
  y: number;
  kind: "empty" | "support" | "weight" | "objective" | "blocked";
  supportState?: "stable" | "weak" | "cracked" | "reinforced";
  load?: 0 | 1 | 2 | 3;
  isObjectiveAnchor?: boolean;
  isBraced?: boolean;
  isDustObscured?: boolean;
};
```

### Readability rules

- Weak/cracked state should be visible even without motion.
- Reinforcement must not look like scoring; it is defensive, not reward.
- Dust Cloud may obscure state intentionally, but should never make the entire board unreadable.
- Board shake should move overlay FX more than the actual board tiles to preserve touch targeting.

### Mobile rules

- Use 64×64 board tiles for an 8×10 board.
- Use 96/128 FX sheets at normal gameplay scale.
- Scale large overlay sheets to board bounds, not screen bounds.
- Use additive blending only for glows and pulse rings.

---

## 9. Export Guidelines for Final Artist Pass

These generated sheets are production workup / implementation-ready placeholders. For a final artist pass:

1. Keep the **same filenames, frame counts, frame sizes, and layouts**.
2. Paint over frames while preserving transparent backgrounds.
3. Do not add nonuniform padding.
4. Keep important motion centered on the same origin.
5. Avoid text inside gameplay sprites except debug/placeholder labels; replace with icons for localization.
6. Use the JSON manifest as the contract between art and Phaser.

---

## 10. QA Checklist

- [ ] Every PNG imports as a uniform spritesheet in Phaser.
- [ ] No visible black background.
- [ ] Frame order is left-to-right, top-to-bottom.
- [ ] Looping animations connect smoothly.
- [ ] Non-looping animations end cleanly.
- [ ] Weak/cracked/reinforced states are visible at 0.75× mobile scale.
- [ ] Dust Cloud obscures without fully hiding the whole board.
- [ ] Large overlay sheets are pooled and reused.
- [ ] Controlled-chain reward is visually distinct from self-damage warning.
- [ ] Objective nodes remain readable during debris and dust.
