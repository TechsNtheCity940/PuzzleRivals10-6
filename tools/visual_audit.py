from PIL import Image
from pathlib import Path
import os
import hashlib

PROJECT_ROOT = Path(__file__).resolve().parent.parent
BASE_FOLDERS = [
    PROJECT_ROOT / "Assets" / "Textures" / "Puzzles",
    PROJECT_ROOT / "Assets" / "Textures" / "Effects",
]
FRAME_SIZE = 256
COLS = 4
ROWS = 4
EXPECTED_W = FRAME_SIZE * COLS
EXPECTED_H = FRAME_SIZE * ROWS

flags = []

for base in BASE_FOLDERS:
    if not os.path.isdir(base):
        continue

    for root, dirs, files in os.walk(base):
        for f in files:
            if not f.lower().endswith('_sheet.png'):
                continue

            path = os.path.join(root, f)
            try:
                source = Image.open(path)
                has_alpha = 'A' in source.getbands()
                img = source.convert('RGBA')
            except Exception as e:
                print(f"ERROR: cannot open {path}: {e}")
                continue

            w, h = img.size
            issues = []
            if (w, h) != (EXPECTED_W, EXPECTED_H):
                issues.append(f"size={w}x{h}")
            if not has_alpha:
                issues.append('no-alpha-channel')

            frames = []
            tw = w // COLS
            th = h // ROWS
            for r in range(ROWS):
                for c in range(COLS):
                    box = (c * tw, r * th, (c + 1) * tw, (r + 1) * th)
                    frame = img.crop(box)
                    frames.append(frame)

            hashes = []
            bbox_areas = []
            for fr in frames:
                data = fr.tobytes()
                hsh = hashlib.md5(data).hexdigest()
                hashes.append(hsh)
                bbox = fr.getchannel('A').getbbox()
                if bbox:
                    bw = bbox[2] - bbox[0]
                    bh = bbox[3] - bbox[1]
                    bbox_areas.append(bw * bh)
                else:
                    bbox_areas.append(0)

            unique = len(set(hashes))
            avg_bbox = sum(bbox_areas) / len(bbox_areas)
            bbox_ratio = avg_bbox / max(1, tw * th)

            if unique <= 1:
                issues.append('single-frame')
            elif unique < 4:
                issues.append(f'low-variation({unique})')
            if bbox_ratio < 0.05:
                issues.append(f'small-content({bbox_ratio:.3f})')
            if any(area == 0 for area in bbox_areas):
                issues.append('empty-frame')

            status = 'OK' if not issues else 'FLAG'
            rel_path = os.path.relpath(path, base)
            display_path = os.path.join(os.path.basename(base), rel_path)
            print(f"{status}: {display_path} | unique_frames={unique} | avg_bbox_ratio={bbox_ratio:.3f} | issues={';'.join(issues)}")
            if issues:
                flags.append((display_path, unique, bbox_ratio, issues))

print('\nAudit summary:')
print(f'Total flagged: {len(flags)}')
for p, u, ratio, iss in flags:
    print(f"- {p}: {', '.join(iss)}")
