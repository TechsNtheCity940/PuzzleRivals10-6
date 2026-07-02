from PIL import Image, ImageSequence
import os

BASE_FOLDERS = [
    r"D:\\PuzzleRivals\\Assets\\Textures\\Puzzles",
    r"D:\\PuzzleRivals\\Assets\\Textures\\Effects",
]
PREV_DIR = r"D:\\PuzzleRivals\\tools\\qa_previews"
MD_PATH = r"D:\\PuzzleRivals\\tools\\QA_report.md"
COLS = 4
ROWS = 4
FRAME_SIZE = 256

os.makedirs(PREV_DIR, exist_ok=True)
entries = []

for base in BASE_FOLDERS:
    if not os.path.isdir(base):
        continue
    for root, dirs, files in os.walk(base):
        for f in files:
            if not f.lower().endswith('.png'):
                continue
            if not ('_sheet.png' in f.lower() or '_redesign_sheet.png' in f.lower()):
                continue
            path = os.path.join(root, f)
            rel = os.path.relpath(path, r"D:\\PuzzleRivals")
            name = os.path.splitext(f)[0]
            try:
                img = Image.open(path).convert('RGBA')
            except Exception as e:
                print('ERR', path, e)
                continue
            w, h = img.size
            tw = w // COLS
            th = h // ROWS
            if tw == 0 or th == 0:
                print('Invalid sheet dimensions:', path)
                continue
            frames = []
            for r in range(ROWS):
                for c in range(COLS):
                    box = (c * tw, r * th, (c + 1) * tw, (r + 1) * th)
                    fr = img.crop(box).convert('RGBA')
                    fr = fr.resize((FRAME_SIZE, FRAME_SIZE), Image.LANCZOS)
                    frames.append(fr)
            thumb_path = os.path.join(PREV_DIR, f"{name}_thumb.png")
            frames[0].save(thumb_path)
            contact = Image.new('RGBA', (FRAME_SIZE * COLS // 2, FRAME_SIZE * ROWS // 2), (0, 0, 0, 0))
            smallw = FRAME_SIZE // 2
            smallh = FRAME_SIZE // 2
            idx = 0
            for r in range(ROWS):
                for c in range(COLS):
                    fr = frames[idx].resize((smallw, smallh), Image.LANCZOS)
                    contact.paste(fr, (c * smallw, r * smallh), fr)
                    idx += 1
            contact_path = os.path.join(PREV_DIR, f"{name}_contact.png")
            contact.save(contact_path)
            gif_path = os.path.join(PREV_DIR, f"{name}_anim.gif")
            frames[0].save(gif_path, save_all=True, append_images=frames[1:], duration=80, loop=0, disposal=2)
            entries.append({
                'name': name,
                'rel': rel,
                'thumb': os.path.relpath(thumb_path, r"D:\\PuzzleRivals"),
                'contact': os.path.relpath(contact_path, r"D:\\PuzzleRivals"),
                'gif': os.path.relpath(gif_path, r"D:\\PuzzleRivals"),
                'folder': os.path.relpath(base, r"D:\\PuzzleRivals")
            })

prefab_dirs = [
    r"D:\\PuzzleRivals\\Assets\\Prefabs\\Puzzles",
    r"D:\\PuzzleRivals\\Assets\\Prefabs\\Effects",
]
prefab_groups = {}
for prefab_dir in prefab_dirs:
    group = os.path.basename(prefab_dir)
    prefab_groups[group] = []
    if os.path.isdir(prefab_dir):
        for f in os.listdir(prefab_dir):
            if f.lower().endswith('.prefab'):
                prefab_groups[group].append(f)

with open(MD_PATH, 'w', encoding='utf-8') as md:
    md.write('# Animation QA Report\n\n')
    md.write('This report lists generated 16-frame sprite sheets and previews. Thumbnails and animated previews are saved under tools/qa_previews/.\n\n')
    md.write('## Sheets\n\n')
    for e in sorted(entries, key=lambda x: (x['folder'], x['name'])):
        md.write(f"### {e['name']} ({e['folder']})\n")
        md.write(f"- Source: `{e['rel']}`\n")
        md.write(f"![thumb]({e['thumb']})\n\n")
        md.write(f"Contact sheet: ![{e['name']}]({e['contact']})\n\n")
        md.write(f"Animated preview: [{e['name']}]({e['gif']})\n\n")
    md.write('\n## Prefabs\n\n')
    for group, prefabs in prefab_groups.items():
        md.write(f"### {group}\n\n")
        if prefabs:
            md.write(f'Found prefab files in Assets/Prefabs/{group}:\n\n')
            for p in sorted(prefabs):
                md.write(f"- {p}\n")
        else:
            md.write(f'No prefabs found in Assets/Prefabs/{group}.\n')
        md.write('\n')

print('QA report generated:', MD_PATH)
