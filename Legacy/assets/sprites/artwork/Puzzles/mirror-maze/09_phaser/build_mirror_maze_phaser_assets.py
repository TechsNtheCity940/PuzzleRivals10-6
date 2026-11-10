from __future__ import annotations

import json
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from PIL import Image


ROOT = Path("C:/TechProjects/PuzzleRivals/public/Puzzles/mirror-maze")
ENGINE_READY = ROOT / "06_exports" / "engine_ready"
SHEETS_ROOT = ROOT / "05_sheets"
MANIFEST_ROOT = ROOT / "07_manifests"
PHASER_ROOT = ROOT / "09_phaser"


@dataclass
class AssetSpec:
    filename: str
    asset_slug: str
    family: str
    animated: bool
    frame_count: int
    frame_size: tuple[int, int]
    fps: int | None
    loop: bool | None
    notes: str


def slug_to_asset_key(filename: str) -> str:
    stem = Path(filename).stem
    if stem.startswith("mirror_maze_main_puzzle_"):
        stem = stem[len("mirror_maze_main_puzzle_") :]
    parts = stem.split("_")
    while parts and parts[-1].startswith("v") and parts[-1][1:].isdigit():
        parts.pop()
    if parts and parts[-1].endswith("f") and parts[-1][:-1].isdigit():
        parts.pop()
    if parts and parts[-1] in {"idle", "loop", "flow", "pulse", "charge"}:
        parts.pop()
    return "_".join(parts)


def version_suffix(filename: str) -> str | None:
    stem_parts = Path(filename).stem.split("_")
    for part in reversed(stem_parts):
        if part.startswith("v") and part[1:].isdigit():
            return part
    return None


def load_specs() -> list[AssetSpec]:
    assets = json.loads((MANIFEST_ROOT / "asset_manifest.json").read_text())
    anims = {
        row["asset_slug"]: row
        for row in json.loads((MANIFEST_ROOT / "animation_manifest.json").read_text())
    }

    specs: list[AssetSpec] = []
    for row in assets:
        filename = row["filename"]
        if not (ENGINE_READY / filename).exists():
            continue
        anim = anims.get(row["asset_slug"])
        target_w, target_h = [int(v) for v in row["target_resolution"].split("x")]
        frame_count = max(1, int(row["frame_count"]))
        frame_width = max(1, target_w // frame_count)
        frame_height = min(target_h, frame_width)
        specs.append(
            AssetSpec(
                filename=filename,
                asset_slug=row["asset_slug"],
                family=row["family"],
                animated=bool(row["animated"]),
                frame_count=frame_count,
                frame_size=(frame_width, frame_height),
                fps=anim["fps"] if anim else None,
                loop=anim["loop"] if anim else None,
                notes=row["notes"],
            )
        )
    return specs


def crop_alpha_bounds(image: Image.Image) -> tuple[int, int, int, int] | None:
    alpha = image.getchannel("A")
    bbox = alpha.getbbox()
    return bbox


def fit_to_frame(source: Image.Image, frame_size: tuple[int, int], padding: int = 12) -> Image.Image:
    fw, fh = frame_size
    out = Image.new("RGBA", frame_size, (0, 0, 0, 0))
    bbox = crop_alpha_bounds(source)
    if not bbox:
        return out
    trimmed = source.crop(bbox)
    max_w = max(1, fw - padding * 2)
    max_h = max(1, fh - padding * 2)
    scale = min(max_w / trimmed.width, max_h / trimmed.height, 1.0)
    resized = trimmed.resize(
        (max(1, round(trimmed.width * scale)), max(1, round(trimmed.height * scale))),
        Image.Resampling.LANCZOS,
    )
    x = (fw - resized.width) // 2
    y = (fh - resized.height) // 2
    out.alpha_composite(resized, (x, y))
    return out


def derive_frames(image: Image.Image, frame_count: int) -> list[Image.Image]:
    width, height = image.size
    frames: list[Image.Image] = []
    slice_width = width / frame_count
    for index in range(frame_count):
        left = round(index * slice_width)
        right = round((index + 1) * slice_width)
        frames.append(image.crop((left, 0, right, height)))
    return frames


def family_subdir(family: str) -> str:
    if family == "ui_icons":
        return "ui"
    if family in {"beam_fx", "trigger_system"}:
        return "fx" if family == "beam_fx" else "animation"
    if family == "board_environment":
        return "static"
    return "animation" if family in {"source_target"} else "static"


def build_outputs(specs: list[AssetSpec]) -> dict[str, Any]:
    analyzed_assets: list[dict[str, Any]] = []
    warnings_by_asset: dict[str, list[str]] = {}
    review_assets: list[str] = []
    key_counts: dict[str, int] = {}

    assets_copy_root = PHASER_ROOT / "assets"
    assets_copy_root.mkdir(parents=True, exist_ok=True)

    for spec in specs:
        source_path = ENGINE_READY / spec.filename
        image = Image.open(source_path).convert("RGBA")
        raw_frames = derive_frames(image, spec.frame_count)
        normalized_frames = [fit_to_frame(frame, spec.frame_size) for frame in raw_frames]

        out_dir = SHEETS_ROOT / family_subdir(spec.family)
        out_dir.mkdir(parents=True, exist_ok=True)
        sheet_w = spec.frame_size[0] * spec.frame_count
        sheet_h = spec.frame_size[1]
        sheet = Image.new("RGBA", (sheet_w, sheet_h), (0, 0, 0, 0))
        frame_rects = []
        confidence = "medium"
        warnings = []

        for index, frame in enumerate(normalized_frames):
            x = index * spec.frame_size[0]
            sheet.alpha_composite(frame, (x, 0))
            bbox = crop_alpha_bounds(frame)
            if bbox:
                frame_rects.append(
                    {
                        "index": index,
                        "frame": {"x": x, "y": 0, "w": spec.frame_size[0], "h": spec.frame_size[1]},
                        "content_bounds": {"x": bbox[0], "y": bbox[1], "w": bbox[2] - bbox[0], "h": bbox[3] - bbox[1]},
                    }
                )
            else:
                warnings.append(f"frame {index} is fully transparent after normalization")

        out_name = spec.filename.replace(".png", "_v03_sheet.png")
        out_path = out_dir / out_name
        sheet.save(out_path)
        phaser_asset_path = assets_copy_root / out_name
        sheet.save(phaser_asset_path)

        if spec.animated and spec.frame_count <= 1:
            warnings.append("animated asset resolved to a single frame")
            confidence = "low"
        base_key = slug_to_asset_key(spec.filename)
        key_counts[base_key] = key_counts.get(base_key, 0) + 1
        asset_key = base_key
        if key_counts[base_key] > 1:
            asset_key = f"{base_key}_{version_suffix(spec.filename) or key_counts[base_key]}"

        if spec.family in {"routing_pieces", "board_environment", "ui_icons"}:
            animation_groups: list[dict[str, Any]] = []
        else:
            state = "loop" if spec.loop else "oneshot"
            animation_groups = [
                {
                    "key": f"{asset_key}_{state}",
                    "start": 0,
                    "end": spec.frame_count - 1,
                    "frameRate": spec.fps or 10,
                    "repeat": -1 if spec.loop else 0,
                }
            ]
        sheetmap = {
            "source_image_path": str(source_path),
            "normalized_sheet_path": str(out_path),
            "puzzle_slug": "mirror-maze",
            "asset_key": asset_key,
            "base_asset_key": base_key,
            "asset_slug": spec.asset_slug,
            "sheet_kind": "animation_strip" if spec.animated else "sprite_sheet",
            "frame_width": spec.frame_size[0],
            "frame_height": spec.frame_size[1],
            "frame_count": spec.frame_count,
            "animation_groups": animation_groups,
            "frame_rects": frame_rects,
            "warnings": warnings,
            "confidence_notes": [
                {
                    "level": confidence,
                    "note": "Frame slicing inferred from manifest frame count and normalized into fixed frame canvases."
                }
            ],
            "companion_metadata_sources_used": [
                str(MANIFEST_ROOT / "asset_manifest.json"),
                str(MANIFEST_ROOT / "animation_manifest.json"),
            ],
            "notes": spec.notes,
        }
        sheetmap_path = MANIFEST_ROOT / "phaser" / f"{asset_key}.sheetmap.json"
        sheetmap_path.write_text(json.dumps(sheetmap, indent=2))

        analyzed_assets.append(
            {
                "asset_key": sheetmap["asset_key"],
                "source_image_path": str(source_path),
                "normalized_sheet_path": str(out_path),
                "sheetmap_path": str(sheetmap_path),
                "frame_width": spec.frame_size[0],
                "frame_height": spec.frame_size[1],
                "frame_count": spec.frame_count,
                "animated": spec.animated,
            }
        )
        warnings_by_asset[sheetmap["asset_key"]] = warnings
        if warnings or confidence != "high":
            review_assets.append(sheetmap["asset_key"])

    generated_manifest = {
        "puzzle_slug": "mirror-maze",
        "analyzed_assets": analyzed_assets,
        "generated_helper_path": str(PHASER_ROOT / "src/game/animations/generated/mirror-maze.animations.ts"),
        "warnings_by_asset": warnings_by_asset,
        "assets_requiring_manual_review": sorted(set(review_assets)),
    }
    (MANIFEST_ROOT / "phaser" / "generated-animations.json").write_text(json.dumps(generated_manifest, indent=2))
    return generated_manifest


def build_report(generated_manifest: dict[str, Any]) -> None:
    lines = [
        "# Mirror Maze Sprite Sheet Animation Report",
        "",
        "## Folders Scanned",
        f"- source art: `{ENGINE_READY}`",
        f"- manifests: `{MANIFEST_ROOT}`",
        "",
        "## Metadata Sources Found",
        f"- `{MANIFEST_ROOT / 'asset_manifest.json'}`",
        f"- `{MANIFEST_ROOT / 'animation_manifest.json'}`",
        "",
        "## Successful Outputs",
    ]
    for asset in generated_manifest["analyzed_assets"]:
        lines.append(
            f"- `{asset['asset_key']}` -> `{Path(asset['normalized_sheet_path']).name}` "
            f"({asset['frame_width']}x{asset['frame_height']}, {asset['frame_count']} frames)"
        )
    lines.extend(
        [
            "",
            "## Warnings and Ambiguous Cases",
        ]
    )
    any_warning = False
    for asset_key, warnings in generated_manifest["warnings_by_asset"].items():
        if warnings:
            any_warning = True
            lines.append(f"- `{asset_key}`: {'; '.join(warnings)}")
    if not any_warning:
        lines.append("- none beyond inferred slicing from manifest-driven frame counts")
    lines.extend(
        [
            "",
            "## Recommended Manual Follow-Up",
            "- Review slice padding on elongated beam FX if they will be rendered at very small scale.",
            "- Review v01 versus v02 mirror sheet choice and keep only the stronger one in runtime builds.",
            "- If the final game uses per-tile board dimensions other than 256x256, scale the normalized sheets once and regenerate the Phaser helper metadata to match.",
        ]
    )
    (PHASER_ROOT / "reports" / "sprite-sheet-animation-report.md").write_text("\n".join(lines))


def build_ts_helper(generated_manifest: dict[str, Any]) -> None:
    assets = generated_manifest["analyzed_assets"]
    preload_lines = []
    register_lines = []
    data_lines = []

    for asset in assets:
        key = asset["asset_key"]
        file_name = Path(asset["normalized_sheet_path"]).name
        preload_lines.append(
            f"  scene.load.spritesheet('{key}', assetBaseUrl + '/{file_name}', {{ frameWidth: {asset['frame_width']}, frameHeight: {asset['frame_height']} }});"
        )
        sheetmap = json.loads((MANIFEST_ROOT / "phaser" / f"{key}.sheetmap.json").read_text())
        for group in sheetmap["animation_groups"]:
            register_lines.append(
                "  if (!scene.anims.exists('{key}')) {{\n"
                "    scene.anims.create({{\n"
                "      key: '{key}',\n"
                "      frames: scene.anims.generateFrameNumbers('{sheet}', {{ start: {start}, end: {end} }}),\n"
                "      frameRate: {fps},\n"
                "      repeat: {repeat},\n"
                "    }});\n"
                "  }}".format(
                    key=group["key"],
                    sheet=key,
                    start=group["start"],
                    end=group["end"],
                    fps=group["frameRate"],
                    repeat=group["repeat"],
                )
            )
        data_lines.append(
            f"  {{ key: '{key}', path: '{file_name}', frameWidth: {asset['frame_width']}, frameHeight: {asset['frame_height']}, frameCount: {asset['frame_count']}, animated: {'true' if asset['animated'] else 'false'} }},"
        )

    ts = f"""import Phaser from 'phaser';

export const mirrorMazeSpriteSheets = [
{chr(10).join(data_lines)}
] as const;

export function preloadMirrorMazeSpriteSheets(scene: Phaser.Scene, assetBaseUrl = 'assets') {{
{chr(10).join(preload_lines)}
}}

export function registerMirrorMazeAnimations(scene: Phaser.Scene) {{
{chr(10).join(register_lines) if register_lines else '  // Static sheets do not register animations.'}
}}
"""
    helper_path = PHASER_ROOT / "src/game/animations/generated/mirror-maze.animations.ts"
    helper_path.write_text(ts)

    data_ts = f"""export const mirrorMazeSheetmaps = {json.dumps(generated_manifest, indent=2)} as const;
"""
    (PHASER_ROOT / "src/game/data" / "mirror-maze-sheetmaps.ts").write_text(data_ts)


def main() -> None:
    specs = load_specs()
    generated_manifest = build_outputs(specs)
    build_report(generated_manifest)
    build_ts_helper(generated_manifest)


if __name__ == "__main__":
    main()
