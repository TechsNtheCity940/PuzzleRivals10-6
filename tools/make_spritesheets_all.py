from PIL import Image
import os

BASE = r"D:\\PuzzleRivals\\Assets\\Textures\\Puzzles"
FRAME_SIZE = 256
COLS = 4
ROWS = 4
OUT_W = FRAME_SIZE * COLS
OUT_H = FRAME_SIZE * ROWS

def find_grid(img_w, img_h):
    for cols in range(1, 9):
        for rows in range(1, 9):
            if cols * rows > 16:
                continue
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


def compose_sheet(frames, out_path):
    if len(frames) == 0:
        return False
    while len(frames) < 16:
        frames.append(frames[-1].copy())
    resized = []
    for f in frames[:16]:
        f = f.convert('RGBA')
        bw, bh = f.size
        scale = min(FRAME_SIZE / bw, FRAME_SIZE / bh)
        nw = max(1, int(bw * scale))
        nh = max(1, int(bh * scale))
        f2 = f.resize((nw, nh), Image.LANCZOS)
        canvas = Image.new('RGBA', (FRAME_SIZE, FRAME_SIZE), (0,0,0,0))
        canvas.paste(f2, ((FRAME_SIZE - nw)//2, (FRAME_SIZE - nh)//2), f2)
        resized.append(canvas)
    sheet = Image.new('RGBA', (OUT_W, OUT_H), (0,0,0,0))
    idx = 0
    for r in range(ROWS):
        for c in range(COLS):
            sheet.paste(resized[idx], (c*FRAME_SIZE, r*FRAME_SIZE), resized[idx])
            idx += 1
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    sheet.save(out_path)
    return True


def should_skip(file_path):
    name = os.path.basename(file_path)
    if name.lower().endswith('_sheet.png'):
        return True
    return False


def main():
    written = 0
    skipped = 0
    for root, dirs, files in os.walk(BASE):
        for f in files:
            if not f.lower().endswith('.png'):
                continue
            if f.lower().endswith('.meta'):
                continue
            if f.startswith('.'):
                continue
            if f.lower().endswith('_sheet.png'):
                continue
            src = os.path.join(root, f)
            out = os.path.join(root, os.path.splitext(f)[0] + '_sheet.png')
            if os.path.exists(out):
                print(f"Skipping existing: {out}")
                skipped += 1
                continue
            try:
                img = Image.open(src)
                frames = extract_frames(img)
                ok = compose_sheet(frames, out)
                if ok:
                    print(f"Wrote: {out}")
                    written += 1
                else:
                    print(f"Failed to compose: {src}")
            except Exception as e:
                print(f"Error processing {src}: {e}")
    print(f"Done. Written={written}, Skipped={skipped}")

if __name__ == '__main__':
    main()
