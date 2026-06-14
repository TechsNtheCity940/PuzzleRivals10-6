from PIL import Image, ImageChops, ImageEnhance, ImageFilter, ImageDraw
import os, random, math

BASE_FOLDERS = [
    r"D:\\PuzzleRivals\\Assets\\Textures\\Puzzles",
    r"D:\\PuzzleRivals\\Assets\\Textures\\Effects",
]
FRAME_SIZE = 256
COLS = 4
ROWS = 4
OUT_W = FRAME_SIZE * COLS
OUT_H = FRAME_SIZE * ROWS

# Re-run audit logic to find flagged files (single-frame or low-variation)

def find_sheets():
    sheets = []
    for base in BASE_FOLDERS:
        for root, dirs, files in os.walk(base):
            for f in files:
                if not f.lower().endswith('_sheet.png'):
                    continue
                sheets.append(os.path.join(root, f))
    return sheets


def extract_frames_from_sheet(path):
    img = Image.open(path).convert('RGBA')
    w,h = img.size
    tw = w // COLS
    th = h // ROWS
    frames = []
    for r in range(ROWS):
        for c in range(COLS):
            box = (c*tw, r*th, (c+1)*tw, (r+1)*th)
            frames.append(img.crop(box))
    return frames


def unique_count(frames):
    seen = set()
    for f in frames:
        seen.add(hash(f.tobytes()))
    return len(seen)


def bbox_area_ratio(frames):
    areas = []
    for f in frames:
        bbox = f.getbbox()
        if bbox:
            bw = bbox[2]-bbox[0]
            bh = bbox[3]-bbox[1]
            areas.append(bw*bh)
        else:
            areas.append(0)
    avg = sum(areas)/len(areas)
    return avg/(FRAME_SIZE*FRAME_SIZE)


def generate_variations_single(frame):
    # Produce 16 frames by subtle variations
    out = []
    for i in range(16):
        t = i/15.0
        ang = (math.sin(t*2*math.pi)*3)  # small rotation
        scale = 1.0 + 0.03*math.sin(t*2*math.pi)
        dx = int(4*math.sin(t*2*math.pi*1.5))
        dy = int(4*math.cos(t*2*math.pi*1.3))
        f = frame.convert('RGBA')
        nw = max(1, int(f.width*scale))
        nh = max(1, int(f.height*scale))
        f2 = f.resize((nw,nh), Image.LANCZOS)
        f2 = f2.rotate(ang, resample=Image.BICUBIC, expand=True)
        canvas = Image.new('RGBA', (FRAME_SIZE, FRAME_SIZE), (0,0,0,0))
        px = (FRAME_SIZE - f2.width)//2 + dx
        py = (FRAME_SIZE - f2.height)//2 + dy
        canvas.paste(f2, (px,py), f2)
        # add subtle noise particles
        draw = ImageDraw.Draw(canvas)
        for p in range(4):
            rx = random.randint(0, FRAME_SIZE-1)
            ry = random.randint(0, FRAME_SIZE-1)
            draw.ellipse((rx,ry,rx+2,ry+2), fill=(255,255,255,int(20*(1-t))))
        # slight blur on some frames
        if i%4==0:
            canvas = canvas.filter(ImageFilter.GaussianBlur(radius=0.6))
        out.append(canvas)
    return out


def generate_interpolated(frames):
    # Given N frames (N>=2 and <16), interpolate to 16 by blending
    unique = []
    seen = set()
    for f in frames:
        hsh = hash(f.tobytes())
        if hsh not in seen:
            seen.add(hsh)
            unique.append(f)
    N = len(unique)
    out = []
    for i in range(16):
        pos = (i/15.0) * N
        idx = int(pos) % N
        nxt = (idx+1) % N
        frac = pos - int(pos)
        a = unique[idx].convert('RGBA')
        b = unique[nxt].convert('RGBA')
        blended = Image.blend(a.resize((FRAME_SIZE,FRAME_SIZE), Image.LANCZOS), b.resize((FRAME_SIZE,FRAME_SIZE), Image.LANCZOS), frac)
        out.append(blended)
    return out


def compose_sheet(frames, out_path):
    # frames list expected len 16
    while len(frames) < 16:
        frames.append(frames[-1].copy())
    sheet = Image.new('RGBA', (OUT_W, OUT_H), (0,0,0,0))
    idx = 0
    for r in range(ROWS):
        for c in range(COLS):
            f = frames[idx].convert('RGBA')
            if f.size != (FRAME_SIZE, FRAME_SIZE):
                f = f.resize((FRAME_SIZE, FRAME_SIZE), Image.LANCZOS)
            sheet.paste(f, (c*FRAME_SIZE, r*FRAME_SIZE), f)
            idx += 1
    sheet.save(out_path)


def get_relative_path(path):
    for base in BASE_FOLDERS:
        if os.path.commonpath([os.path.abspath(path), os.path.abspath(base)]) == os.path.abspath(base):
            return os.path.relpath(path, base)
    return os.path.basename(path)


def process():
    sheets = find_sheets()
    processed = []
    for s in sheets:
        frames = extract_frames_from_sheet(s)
        uc = unique_count(frames)
        ratio = bbox_area_ratio(frames)
        rel = get_relative_path(s)
        flag = False
        if uc <= 1 or uc < 4 or ratio < 0.05:
            flag = True
        if not flag:
            continue
        # generate
        if uc <= 1:
            # use frame 0
            new_frames = generate_variations_single(frames[0])
        else:
            new_frames = generate_interpolated(frames)
        out = os.path.splitext(s)[0] + '_redesign_sheet.png'
        compose_sheet(new_frames, out)
        print(f'Wrote redesign: {get_relative_path(out)} (was {rel})')
        processed.append(out)
    print(f'Done. Processed {len(processed)} assets.')

if __name__ == '__main__':
    process()
