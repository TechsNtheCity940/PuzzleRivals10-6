extends PuzzleBase

# Combo Rush — N numbered cells scattered in a grid. Click them in
# ascending order. A wrong click resets your run back to 1.

const CELL_PX: float = 92.0
const PADDING: float = 16.0
const GRID_SIZE: int = 5

const NODE_TEX: String = "res://assets/sprites/items/combo_node.png"

const TINT_UNCLAIMED: Color = Color(1.0, 1.0, 1.0, 1.0)
const TINT_CLAIMED: Color = Color(0.62, 1.45, 0.55, 1.0)
const BG_UNCLAIMED: Color = Color(0.0, 0.90, 1.0, 0.85)
const BG_CLAIMED: Color = Color(0.61, 1.0, 0.18, 0.95)

var sequence_length: int = 6  # how many numbers (1..N)
var current_target: int = 1
var cells: Array = []  # Array of { control, number, claimed }

var _node_tex: Texture2D = null

func _load_tex(path: String) -> Texture2D:
	if ResourceLoader.exists(path):
		return load(path) as Texture2D
	return null

func setup(d: int = 1) -> void:
	super.setup(d)
	sequence_length = clampi(5 + d, 5, 10)
	_node_tex = _load_tex(NODE_TEX)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	cells.clear()
	current_target = 1

	var board_w: float = CELL_PX * GRID_SIZE + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_w)
	size = Vector2(board_w, board_w)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Pick random positions for the sequence
	var slots: Array = []
	for y in GRID_SIZE:
		for x in GRID_SIZE:
			slots.append(Vector2i(x, y))
	slots.shuffle()
	for i in sequence_length:
		var pos: Vector2i = slots[i]
		var cell := _make_cell(pos.x, pos.y, i + 1)
		cells.append({
			"control": cell,
			"number": i + 1,
			"claimed": false,
			"x": pos.x,
			"y": pos.y,
		})

func _make_cell(x: int, y: int, n: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 8.0, CELL_PX - 8.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)

	# child 0: background color (fallback when sprite missing)
	var rect := ColorRect.new()
	rect.color = BG_UNCLAIMED
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(rect)
	rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# child 1: glossy hexagonal tech-button sprite
	var node_tex := TextureRect.new()
	node_tex.texture = _node_tex
	node_tex.modulate = TINT_UNCLAIMED
	node_tex.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	node_tex.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	node_tex.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(node_tex)
	node_tex.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# child 2: the number, drawn on top of the button
	var label := Label.new()
	label.text = str(n)
	label.add_theme_font_size_override("font_size", 40)
	label.add_theme_color_override("font_color", Color(0.95, 1.0, 1.0))
	label.add_theme_color_override("font_outline_color", Color(0, 0, 0, 0.85))
	label.add_theme_constant_override("outline_size", 6)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(label)
	label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, n))
	return cell

func _on_cell_click(event: InputEvent, n: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	# Already claimed cells just bounce
	for entry in cells:
		if int(entry.number) == n and bool(entry.claimed):
			return
	if n == current_target:
		_mark_claimed(n, true)
		current_target += 1
		AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -8.0)
		piece_placed.emit()
		if current_target > sequence_length:
			AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
			_emit_solved(80 + difficulty * 35)
	else:
		# Reset
		for entry in cells:
			_mark_claimed(int(entry.number), false)
		current_target = 1
		AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -8.0, 0.2)

func _mark_claimed(n: int, claimed: bool) -> void:
	for entry in cells:
		if int(entry.number) == n:
			entry.claimed = claimed
			var node := entry.control as Control
			if is_instance_valid(node):
				var rect := node.get_child(0) as ColorRect
				if rect != null:
					rect.color = BG_CLAIMED if claimed else BG_UNCLAIMED
				var node_tex := node.get_child(1) as TextureRect
				if node_tex != null:
					node_tex.modulate = TINT_CLAIMED if claimed else TINT_UNCLAIMED
			return

func get_progress() -> float:
	return float(current_target - 1) / float(sequence_length)

func auto_solve_step() -> void:
	for entry in cells:
		if int(entry.number) == current_target:
			var fake := InputEventMouseButton.new()
			fake.button_index = MOUSE_BUTTON_LEFT
			fake.pressed = true
			_on_cell_click(fake, current_target)
			return

func ai_step() -> void:
	auto_solve_step()
