extends PuzzleBase

# Collapse Grid — click a cell to vaporize it plus every same-color
# orthogonally-adjacent cell (flood fill). Solve when the remaining cell
# count drops below the threshold. The generator smooths cells into
# clusters so plenty of large flood groups exist — the threshold is always
# reachable within the time limit.

const COLORS: Array = [
	Color(1.0, 0.18, 0.78),
	Color(0.0, 0.90, 1.0),
	Color(0.61, 1.0, 0.18),
	Color(1.0, 0.54, 0.12),
	Color(0.78, 0.35, 1.0),
]

const CELL_PX: float = 78.0
const PADDING: float = 16.0

var grid_w: int = 6
var grid_h: int = 6
var num_colors: int = 4
var cells: Array = []        # 2D: cells[y][x] = color_idx or -1
var cell_nodes: Array = []
var threshold: int = 12

func setup(d: int = 1) -> void:
	super.setup(d)
	# Tighter color palette = more guaranteed clusters.
	num_colors = clampi(3 + (d - 1) / 2, 3, 5)
	grid_w = 6
	grid_h = 6
	# Solve when ~65% of the board is cleared.
	var total: int = grid_w * grid_h
	threshold = clampi(int(float(total) * 0.35), 8, total - 12)
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	cells.clear()
	cell_nodes.clear()

	var board_w := CELL_PX * grid_w + PADDING * 2.0
	var board_h := CELL_PX * grid_h + PADDING * 2.0
	custom_minimum_size = Vector2(board_w, board_h)
	size = Vector2(board_w, board_h)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Step 1 — random fill.
	for y in grid_h:
		var row: Array = []
		for x in grid_w:
			row.append(randi() % num_colors)
		cells.append(row)
	# Step 2 — cluster-smoothing passes so cells favour their neighbours.
	for _i in 3:
		for y in grid_h:
			for x in grid_w:
				if randi() % 3 == 0 and x > 0:
					cells[y][x] = cells[y][x - 1]
				elif randi() % 3 == 0 and y > 0:
					cells[y][x] = cells[y - 1][x]
	# Step 3 — seed a few guaranteed BIG clusters by stamping 2x2 blocks.
	var stamps: int = 4
	while stamps > 0:
		var sx: int = randi() % (grid_w - 1)
		var sy: int = randi() % (grid_h - 1)
		var c: int = randi() % num_colors
		cells[sy][sx] = c
		cells[sy][sx + 1] = c
		cells[sy + 1][sx] = c
		cells[sy + 1][sx + 1] = c
		stamps -= 1

	# Step 4 — instantiate cells.
	for y in grid_h:
		var row_nodes: Array = []
		for x in grid_w:
			row_nodes.append(_make_cell(x, y, cells[y][x]))
		cell_nodes.append(row_nodes)

func _make_cell(x: int, y: int, color_idx: int) -> Control:
	var cell := Control.new()
	cell.custom_minimum_size = Vector2(CELL_PX - 6.0, CELL_PX - 6.0)
	cell.size = cell.custom_minimum_size
	cell.position = Vector2(PADDING + x * CELL_PX, PADDING + y * CELL_PX)
	cell.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(cell)
	# Dark backing tile so the gem reads against the board.
	var backing := ColorRect.new()
	backing.color = COLORS[color_idx].darkened(0.7)
	backing.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cell.add_child(backing)
	backing.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# 3D crystal sprite tinted with the cell's color.
	const CRYSTAL_PATH := "res://assets/sprites/items/crystal_gem.png"
	if ResourceLoader.exists(CRYSTAL_PATH):
		var gem := TextureRect.new()
		gem.texture = load(CRYSTAL_PATH)
		gem.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		gem.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		gem.mouse_filter = Control.MOUSE_FILTER_IGNORE
		gem.modulate = COLORS[color_idx]
		cell.add_child(gem)
		gem.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		gem.offset_left = 4
		gem.offset_top = 4
		gem.offset_right = -4
		gem.offset_bottom = -4
	else:
		# Fallback to flat colored fill if the sprite isn't available.
		var inner := ColorRect.new()
		inner.color = COLORS[color_idx].lightened(0.3)
		inner.mouse_filter = Control.MOUSE_FILTER_IGNORE
		cell.add_child(inner)
		inner.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		inner.offset_left = 8
		inner.offset_top = 8
		inner.offset_right = -8
		inner.offset_bottom = -8
	cell.gui_input.connect(func(event: InputEvent): _on_cell_click(event, x, y))
	cell.set_meta("color_idx", color_idx)
	return cell

func _on_cell_click(event: InputEvent, x: int, y: int) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	var color_idx: int = cells[y][x]
	if color_idx < 0:
		return
	var removed: Array = _flood_collapse(x, y, color_idx)
	if removed.is_empty():
		return
	AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -10.0, 0.05)
	piece_placed.emit()
	for p in removed:
		var nx: int = int(p.x)
		var ny: int = int(p.y)
		cells[ny][nx] = -1
		var node: Control = cell_nodes[ny][nx]
		if is_instance_valid(node):
			var tw := node.create_tween().set_parallel(true)
			tw.tween_property(node, "scale", Vector2(0.0, 0.0), 0.2).set_trans(Tween.TRANS_BACK)
			tw.tween_property(node, "modulate:a", 0.0, 0.18)
			tw.chain().tween_callback(func():
				if is_instance_valid(node):
					node.queue_free()
			)
	_check_solved()

func _flood_collapse(sx: int, sy: int, color: int) -> Array:
	var found: Array = []
	var queue: Array = [Vector2i(sx, sy)]
	var seen: Dictionary = {}
	while not queue.is_empty():
		var p: Vector2i = queue.pop_back()
		var key: int = p.x * 1000 + p.y
		if seen.has(key):
			continue
		seen[key] = true
		if p.x < 0 or p.y < 0 or p.x >= grid_w or p.y >= grid_h:
			continue
		if int(cells[p.y][p.x]) != color:
			continue
		found.append(p)
		queue.append(Vector2i(p.x + 1, p.y))
		queue.append(Vector2i(p.x - 1, p.y))
		queue.append(Vector2i(p.x, p.y + 1))
		queue.append(Vector2i(p.x, p.y - 1))
	return found

func _remaining() -> int:
	var n: int = 0
	for row in cells:
		for c in row:
			if int(c) >= 0:
				n += 1
	return n

func _check_solved() -> void:
	if _remaining() <= threshold:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(90 + difficulty * 40)

func get_progress() -> float:
	var total: int = grid_w * grid_h
	var goal: int = total - threshold
	var cleared: int = total - _remaining()
	if goal <= 0:
		return 0.0
	return clampf(float(cleared) / float(goal), 0.0, 1.0)

func auto_solve_step() -> void:
	# Prefer cells that flood-collapse the largest group.
	var best_pick: Vector2i = Vector2i(-1, -1)
	var best_size: int = 0
	for y in grid_h:
		for x in grid_w:
			var c: int = int(cells[y][x])
			if c < 0:
				continue
			var size: int = _flood_size(x, y, c)
			if size > best_size:
				best_size = size
				best_pick = Vector2i(x, y)
	if best_pick.x < 0:
		return
	var fake := InputEventMouseButton.new()
	fake.button_index = MOUSE_BUTTON_LEFT
	fake.pressed = true
	_on_cell_click(fake, best_pick.x, best_pick.y)

func _flood_size(sx: int, sy: int, color: int) -> int:
	var queue: Array = [Vector2i(sx, sy)]
	var seen: Dictionary = {}
	var count: int = 0
	while not queue.is_empty():
		var p: Vector2i = queue.pop_back()
		var key: int = p.x * 1000 + p.y
		if seen.has(key):
			continue
		seen[key] = true
		if p.x < 0 or p.y < 0 or p.x >= grid_w or p.y >= grid_h:
			continue
		if int(cells[p.y][p.x]) != color:
			continue
		count += 1
		queue.append(Vector2i(p.x + 1, p.y))
		queue.append(Vector2i(p.x - 1, p.y))
		queue.append(Vector2i(p.x, p.y + 1))
		queue.append(Vector2i(p.x, p.y - 1))
	return count

func ai_step() -> void:
	auto_solve_step()
