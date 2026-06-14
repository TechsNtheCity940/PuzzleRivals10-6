extends PuzzleBase

# Crystal Rush — clear pairs of adjacent same-color crystals to collect
# them. Click any two TOUCHING crystals; if their colors match, both burst
# and you score. Solve when the score target is reached. Generator pre-seeds
# adjacent same-color clusters so plenty of matches exist.

const COLORS: Array = [
	Color(1.0, 0.18, 0.78),
	Color(0.0, 0.90, 1.0),
	Color(0.61, 1.0, 0.18),
	Color(1.0, 0.54, 0.12),
	Color(0.78, 0.35, 1.0),
]

const CELL_PX: float = 88.0
const PADDING: float = 16.0
const HEADER_H: float = 60.0

var grid_w: int = 6
var grid_h: int = 5
var num_colors: int = 4
var cells: Array = []        # 2D color_idx or -1
var cell_nodes: Array = []
var target_score: int = 12
var current_score: int = 0
var pending_pos: Vector2i = Vector2i(-1, -1)
var header_label: Label = null

func setup(d: int = 1) -> void:
	super.setup(d)
	num_colors = clampi(3 + (d - 1) / 2, 3, 5)
	# Always at least 12 collected to solve; difficulty raises it modestly.
	target_score = clampi(12 + d * 2, 12, 22)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	cells.clear()
	cell_nodes.clear()
	current_score = 0
	pending_pos = Vector2i(-1, -1)

	var board_w := CELL_PX * grid_w + PADDING * 2.0
	var board_h := HEADER_H + CELL_PX * grid_h + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_h)
	size = Vector2(board_w, board_h)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	header_label = Label.new()
	header_label.add_theme_font_size_override("font_size", 26)
	header_label.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	header_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	header_label.add_theme_constant_override("outline_size", 5)
	header_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	header_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	header_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(header_label)
	header_label.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	header_label.offset_top = 6
	header_label.offset_bottom = HEADER_H - 6

	# Initial random fill.
	for y in grid_h:
		var row: Array = []
		for x in grid_w:
			row.append(randi() % num_colors)
		cells.append(row)
	# Smoothing — cells sometimes copy a neighbor's color, creating clusters.
	for _i in 2:
		for y in grid_h:
			for x in grid_w:
				if randi() % 3 == 0 and x > 0:
					cells[y][x] = cells[y][x - 1]
				elif randi() % 3 == 0 and y > 0:
					cells[y][x] = cells[y - 1][x]
	# Force at least target_score / 2 guaranteed matching pairs by
	# overwriting random adjacent cells.
	var guaranteed_pairs: int = (target_score / 2) + 2
	while guaranteed_pairs > 0:
		var x: int = randi() % (grid_w - 1)
		var y: int = randi() % grid_h
		var c: int = randi() % num_colors
		cells[y][x] = c
		cells[y][x + 1] = c
		guaranteed_pairs -= 1

	for y in grid_h:
		var row_nodes: Array = []
		for x in grid_w:
			row_nodes.append(_make_cell(x, y, cells[y][x]))
		cell_nodes.append(row_nodes)

	_refresh_header()

func _refresh_header() -> void:
	if is_instance_valid(header_label):
		header_label.text = "COLLECT CRYSTALS  %d / %d" % [current_score, target_score]

func _make_cell(x: int, y: int, color_idx: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 8.0, CELL_PX - 8.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, HEADER_H + PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)
	var draw := _CrystalDraw.new()
	draw.tint = COLORS[color_idx]
	draw.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(draw)
	draw.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	cell.set_meta("color_idx", color_idx)
	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	return cell

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if int(cells[y][x]) < 0:
		return
	# Clicking the same cell twice cancels the selection.
	if pending_pos == Vector2i(x, y):
		pending_pos = Vector2i(-1, -1)
		_set_highlight(x, y, false)
		return
	if pending_pos == Vector2i(-1, -1):
		# First selection.
		pending_pos = Vector2i(x, y)
		_set_highlight(x, y, true)
		AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -12.0, 0.05)
		return
	# Second selection — must be adjacent to pending.
	var dx: int = absi(x - pending_pos.x)
	var dy: int = absi(y - pending_pos.y)
	var prev: Vector2i = pending_pos
	_set_highlight(prev.x, prev.y, false)
	pending_pos = Vector2i(-1, -1)
	if dx + dy != 1:
		# Not adjacent — start a new selection on this cell instead.
		pending_pos = Vector2i(x, y)
		_set_highlight(x, y, true)
		return
	if int(cells[y][x]) != int(cells[prev.y][prev.x]):
		# Wrong colors — flash and reset.
		_flash_cell(prev.x, prev.y, Color(1.0, 0.3, 0.3))
		_flash_cell(x, y, Color(1.0, 0.3, 0.3))
		AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -10.0, 0.2)
		return
	# Match! Burst both crystals.
	current_score += 2
	piece_placed.emit()
	AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -8.0)
	_burst_cell(prev.x, prev.y)
	_burst_cell(x, y)
	_refresh_header()
	if current_score >= target_score:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(95 + difficulty * 35)

func _set_highlight(x: int, y: int, on: bool) -> void:
	var node: Control = cell_nodes[y][x]
	if not is_instance_valid(node):
		return
	node.modulate = Color(1.2, 1.2, 1.2) if on else Color(1, 1, 1)

func _flash_cell(x: int, y: int, color: Color) -> void:
	var node: Control = cell_nodes[y][x]
	if not is_instance_valid(node):
		return
	node.modulate = color
	var tw := node.create_tween()
	tw.tween_property(node, "modulate", Color(1, 1, 1), 0.35)

func _burst_cell(x: int, y: int) -> void:
	cells[y][x] = -1
	var node: Control = cell_nodes[y][x]
	if not is_instance_valid(node):
		return
	node.pivot_offset = node.size / 2.0
	var tw := node.create_tween().set_parallel(true)
	tw.tween_property(node, "scale", Vector2(0.05, 0.05), 0.25).set_trans(Tween.TRANS_BACK)
	tw.tween_property(node, "modulate:a", 0.0, 0.20)
	tw.chain().tween_callback(func():
		if is_instance_valid(node):
			node.queue_free()
	)

func get_progress() -> float:
	return clampf(float(current_score) / float(target_score), 0.0, 1.0)

func auto_solve_step() -> void:
	# Find any matching adjacent pair and click both.
	for y in grid_h:
		for x in grid_w:
			var c: int = int(cells[y][x])
			if c < 0:
				continue
			# Check right neighbor.
			if x + 1 < grid_w and int(cells[y][x + 1]) == c:
				_dispatch_click(x, y)
				_dispatch_click(x + 1, y)
				return
			# Check bottom neighbor.
			if y + 1 < grid_h and int(cells[y + 1][x]) == c:
				_dispatch_click(x, y)
				_dispatch_click(x, y + 1)
				return

func _dispatch_click(x: int, y: int) -> void:
	var fake := InputEventMouseButton.new()
	fake.button_index = MOUSE_BUTTON_LEFT
	fake.pressed = true
	_on_cell_click(fake, x, y)

func ai_step() -> void:
	auto_solve_step()


class _CrystalDraw extends Control:
	const CRYSTAL_PATH := "res://assets/sprites/items/crystal_gem.png"
	var tint: Color = Color.WHITE:
		set(value):
			tint = value
			if _tex_rect != null and is_instance_valid(_tex_rect):
				_tex_rect.modulate = value
	var _tex_rect: TextureRect = null
	var _halo: ColorRect = null

	func _ready() -> void:
		# Soft colored halo behind the gem so the tint reads at a glance.
		_halo = ColorRect.new()
		_halo.mouse_filter = Control.MOUSE_FILTER_IGNORE
		_halo.color = Color(tint.r, tint.g, tint.b, 0.0)
		add_child(_halo)
		_halo.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		_halo.offset_left = 8
		_halo.offset_top = 8
		_halo.offset_right = -8
		_halo.offset_bottom = -8

		_tex_rect = TextureRect.new()
		_tex_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		_tex_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		_tex_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
		if ResourceLoader.exists(CRYSTAL_PATH):
			_tex_rect.texture = load(CRYSTAL_PATH)
		_tex_rect.modulate = tint
		add_child(_tex_rect)
		_tex_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	func _draw() -> void:
		# Procedural glow behind the gem so colored crystals stand out
		# even when the texture is white.
		var center := size / 2.0
		var radius: float = minf(size.x, size.y) * 0.48
		draw_circle(center, radius * 1.05, Color(tint.r, tint.g, tint.b, 0.28))
