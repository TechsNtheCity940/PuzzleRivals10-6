extends PuzzleBase

# Rule Breaker — the rule changes every few seconds. Click cells matching
# the active rule color. Wrong clicks subtract a point. Solve at N score.

const COLORS: Array = [
	Color(1.0, 0.18, 0.78),
	Color(0.0, 0.90, 1.0),
	Color(0.61, 1.0, 0.18),
	Color(1.0, 0.54, 0.12),
]

const COLOR_NAMES: Array = ["MAGENTA", "CYAN", "LIME", "ORANGE"]

const CELL_PX: float = 100.0
const PADDING: float = 16.0
const RULE_INTERVAL: float = 3.5
const HEADER_HEIGHT: float = 80.0

const GEM_TEX: String = "res://assets/sprites/items/crystal_gem.png"

var grid_size: int = 4
var target_score: int = 8
var score: int = 0
var cells: Array = []        # 2D color_idx
var cell_nodes: Array = []
var current_rule: int = 0
var rule_timer: float = 0.0
var rule_label: Label = null

var _gem_tex: Texture2D = null

func _load_tex(path: String) -> Texture2D:
	if ResourceLoader.exists(path):
		return load(path) as Texture2D
	return null

func setup(d: int = 1) -> void:
	super.setup(d)
	target_score = clampi(8 + d * 2, 8, 16)
	_gem_tex = _load_tex(GEM_TEX)
	_build()
	set_process(true)

func _build() -> void:
	for c in get_children():
		c.queue_free()
	cells.clear()
	cell_nodes.clear()
	score = 0
	rule_timer = 0.0
	current_rule = randi() % COLORS.size()

	var board_w := CELL_PX * grid_size + PADDING * 2.0
	var board_h := board_w + HEADER_HEIGHT
	custom_minimum_size = Vector2(board_w, board_h)
	size = Vector2(board_w, board_h)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Header / rule banner
	rule_label = Label.new()
	rule_label.add_theme_font_size_override("font_size", 32)
	rule_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	rule_label.add_theme_constant_override("outline_size", 6)
	rule_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	rule_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	rule_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(rule_label)
	rule_label.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	rule_label.offset_top = 8
	rule_label.offset_bottom = HEADER_HEIGHT - 8

	for y in grid_size:
		var row_state: Array = []
		var row_nodes: Array = []
		for x in grid_size:
			var c_idx := randi() % COLORS.size()
			row_state.append(c_idx)
			row_nodes.append(_make_cell(x, y, c_idx))
		cells.append(row_state)
		cell_nodes.append(row_nodes)

	_refresh_rule()

func _make_cell(x: int, y: int, color_idx: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 8.0, CELL_PX - 8.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, HEADER_HEIGHT + PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)
	# child 0: dark socket / fallback color
	var rect := ColorRect.new()
	rect.color = (COLORS[color_idx] as Color).darkened(0.55)
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(rect)
	rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 1: faceted crystal gem, tinted to the cell's color
	var gem := TextureRect.new()
	gem.texture = _gem_tex
	gem.modulate = COLORS[color_idx]
	gem.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	gem.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	gem.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(gem)
	gem.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	cell.set_meta("color_idx", color_idx)
	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	return cell

func _refresh_rule() -> void:
	if rule_label != null and is_instance_valid(rule_label):
		rule_label.text = "CLICK %s (%d / %d)" % [COLOR_NAMES[current_rule], score, target_score]
		rule_label.add_theme_color_override("font_color", COLORS[current_rule])

func _process(delta: float) -> void:
	if _solved or not input_enabled:
		return
	rule_timer += delta
	if rule_timer >= RULE_INTERVAL:
		rule_timer = 0.0
		var prev := current_rule
		while current_rule == prev:
			current_rule = randi() % COLORS.size()
		_refresh_rule()

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	var c_idx: int = cells[y][x]
	if c_idx == current_rule:
		score += 1
		# Recolor the clicked cell to something else for variety
		var new_c := (c_idx + 1 + randi() % (COLORS.size() - 1)) % COLORS.size()
		cells[y][x] = new_c
		var node: Control = cell_nodes[y][x]
		if is_instance_valid(node):
			var rect := node.get_child(0) as ColorRect
			if rect != null:
				rect.color = (COLORS[new_c] as Color).darkened(0.55)
			var gem := node.get_child(1) as TextureRect
			if gem != null:
				gem.modulate = COLORS[new_c]
		AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -8.0, 0.1)
	else:
		score = maxi(0, score - 1)
		AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -10.0, 0.2)
	piece_placed.emit()
	_refresh_rule()
	if score >= target_score:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		set_process(false)
		_emit_solved(90 + difficulty * 40)

func get_progress() -> float:
	return clampf(float(score) / float(target_score), 0.0, 1.0)

func auto_solve_step() -> void:
	# Find a cell matching current rule.
	for y in grid_size:
		for x in grid_size:
			if int(cells[y][x]) == current_rule:
				var fake := InputEventMouseButton.new()
				fake.button_index = MOUSE_BUTTON_LEFT
				fake.pressed = true
				_on_cell_click(fake, x, y)
				return

func ai_step() -> void:
	auto_solve_step()
