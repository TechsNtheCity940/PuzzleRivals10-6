from PIL import Image
import os

# Config: list of source files to process relative to repo root
SOURCES = [
    r"Assets/Textures/Puzzles/CollapseGrid/collapse_anim.png",
    r"Assets/Textures/Puzzles/CollapseGrid/collapse_grid_debris_fall.png",
    r"Assets/Textures/Puzzles/CollapseGrid/collapse_grid_dust_plume.png",
    r"Assets/Textures/Puzzles/CollapseGrid/collapse_grid_crack_spread.png",
    r"Assets/Textures/Puzzles/CollapseGrid/collapse_grid_player_card_shimmer.png",
    r"Assets/Textures/Puzzles/CollapseGrid/collapse_grid_remove_tile.png",
]

FRAME_SIZE = 256  # per-frame size in pixels
COLS = 4
ROWS = 4
OUT_W = FRAME_SIZE * COLS
OUT_H = FRAME_SIZE * ROWS

repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))

def find_grid(img_w, img_h):
    # Find integer cols, rows where tile_w == tile_h
    for cols in range(1, 9):
        for rows in range(1, 9):
            if cols * rows > 16:
                continue
            if img_w % cols != 0 or img_h % rows != 0:
                continue
            tile_w = img_w // cols
            tile_h = img_h // rows
            if abs(tile_w - tile_h) <= 2:  # allow small tolerance
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
                frame = img.crop(box)
                frames.append(frame)
    else:
        # single frame
        frames = [img.copy()]
    return frames


def compose_sheet(frames, out_path):
    # Ensure frames length 16
    if len(frames) == 0:
        return False
    while len(frames) < 16:
        frames.append(frames[-1].copy())
    # Resize frames to FRAME_SIZE while preserving aspect, center on transparent background
    resized = []
    for f in frames[:16]:
        f = f.convert('RGBA')
        bw, bh = f.size
        scale = min(FRAME_SIZE / bw, FRAME_SIZE / bh)
        nw = int(bw * scale)
        nh = int(bh * scale)
        f2 = f.resize((nw, nh), Image.LANCZOS)
        canvas = Image.new('RGBA', (FRAME_SIZE, FRAME_SIZE), (0, 0, 0, 0))
        canvas.paste(f2, ((FRAME_SIZE - nw) // 2, (FRAME_SIZE - nh) // 2), f2)
        resized.append(canvas)
    # Create output sheet
    sheet = Image.new('RGBA', (OUT_W, OUT_H), (0, 0, 0, 0))
    idx = 0
    for r in range(ROWS):
        for c in range(COLS):
            sheet.paste(resized[idx], (c * FRAME_SIZE, r * FRAME_SIZE), resized[idx])
            idx += 1
    # Ensure output dir
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    sheet.save(out_path)
    return True


def process_source(rel_path):
    src = os.path.join(repo_root, rel_path)
    if not os.path.isfile(src):
        print(f"Source missing: {rel_path}")
        return False
    img = Image.open(src)
    frames = extract_frames(img)
    out_rel = os.path.splitext(rel_path)[0] + '_sheet.png'
    out = os.path.join(repo_root, out_rel)
    ok = compose_sheet(frames, out)
    if ok:
        print(f"Wrote: {out_rel}")
    else:
        print(f"Failed: {rel_path}")
    return ok


def main():
    for s in SOURCES:
        process_source(s)

if __name__ == '__main__':
    main()
