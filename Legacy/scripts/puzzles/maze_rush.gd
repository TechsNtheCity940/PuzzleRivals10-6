extends PuzzleBase

# Maze Rush — navigate from top-left to bottom-right. Click an adjacent
# empty cell to walk into it. Walls block. Solve when goal reached.
#
# The maze generator GUARANTEES a path from start to goal (random walk +
# extra carved corridors) and then peppers walls into non-path cells.

const CELL_PX: float = 70.0
const PADDING: float = 16.0
const WALL: int = 1
const FLOOR: int = 0

const WALL_TEX: String = "res://assets/textures/walls/maze_wall.png"
const FLOOR_TEX: String = "res://assets/textures/floors/circuit_floor.png"
const PLAYER_TEX: String = "res://assets/sprites/effects/player_orb.png"
const GOAL_TEX: String = "res://assets/sprites/effects/exit_portal.png"

var grid_size: int = 7
var maze: Array = []
var cell_nodes: Array = []
var player_pos: Vector2i = Vector2i.ZERO
var goal_pos: Vector2i = Vector2i.ZERO

var _wall_tex: Texture2D = null
var _floor_tex: Texture2D = null
var _player_tex: Texture2D = null
var _goal_tex: Texture2D = null

func _load_tex(path: String) -> Texture2D:
	if ResourceLoader.exists(path):
		return load(path) as Texture2D
	return null

func setup(d: int = 1) -> void:
	super.setup(d)
	grid_size = clampi(6 + d - 1, 5, 8)
	_wall_tex = _load_tex(WALL_TEX)
	_floor_tex = _load_tex(FLOOR_TEX)
	_player_tex = _load_tex(PLAYER_TEX)
	_goal_tex = _load_tex(GOAL_TEX)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	maze.clear()
	cell_nodes.clear()

	var board_w := CELL_PX * grid_size + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_w)
	size = Vector2(board_w, board_w)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Step 1: start with all walls.
	for y in grid_size:
		var row: Array = []
		for x in grid_size:
			row.append(WALL)
		maze.append(row)

	# Step 2: random pick of start + goal corners. Randomise to give layout variety.
	var corner_choice: int = randi() % 4
	match corner_choice:
		0:
			player_pos = Vector2i(0, 0)
			goal_pos = Vector2i(grid_size - 1, grid_size - 1)
		1:
			player_pos = Vector2i(grid_size - 1, 0)
			goal_pos = Vector2i(0, grid_size - 1)
		2:
			player_pos = Vector2i(0, grid_size - 1)
			goal_pos = Vector2i(grid_size - 1, 0)
		_:
			player_pos = Vector2i(grid_size - 1, grid_size - 1)
			goal_pos = Vector2i(0, 0)

	# Step 3: GUARANTEE a path with a random walk from start to goal.
	var x: int = player_pos.x
	var y: int = player_pos.y
	maze[y][x] = FLOOR
	var safety: int = grid_size * grid_size * 4
	while (x != goal_pos.x or y != goal_pos.y) and safety > 0:
		safety -= 1
		# Bias movement toward the goal but allow some wandering.
		var dx: int = signi(goal_pos.x - x)
		var dy: int = signi(goal_pos.y - y)
		# 60% biased step, 40% random adjacent step
		var move_x: bool = randf() < 0.5
		if randf() < 0.4:
			# Random direction step
			move_x = randi() % 2 == 0
			dx = (randi() % 3) - 1
			dy = (randi() % 3) - 1
		var nx: int = x
		var ny: int = y
		if move_x and dx != 0:
			nx = clampi(x + dx, 0, grid_size - 1)
		elif dy != 0:
			ny = clampi(y + dy, 0, grid_size - 1)
		elif dx != 0:
			nx = clampi(x + dx, 0, grid_size - 1)
		if nx == x and ny == y:
			# No progress — force a step toward goal.
			if dx != 0:
				nx = clampi(x + dx, 0, grid_size - 1)
			elif dy != 0:
				ny = clampi(y + dy, 0, grid_size - 1)
		x = nx
		y = ny
		maze[y][x] = FLOOR
	maze[goal_pos.y][goal_pos.x] = FLOOR

	# Step 4: carve extra random floor pockets so it doesn't look like one corridor.
	var extras: int = grid_size * grid_size / 3
	for _i in extras:
		var rx: int = randi() % grid_size
		var ry: int = randi() % grid_size
		if Vector2i(rx, ry) != player_pos and Vector2i(rx, ry) != goal_pos:
			if randf() < 0.5:
				maze[ry][rx] = FLOOR

	# Step 5: build cells.
	for cy in grid_size:
		var row_nodes: Array = []
		for cx in grid_size:
			row_nodes.append(_make_cell(cx, cy))
		cell_nodes.append(row_nodes)
	_refresh_all_cells()

func _make_cell(x: int, y: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 4.0, CELL_PX - 4.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)
	# child 0: background tint (also the fallback when textures are missing)
	var rect := ColorRect.new()
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(rect)
	rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 1: surface texture (floor / wall) filling the cell
	var terrain := TextureRect.new()
	terrain.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	terrain.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	terrain.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(terrain)
	terrain.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# child 2: entity sprite (player orb / exit portal), centered on top
	var entity := TextureRect.new()
	entity.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	entity.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	entity.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(entity)
	entity.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	return cell

func _refresh_cell(x: int, y: int) -> void:
	var node: Control = cell_nodes[y][x]
	if not is_instance_valid(node):
		return
	var rect := node.get_child(0) as ColorRect
	var terrain := node.get_child(1) as TextureRect
	var entity := node.get_child(2) as TextureRect
	if rect == null or terrain == null or entity == null:
		return
	var v: int = maze[y][x]
	var is_player: bool = Vector2i(x, y) == player_pos
	var is_goal: bool = Vector2i(x, y) == goal_pos
	entity.texture = null
	if v == WALL:
		rect.color = Color(0.10, 0.13, 0.22, 1.0)
		terrain.texture = _wall_tex
		terrain.modulate = Color(1.0, 1.0, 1.0, 1.0)
	else:
		terrain.texture = _floor_tex
		if is_player:
			rect.color = Color(0.0, 0.90, 1.0, 0.95)
			terrain.modulate = Color(1.0, 1.0, 1.0, 1.0)
			entity.texture = _player_tex
		elif is_goal:
			rect.color = Color(0.61, 1.0, 0.18, 0.95)
			terrain.modulate = Color(1.0, 1.0, 1.0, 1.0)
			entity.texture = _goal_tex
		elif _is_adjacent_to_player(x, y):
			rect.color = Color(0.0, 0.90, 1.0, 0.30)
			terrain.modulate = Color(0.55, 1.0, 1.15, 1.0)
		else:
			rect.color = Color(0.05, 0.08, 0.16, 0.95)
			terrain.modulate = Color(0.82, 0.86, 0.96, 1.0)

func _refresh_all_cells() -> void:
	for y in grid_size:
		for x in grid_size:
			_refresh_cell(x, y)

func _is_adjacent_to_player(x: int, y: int) -> bool:
	return absi(x - player_pos.x) + absi(y - player_pos.y) == 1

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if not _is_adjacent_to_player(x, y):
		return
	if int(maze[y][x]) == WALL:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -12.0, 0.2)
		return
	player_pos = Vector2i(x, y)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -10.0, 0.1)
	piece_placed.emit()
	_refresh_all_cells()
	if player_pos == goal_pos:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(100 + difficulty * 40)

func get_progress() -> float:
	var total: int = absi(goal_pos.x - 0) + absi(goal_pos.y - 0) + grid_size
	if total <= 0:
		return 0.0
	# Use BFS distance heuristic — Manhattan distance to goal vs starting Manhattan.
	var start_dist: int = absi(goal_pos.x) + absi(goal_pos.y)
	if start_dist <= 0:
		return 1.0
	var cur_dist: int = absi(goal_pos.x - player_pos.x) + absi(goal_pos.y - player_pos.y)
	return clampf(1.0 - float(cur_dist) / float(start_dist), 0.0, 1.0)

func auto_solve_step() -> void:
	# BFS to goal, take first step of the shortest path.
	var queue: Array = [player_pos]
	var came_from: Dictionary = { player_pos: player_pos }
	var found: bool = false
	while not queue.is_empty():
		var p: Vector2i = queue.pop_front()
		if p == goal_pos:
			found = true
			break
		for d: Vector2i in [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)]:
			var nx: int = p.x + d.x
			var ny: int = p.y + d.y
			if nx < 0 or ny < 0 or nx >= grid_size or ny >= grid_size:
				continue
			if int(maze[ny][nx]) == WALL:
				continue
			var np := Vector2i(nx, ny)
			if came_from.has(np):
				continue
			came_from[np] = p
			queue.append(np)
	if not found:
		return
	# Walk back from goal to find first step.
	var step: Vector2i = goal_pos
	while came_from.get(step, step) != player_pos and came_from.get(step, step) != step:
		step = came_from[step]
	var fake := InputEventMouseButton.new()
	fake.button_index = MOUSE_BUTTON_LEFT
	fake.pressed = true
	_on_cell_click(fake, step.x, step.y)

func ai_step() -> void:
	auto_solve_step()
