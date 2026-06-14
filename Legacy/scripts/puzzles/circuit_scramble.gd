extends PuzzleBase

# Circuit Scramble — wire each source (left) to its matching sink (right).
# Sinks are scrambled in random vertical order. Click a source, then a sink.
# Mismatched pair flashes red and breaks. Solve = all wired correctly.

const WIRE_COLORS: Array = [
	Color(1.0, 0.18, 0.78),  # magenta
	Color(0.0, 0.90, 1.0),   # cyan
	Color(0.61, 1.0, 0.18),  # lime
	Color(1.0, 0.54, 0.12),  # orange
	Color(0.78, 0.35, 1.0),  # purple
	Color(1.0, 0.92, 0.18),  # yellow
]

var wire_count: int = 4
var sources: Array = []   # Array of nodes
var sinks: Array = []
var sink_color_index: Array = []  # color idx at each sink slot
var wires: Dictionary = {}        # color_idx -> { sink_idx, line }
var pending_source_idx: int = -1
var _solved_count: int = 0
var _wire_layer: Control = null
var _board_size: Vector2 = Vector2(720, 720)

func setup(d: int = 1) -> void:
	super.setup(d)
	wire_count = clamp(3 + d - 1, 3, WIRE_COLORS.size())
	_build_board()

func _build_board() -> void:
	for c in get_children():
		c.queue_free()
	sources.clear()
	sinks.clear()
	sink_color_index.clear()
	wires.clear()
	pending_source_idx = -1
	_solved_count = 0

	_board_size = Vector2(620.0, 90.0 * wire_count + 80.0)
	custom_minimum_size = _board_size
	size = _board_size

	# Frame
	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Border glow
	var border := _BorderGlow.new()
	border.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(border)
	border.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Wire layer (lines drawn here)
	_wire_layer = _WireLayer.new()
	_wire_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_wire_layer)
	_wire_layer.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Scramble sink color order
	sink_color_index.resize(wire_count)
	var indices := []
	for i in wire_count:
		indices.append(i)
	indices.shuffle()
	for i in wire_count:
		sink_color_index[i] = indices[i]

	# Sources on the left
	var top_margin := 50.0
	var spacing := 90.0
	for i in wire_count:
		var y := top_margin + i * spacing
		var node := _make_terminal(i, true, WIRE_COLORS[i])
		add_child(node)
		node.position = Vector2(50, y - 35)
		sources.append(node)

	# Sinks on the right (scrambled)
	for i in wire_count:
		var y := top_margin + i * spacing
		var color: Color = WIRE_COLORS[sink_color_index[i]]
		var node := _make_terminal(i, false, color)
		add_child(node)
		node.position = Vector2(_board_size.x - 50 - 70, y - 35)
		sinks.append(node)

func _make_terminal(idx: int, is_source: bool, color: Color) -> Control:
	var node := Control.new()
	node.custom_minimum_size = Vector2(70, 70)
	node.size = node.custom_minimum_size
	node.mouse_filter = Control.MOUSE_FILTER_STOP

	var dot := _TerminalDraw.new()
	dot.terminal_color = color
	dot.is_source = is_source
	dot.mouse_filter = Control.MOUSE_FILTER_IGNORE
	node.add_child(dot)
	dot.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	node.gui_input.connect(func(event: InputEvent): _on_terminal_input(event, idx, is_source))
	return node

func _on_terminal_input(event: InputEvent, idx: int, is_source: bool) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	if is_source:
		# Already wired? Disconnect.
		if wires.has(idx):
			_disconnect_source(idx)
		pending_source_idx = idx
		AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -12.0, 0.05)
		_redraw_wires()
	else:
		if pending_source_idx < 0:
			return
		var src_color_idx := pending_source_idx
		var sink_color_idx: int = sink_color_index[idx]
		# Disconnect any existing wire to this sink
		for c_idx in wires.keys():
			if int(wires[c_idx].sink_idx) == idx:
				_disconnect_source(c_idx)
				break
		wires[src_color_idx] = { "sink_idx": idx, "correct": src_color_idx == sink_color_idx }
		pending_source_idx = -1
		if src_color_idx == sink_color_idx:
			AudioManager.play_sfx(GameState.SFX_PUZZLE_PLACE, -8.0)
		else:
			AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -8.0, 0.1)
		piece_placed.emit()
		_redraw_wires()
		_check_solved()

func _disconnect_source(idx: int) -> void:
	if wires.has(idx):
		wires.erase(idx)
	_redraw_wires()

func _redraw_wires() -> void:
	if _wire_layer == null or not is_instance_valid(_wire_layer):
		return
	(_wire_layer as _WireLayer).wires = wires
	(_wire_layer as _WireLayer).source_positions = _source_positions()
	(_wire_layer as _WireLayer).sink_positions = _sink_positions()
	(_wire_layer as _WireLayer).pending_source_idx = pending_source_idx
	(_wire_layer as _WireLayer).wire_colors = WIRE_COLORS
	_wire_layer.queue_redraw()

func _source_positions() -> Array:
	var out: Array = []
	for s in sources:
		var c := s as Control
		if is_instance_valid(c):
			out.append(c.position + c.size / 2.0)
	return out

func _sink_positions() -> Array:
	var out: Array = []
	for s in sinks:
		var c := s as Control
		if is_instance_valid(c):
			out.append(c.position + c.size / 2.0)
	return out

func _check_solved() -> void:
	var correct := 0
	for color_idx in wires.keys():
		if bool(wires[color_idx].correct):
			correct += 1
	_solved_count = correct
	if correct >= wire_count:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(80 + difficulty * 40)

func get_progress() -> float:
	if wire_count <= 0:
		return 0.0
	return float(_solved_count) / float(wire_count)

func auto_solve_step() -> void:
	# Find a correct wire to add automatically.
	for src in wire_count:
		if wires.has(src) and bool(wires[src].correct):
			continue
		# Find the sink that matches this color
		for sink_idx in wire_count:
			if int(sink_color_index[sink_idx]) == src:
				wires[src] = { "sink_idx": sink_idx, "correct": true }
				_redraw_wires()
				_check_solved()
				return

func ai_step() -> void:
	auto_solve_step()


# --- Drawing primitives ---

class _TerminalDraw extends Control:
	var terminal_color: Color = Color.WHITE
	var is_source: bool = true

	func _draw() -> void:
		var center := size / 2.0
		var radius := minf(size.x, size.y) * 0.42
		# Glow
		draw_circle(center, radius * 1.5, Color(terminal_color.r, terminal_color.g, terminal_color.b, 0.18))
		draw_circle(center, radius * 1.15, Color(terminal_color.r, terminal_color.g, terminal_color.b, 0.35))
		# Ring
		draw_arc(center, radius, 0, TAU, 32, Color(0.05, 0.06, 0.12), radius * 0.18, true)
		draw_arc(center, radius * 0.92, 0, TAU, 32, terminal_color, radius * 0.18, true)
		# Inner dot
		draw_circle(center, radius * 0.45, terminal_color.lightened(0.4))
		draw_circle(center, radius * 0.2, Color.WHITE)


class _WireLayer extends Control:
	var wires: Dictionary = {}
	var source_positions: Array = []
	var sink_positions: Array = []
	var pending_source_idx: int = -1
	var wire_colors: Array = []

	func _draw() -> void:
		for src_idx in wires.keys():
			var sink_idx: int = int(wires[src_idx].sink_idx)
			var correct: bool = bool(wires[src_idx].correct)
			if src_idx < source_positions.size() and sink_idx < sink_positions.size():
				var sp: Vector2 = source_positions[src_idx]
				var ep: Vector2 = sink_positions[sink_idx]
				var color: Color = wire_colors[src_idx] if src_idx < wire_colors.size() else Color.WHITE
				if not correct:
					color = Color(1.0, 0.2, 0.2)
				_draw_wire(sp, ep, color)
		# Pending wire follows mouse
		if pending_source_idx >= 0 and pending_source_idx < source_positions.size():
			var sp: Vector2 = source_positions[pending_source_idx]
			var ep := get_local_mouse_position()
			var color: Color = wire_colors[pending_source_idx] if pending_source_idx < wire_colors.size() else Color.WHITE
			_draw_wire(sp, ep, Color(color.r, color.g, color.b, 0.5))

	func _process(_delta: float) -> void:
		if pending_source_idx >= 0:
			queue_redraw()

	func _draw_wire(a: Vector2, b: Vector2, color: Color) -> void:
		# Glow
		draw_line(a, b, Color(color.r, color.g, color.b, 0.25), 14.0, true)
		draw_line(a, b, color, 6.0, true)
		draw_line(a, b, Color(1, 1, 1, 0.5), 2.0, true)


class _BorderGlow extends Control:
	func _draw() -> void:
		var rect := Rect2(Vector2.ZERO, size)
		draw_rect(rect, Color(0.0, 0.90, 1.0, 0.6), false, 3.0)
		# Inner glow line
		draw_rect(rect.grow(-6.0), Color(0.0, 0.90, 1.0, 0.2), false, 1.5)
