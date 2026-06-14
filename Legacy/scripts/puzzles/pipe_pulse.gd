extends PuzzleBase

# Pipe Pulse — rotate every pipe tile back to its solved orientation.
# The grid is generated solved, then each tile is scrambled by a random
# multiple of 90°. Player clicks tiles to rotate them clockwise 90°.
# Solved when every tile is back to rotation 0.

const SHAPE_STRAIGHT := 0  # horizontal line (rotates to vertical at 90°)
const SHAPE_CURVE := 1     # corner connecting right+down (rotates around)
const SHAPE_TJUNCT := 2    # T-junction connecting three sides

const PIPE_COLOR := Color(0.0, 0.90, 1.0)
const PIPE_GLOW := Color(1.0, 1.0, 1.0, 0.35)
const TILE_BG := Color(0.06, 0.08, 0.18, 0.92)
const TILE_BG_SOLVED := Color(0.08, 0.18, 0.10, 0.95)
const TILE_BORDER := Color(0.18, 0.30, 0.55)
const TILE_BORDER_HOT := Color(0.0, 0.90, 1.0)

var grid_size: int = 4
var tile_px: float = 110.0
var tiles: Array = []  # Array of { control, shape, rotation, target_rotation }
var _correct_count: int = 0
var _total_tiles: int = 0
var _bg_panel: PanelContainer = null

func setup(d: int = 1) -> void:
	super.setup(d)
	grid_size = clamp(3 + d - 1, 3, 5)
	# Slightly shrink tiles as the grid grows so it fits the play area.
	tile_px = 130.0 if grid_size <= 3 else (110.0 if grid_size == 4 else 92.0)
	_build_grid()

func _build_grid() -> void:
	for c in get_children():
		c.queue_free()
	tiles.clear()
	_correct_count = 0
	_total_tiles = grid_size * grid_size

	var board_size := tile_px * float(grid_size) + 32.0
	custom_minimum_size = Vector2(board_size, board_size)
	size = Vector2(board_size, board_size)

	# Frame
	_bg_panel = PanelContainer.new()
	_bg_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var sb := StyleBoxFlat.new()
	sb.bg_color = Color(0.03, 0.05, 0.13, 0.85)
	sb.border_color = Color(0.0, 0.90, 1.0, 0.6)
	sb.set_border_width_all(3)
	sb.set_corner_radius_all(12)
	sb.shadow_color = Color(0.0, 0.90, 1.0, 0.25)
	sb.shadow_size = 12
	_bg_panel.add_theme_stylebox_override("panel", sb)
	add_child(_bg_panel)
	_bg_panel.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	for row in grid_size:
		for col in grid_size:
			var shape := SHAPE_STRAIGHT
			var roll := randi() % 100
			if roll < 50:
				shape = SHAPE_STRAIGHT
			elif roll < 85:
				shape = SHAPE_CURVE
			else:
				shape = SHAPE_TJUNCT

			# Random scrambled rotation (1..3 quarter-turns from solved).
			var scrambled_rot := (1 + randi() % 3) * 90
			var control := _make_tile(row, col, shape, scrambled_rot)
			add_child(control)
			control.position = Vector2(16 + col * tile_px, 16 + row * tile_px)
			tiles.append({
				"control": control,
				"shape": shape,
				"rotation": scrambled_rot,
				"target_rotation": 0,
			})

func _make_tile(row: int, col: int, shape: int, rotation_deg: int) -> Control:
	var holder := Control.new()
	holder.custom_minimum_size = Vector2(tile_px - 8.0, tile_px - 8.0)
	holder.size = holder.custom_minimum_size
	holder.mouse_filter = Control.MOUSE_FILTER_STOP

	# Background panel (clickable)
	var bg := ColorRect.new()
	bg.color = TILE_BG
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Pipe drawer
	var pipe := _PipeDraw.new()
	pipe.shape = shape
	pipe.rotation_deg = float(rotation_deg)
	pipe.mouse_filter = Control.MOUSE_FILTER_IGNORE
	pipe.size = holder.size
	holder.add_child(pipe)
	pipe.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Click handling
	holder.gui_input.connect(func(event: InputEvent): _on_tile_input(event, row, col))
	holder.mouse_entered.connect(func(): _on_tile_hover(row, col, true))
	holder.mouse_exited.connect(func(): _on_tile_hover(row, col, false))
	return holder

func _on_tile_hover(row: int, col: int, entered: bool) -> void:
	var idx := row * grid_size + col
	if idx < 0 or idx >= tiles.size():
		return
	var holder: Control = tiles[idx].control
	if not is_instance_valid(holder):
		return
	var bg := holder.get_child(0) as ColorRect
	if bg == null:
		return
	if int(tiles[idx].rotation) == int(tiles[idx].target_rotation):
		bg.color = TILE_BG_SOLVED
	else:
		bg.color = TILE_BG.lightened(0.10) if entered else TILE_BG

func _on_tile_input(event: InputEvent, row: int, col: int) -> void:
	if not input_enabled or _solved:
		return
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		_rotate_tile(row, col)

func _rotate_tile(row: int, col: int) -> void:
	var idx := row * grid_size + col
	if idx < 0 or idx >= tiles.size():
		return
	var entry: Dictionary = tiles[idx]
	var new_rot := int(entry.rotation) + 90
	if new_rot >= 360:
		new_rot = 0
	entry.rotation = new_rot
	tiles[idx] = entry
	var holder: Control = entry.control
	if not is_instance_valid(holder):
		return
	var pipe := holder.get_child(1) as _PipeDraw
	if pipe != null:
		# Animate the rotation visually
		var tw := holder.create_tween()
		tw.tween_property(pipe, "rotation_deg", float(new_rot), 0.12) \
			.set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -10.0, 0.05)
	piece_placed.emit()
	_check_solved()

func _check_solved() -> void:
	if _solved:
		return
	var correct := 0
	for entry in tiles:
		if int(entry.rotation) % 360 == int(entry.target_rotation):
			correct += 1
			# Highlight solved tiles
			var holder := entry.control as Control
			if is_instance_valid(holder):
				var bg := holder.get_child(0) as ColorRect
				if bg != null:
					bg.color = TILE_BG_SOLVED
		else:
			var holder2 := entry.control as Control
			if is_instance_valid(holder2):
				var bg2 := holder2.get_child(0) as ColorRect
				if bg2 != null and bg2.color != TILE_BG:
					bg2.color = TILE_BG
	_correct_count = correct
	if correct >= _total_tiles:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(100 + difficulty * 50)

func get_progress() -> float:
	if _total_tiles <= 0:
		return 0.0
	return float(_correct_count) / float(_total_tiles)

func auto_solve_step() -> void:
	# Pick a random un-rotated tile and rotate it correctly.
	var candidates: Array = []
	for i in tiles.size():
		if int(tiles[i].rotation) != int(tiles[i].target_rotation):
			candidates.append(i)
	if candidates.is_empty():
		return
	var idx: int = candidates[randi() % candidates.size()]
	var row: int = idx / grid_size
	var col: int = idx % grid_size
	# Rotate the tile until correct.
	for r in 4:
		if int(tiles[idx].rotation) == int(tiles[idx].target_rotation):
			break
		_rotate_tile(row, col)

func ai_step() -> void:
	# Same as auto_solve_step but used by AI bots.
	auto_solve_step()


# --- Pipe drawing primitive ---

class _PipeDraw extends Control:
	const TEX_PATHS: Array = [
		"res://assets/sprites/effects/pipe_straight_horizontal.png",
		"res://assets/sprites/effects/pipe_curve_topright.png",
		"res://assets/sprites/effects/pipe_tjunction_right.png",
	]

	var shape: int = 0:
		set(value):
			shape = value
			_refresh_texture()
	var rotation_deg: float = 0.0:
		set(value):
			rotation_deg = value
			if _tex_rect != null and is_instance_valid(_tex_rect):
				_tex_rect.rotation_degrees = value
				_tex_rect.pivot_offset = _tex_rect.size / 2.0

	var _tex_rect: TextureRect = null

	func _ready() -> void:
		_tex_rect = TextureRect.new()
		_tex_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		_tex_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		_tex_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
		add_child(_tex_rect)
		_tex_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		_tex_rect.pivot_offset = _tex_rect.size / 2.0
		_tex_rect.rotation_degrees = rotation_deg
		_refresh_texture()

	func _refresh_texture() -> void:
		if _tex_rect == null or not is_instance_valid(_tex_rect):
			return
		if shape >= 0 and shape < TEX_PATHS.size():
			var path: String = TEX_PATHS[shape]
			if ResourceLoader.exists(path):
				_tex_rect.texture = load(path)
