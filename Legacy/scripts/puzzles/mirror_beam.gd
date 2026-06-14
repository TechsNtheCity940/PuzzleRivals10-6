extends PuzzleBase

# Mirror Beam Mayhem — a real light-routing puzzle.
#
#   * A PRIMARY emitter on the left edge fires a beam east.
#   * Rotatable MIRRORS (click to flip / <-> \) bend the beam 90 degrees.
#   * A PRISM splits any beam that enters it into the two perpendicular
#     directions (one beam in, two beams out).
#   * An ACTIVATOR gate, once struck by a beam, opens a SECOND emitter.
#   * The second beam must be routed (through the prism) to the TARGET.
#
# Win when the target is lit. The generator builds a guaranteed-solvable
# solution, verifies it by simulation, then scrambles the mirrors — so every
# round is a fresh, solvable layout that takes real planning to crack.

const CELL_PX: float = 76.0
const PADDING: float = 18.0

# Cell types.
const EMPTY: int = 0
const MIRROR: int = 1
const PRISM: int = 2
const ACTIVATOR: int = 3
const TARGET: int = 4

# Mirror orientations.
const SLASH: int = 1      # "/"
const BACKSLASH: int = 2  # "\"

# Travel directions.
const E: Vector2i = Vector2i(1, 0)
const W: Vector2i = Vector2i(-1, 0)
const N: Vector2i = Vector2i(0, -1)
const S: Vector2i = Vector2i(0, 1)

const BEAM_PRIMARY: Color = Color(0.25, 0.95, 1.0)
const BEAM_SECONDARY: Color = Color(1.0, 0.40, 0.88)

var grid_size: int = 6
var cell_type: Array = []        # 2D ints
var mirror_orient: Array = []    # 2D ints (SLASH/BACKSLASH)
var solved_orient: Array = []    # 2D ints — the known-good orientation
var mirror_positions: Array = []  # Array[Vector2i]
var cell_nodes: Array = []

var mirror_draws: Dictionary = {}   # Vector2i -> _MirrorDraw
var activator_draw: Control = null
var target_draw: Control = null
var beam_overlay: Control = null

var primary_pos: Vector2i = Vector2i(0, 0)
var primary_dir: Vector2i = Vector2i(1, 0)
var secondary_pos: Vector2i = Vector2i(-1, -1)
var secondary_dir: Vector2i = Vector2i(0, 1)
var activator_pos: Vector2i = Vector2i(-1, -1)
var target_pos: Vector2i = Vector2i(-1, -1)
var prism_pos: Vector2i = Vector2i(-1, -1)

func setup(d: int = 1) -> void:
	super.setup(d)
	grid_size = clampi(6 + (d - 1) / 2, 6, 8)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	mirror_positions = []

	var board_w := CELL_PX * grid_size + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_w + 56.0)
	size = Vector2(board_w, board_w + 56.0)

	var root_bg := ColorRect.new()
	root_bg.color = Color(0.03, 0.05, 0.13, 0.92)
	root_bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(root_bg)
	root_bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var ok := false
	var attempts := 0
	while not ok and attempts < 90:
		attempts += 1
		ok = _try_layout()
	if not ok:
		_fallback_layout()

	_instantiate_cells()
	_scramble()
	_redraw()

	var lbl := Label.new()
	lbl.text = "Light the ACTIVATOR to open the 2nd beam, then route it to the TARGET."
	lbl.add_theme_font_size_override("font_size", 20)
	lbl.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	lbl.add_theme_constant_override("outline_size", 4)
	lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(lbl)
	lbl.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	lbl.offset_top = -50
	lbl.offset_bottom = -6

# ------------------------------------------------------------------ layout

func _reset_grids() -> void:
	cell_type = []
	mirror_orient = []
	solved_orient = []
	for y in grid_size:
		var tr: Array = []
		var mr: Array = []
		var sr2: Array = []
		for x in grid_size:
			tr.append(EMPTY)
			mr.append(SLASH)
			sr2.append(SLASH)
		cell_type.append(tr)
		mirror_orient.append(mr)
		solved_orient.append(sr2)

func _try_layout() -> bool:
	_reset_grids()
	mirror_positions = []
	activator_pos = Vector2i(-1, -1)
	target_pos = Vector2i(-1, -1)
	prism_pos = Vector2i(-1, -1)

	var sr := randi() % grid_size
	primary_pos = Vector2i(0, sr)
	primary_dir = E

	var sc := 1 + randi() % (grid_size - 2)
	secondary_pos = Vector2i(sc, 0)
	secondary_dir = S

	# --- Stage 1: primary beam -> activator (two mirror turns). ---
	var c1 := 1 + randi() % (grid_size - 2)
	if c1 >= grid_size - 1:
		return false
	var ar := randi() % grid_size
	if ar == sr:
		return false
	var ac := c1 + 1 + randi() % (grid_size - 1 - c1)
	var dout1 := S if ar > sr else N
	if not _place_mirror(Vector2i(c1, sr), _mirror_for_turn(E, dout1)):
		return false
	if not _place_mirror(Vector2i(c1, ar), _mirror_for_turn(dout1, E)):
		return false
	if not _place_type(Vector2i(ac, ar), ACTIVATOR):
		return false
	activator_pos = Vector2i(ac, ar)

	# --- Stage 2: secondary beam -> mirror -> prism -> mirror -> target. ---
	if sc >= grid_size - 1:
		return false
	var r2 := 1 + randi() % (grid_size - 2)
	if r2 >= grid_size - 1:
		return false
	var pc := sc + 1 + randi() % (grid_size - 1 - sc)
	if pc >= grid_size - 1:
		return false
	var r4 := r2 + 1 + randi() % (grid_size - 1 - r2)
	var tc := pc + 1 + randi() % (grid_size - 1 - pc)
	if not _place_mirror(Vector2i(sc, r2), _mirror_for_turn(S, E)):
		return false
	if not _place_type(Vector2i(pc, r2), PRISM):
		return false
	prism_pos = Vector2i(pc, r2)
	if not _place_mirror(Vector2i(pc, r4), _mirror_for_turn(S, E)):
		return false
	if not _place_type(Vector2i(tc, r4), TARGET):
		return false
	target_pos = Vector2i(tc, r4)

	# --- Verify the SOLVED layout actually works. ---
	var solved_state := _simulate()
	if not bool(solved_state.target_lit):
		return false
	if not bool(solved_state.activator_lit):
		return false
	# The activator must be genuinely required (primary alone can't win).
	var primary_only := _trace_once(false, false, [])
	if bool(primary_only.target_lit):
		return false
	return true

func _fallback_layout() -> void:
	# Simple but always-solvable: primary -> two mirrors -> target. No prism.
	_reset_grids()
	mirror_positions = []
	activator_pos = Vector2i(-1, -1)
	prism_pos = Vector2i(-1, -1)
	primary_pos = Vector2i(0, 0)
	primary_dir = E
	secondary_pos = Vector2i(-1, -1)
	var c1 := grid_size / 2
	var ar := grid_size - 1
	_place_mirror(Vector2i(c1, 0), _mirror_for_turn(E, S))
	_place_mirror(Vector2i(c1, ar), _mirror_for_turn(S, E))
	_place_type(Vector2i(grid_size - 1, ar), TARGET)
	target_pos = Vector2i(grid_size - 1, ar)

func _place_mirror(pos: Vector2i, orient: int) -> bool:
	if pos.x < 0 or pos.x >= grid_size or pos.y < 0 or pos.y >= grid_size:
		return false
	if pos == primary_pos or pos == secondary_pos:
		return false
	if int(cell_type[pos.y][pos.x]) != EMPTY:
		return false
	cell_type[pos.y][pos.x] = MIRROR
	mirror_orient[pos.y][pos.x] = orient
	solved_orient[pos.y][pos.x] = orient
	mirror_positions.append(pos)
	return true

func _place_type(pos: Vector2i, t: int) -> bool:
	if pos.x < 0 or pos.x >= grid_size or pos.y < 0 or pos.y >= grid_size:
		return false
	if pos == primary_pos or pos == secondary_pos:
		return false
	if int(cell_type[pos.y][pos.x]) != EMPTY:
		return false
	cell_type[pos.y][pos.x] = t
	return true

# Orientation that bends travel-direction `din` into perpendicular `dout`.
func _mirror_for_turn(din: Vector2i, dout: Vector2i) -> int:
	if (din == E and dout == N) or (din == N and dout == E) \
			or (din == W and dout == S) or (din == S and dout == W):
		return SLASH
	return BACKSLASH

func _reflect(d: Vector2i, orient: int) -> Vector2i:
	if orient == SLASH:
		return Vector2i(-d.y, -d.x)
	return Vector2i(d.y, d.x)

func _perp(d: Vector2i) -> Array:
	if d.y == 0:
		return [N, S]
	return [E, W]

func _other(o: int) -> int:
	return BACKSLASH if o == SLASH else SLASH

func _scramble() -> void:
	var flips := 0
	for p: Vector2i in mirror_positions:
		if randf() < 0.7:
			mirror_orient[p.y][p.x] = _other(int(solved_orient[p.y][p.x]))
			flips += 1
	if flips < 2 and mirror_positions.size() >= 2:
		for i in 2:
			var p: Vector2i = mirror_positions[i]
			mirror_orient[p.y][p.x] = _other(int(solved_orient[p.y][p.x]))
	# Never start already-solved.
	var guard := 0
	while bool(_simulate().target_lit) and guard < 12:
		guard += 1
		var rp: Vector2i = mirror_positions[randi() % mirror_positions.size()]
		mirror_orient[rp.y][rp.x] = _other(int(mirror_orient[rp.y][rp.x]))

# ------------------------------------------------------------------ simulation

# Trace all beams once. If `collect`, append pixel-space segments to seg_out.
func _trace_once(secondary_on: bool, collect: bool, seg_out: Array) -> Dictionary:
	var activator_lit := false
	var target_lit := false
	var visited: Dictionary = {}
	var stack: Array = []
	stack.append([primary_pos, primary_dir, BEAM_PRIMARY])
	if secondary_on and secondary_pos.x >= 0:
		stack.append([secondary_pos, secondary_dir, BEAM_SECONDARY])
	var half := CELL_PX * 0.5
	while not stack.is_empty():
		var beam: Array = stack.pop_back()
		var pos: Vector2i = beam[0]
		var dir: Vector2i = beam[1]
		var col: Color = beam[2]
		var steps := 0
		while pos.x >= 0 and pos.x < grid_size and pos.y >= 0 and pos.y < grid_size and steps < grid_size * grid_size * 2:
			steps += 1
			var vkey := "%d,%d,%d,%d" % [pos.x, pos.y, dir.x, dir.y]
			if visited.has(vkey):
				break
			visited[vkey] = true
			var center := _cell_center(pos)
			var entry := center - Vector2(dir.x, dir.y) * half
			var t: int = int(cell_type[pos.y][pos.x])
			if t == MIRROR:
				var nd := _reflect(dir, int(mirror_orient[pos.y][pos.x]))
				if collect:
					seg_out.append([entry, center, col])
					seg_out.append([center, center + Vector2(nd.x, nd.y) * half, col])
				dir = nd
				pos += dir
			elif t == PRISM:
				if collect:
					seg_out.append([entry, center, col])
				for nd: Vector2i in _perp(dir):
					if collect:
						seg_out.append([center, center + Vector2(nd.x, nd.y) * half, col])
					stack.append([pos + nd, nd, col])
				break
			elif t == ACTIVATOR:
				activator_lit = true
				if collect:
					seg_out.append([entry, center, col])
				break
			elif t == TARGET:
				target_lit = true
				if collect:
					seg_out.append([entry, center, col])
				break
			else:
				if collect:
					seg_out.append([entry, center + Vector2(dir.x, dir.y) * half, col])
				pos += dir
	return {"activator_lit": activator_lit, "target_lit": target_lit}

# Fixpoint: primary may light the activator, which opens the secondary beam.
func _simulate(collect: bool = false, seg_out: Array = []) -> Dictionary:
	var secondary_on := false
	var result: Dictionary = {"activator_lit": false, "target_lit": false}
	for _p in range(4):
		result = _trace_once(secondary_on, false, [])
		if bool(result.activator_lit) and not secondary_on:
			secondary_on = true
			continue
		break
	if collect:
		result = _trace_once(secondary_on, true, seg_out)
	result["secondary_on"] = secondary_on
	return result

func _cell_center(p: Vector2i) -> Vector2:
	return Vector2(PADDING + p.x * CELL_PX + CELL_PX * 0.5, PADDING + p.y * CELL_PX + CELL_PX * 0.5)

# ------------------------------------------------------------------ visuals

func _instantiate_cells() -> void:
	mirror_draws = {}
	activator_draw = null
	target_draw = null
	cell_nodes = []
	for y in grid_size:
		var row_nodes: Array = []
		for x in grid_size:
			row_nodes.append(_make_cell(x, y))
		cell_nodes.append(row_nodes)
	beam_overlay = _BeamOverlay.new()
	beam_overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(beam_overlay)
	beam_overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _make_cell(x: int, y: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 4.0, CELL_PX - 4.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)

	var bg := ColorRect.new()
	bg.color = Color(0.06, 0.09, 0.18, 0.95)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var border := ColorRect.new()
	border.color = Color(0.10, 0.18, 0.30, 1.0)
	border.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(border)
	border.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	border.offset_left = 1
	border.offset_top = 1
	border.offset_right = -1
	border.offset_bottom = -1

	var inner := ColorRect.new()
	inner.color = Color(0.05, 0.08, 0.16, 1.0)
	inner.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(inner)
	inner.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	inner.offset_left = 2
	inner.offset_top = 2
	inner.offset_right = -2
	inner.offset_bottom = -2

	var pos := Vector2i(x, y)
	var t: int = int(cell_type[y][x])
	if t == MIRROR:
		var md := _MirrorDraw.new()
		md.kind = int(mirror_orient[y][x])
		md.mouse_filter = Control.MOUSE_FILTER_IGNORE
		cell.add_child(md)
		md.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		mirror_draws[pos] = md
	elif t == PRISM:
		var pd := _PrismDraw.new()
		pd.mouse_filter = Control.MOUSE_FILTER_IGNORE
		cell.add_child(pd)
		pd.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	elif t == ACTIVATOR:
		var ad := _ActivatorDraw.new()
		ad.mouse_filter = Control.MOUSE_FILTER_IGNORE
		cell.add_child(ad)
		ad.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		activator_draw = ad
	elif t == TARGET:
		var tg := _TargetDraw.new()
		tg.mouse_filter = Control.MOUSE_FILTER_IGNORE
		cell.add_child(tg)
		tg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		target_draw = tg

	if pos == primary_pos:
		bg.color = Color(0.0, 0.50, 0.65, 0.85)
	elif pos == secondary_pos:
		bg.color = Color(0.55, 0.0, 0.45, 0.75)

	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	return cell

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if int(cell_type[y][x]) != MIRROR:
		return
	mirror_orient[y][x] = _other(int(mirror_orient[y][x]))
	AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -10.0, 0.05)
	piece_placed.emit()
	_redraw()

func _redraw() -> void:
	var seg: Array = []
	var st := _simulate(true, seg)

	for p: Vector2i in mirror_positions:
		var md = mirror_draws.get(p)
		if md != null and is_instance_valid(md):
			md.set("kind", int(mirror_orient[p.y][p.x]))
			md.call("_refresh")
	if activator_draw != null and is_instance_valid(activator_draw):
		activator_draw.set("lit", bool(st.activator_lit))
	if target_draw != null and is_instance_valid(target_draw):
		target_draw.set("lit", bool(st.target_lit))
	if beam_overlay != null and is_instance_valid(beam_overlay):
		beam_overlay.set("segments", seg)
		beam_overlay.set("emitters", _emitter_markers(bool(st.secondary_on)))
		beam_overlay.queue_redraw()

	if bool(st.target_lit) and not _solved:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(120 + difficulty * 50)

func _emitter_markers(secondary_on: bool) -> Array:
	var out: Array = []
	var pc := _cell_center(primary_pos)
	out.append([Vector2(PADDING - 2.0, pc.y), BEAM_PRIMARY, true])
	if secondary_pos.x >= 0:
		var sc := _cell_center(secondary_pos)
		out.append([Vector2(sc.x, PADDING - 2.0), BEAM_SECONDARY, secondary_on])
	return out

# ------------------------------------------------------------------ contract

func get_progress() -> float:
	var st := _simulate()
	if bool(st.target_lit):
		return 1.0
	if bool(st.activator_lit):
		return 0.65
	if mirror_positions.is_empty():
		return 0.0
	var matched := 0
	for p: Vector2i in mirror_positions:
		if int(mirror_orient[p.y][p.x]) == int(solved_orient[p.y][p.x]):
			matched += 1
	return clampf(0.5 * float(matched) / float(mirror_positions.size()), 0.0, 0.55)

func auto_solve_step() -> void:
	for p: Vector2i in mirror_positions:
		if int(mirror_orient[p.y][p.x]) != int(solved_orient[p.y][p.x]):
			var fake := InputEventMouseButton.new()
			fake.button_index = MOUSE_BUTTON_LEFT
			fake.pressed = true
			_on_cell_click(fake, p.x, p.y)
			return

func ai_step() -> void:
	auto_solve_step()

# ------------------------------------------------------------------ inner draw classes

class _BeamOverlay extends Control:
	var segments: Array = []
	var emitters: Array = []

	func _draw() -> void:
		for s: Array in segments:
			var a: Vector2 = s[0]
			var b: Vector2 = s[1]
			var c: Color = s[2]
			draw_line(a, b, Color(c.r, c.g, c.b, 0.20), 10.0)
			draw_line(a, b, Color(c.r, c.g, c.b, 0.85), 3.5)
			draw_line(a, b, Color(1, 1, 1, 0.85), 1.4)
		for e: Array in emitters:
			var p: Vector2 = e[0]
			var col: Color = e[1]
			var on: bool = e[2]
			var alpha: float = 1.0 if on else 0.22
			draw_circle(p, 13.0, Color(col.r, col.g, col.b, alpha * 0.30))
			draw_circle(p, 7.0, Color(col.r, col.g, col.b, alpha))


class _MirrorDraw extends Control:
	var kind: int = 1
	var _tex_rect: TextureRect = null
	const MIRROR_PATH := "res://assets/sprites/effects/mirror_diagonal.png"

	func _ready() -> void:
		_tex_rect = TextureRect.new()
		_tex_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		_tex_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		_tex_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
		if ResourceLoader.exists(MIRROR_PATH):
			_tex_rect.texture = load(MIRROR_PATH)
		add_child(_tex_rect)
		_tex_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		_refresh()

	func _notification(what: int) -> void:
		if what == NOTIFICATION_RESIZED:
			_refresh()

	func _refresh() -> void:
		if _tex_rect == null or not is_instance_valid(_tex_rect):
			return
		_tex_rect.pivot_offset = _tex_rect.size / 2.0
		# Sprite ships as a "/" mirror; rotate 90 degrees for "\".
		_tex_rect.rotation_degrees = 0.0 if kind == 1 else 90.0


class _PrismDraw extends Control:
	func _draw() -> void:
		var w := size.x
		var h := size.y
		var p1 := Vector2(w * 0.5, h * 0.16)
		var p2 := Vector2(w * 0.16, h * 0.84)
		var p3 := Vector2(w * 0.84, h * 0.84)
		draw_colored_polygon(PackedVector2Array([p1, p2, p3]), Color(0.80, 0.90, 1.0, 0.28))
		# Refractive rainbow edges.
		draw_line(p1, p2, Color(1.0, 0.30, 0.45, 0.95), 2.5)
		draw_line(p2, p3, Color(0.35, 1.0, 0.55, 0.95), 2.5)
		draw_line(p3, p1, Color(0.40, 0.65, 1.0, 0.95), 2.5)
		draw_polyline(PackedVector2Array([p1, p2, p3, p1]), Color(1, 1, 1, 0.75), 1.4)


class _ActivatorDraw extends Control:
	var lit: bool = false:
		set(value):
			lit = value
			queue_redraw()

	func _draw() -> void:
		var c := size / 2.0
		var r := minf(size.x, size.y) * 0.34
		var col := Color(1.0, 0.85, 0.20) if lit else Color(0.45, 0.45, 0.55)
		if lit:
			draw_circle(c, r * 1.7, Color(col.r, col.g, col.b, 0.28))
		# Diamond gate.
		var pts := PackedVector2Array([
			c + Vector2(0, -r), c + Vector2(r, 0), c + Vector2(0, r), c + Vector2(-r, 0)
		])
		draw_polyline(PackedVector2Array([pts[0], pts[1], pts[2], pts[3], pts[0]]), col, 3.5)
		draw_circle(c, r * 0.42, Color(col.r, col.g, col.b, 0.9 if lit else 0.4))


class _TargetDraw extends Control:
	var lit: bool = false:
		set(value):
			lit = value
			queue_redraw()

	func _draw() -> void:
		var c := size / 2.0
		var r := minf(size.x, size.y) * 0.36
		var col := Color(0.30, 1.0, 0.45) if lit else Color(0.55, 0.62, 0.58)
		if lit:
			draw_circle(c, r * 1.8, Color(col.r, col.g, col.b, 0.30))
		draw_arc(c, r, 0.0, TAU, 36, col, 4.0)
		draw_arc(c, r * 0.62, 0.0, TAU, 28, col, 3.0)
		draw_circle(c, r * 0.26, col)
