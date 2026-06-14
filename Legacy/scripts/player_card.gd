class_name PlayerCard
extends Control

# PlayerCard — reusable visual element that renders a banner background,
# decorative frame border, emblem icon, username and level. All visuals
# are drawn procedurally so frames/banners/emblems are pure data.

var username: String = "RIVAL"
var level: int = 1
var rp_progress: float = 0.0
var tier_label: String = "NOVICE"
var tier_color: Color = Color(0.6, 0.7, 0.8)
var frame_color: Color = Color(0.0, 0.90, 1.0)
var banner_colors: Array = [Color(0.05, 0.06, 0.18), Color(0.0, 0.65, 0.85)]
var emblem_color: Color = Color(1.0, 0.92, 0.18)
var emblem_id: String = "emblem_bolt"

var compact: bool = false  # When true, smaller font sizes for in-arena HUD use.

var _username_label: Label = null
var _tier_label: Label = null
var _level_label: Label = null
var _emblem: Control = null

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	if custom_minimum_size == Vector2.ZERO:
		custom_minimum_size = Vector2(440, 130)
	_build_layout()

func _build_layout() -> void:
	for c in get_children():
		c.queue_free()

	var card_w: float = maxf(custom_minimum_size.x, size.x)
	var card_h: float = maxf(custom_minimum_size.y, size.y)
	var emblem_size: float = card_h * 0.60

	_emblem = _EmblemDraw.new()
	_emblem.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_emblem.custom_minimum_size = Vector2(emblem_size, emblem_size)
	_emblem.size = _emblem.custom_minimum_size
	_emblem.position = Vector2(card_h * 0.12, (card_h - emblem_size) / 2.0)
	add_child(_emblem)

	var text_x: float = _emblem.position.x + emblem_size + 14.0
	var text_w: float = card_w - text_x - 120.0

	_username_label = Label.new()
	_username_label.add_theme_font_size_override("font_size", 26 if compact else 30)
	_username_label.add_theme_color_override("font_color", Color(1, 1, 1))
	_username_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	_username_label.add_theme_constant_override("outline_size", 5)
	_username_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_username_label.position = Vector2(text_x, 10)
	_username_label.size = Vector2(text_w, 38)
	add_child(_username_label)

	_tier_label = Label.new()
	_tier_label.add_theme_font_size_override("font_size", 14 if compact else 16)
	_tier_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	_tier_label.add_theme_constant_override("outline_size", 4)
	_tier_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_tier_label.position = Vector2(text_x, 46)
	_tier_label.size = Vector2(text_w, 22)
	add_child(_tier_label)

	_level_label = Label.new()
	_level_label.add_theme_font_size_override("font_size", 22 if compact else 26)
	_level_label.add_theme_color_override("font_color", Color(1, 1, 1))
	_level_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	_level_label.add_theme_constant_override("outline_size", 5)
	_level_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	_level_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_level_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_level_label.position = Vector2(card_w - 120.0, 12)
	_level_label.size = Vector2(108.0, 50)
	add_child(_level_label)
	_refresh_labels()

func set_data(args: Dictionary) -> void:
	username = str(args.get("username", username))
	level = int(args.get("level", level))
	tier_label = str(args.get("tier", tier_label))
	tier_color = args.get("tier_color", tier_color)
	frame_color = args.get("frame_color", frame_color)
	banner_colors = args.get("banner_colors", banner_colors)
	emblem_color = args.get("emblem_color", emblem_color)
	emblem_id = str(args.get("emblem_id", emblem_id))
	rp_progress = float(args.get("rp_progress", rp_progress))
	_refresh_labels()
	queue_redraw()

func bind_local() -> void:
	set_data(PlayerProfile.current_card_data())

func _refresh_labels() -> void:
	if is_instance_valid(_username_label):
		_username_label.text = username
	if is_instance_valid(_tier_label):
		_tier_label.text = tier_label
		_tier_label.add_theme_color_override("font_color", tier_color)
	if is_instance_valid(_level_label):
		_level_label.text = "LVL %d" % level
	if is_instance_valid(_emblem):
		(_emblem as _EmblemDraw).emblem_color = emblem_color
		(_emblem as _EmblemDraw).emblem_id = emblem_id
		_emblem.queue_redraw()

func _draw() -> void:
	var rect := Rect2(Vector2.ZERO, size)
	# Banner gradient
	var c1: Color = banner_colors[0] if banner_colors.size() > 0 else Color(0.05, 0.06, 0.18)
	var c2: Color = banner_colors[1] if banner_colors.size() > 1 else Color(0.0, 0.65, 0.85)
	var bands: int = 24
	for i in bands:
		var t: float = float(i) / float(bands)
		var color: Color = c1.lerp(c2, t)
		var band_rect := Rect2(0, t * rect.size.y, rect.size.x, rect.size.y / float(bands) + 1.0)
		draw_rect(band_rect, color, true)
	# Dim overlay so text reads cleanly
	draw_rect(rect, Color(0.0, 0.0, 0.0, 0.30), true)
	# Frame border (4-layer halo)
	for i in 4:
		var inset: float = -float(i) * 2.0
		draw_rect(rect.grow(inset), Color(frame_color.r, frame_color.g, frame_color.b, 0.30 - float(i) * 0.06), false, 2.0)
	draw_rect(rect, frame_color, false, 3.0)
	# RP progress bar
	var bar_h: float = 8.0
	var bar_y: float = rect.size.y - bar_h - 8.0
	var bar_x: float = 12.0
	var bar_w: float = rect.size.x - 24.0
	draw_rect(Rect2(bar_x, bar_y, bar_w, bar_h), Color(0, 0, 0, 0.6), true)
	draw_rect(Rect2(bar_x, bar_y, bar_w * clampf(rp_progress, 0.0, 1.0), bar_h), tier_color, true)
	draw_rect(Rect2(bar_x, bar_y, bar_w, bar_h), Color(1, 1, 1, 0.35), false, 1.0)


# --- Emblem inner class (procedural icon drawing) ---

class _EmblemDraw extends Control:
	var emblem_color: Color = Color.WHITE
	var emblem_id: String = "emblem_bolt"

	func _draw() -> void:
		var center := size / 2.0
		var radius: float = minf(size.x, size.y) * 0.42
		# Halo
		draw_circle(center, radius * 1.35, Color(emblem_color.r, emblem_color.g, emblem_color.b, 0.32))
		match emblem_id:
			"emblem_bolt":
				var pts := PackedVector2Array([
					center + Vector2(-radius * 0.30, -radius * 0.95),
					center + Vector2(radius * 0.50, -radius * 0.10),
					center + Vector2(radius * 0.05, -radius * 0.10),
					center + Vector2(radius * 0.45, radius * 0.95),
					center + Vector2(-radius * 0.45, radius * 0.10),
					center + Vector2(0.0, radius * 0.10),
				])
				_fill(pts, emblem_color)
				_outline(pts, Color(1, 1, 1, 0.85), 2.0)
			"emblem_diamond":
				var pts := PackedVector2Array([
					center + Vector2(0.0, -radius),
					center + Vector2(radius, 0.0),
					center + Vector2(0.0, radius),
					center + Vector2(-radius, 0.0),
				])
				_fill(pts, emblem_color)
				var hl := PackedVector2Array([
					center + Vector2(0.0, -radius * 0.80),
					center + Vector2(radius * 0.30, -radius * 0.20),
					center + Vector2(-radius * 0.10, radius * 0.10),
					center + Vector2(-radius * 0.40, -radius * 0.20),
				])
				_fill(hl, Color(1, 1, 1, 0.6))
				_outline(pts, Color(1, 1, 1, 0.85), 2.0)
			"emblem_skull":
				draw_circle(center, radius, emblem_color)
				draw_circle(center + Vector2(-radius * 0.38, -radius * 0.10), radius * 0.20, Color(0, 0, 0))
				draw_circle(center + Vector2(radius * 0.38, -radius * 0.10), radius * 0.20, Color(0, 0, 0))
				draw_rect(Rect2(center + Vector2(-radius * 0.35, radius * 0.30), Vector2(radius * 0.70, radius * 0.18)), Color(0, 0, 0), true)
				draw_arc(center, radius * 0.95, 0.0, TAU, 28, Color(1, 1, 1, 0.5), 2.0, true)
			"emblem_crown":
				var bw: float = radius * 1.25
				var base_y: float = radius * 0.30
				var top_y: float = -radius * 0.15
				_fill(PackedVector2Array([
					center + Vector2(-bw / 2.0, base_y),
					center + Vector2(-bw / 2.0 * 0.85, top_y),
					center + Vector2(bw / 2.0 * 0.85, top_y),
					center + Vector2(bw / 2.0, base_y),
				]), emblem_color)
				for sx in [-0.7, 0.0, 0.7]:
					_fill(PackedVector2Array([
						center + Vector2(bw / 2.0 * float(sx) - radius * 0.18, top_y),
						center + Vector2(bw / 2.0 * float(sx), top_y - radius * 0.55),
						center + Vector2(bw / 2.0 * float(sx) + radius * 0.18, top_y),
					]), emblem_color)
				draw_circle(center + Vector2(0.0, top_y - radius * 0.65), radius * 0.12, Color(1, 1, 1, 0.9))
			"emblem_eye":
				draw_circle(center, radius, emblem_color)
				draw_circle(center, radius * 0.55, Color(0.04, 0.05, 0.12))
				draw_circle(center, radius * 0.28, Color(1, 1, 1))
				draw_circle(center, radius * 0.10, Color(0, 0, 0))
			"emblem_star":
				var pts := PackedVector2Array()
				for i in 10:
					var ang: float = -PI / 2.0 + float(i) * PI / 5.0
					var r: float = radius if i % 2 == 0 else radius * 0.45
					pts.append(center + Vector2(cos(ang), sin(ang)) * r)
				_fill(pts, emblem_color)
				_outline(pts, Color(1, 1, 1, 0.8), 1.5)
			"emblem_burst":
				draw_circle(center, radius * 0.75, emblem_color)
				for i in 8:
					var ang: float = float(i) * PI / 4.0
					var p1: Vector2 = center + Vector2(cos(ang), sin(ang)) * radius * 0.75
					var p2: Vector2 = center + Vector2(cos(ang), sin(ang)) * radius * 1.20
					draw_line(p1, p2, emblem_color, 3.0, true)
				draw_circle(center, radius * 0.30, Color(1, 1, 0.85))
			"emblem_orbit":
				draw_circle(center, radius * 0.34, emblem_color)
				draw_arc(center, radius * 0.78, 0.0, TAU, 32, emblem_color, 3.0, true)
				draw_arc(center, radius * 1.08, 0.0, TAU, 32, Color(emblem_color.r, emblem_color.g, emblem_color.b, 0.7), 2.0, true)
				draw_circle(center + Vector2(radius * 0.78, 0.0), radius * 0.16, Color(1, 1, 1, 0.92))
			"emblem_legend":
				draw_circle(center, radius * 1.05, Color(emblem_color.r, emblem_color.g, emblem_color.b, 0.5))
				var pts := PackedVector2Array()
				for i in 12:
					var ang: float = float(i) * PI / 6.0
					var r: float = radius * 0.95 if i % 2 == 0 else radius * 0.5
					pts.append(center + Vector2(cos(ang), sin(ang)) * r)
				_fill(pts, emblem_color)
				_outline(pts, Color(1, 1, 1, 0.85), 2.0)
			_:
				draw_circle(center, radius * 0.85, emblem_color)

	func _fill(pts: PackedVector2Array, color: Color) -> void:
		var colors := PackedColorArray()
		for _i in pts.size():
			colors.append(color)
		draw_polygon(pts, colors)

	func _outline(pts: PackedVector2Array, color: Color, thickness: float) -> void:
		var closed := pts.duplicate()
		closed.append(pts[0])
		draw_polyline(closed, color, thickness, true)
