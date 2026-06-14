extends PuzzleBase

# Spatial Spin — two concentric rings, each with 6 colored segments.
# Click the outer or inner area to rotate that ring 60° clockwise.
# Solve when all segments are radially aligned by color.

const SEGMENTS: int = 6
const RING_INNER_R: float = 80.0
const RING_MID_R: float = 150.0
const RING_OUTER_R: float = 220.0

const COLORS: Array = [
	Color(1.0, 0.18, 0.78),
	Color(0.0, 0.90, 1.0),
	Color(0.61, 1.0, 0.18),
	Color(1.0, 0.54, 0.12),
	Color(0.78, 0.35, 1.0),
	Color(1.0, 0.92, 0.18),
]

var inner_rotation: int = 0   # 0..SEGMENTS-1
var outer_rotation: int = 0

var ring_view: _RingDraw = null
var inner_hit: Control = null
var outer_hit: Control = null

func setup(d: int = 1) -> void:
	super.setup(d)
	# Scramble rotations
	inner_rotation = 1 + randi() % (SEGMENTS - 1)
	outer_rotation = 1 + randi() % (SEGMENTS - 1)
	if inner_rotation == outer_rotation:
		outer_rotation = (outer_rotation + 1) % SEGMENTS
	_build()

func _build() -> void:
	for c in get_children():
		c.queue_free()
	var board_w := (RING_OUTER_R + 24.0) * 2.0
	custom_minimum_size = Vector2(board_w, board_w + 40.0)
	size = Vector2(board_w, board_w + 40.0)

	var bg := ColorRect.new()
	bg.color = Color(0.03, 0.05, 0.13, 0.85)
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	ring_view = _RingDraw.new()
	ring_view.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(ring_view)
	ring_view.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Click hitboxes: inner circle + outer ring
	inner_hit = Control.new()
	inner_hit.mouse_filter = Control.MOUSE_FILTER_STOP
	inner_hit.custom_minimum_size = Vector2(RING_INNER_R * 2.0, RING_INNER_R * 2.0)
	inner_hit.size = inner_hit.custom_minimum_size
	add_child(inner_hit)
	inner_hit.anchor_left = 0.5
	inner_hit.anchor_right = 0.5
	inner_hit.anchor_top = 0.0
	inner_hit.anchor_bottom = 0.0
	inner_hit.offset_left = -RING_INNER_R
	inner_hit.offset_right = RING_INNER_R
	inner_hit.offset_top = RING_OUTER_R + 24.0 - RING_INNER_R
	inner_hit.offset_bottom = RING_OUTER_R + 24.0 + RING_INNER_R
	inner_hit.gui_input.connect(func(event: InputEvent): _on_inner_click(event))

	outer_hit = Control.new()
	outer_hit.mouse_filter = Control.MOUSE_FILTER_STOP
	outer_hit.custom_minimum_size = Vector2(board_w, board_w)
	outer_hit.size = Vector2(board_w, board_w)
	add_child(outer_hit)
	outer_hit.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	outer_hit.offset_top = 24.0
	outer_hit.offset_bottom = 24.0 + board_w
	outer_hit.gui_input.connect(func(event: InputEvent): _on_outer_click(event))
	# Keep inner hit on top of outer
	move_child(outer_hit, 1)
	move_child(inner_hit, get_child_count() - 1)
	_refresh()

	# Header label
	var lbl := Label.new()
	lbl.text = "Click center or outer ring to spin"
	lbl.add_theme_font_size_override("font_size", 20)
	lbl.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	lbl.add_theme_constant_override("outline_size", 4)
	lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(lbl)
	lbl.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	lbl.offset_top = -32
	lbl.offset_bottom = -4

func _refresh() -> void:
	if is_instance_valid(ring_view):
		ring_view.inner_rot = inner_rotation
		ring_view.outer_rot = outer_rotation
		ring_view.center = Vector2(size.x / 2.0, 24.0 + RING_OUTER_R)
		ring_view.ring_colors = COLORS
		ring_view.queue_redraw()

func _on_inner_click(event: InputEvent) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	inner_rotation = (inner_rotation + 1) % SEGMENTS
	AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -8.0, 0.05)
	piece_placed.emit()
	_refresh()
	_check_solved()

func _on_outer_click(event: InputEvent) -> void:
	if not input_enabled or _solved:
		return
	if not (event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT):
		return
	# Ignore if the click landed inside the inner hit area
	var center := Vector2(size.x / 2.0, 24.0 + RING_OUTER_R)
	var local: Vector2 = (event as InputEventMouseButton).position
	# event.position is in the outer_hit's local coords; we have to convert
	# back to puzzle-local since outer_hit is positioned offset 24 down.
	if (local - Vector2(size.x / 2.0, RING_OUTER_R)).length() < RING_INNER_R:
		return
	outer_rotation = (outer_rotation + 1) % SEGMENTS
	AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -8.0, 0.05)
	piece_placed.emit()
	_refresh()
	_check_solved()

func _check_solved() -> void:
	if inner_rotation == outer_rotation:
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
		_emit_solved(85 + difficulty * 40)

func get_progress() -> float:
	# Distance between the rotation offsets.
	var diff := absi(inner_rotation - outer_rotation)
	diff = mini(diff, SEGMENTS - diff)
	return clampf(1.0 - float(diff) / float(SEGMENTS / 2), 0.0, 1.0)

func auto_solve_step() -> void:
	if inner_rotation == outer_rotation:
		return
	# Rotate inner ring once.
	var fake := InputEventMouseButton.new()
	fake.button_index = MOUSE_BUTTON_LEFT
	fake.pressed = true
	_on_inner_click(fake)

func ai_step() -> void:
	auto_solve_step()


class _RingDraw extends Control:
	var inner_rot: int = 0
	var outer_rot: int = 0
	var center: Vector2 = Vector2.ZERO
	var ring_colors: Array = []

	func _draw() -> void:
		if ring_colors.is_empty():
			return
		var seg_angle: float = TAU / 6.0
		# Backing disc for depth.
		draw_circle(center, 224.0, Color(0.02, 0.04, 0.10, 0.95))
		draw_circle(center, 221.0, Color(0.06, 0.09, 0.18, 1.0))
		# Outer ring segments (glossy).
		for i in 6:
			var os: float = i * seg_angle - PI / 2.0 + outer_rot * seg_angle
			_draw_glossy_segment(os, os + seg_angle, 152.0, 220.0, ring_colors[i % ring_colors.size()])
		# Dark groove separating the two rings.
		draw_arc(center, 146.0, 0.0, TAU, 96, Color(0.02, 0.03, 0.08, 1.0), 8.0, true)
		# Inner ring segments (glossy, slightly brighter).
		for i in 6:
			var is_a: float = i * seg_angle - PI / 2.0 + inner_rot * seg_angle
			var c: Color = (ring_colors[i % ring_colors.size()] as Color).lightened(0.12)
			_draw_glossy_segment(is_a, is_a + seg_angle, 30.0, 80.0, c)
		# Metallic center hub.
		_draw_hub()

	func _dir(ang: float) -> Vector2:
		return Vector2(cos(ang), sin(ang))

	func _arc_points(a0: float, a1: float, r: float) -> PackedVector2Array:
		var pts: PackedVector2Array = []
		var steps := 12
		for s in steps + 1:
			var t := float(s) / float(steps)
			var ang := lerpf(a0, a1, t)
			pts.append(center + _dir(ang) * r)
		return pts

	func _fill_band(a0: float, a1: float, r0: float, r1: float, color: Color) -> void:
		var points: PackedVector2Array = []
		var steps := 10
		for s in steps + 1:
			var t := float(s) / float(steps)
			var ang := lerpf(a0, a1, t)
			points.append(center + _dir(ang) * r1)
		for s in steps + 1:
			var t := 1.0 - float(s) / float(steps)
			var ang := lerpf(a0, a1, t)
			points.append(center + _dir(ang) * r0)
		var colors := PackedColorArray()
		for _i in points.size():
			colors.append(color)
		draw_polygon(points, colors)

	func _draw_glossy_segment(a0: float, a1: float, r0: float, r1: float, color: Color) -> void:
		# Stacked radial gradient fakes a glossy curved surface.
		var span: float = r1 - r0
		var bands := 6
		for b in bands:
			var t0: float = float(b) / float(bands)
			var t1: float = float(b + 1) / float(bands)
			var tc: float = (t0 + t1) * 0.5
			var gloss: float = 0.45 - absf(tc - 0.32) * 1.6
			gloss = clampf(gloss, -0.35, 0.45)
			var band_col: Color
			if gloss >= 0.0:
				band_col = color.lightened(gloss)
			else:
				band_col = color.darkened(-gloss)
			_fill_band(a0, a1, r0 + span * t0, r0 + span * t1, band_col)
		# Outer rim + radial separators.
		draw_polyline(_arc_points(a0, a1, r1), Color(0.02, 0.03, 0.08, 0.9), 2.5, true)
		draw_line(center + _dir(a0) * r0, center + _dir(a0) * r1, Color(0.02, 0.03, 0.08, 0.9), 2.0, true)
		# Bright specular highlight near the inner edge.
		draw_polyline(_arc_points(a0 + 0.05, a1 - 0.05, r0 + span * 0.18), Color(1, 1, 1, 0.26), 3.0, true)

	func _draw_hub() -> void:
		draw_circle(center, 26.0, Color(0.02, 0.03, 0.08, 1.0))
		draw_circle(center, 23.0, Color(0.30, 0.34, 0.42, 1.0))
		draw_circle(center, 18.0, Color(0.55, 0.60, 0.70, 1.0))
		draw_circle(center, 12.0, Color(0.80, 0.86, 0.95, 1.0))
		draw_circle(center + Vector2(-4.0, -4.0), 4.0, Color(1, 1, 1, 0.8))
