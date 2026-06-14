import Phaser from 'phaser';
import {
  preloadMirrorMazeSpriteSheets,
  registerMirrorMazeAnimations,
} from '../animations/generated/mirror-maze.animations';

const CELL = 96;
const GRID_ORIGIN = { x: 168, y: 156 };
const COLS = 6;
const ROWS = 5;
const BOARD_WIDTH = CELL * COLS;
const BOARD_HEIGHT = CELL * ROWS;

type Direction = 0 | 1 | 2 | 3;

type CellPos = {
  col: number;
  row: number;
};

type MirrorPiece = {
  id: string;
  col: number;
  row: number;
  orientation: 0 | 1 | 2 | 3;
  sprite: Phaser.GameObjects.Sprite;
  home: CellPos;
};

type BeamSegment = {
  x: number;
  y: number;
  width: number;
  angle: number;
};

type BeamFx = {
  kind: 'split' | 'impact';
  col: number;
  row: number;
};

type TraceResult = {
  segments: BeamSegment[];
  targetHit: boolean;
  activatorHit: boolean;
  fx: BeamFx[];
};

const DIRS: Record<Direction, { dc: number; dr: number }> = {
  0: { dc: 0, dr: -1 },
  1: { dc: 1, dr: 0 },
  2: { dc: 0, dr: 1 },
  3: { dc: -1, dr: 0 },
};

function keyForCell(col: number, row: number) {
  return `${col},${row}`;
}

function centerOf(col: number, row: number) {
  return {
    x: GRID_ORIGIN.x + col * CELL,
    y: GRID_ORIGIN.y + row * CELL,
  };
}

function reflect(direction: Direction, orientation: number): Direction {
  const slash = orientation % 2 === 0;
  if (slash) {
    return ({ 0: 1, 1: 0, 2: 3, 3: 2 } as const)[direction];
  }
  return ({ 0: 3, 3: 0, 2: 1, 1: 2 } as const)[direction];
}

export class MirrorMazePreviewScene extends Phaser.Scene {
  private mirrors: MirrorPiece[] = [];
  private sockets = new Set<string>([
    keyForCell(1, 1),
    keyForCell(2, 1),
    keyForCell(3, 1),
    keyForCell(1, 2),
    keyForCell(2, 2),
    keyForCell(4, 1),
    keyForCell(4, 2),
  ]);
  private primarySource = { col: 0, row: 2, dir: 1 as Direction };
  private secondarySource = { col: 5, row: 1, dir: 3 as Direction };
  private target = { col: 1, row: 3 };
  private prism = { col: 4, row: 3 };
  private activator = { col: 2, row: 4 };
  private blocker = { col: 4, row: 4 };

  private beamSprites: Phaser.GameObjects.Sprite[] = [];
  private fxSprites: Phaser.GameObjects.Sprite[] = [];
  private socketSprites: Phaser.GameObjects.Sprite[] = [];
  private targetSprite!: Phaser.GameObjects.Sprite;
  private activatorSprite!: Phaser.GameObjects.Sprite;
  private secondarySourceSprite!: Phaser.GameObjects.Sprite;
  private statusText!: Phaser.GameObjects.Text;
  private hintText!: Phaser.GameObjects.Text;
  private objectiveText!: Phaser.GameObjects.Text;
  private dragState = new Map<string, { startX: number; startY: number; moved: boolean; handledTap: boolean }>();
  private targetHalo?: Phaser.GameObjects.Arc;

  constructor() {
    super('MirrorMazePreviewScene');
  }

  preload() {
    preloadMirrorMazeSpriteSheets(this, 'assets');
  }

  create() {
    registerMirrorMazeAnimations(this);
    this.cameras.main.setBackgroundColor('#04070b');

    this.drawBoardShell();
    this.drawTilesAndSockets();
    this.addHud();
    this.createStaticPieces();
    this.createMirrors();
    this.bindInput();
    this.retraceBeams();
    this.scale.on('resize', this.layout, this);
    this.layout();
  }

  private drawBoardShell() {
    const graphics = this.add.graphics();
    graphics.fillStyle(0x07111b, 1);
    graphics.fillRoundedRect(104, 116, BOARD_WIDTH + 128, BOARD_HEIGHT + 84, 24);
    graphics.lineStyle(2, 0x163042, 1);
    graphics.strokeRoundedRect(104, 116, BOARD_WIDTH + 128, BOARD_HEIGHT + 84, 24);

    graphics.fillStyle(0x06090f, 1);
    graphics.fillRoundedRect(840, 116, 332, BOARD_HEIGHT + 84, 24);
    graphics.lineStyle(2, 0x1d3142, 0.9);
    graphics.strokeRoundedRect(840, 116, 332, BOARD_HEIGHT + 84, 24);
  }

  private drawTilesAndSockets() {
    for (let row = 0; row < ROWS; row += 1) {
      for (let col = 0; col < COLS; col += 1) {
        const { x, y } = centerOf(col, row);
        this.add.sprite(x, y, 'board_board_tile_sheet', (row + col) % 4)
          .setDisplaySize(88, 88)
          .setAlpha(0.95);
      }
    }

    for (const socket of this.sockets) {
      const [col, row] = socket.split(',').map(Number);
      const { x, y } = centerOf(col, row);
      const sprite = this.add.sprite(x, y, 'board_beam_socket_sheet', (col + row) % 4)
        .setDisplaySize(92, 92)
        .setAlpha(0.8);
      this.socketSprites.push(sprite);
    }
  }

  private addHud() {
    this.add.text(150, 66, 'Mirror Maze Prototype', {
      fontFamily: 'Segoe UI',
      fontSize: '28px',
      color: '#eff8ff',
    }).setAlpha(0.96);

    this.add.text(150, 98, 'Drag mirrors onto sockets. Tap a mirror to rotate it. Light the target.', {
      fontFamily: 'Segoe UI',
      fontSize: '14px',
      color: '#8cadc3',
    }).setAlpha(0.98);

    this.add.text(868, 148, 'Prototype Notes', {
      fontFamily: 'Segoe UI',
      fontSize: '24px',
      color: '#f1f8fd',
    });

    this.objectiveText = this.add.text(868, 194, 'Objective: route the primary beam into the activator, then use the secondary beam to hit the target through the prism.', {
      fontFamily: 'Segoe UI',
      fontSize: '14px',
      color: '#93b4c8',
      wordWrap: { width: 276 },
      lineSpacing: 6,
    });

    this.hintText = this.add.text(868, 322, 'Hints:\n- Mirrors can sit on any glowing socket.\n- Slash and backslash mirror logic alternate as you rotate.\n- The prism splits into every direction except the one it came from.', {
      fontFamily: 'Segoe UI',
      fontSize: '13px',
      color: '#7ca2bb',
      lineSpacing: 6,
      wordWrap: { width: 276 },
    });

    this.statusText = this.add.text(868, 510, 'Routing beam…', {
      fontFamily: 'Segoe UI',
      fontSize: '20px',
      color: '#cbf0ff',
      wordWrap: { width: 276 },
      lineSpacing: 6,
    });
  }

  private createStaticPieces() {
    const primary = centerOf(this.primarySource.col, this.primarySource.row);
    const primarySource = this.add.sprite(primary.x, primary.y, 'source_source_emitter', 0)
      .setDisplaySize(92, 92);
    primarySource.play('source_source_emitter_loop');

    const secondary = centerOf(this.secondarySource.col, this.secondarySource.row);
    this.secondarySourceSprite = this.add.sprite(secondary.x, secondary.y, 'source_source_emitter', 0)
      .setDisplaySize(92, 92)
      .setAlpha(0.45);

    const targetPos = centerOf(this.target.col, this.target.row);
    this.targetHalo = this.add.circle(targetPos.x, targetPos.y, 46, 0x55d5ff, 0.08);
    this.targetHalo.setStrokeStyle(2, 0x55d5ff, 0.18);
    this.targetSprite = this.add.sprite(targetPos.x, targetPos.y, 'target_target_receiver', 0)
      .setDisplaySize(92, 92)
      .setAlpha(0.82);

    const prismPos = centerOf(this.prism.col, this.prism.row);
    this.add.sprite(prismPos.x, prismPos.y, 'routing_prism_sheet', 0)
      .setDisplaySize(92, 92)
      .setAlpha(0.98);

    const activatorPos = centerOf(this.activator.col, this.activator.row);
    this.activatorSprite = this.add.sprite(activatorPos.x, activatorPos.y, 'trigger_activator_v04', 0)
      .setDisplaySize(92, 92)
      .setAlpha(0.7);

    const blockerPos = centerOf(this.blocker.col, this.blocker.row);
    this.add.sprite(blockerPos.x, blockerPos.y, 'routing_blocker_sheet', 0)
      .setDisplaySize(92, 92)
      .setAlpha(0.96);
  }

  private createMirrors() {
    const specs = [
      { id: 'mirror-a', col: 2, row: 2, orientation: 1 as 0 | 1 | 2 | 3 },
      { id: 'mirror-b', col: 4, row: 1, orientation: 0 as 0 | 1 | 2 | 3 },
    ];

    this.mirrors = specs.map((spec) => {
      const { x, y } = centerOf(spec.col, spec.row);
      const sprite = this.add.sprite(x, y, 'routing_angled_mirror_sheet_v02', spec.orientation)
        .setDisplaySize(92, 92)
        .setInteractive({ cursor: 'pointer' });
      this.input.setDraggable(sprite);
      return {
        ...spec,
        sprite,
        home: { col: spec.col, row: spec.row },
      };
    });

    this.mirrors.forEach((mirror) => {
      mirror.sprite.on('pointerup', (pointer: Phaser.Input.Pointer) => {
        const state = this.dragState.get(mirror.id);
        if (!state || state.moved || pointer.getDistance() > 8) {
          return;
        }
        state.handledTap = true;
        mirror.orientation = ((mirror.orientation + 1) % 4) as 0 | 1 | 2 | 3;
        mirror.sprite.setFrame(mirror.orientation);
        mirror.sprite.setPosition(state.startX, state.startY);
        this.retraceBeams();
      });
    });
  }

  private bindInput() {
    this.input.on('dragstart', (_pointer: Phaser.Input.Pointer, gameObject: Phaser.GameObjects.GameObject) => {
      const mirror = this.findMirrorBySprite(gameObject);
      if (!mirror) {
        return;
      }
      this.dragState.set(mirror.id, {
        startX: mirror.sprite.x,
        startY: mirror.sprite.y,
        moved: false,
        handledTap: false,
      });
      mirror.sprite.setScale(1.06);
      mirror.sprite.setDepth(20);
    });

    this.input.on('drag', (_pointer: Phaser.Input.Pointer, gameObject: Phaser.GameObjects.GameObject, dragX: number, dragY: number) => {
      const mirror = this.findMirrorBySprite(gameObject);
      if (!mirror) {
        return;
      }
      const state = this.dragState.get(mirror.id);
      if (state) {
        state.moved = true;
      }
      mirror.sprite.x = dragX;
      mirror.sprite.y = dragY;
    });

    this.input.on('dragend', (_pointer: Phaser.Input.Pointer, gameObject: Phaser.GameObjects.GameObject) => {
      const mirror = this.findMirrorBySprite(gameObject);
      if (!mirror) {
        return;
      }
      const state = this.dragState.get(mirror.id);
      mirror.sprite.setScale(1);
      mirror.sprite.setDepth(0);

      if (state?.handledTap) {
        mirror.sprite.setPosition(state.startX, state.startY);
      } else {
        this.snapMirrorToSocket(mirror, state);
      }

      this.dragState.delete(mirror.id);
      this.retraceBeams();
    });
  }

  private snapMirrorToSocket(mirror: MirrorPiece, state?: { startX: number; startY: number; moved: boolean; handledTap: boolean }) {
    let chosen: CellPos | null = null;
    let bestDistance = Number.POSITIVE_INFINITY;

    for (const socket of this.sockets) {
      const [col, row] = socket.split(',').map(Number);
      if (this.isMirrorOccupying(col, row, mirror.id)) {
        continue;
      }
      const { x, y } = centerOf(col, row);
      const distance = Phaser.Math.Distance.Between(mirror.sprite.x, mirror.sprite.y, x, y);
      if (distance < bestDistance) {
        bestDistance = distance;
        chosen = { col, row };
      }
    }

    if (!chosen || bestDistance > 64) {
      chosen = { col: mirror.col, row: mirror.row };
      if (state) {
        const fallback = this.closestSocketForPosition(state.startX, state.startY);
        if (fallback) {
          chosen = fallback;
        }
      }
    }

    mirror.col = chosen.col;
    mirror.row = chosen.row;
    const snap = centerOf(chosen.col, chosen.row);
    mirror.sprite.setPosition(snap.x, snap.y);
  }

  private closestSocketForPosition(x: number, y: number): CellPos | null {
    let winner: CellPos | null = null;
    let best = Number.POSITIVE_INFINITY;
    for (const socket of this.sockets) {
      const [col, row] = socket.split(',').map(Number);
      const center = centerOf(col, row);
      const dist = Phaser.Math.Distance.Between(x, y, center.x, center.y);
      if (dist < best) {
        best = dist;
        winner = { col, row };
      }
    }
    return winner;
  }

  private findMirrorBySprite(gameObject: Phaser.GameObjects.GameObject) {
    return this.mirrors.find((mirror) => mirror.sprite === gameObject);
  }

  private isMirrorOccupying(col: number, row: number, ignoreId?: string) {
    return this.mirrors.some((mirror) => mirror.id !== ignoreId && mirror.col === col && mirror.row === row);
  }

  private pieceAt(col: number, row: number) {
    const mirror = this.mirrors.find((entry) => entry.col === col && entry.row === row);
    if (mirror) {
      return { kind: 'mirror' as const, mirror };
    }
    if (col === this.target.col && row === this.target.row) {
      return { kind: 'target' as const };
    }
    if (col === this.prism.col && row === this.prism.row) {
      return { kind: 'prism' as const };
    }
    if (col === this.activator.col && row === this.activator.row) {
      return { kind: 'activator' as const };
    }
    if (col === this.blocker.col && row === this.blocker.row) {
      return { kind: 'blocker' as const };
    }
    return null;
  }

  private retraceBeams() {
    this.beamSprites.forEach((sprite) => sprite.destroy());
    this.beamSprites = [];
    this.fxSprites.forEach((sprite) => sprite.destroy());
    this.fxSprites = [];

    const primaryTrace = this.traceFromSource(this.primarySource.col, this.primarySource.row, this.primarySource.dir);
    const traces = [primaryTrace];
    if (primaryTrace.activatorHit) {
      traces.push(this.traceFromSource(this.secondarySource.col, this.secondarySource.row, this.secondarySource.dir));
      this.secondarySourceSprite.setAlpha(1);
      this.secondarySourceSprite.play('source_source_emitter_loop');
      this.activatorSprite.setAlpha(1);
      this.activatorSprite.play('trigger_activator_v04_loop');
    } else {
      this.secondarySourceSprite.setAlpha(0.42);
      this.secondarySourceSprite.stop();
      this.secondarySourceSprite.setFrame(0);
      this.activatorSprite.setAlpha(0.68);
      this.activatorSprite.stop();
      this.activatorSprite.setFrame(0);
    }

    const allSegments = traces.flatMap((trace) => trace.segments);
    const allFx = traces.flatMap((trace) => trace.fx);
    const targetHit = traces.some((trace) => trace.targetHit);

    allSegments.forEach((segment) => {
      const sprite = this.add.sprite(segment.x, segment.y, 'beam_traveling_beam_v04', 0)
        .setDisplaySize(Math.max(22, segment.width), 18)
        .setAngle(segment.angle)
        .setDepth(1);
      sprite.play('beam_traveling_beam_v04_loop');
      this.beamSprites.push(sprite);
    });

    allFx.forEach((fx) => {
      const { x, y } = centerOf(fx.col, fx.row);
      const key = fx.kind === 'split' ? 'beam_prism_split_overlay_v04' : 'beam_blocked_impact_v04';
      const anim = fx.kind === 'split' ? 'beam_prism_split_overlay_v04_loop' : 'beam_blocked_impact_v04_loop';
      const sprite = this.add.sprite(x, y, key, 0)
        .setDisplaySize(fx.kind === 'split' ? 102 : 68, fx.kind === 'split' ? 102 : 68)
        .setDepth(3);
      sprite.play(anim);
      this.fxSprites.push(sprite);
    });

    if (targetHit) {
      this.targetSprite.play('target_target_receiver_loop');
      this.targetSprite.setAlpha(1);
      this.targetHalo?.setFillStyle(0x95ffbd, 0.18);
      this.targetHalo?.setStrokeStyle(2, 0x95ffbd, 0.55);
      this.statusText.setText('Target lit. Secondary beam route is complete.');
      this.statusText.setColor('#b8ffd6');
    } else {
      this.targetSprite.stop();
      this.targetSprite.setFrame(0);
      this.targetSprite.setAlpha(0.84);
      this.targetHalo?.setFillStyle(0x55d5ff, 0.08);
      this.targetHalo?.setStrokeStyle(2, 0x55d5ff, 0.18);
      this.statusText.setText(primaryTrace.activatorHit
        ? 'Activator engaged. Route the secondary beam through the prism and into the target.'
        : 'Primary beam is live. Use the first mirror to feed the activator.');
      this.statusText.setColor('#cbf0ff');
    }
  }

  private traceFromSource(col: number, row: number, dir: Direction): TraceResult {
    const segments: BeamSegment[] = [];
    const fx: BeamFx[] = [];
    const visited = new Set<string>();
    let targetHit = false;
    let activatorHit = false;

    const walk = (startCol: number, startRow: number, direction: Direction) => {
      let currentCol = startCol;
      let currentRow = startRow;
      let currentDir = direction;
      let previousPoint = centerOf(startCol, startRow);

      for (let step = 0; step < 18; step += 1) {
        const delta = DIRS[currentDir];
        currentCol += delta.dc;
        currentRow += delta.dr;

        if (currentCol < 0 || currentRow < 0 || currentCol >= COLS || currentRow >= ROWS) {
          const edgePoint = {
            x: previousPoint.x + delta.dc * CELL * 0.5,
            y: previousPoint.y + delta.dr * CELL * 0.5,
          };
          this.pushSegment(segments, previousPoint, edgePoint, currentDir);
          return;
        }

        const stateKey = `${currentCol},${currentRow},${currentDir}`;
        if (visited.has(stateKey)) {
          return;
        }
        visited.add(stateKey);

        const point = centerOf(currentCol, currentRow);
        this.pushSegment(segments, previousPoint, point, currentDir);
        previousPoint = point;

        const piece = this.pieceAt(currentCol, currentRow);
        if (!piece) {
          continue;
        }

        if (piece.kind === 'mirror') {
          currentDir = reflect(currentDir, piece.mirror.orientation);
          continue;
        }

        if (piece.kind === 'activator') {
          activatorHit = true;
          continue;
        }

        if (piece.kind === 'target') {
          targetHit = true;
          return;
        }

        if (piece.kind === 'blocker') {
          fx.push({ kind: 'impact', col: currentCol, row: currentRow });
          return;
        }

        if (piece.kind === 'prism') {
          fx.push({ kind: 'split', col: currentCol, row: currentRow });
          const blockedDir = ((currentDir + 2) % 4) as Direction;
          const splitDirs = ([0, 1, 2, 3] as Direction[]).filter((entry) => entry !== blockedDir);
          splitDirs.forEach((splitDir) => walk(currentCol, currentRow, splitDir));
          return;
        }
      }
    };

    walk(col, row, dir);
    return { segments, targetHit, activatorHit, fx };
  }

  private pushSegment(segments: BeamSegment[], from: { x: number; y: number }, to: { x: number; y: number }, direction: Direction) {
    const width = Math.max(18, Phaser.Math.Distance.Between(from.x, from.y, to.x, to.y));
    if (width <= 2) {
      return;
    }
    segments.push({
      x: (from.x + to.x) * 0.5,
      y: (from.y + to.y) * 0.5,
      width,
      angle: direction === 0 || direction === 2 ? 90 : 0,
    });
  }

  private layout(gameSize?: Phaser.Structs.Size) {
    const width = gameSize?.width ?? this.scale.width;
    const height = gameSize?.height ?? this.scale.height;
    this.cameras.main.setViewport(0, 0, width, height);
  }
}
