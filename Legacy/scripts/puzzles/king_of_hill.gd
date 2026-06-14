extends PuzzleBase

# Puzzle King of the Hill — claim territory by clicking cells adjacent to
# your current claim. Solve when N% of the grid is yours. Each setup picks
# a different starting cell + a different number of rival cells that you
# must capture before declaring victory.

const CELL_PX: float = 64.0
const PADDING: float = 16.0

const FLOOR_TEX: String = "res://assets/textures/floors/circuit_floor.png"
const CLAIM_TEX: String = "res://assets/sprites/items/claim_crystal.png"
const RIVAL_TEX: String = "res://assets/sprites/items/rival_core.png"

var grid_size: int = 7
var grid: Array = []         # 2D bool: claimed
var blocked: Array = []      # 2D bool: rival-controlled (cannot claim)
var cell_nodes: Array = []
var claim_threshold: int = 0
var claim_count: int = 0
var start_pos: Vector2i = Vector2i.ZERO

var _floor_tex: Texture2D = null
var _claim_tex: Texture2D = null
var _rival_tex: Texture2D = null

func _load_tex(path: String) -> Texture2D:
	if ResourceLoader.exists(path):
		return load(path) as Texture2D
	return null

func setup(d: int = 1) -> void:
	super.setup(d)
	grid_size = clampi(6 + (d - 1) / 2, 6, 8)
	_floor_tex = _load_tex(FLOOR_TEX)
	_claim_tex = _load_tex(CLAIM_TEX)
	_rival_tex = _load_tex(RIVAL_TEX)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	grid.clear()
	blocked.clear()
	cell_nodes.clear()
	claim_count = 0

	var board_w := CELL_PX * grid_size + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_w)
	size = Vector2(board_w, board_w)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	for y in grid_size:
		var row: Array = []
		var brow: Array = []
		var row_nodes: Array = []
		for x in grid_size:
			row.append(false)
			brow.append(false)
			row_nodes.append(_make_cell(x, y))
		grid.append(row)
		blocked.append(brow)
		cell_nodes.append(row_nodes)

	# Pick a RANDOM starting cell anywhere on the board (not just center).
	start_pos = Vector2i(randi() % grid_size, randi() % grid_size)
	_claim(start_pos.x, start_pos.y)
	# Sometimes pre-claim a small starting cluster.
	if randf() < 0.4:
		for d: Vector2i in [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)]:
			var nx: int = start_pos.x + d.x
			var ny: int = start_pos.y + d.y
			if nx >= 0 and ny >= 0 and nx < grid_size and ny < grid_size:
				if randf() < 0.5:
					_claim(nx, ny)

	# Scatter a few "rival" blocked cells the player has to route around.
	var blocker_count: int = (grid_size * grid_size) / 10 + difficulty
	var safety: int = 40
	while blocker_count > 0 and safety > 0:
		safety -= 1
		var bx: int = randi() % grid_size
		var by: int = randi() % grid_size
		if Vector2i(bx, by) == start_pos:
			continue
		if absi(bx - start_pos.x) + absi(by - start_pos.y) <= 1:
			continue  # don't smother the start
		if bool(grid[by][bx]) or bool(blocked[by][bx]):
			continue
		blocked[by][bx] = true
		blocker_count -= 1

	var total: int = grid_size * grid_size
	var open: int = total
	for y in grid_size:
		for x in grid_size:
			if bool(blocked[y][x]):
				open -= 1
	claim_threshold = int(float(open) * randf_range(0.50, 0.65))
	claim_threshold = clampi(claim_threshold, 8, open)

	_refresh_all()

func _make_cell(x: int, y: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 4.0, CELL_PX - 4.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)
	# child 0: background tint + fallback color
	var rect := ColorRect.new()
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(rect)
	rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 1: floor surface texture
	var terrain := TextureRect.new()
	terrain.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	terrain.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	terrain.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(terrain)
	terrain.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 2: claim crystal / rival core sprite
	var entity := TextureRect.new()
	entity.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	entity.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	entity.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(entity)
	entity.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	return cell

func _claim(x: int, y: int) -> void:
	if x < 0 or y < 0 or x >= grid_size or y >= grid_size:
		return
	if bool(grid[y][x]) or bool(blocked[y][x]):
		return
	grid[y][x] = true
	claim_count += 1

func _is_adjacent_to_claimed(x: int, y: int) -> bool:
	for d: Vector2i in [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)]:
		var nx: int = x + d.x
		var ny: int = y + d.y
		if nx < 0 or ny < 0 or nx >= grid_size or ny >= grid_size:
			continue
		if bool(grid[ny][nx]):
			return true
	return false

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if bool(grid[y][x]) or bool(blocked[y][x]):
		return
	if not _is_adjacent_to_claimed(x, y):
		return
	_claim(x, y)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -10.0, 0.05)
	piece_placed.emit()
	_refresh_all()
	if claim_count >= claim_threshold:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(95 + difficulty * 35)

func _refresh_all() -> void:
	for y in grid_size:
		for x in grid_size:
			var node: Control = cell_nodes[y][x]
			if not is_instance_valid(node):
				continue
			var rect := node.get_child(0) as ColorRect
			var terrain := node.get_child(1) as TextureRect
			var entity := node.get_child(2) as TextureRect
			if rect == null or terrain == null or entity == null:
				continue
			terrain.texture = _floor_tex
			entity.texture = null
			if bool(blocked[y][x]):
				rect.color = Color(0.45, 0.10, 0.18, 0.92)
				terrain.modulate = Color(0.95, 0.55, 0.60, 1.0)
				entity.texture = _rival_tex
			elif bool(grid[y][x]):
				rect.color = Color(1.0, 0.54, 0.12, 0.95)
				terrain.modulate = Color(1.0, 0.85, 0.65, 1.0)
				entity.texture = _claim_tex
			elif _is_adjacent_to_claimed(x, y):
				rect.color = Color(1.0, 0.54, 0.12, 0.30)
				terrain.modulate = Color(1.2, 0.95, 0.7, 1.0)
			else:
				rect.color = Color(0.05, 0.08, 0.16, 0.95)
				terrain.modulate = Color(0.85, 0.88, 0.98, 1.0)

func get_progress() -> float:
	if claim_threshold <= 0:
		return 0.0
	return clampf(float(claim_count) / float(claim_threshold), 0.0, 1.0)

func auto_solve_step() -> void:
	var candidates: Array = []
	for y in grid_size:
		for x in grid_size:
			if not bool(grid[y][x]) and not bool(blocked[y][x]) and _is_adjacent_to_claimed(x, y):
				candidates.append(Vector2i(x, y))
	if candidates.is_empty():
		return
	var pick: Vector2i = candidates[randi() % candidates.size()]
	var fake := InputEventMouseButton.new()
	fake.button_index = MOUSE_BUTTON_LEFT
	fake.pressed = true
	_on_cell_click(fake, pick.x, pick.y)

func ai_step() -> void:
	auto_solve_step()
