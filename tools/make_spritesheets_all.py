from PIL import Image
import os
import argparse

FRAME_SIZE = 256
COLS = 4
ROWS = 4
OUT_W = FRAME_SIZE * COLS
OUT_H = FRAME_SIZE * ROWS

DEFAULT_BASES = [
    r"D:\\PuzzleRivals\\Assets\\Textures\\Puzzles",
    r"D:\\PuzzleRivals\\Assets\\Textures\\Effects",
]


def find_grid(img_w, img_h):
    for cols in range(1, 13):
        for rows in range(1, 13):
            if img_w % cols != 0 or img_h % rows != 0:
                continue
            tile_w = img_w // cols
            tile_h = img_h // rows
            if abs(tile_w - tile_h) <= 2:
                return cols, rows, tile_w, tile_h
    return None


def extract_frames(img):
    w, h = img.size
    grid = find_grid(w, h)
    frames = []
    if grid:
        cols, rows, tw, th = grid
        for r in range(rows):
            for c in range(cols):
                box = (c * tw, r * th, (c + 1) * tw, (r + 1) * th)
                frames.append(img.crop(box))
    else:
        frames = [img.copy()]
    return frames


def normalize_frames(frames):
    if len(frames) == 0:
        return []
    if len(frames) == 16:
        return frames
    if len(frames) < 16:
        while len(frames) < 16:
            frames.append(frames[-1].copy())
        return frames

    # More than 16 frames: sample evenly
    sampled = []
    count = len(frames)
    for i in range(16):
        index = int(round(i * (count - 1) / 15.0))
        sampled.append(frames[index])
    return sampled


def compose_sheet(frames, out_path):
    frames = normalize_frames(frames)
    if len(frames) == 0:
        return False
    resized = []
    for f in frames[:16]:
        f = f.convert('RGBA')
        bw, bh = f.size
        scale = min(FRAME_SIZE / bw, FRAME_SIZE / bh)
        nw = max(1, int(bw * scale))
        nh = max(1, int(bh * scale))
        f2 = f.resize((nw, nh), Image.LANCZOS)
        canvas = Image.new('RGBA', (FRAME_SIZE, FRAME_SIZE), (0, 0, 0, 0))
        canvas.paste(f2, ((FRAME_SIZE - nw) // 2, (FRAME_SIZE - nh) // 2), f2)
        resized.append(canvas)
    sheet = Image.new('RGBA', (OUT_W, OUT_H), (0, 0, 0, 0))
    idx = 0
    for r in range(ROWS):
        for c in range(COLS):
            sheet.paste(resized[idx], (c * FRAME_SIZE, r * FRAME_SIZE), resized[idx])
            idx += 1
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    sheet.save(out_path)
    return True


def should_skip(file_path):
    name = os.path.basename(file_path).lower()
    if name.endswith('_sheet.png') or name.endswith('_redesign_sheet.png'):
        return True
    return False


def should_write_sheet(src, out):
    if not os.path.exists(out):
        return True
    return os.path.getmtime(src) > os.path.getmtime(out)


def process_folder(base_path):
    written = 0
    skipped = 0
    for root, _, files in os.walk(base_path):
        for f in files:
            if not f.lower().endswith('.png'):
                continue
            if f.lower().endswith('.meta'):
                continue
            if f.startswith('.'):
                continue
            if should_skip(f):
                continue
            src = os.path.join(root, f)
            out = os.path.join(root, os.path.splitext(f)[0] + '_sheet.png')
            if not should_write_sheet(src, out):
                print(f"Skipping unchanged sheet: {out}")
                skipped += 1
                continue
            try:
                img = Image.open(src)
                frames = extract_frames(img)
                is_new = not os.path.exists(out)
                ok = compose_sheet(frames, out)
                if ok:
                    status = "Wrote" if is_new else "Updated"
                    print(f"{status}: {out}")
                    written += 1
                else:
                    print(f"Failed to compose: {src}")
            except Exception as e:
                print(f"Error processing {src}: {e}")
    return written, skipped


def main():
    parser = argparse.ArgumentParser(description='Build 16-frame sprite sheets from puzzle/effect textures.')
    parser.add_argument('--base', nargs='*', default=DEFAULT_BASES,
                        help='Base directories to scan (default: puzzles and effects).')
    args = parser.parse_args()

    total_written = 0
    total_skipped = 0
    for base in args.base:
        print(f"Processing base: {base}")
        written, skipped = process_folder(base)
        total_written += written
        total_skipped += skipped
    print(f"Done. Written={total_written}, Skipped={total_skipped}")


if __name__ == '__main__':
    main()