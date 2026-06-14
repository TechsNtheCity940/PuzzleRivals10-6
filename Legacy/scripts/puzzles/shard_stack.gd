extends PuzzleBase

# Shard Stack — click each column to drop a shard there. Every round has
# random per-column target heights and random pre-stacked shards, so the
# layout is fresh each time. Solve when every column has reached its
# target height.

const COL_W: float = 96.0
const ROW_H: float = 56.0
const PADDING: float = 18.0

const SHARD_COLORS: Array = [
	Color(1.0, 0.18, 0.78),
	Color(0.0, 0.90, 1.0),
	Color(0.61, 1.0, 0.18),
	Color(1.0, 0.54, 0.12),
	Color(0.78, 0.35, 1.0),
]

var columns: int = 5
var max_height: int = 5
var target_heights: Array[int] = []
var stacks: Array[int] = []
var col_nodes: Array = []
var col_colors: Array[int] = []
var target_markers: Array = []

func setup(d: int = 1) -> void:
	super.setup(d)
	# Number of columns scales with difficulty (4-6).
	columns = clampi(4 + (d - 1) / 2, 4, 6)
	max_height = clampi(4 + d / 2, 4, 6)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	stacks.clear()
	col_nodes.clear()
	target_heights.clear()
	col_colors.clear()
	target_markers.clear()

	# Generate per-column targets (random, but always reachable from start).
	stacks.resize(columns)
	target_heights.resize(columns)
	col_colors.resize(columns)
	for i in columns:
		var start_count: int = randi() % maxi(2, max_height - 1)
		stacks[i] = start_count
		# Pick a target strictly greater than the start.
		var t: int = start_count + 1 + randi() % maxi(1, max_height - start_count - 1 + 1)
		target_heights[i] = clampi(t, start_count + 1, max_height)
		col_colors[i] = randi() % SHARD_COLORS.size()

	var board_w := COL_W * columns + PADDING * 2.0
	var board_h := ROW_H * max_height + PADDING * 2.0 + 70.0
	custom_minimum_size = Vector2(board_w, board_h)
	size = Vector2(board_w, board_h)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	for i in columns:
		var col := Control.new()
		var col_h := ROW_H * max_height + 12.0
		col.custom_minimum_size = Vector2(COL_W - 8.0, col_h)
		col.size = col.custom_minimum_size
		col.position = Vector2(PADDING + i * COL_W, PADDING)
		col.mouse_filter = Control.MOUSE_FILTER_STOP
		add_child(col)

		var col_bg := ColorRect.new()
		col_bg.color = Color(0.05, 0.08, 0.16, 0.95)
		col_bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
		col.add_child(col_bg)
		col_bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

		var border := ColorRect.new()
		border.color = Color(0.0, 0.90, 1.0, 0.40)
		border.mouse_filter = Control.MOUSE_FILTER_IGNORE
		col.add_child(border)
		border.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		border.offset_left = 3
		border.offset_top = 3
		border.offset_right = -3
		border.offset_bottom = -3

		var inner_bg := ColorRect.new()
		inner_bg.color = Color(0.05, 0.08, 0.16, 0.95)
		inner_bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
		col.add_child(inner_bg)
		inner_bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		inner_bg.offset_left = 6
		inner_bg.offset_top = 6
		inner_bg.offset_right = -6
		inner_bg.offset_bottom = -6

		# Target marker line — shows the player how high they need to stack.
		var target_y: float = col_h - 8.0 - target_heights[i] * ROW_H
		var marker := ColorRect.new()
		marker.color = Color(1.0, 0.95, 0.2, 0.7)
		marker.mouse_filter = Control.MOUSE_FILTER_IGNORE
		marker.custom_minimum_size = Vector2(col.size.x - 16.0, 3.0)
		marker.size = marker.custom_minimum_size
		marker.position = Vector2(8.0, target_y)
		col.add_child(marker)
		target_markers.append(marker)

		# Render any pre-stacked shards.
		for s in stacks[i]:
			_render_shard_at(col, i, s)

		col.gui_input.connect(func(event: InputEvent): _on_column_click(event, i))
		col_nodes.append(col)

	var lbl := Label.new()
	lbl.text = "Fill every column up to the yellow line."
	lbl.add_theme_font_size_override("font_size", 22)
	lbl.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	lbl.add_theme_constant_override("outline_size", 4)
	lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(lbl)
	lbl.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	lbl.offset_top = -60
	lbl.offset_bottom = -10

func _render_shard_at(col: Control, col_idx: int, row_from_bottom: int) -> void:
	var height: float = ROW_H - 6.0
	var bottom_y: float = col.size.y - 8.0 - row_from_bottom * ROW_H - height
	col.add_child(_build_shard_sprite(col_idx, height, Vector2(8.0, bottom_y)))

func _on_column_click(event: InputEvent, col: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if int(stacks[col]) >= int(target_heights[col]):
		return
	stacks[col] = int(stacks[col]) + 1
	AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -8.0, 0.05)
	piece_placed.emit()
	_drop_shard_visual(col, int(stacks[col]) - 1)
	_check_solved()

func _drop_shard_visual(col_idx: int, row_from_bottom: int) -> void:
	var col: Control = col_nodes[col_idx]
	if not is_instance_valid(col):
		return
	var height: float = ROW_H - 6.0
	var bottom_y: float = col.size.y - 8.0 - row_from_bottom * ROW_H - height
	var shard := _build_shard_sprite(col_idx, height, Vector2(8.0, 0.0))
	col.add_child(shard)
	var tw := shard.create_tween()
	tw.tween_property(shard, "position", Vector2(8.0, bottom_y), 0.25).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)

func _build_shard_sprite(col_idx: int, height: float, pos: Vector2) -> Control:
	const SHARD_PATH := "res://assets/sprites/items/crystal_shard.png"
	var container := Control.new()
	container.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.custom_minimum_size = Vector2(COL_W - 22.0, height)
	container.size = container.custom_minimum_size
	container.position = pos
	var tint: Color = SHARD_COLORS[col_colors[col_idx]]
	# Halo backing so the tint comes through clearly.
	var halo := ColorRect.new()
	halo.color = Color(tint.r, tint.g, tint.b, 0.30)
	halo.mouse_filter = Control.MOUSE_FILTER_IGNORE
	container.add_child(halo)
	halo.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	if ResourceLoader.exists(SHARD_PATH):
		var sprite := TextureRect.new()
		sprite.texture = load(SHARD_PATH)
		sprite.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		sprite.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		sprite.mouse_filter = Control.MOUSE_FILTER_IGNORE
		sprite.modulate = tint
		container.add_child(sprite)
		sprite.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	else:
		# Fallback flat shard if sprite is missing.
		var rect := ColorRect.new()
		rect.color = tint
		rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
		container.add_child(rect)
		rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	return container

func _check_solved() -> void:
	for i in columns:
		if int(stacks[i]) < int(target_heights[i]):
			return
	AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
	_emit_solved(90 + difficulty * 35)

func get_progress() -> float:
	var sum: int = 0
	var goal: int = 0
	for i in columns:
		sum += int(stacks[i])
		goal += int(target_heights[i])
	if goal <= 0:
		return 0.0
	return clampf(float(sum) / float(goal), 0.0, 1.0)

func auto_solve_step() -> void:
	for i in columns:
		if int(stacks[i]) < int(target_heights[i]):
			var fake := InputEventMouseButton.new()
			fake.button_index = MOUSE_BUTTON_LEFT
			fake.pressed = true
			_on_column_click(fake, i)
			return

func ai_step() -> void:
	auto_solve_step()
