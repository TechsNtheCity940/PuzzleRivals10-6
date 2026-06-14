extends PuzzleBase

# Score Attack — cells randomly light up; click them before they fade.
# Solve when N hits scored. Misses don't reset.

const CELL_PX: float = 88.0
const PADDING: float = 16.0
const SPAWN_INTERVAL: float = 0.7
const FADE_TIME: float = 1.4

const FLOOR_TEX: String = "res://assets/textures/floors/circuit_floor.png"
const TARGET_TEX: String = "res://assets/sprites/effects/score_target.png"

var grid_size: int = 5
var target_score: int = 12
var current_score: int = 0
var cell_nodes: Array = []
var lit_state: Array = []      # 2D float: fade progress 1->0; 0 = unlit
var spawn_timer: float = 0.0

var _floor_tex: Texture2D = null
var _target_tex: Texture2D = null

func _load_tex(path: String) -> Texture2D:
	if ResourceLoader.exists(path):
		return load(path) as Texture2D
	return null

func setup(d: int = 1) -> void:
	super.setup(d)
	grid_size = 5
	target_score = clampi(10 + d * 2, 10, 24)
	_floor_tex = _load_tex(FLOOR_TEX)
	_target_tex = _load_tex(TARGET_TEX)
	_build()
	set_process(true)

func _build() -> void:
	for c in get_children():
		c.queue_free()
	cell_nodes.clear()
	lit_state.clear()
	current_score = 0
	spawn_timer = 0.0

	var board_w := CELL_PX * grid_size + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_w + 40)
	size = Vector2(board_w, board_w + 40)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	for y in grid_size:
		var row: Array = []
		var row_nodes: Array = []
		for x in grid_size:
			row.append(0.0)
			row_nodes.append(_make_cell(x, y))
		lit_state.append(row)
		cell_nodes.append(row_nodes)

	# Header label
	var lbl := Label.new()
	lbl.text = "Click the lit cells!"
	lbl.add_theme_font_size_override("font_size", 22)
	lbl.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	lbl.add_theme_constant_override("outline_size", 4)
	lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(lbl)
	lbl.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	lbl.offset_top = -32
	lbl.offset_bottom = -4

func _make_cell(x: int, y: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 6.0, CELL_PX - 6.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)
	# child 0: dark base + fallback color
	var rect := ColorRect.new()
	rect.color = Color(0.05, 0.08, 0.16, 0.95)
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(rect)
	rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 1: circuit-floor surface
	var terrain := TextureRect.new()
	terrain.texture = _floor_tex
	terrain.modulate = Color(0.7, 0.74, 0.85, 1.0)
	terrain.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	terrain.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	terrain.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(terrain)
	terrain.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 2: glowing target button (shown when lit)
	var target := TextureRect.new()
	target.texture = _target_tex
	target.modulate = Color(1.0, 1.0, 1.0, 0.0)
	target.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	target.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	target.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(target)
	target.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	return cell

func _process(delta: float) -> void:
	if _solved or not input_enabled:
		return
	spawn_timer += delta
	if spawn_timer >= SPAWN_INTERVAL:
		spawn_timer = 0.0
		_spawn_lit()
	# Tick fades
	for y in grid_size:
		for x in grid_size:
			if float(lit_state[y][x]) > 0.0:
				lit_state[y][x] = maxf(0.0, float(lit_state[y][x]) - delta / FADE_TIME)
			_paint_cell(x, y)

func _spawn_lit() -> void:
	# Pick a random unlit cell.
	var candidates: Array = []
	for y in grid_size:
		for x in grid_size:
			if float(lit_state[y][x]) <= 0.0:
				candidates.append(Vector2i(x, y))
	if candidates.is_empty():
		return
	var pick: Vector2i = candidates[randi() % candidates.size()]
	lit_state[pick.y][pick.x] = 1.0

func _paint_cell(x: int, y: int) -> void:
	var node: Control = cell_nodes[y][x]
	if not is_instance_valid(node):
		return
	var rect := node.get_child(0) as ColorRect
	var target := node.get_child(2) as TextureRect
	if rect == null:
		return
	var lit: float = lit_state[y][x]
	if _target_tex != null and target != null:
		# Texture path: keep base dark, fade the glowing target button in/out.
		rect.color = Color(0.05, 0.08, 0.16, 0.95)
		var a: float = 0.0
		if lit > 0.0:
			a = clampf(0.35 + 0.65 * lit, 0.0, 1.0)
		target.modulate = Color(1.0, 1.0, 1.0, a)
		var s: float = 0.82 + 0.18 * lit
		target.pivot_offset = target.size / 2.0
		target.scale = Vector2(s, s)
	else:
		# Fallback: flat colored cell as before.
		if target != null:
			target.modulate = Color(1.0, 1.0, 1.0, 0.0)
		if lit > 0.0:
			rect.color = Color(0.0, 0.90, 1.0, 0.4 + 0.55 * lit)
		else:
			rect.color = Color(0.05, 0.08, 0.16, 0.95)

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if float(lit_state[y][x]) <= 0.0:
		return
	lit_state[y][x] = 0.0
	current_score += 1
	AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -8.0, 0.1)
	piece_placed.emit()
	_paint_cell(x, y)
	if current_score >= target_score:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		set_process(false)
		_emit_solved(90 + difficulty * 35)

func get_progress() -> float:
	return clampf(float(current_score) / float(target_score), 0.0, 1.0)

func auto_solve_step() -> void:
	for y in grid_size:
		for x in grid_size:
			if float(lit_state[y][x]) > 0.1:
				var fake := InputEventMouseButton.new()
				fake.button_index = MOUSE_BUTTON_LEFT
				fake.pressed = true
				_on_cell_click(fake, x, y)
				return

func ai_step() -> void:
	auto_solve_step()
