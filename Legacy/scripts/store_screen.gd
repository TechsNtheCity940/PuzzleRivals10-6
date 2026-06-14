extends Control

# Store Screen — spend coins earned in matches on store-exclusive cosmetics.
# Items are pulled from PlayerProfile's catalogs where source == "store".
# Buying unlocks (and auto-equips) the cosmetic; owned items can be equipped.

const PlayerCardClass := preload("res://scripts/player_card.gd")

var coins_label: Label = null
var grid: GridContainer = null
var toast_layer: Control = null

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	if ResourceLoader.exists("res://assets/ui/theme.tres"):
		theme = load("res://assets/ui/theme.tres")
	_build_background()
	_build_ui()
	if not PlayerProfile.profile_changed.is_connected(_refresh):
		PlayerProfile.profile_changed.connect(_refresh)
	_refresh()

func _build_background() -> void:
	var bg := TextureRect.new()
	if ResourceLoader.exists("res://assets/textures/backgrounds/menu_bg.png"):
		bg.texture = load("res://assets/textures/backgrounds/menu_bg.png")
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	var overlay := ColorRect.new()
	overlay.color = Color(0.04, 0.06, 0.12, 0.74)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _build_ui() -> void:
	var title := Label.new()
	title.text = "STORE"
	title.add_theme_font_size_override("font_size", 52)
	title.add_theme_color_override("font_color", Color(1.0, 0.92, 0.18))
	title.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	title.add_theme_constant_override("outline_size", 8)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(title)
	title.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	title.offset_top = 20
	title.offset_bottom = 80

	var subtitle := Label.new()
	subtitle.text = "Spend coins earned in matches on exclusive cosmetics."
	subtitle.add_theme_font_size_override("font_size", 22)
	subtitle.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	subtitle.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	subtitle.add_theme_constant_override("outline_size", 4)
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(subtitle)
	subtitle.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	subtitle.offset_top = 84
	subtitle.offset_bottom = 116

	# Coin balance pill, top-right.
	var coin_panel := PanelContainer.new()
	coin_panel.theme_type_variation = "DialogPanel"
	coin_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(coin_panel)
	coin_panel.anchor_left = 1.0
	coin_panel.anchor_right = 1.0
	coin_panel.anchor_top = 0.0
	coin_panel.anchor_bottom = 0.0
	coin_panel.offset_left = -360
	coin_panel.offset_right = -24
	coin_panel.offset_top = 24
	coin_panel.offset_bottom = 92
	var coin_margin := MarginContainer.new()
	coin_margin.add_theme_constant_override("margin_left", 18)
	coin_margin.add_theme_constant_override("margin_right", 18)
	coin_margin.add_theme_constant_override("margin_top", 6)
	coin_margin.add_theme_constant_override("margin_bottom", 6)
	coin_panel.add_child(coin_margin)
	coins_label = Label.new()
	coins_label.text = "COINS: 0"
	coins_label.add_theme_font_size_override("font_size", 30)
	coins_label.add_theme_color_override("font_color", Color(1.0, 0.92, 0.18))
	coins_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	coins_label.add_theme_constant_override("outline_size", 5)
	coins_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	coins_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	coins_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	coin_margin.add_child(coins_label)

	# Items grid, centered in the middle band.
	var holder := CenterContainer.new()
	holder.mouse_filter = Control.MOUSE_FILTER_PASS
	add_child(holder)
	holder.anchor_left = 0.0
	holder.anchor_right = 1.0
	holder.anchor_top = 0.0
	holder.anchor_bottom = 1.0
	holder.offset_left = 40
	holder.offset_right = -40
	holder.offset_top = 150
	holder.offset_bottom = -120
	grid = GridContainer.new()
	grid.columns = 3
	grid.add_theme_constant_override("h_separation", 26)
	grid.add_theme_constant_override("v_separation", 26)
	grid.mouse_filter = Control.MOUSE_FILTER_PASS
	holder.add_child(grid)

	# Back button, bottom-center.
	var back_btn := _make_button("BACK TO MENU", Vector2(360, 60))
	back_btn.pressed.connect(_on_back_pressed)
	add_child(back_btn)
	back_btn.anchor_left = 0.5
	back_btn.anchor_right = 0.5
	back_btn.anchor_top = 1.0
	back_btn.anchor_bottom = 1.0
	back_btn.offset_left = -180
	back_btn.offset_right = 180
	back_btn.offset_top = -90
	back_btn.offset_bottom = -30

	# Toast layer for purchase feedback (on top, click-through).
	toast_layer = Control.new()
	toast_layer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(toast_layer)
	toast_layer.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _store_items() -> Array:
	var out: Array = []
	for f in PlayerProfile.FRAMES:
		if str(f.get("source", "")) == "store":
			out.append({"kind": "frame", "def": f})
	for b in PlayerProfile.BANNERS:
		if str(b.get("source", "")) == "store":
			out.append({"kind": "banner", "def": b})
	for e in PlayerProfile.EMBLEMS:
		if str(e.get("source", "")) == "store":
			out.append({"kind": "emblem", "def": e})
	return out

func _refresh() -> void:
	if is_instance_valid(coins_label):
		coins_label.text = "COINS: %d" % PlayerProfile.coins
	if not is_instance_valid(grid):
		return
	for c in grid.get_children():
		c.queue_free()
	for item in _store_items():
		grid.add_child(_make_store_card(item as Dictionary))

func _accent_for(kind: String, def: Dictionary) -> Color:
	if kind == "banner":
		var cols: Array = def.get("colors", [])
		if cols.size() >= 2:
			return cols[1]
		return Color(0.0, 0.90, 1.0)
	return def.get("color", Color(0.0, 0.90, 1.0))

func _is_equipped(kind: String, item_id: String) -> bool:
	match kind:
		"frame":  return PlayerProfile.equipped_frame == item_id
		"banner": return PlayerProfile.equipped_banner == item_id
		"emblem": return PlayerProfile.equipped_emblem == item_id
	return false

func _make_store_card(item: Dictionary) -> Control:
	var kind: String = str(item.kind)
	var def: Dictionary = item.def
	var item_id: String = str(def.id)
	var price: int = int(def.get("price", 0))
	var owned: bool = PlayerProfile.is_unlocked(item_id, kind)
	var equipped: bool = _is_equipped(kind, item_id)
	var accent: Color = _accent_for(kind, def)

	var card := PanelContainer.new()
	card.theme_type_variation = "DialogPanel"
	card.custom_minimum_size = Vector2(300, 330)
	card.mouse_filter = Control.MOUSE_FILTER_STOP

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 16)
	margin.add_theme_constant_override("margin_right", 16)
	margin.add_theme_constant_override("margin_top", 14)
	margin.add_theme_constant_override("margin_bottom", 14)
	card.add_child(margin)

	var vb := VBoxContainer.new()
	vb.alignment = BoxContainer.ALIGNMENT_CENTER
	vb.add_theme_constant_override("separation", 8)
	margin.add_child(vb)

	vb.add_child(_make_preview(kind, def, accent))

	var name_lbl := Label.new()
	name_lbl.text = str(def.label)
	name_lbl.add_theme_font_size_override("font_size", 24)
	name_lbl.add_theme_color_override("font_color", Color(1, 1, 1))
	name_lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	name_lbl.add_theme_constant_override("outline_size", 4)
	name_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vb.add_child(name_lbl)

	var kind_lbl := Label.new()
	kind_lbl.text = kind.to_upper()
	kind_lbl.add_theme_font_size_override("font_size", 15)
	kind_lbl.add_theme_color_override("font_color", accent)
	kind_lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	kind_lbl.add_theme_constant_override("outline_size", 3)
	kind_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	kind_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vb.add_child(kind_lbl)

	vb.add_child(_make_action_button(kind, item_id, price, owned, equipped))
	return card

func _make_preview(kind: String, def: Dictionary, accent: Color) -> Control:
	var holder := Control.new()
	holder.custom_minimum_size = Vector2(240, 124)
	holder.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var sw := _PreviewDraw.new()
	sw.kind = kind
	sw.accent = accent
	if kind == "banner":
		sw.colors = def.get("colors", [])
	sw.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.add_child(sw)
	sw.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	if kind == "emblem":
		var em := PlayerCardClass._EmblemDraw.new()
		em.emblem_color = def.get("color", Color.WHITE)
		em.emblem_id = str(def.id)
		em.mouse_filter = Control.MOUSE_FILTER_IGNORE
		holder.add_child(em)
		em.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	return holder

func _make_action_button(kind: String, item_id: String, price: int, owned: bool, equipped: bool) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = Vector2(252, 56)
	btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	btn.focus_mode = Control.FOCUS_NONE
	btn.add_theme_font_size_override("font_size", 22)
	btn.add_theme_color_override("font_color", Color(1, 1, 1))
	btn.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	btn.add_theme_constant_override("outline_size", 4)

	var border: Color
	var enabled := true
	if owned and equipped:
		btn.text = "EQUIPPED"
		border = Color(1.0, 0.92, 0.18)
		enabled = false
	elif owned:
		btn.text = "EQUIP"
		border = Color(0.0, 0.90, 1.0)
	elif PlayerProfile.coins >= price:
		btn.text = "BUY  -  %d" % price
		border = Color(0.61, 1.0, 0.18)
	else:
		btn.text = "%d COINS" % price
		border = Color(0.6, 0.45, 0.45)
		enabled = false

	var sb := StyleBoxFlat.new()
	sb.bg_color = Color(0.05, 0.07, 0.16, 0.95)
	sb.border_color = border
	sb.set_border_width_all(2)
	sb.set_corner_radius_all(10)
	btn.add_theme_stylebox_override("normal", sb)
	btn.add_theme_stylebox_override("hover", sb)
	btn.add_theme_stylebox_override("pressed", sb)
	btn.add_theme_stylebox_override("focus", sb)
	btn.add_theme_stylebox_override("disabled", sb)
	btn.disabled = not enabled
	if not enabled:
		btn.modulate = Color(0.6, 0.6, 0.7) if not equipped else Color(1, 1, 1)

	if enabled:
		btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -14.0))
		if owned:
			btn.pressed.connect(func(): _equip_item(kind, item_id))
		else:
			btn.pressed.connect(func(): _buy_item(kind, item_id))
	return btn

func _buy_item(kind: String, item_id: String) -> void:
	var def: Dictionary = PlayerProfile._def_for(kind, item_id)
	if PlayerProfile.purchase(kind, item_id):
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -4.0)
		PlayerProfile.equip(kind, item_id)
		_toast("PURCHASED  %s!" % str(def.get("label", "ITEM")), Color(0.61, 1.0, 0.18))
	else:
		AudioManager.play_sfx(GameState.SFX_UI_CLICK, -8.0)
		_toast("NOT ENOUGH COINS", Color(1.0, 0.45, 0.35))

func _equip_item(kind: String, item_id: String) -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -6.0)
	PlayerProfile.equip(kind, item_id)

func _toast(text: String, color: Color) -> void:
	if not is_instance_valid(toast_layer):
		return
	var lbl := Label.new()
	lbl.text = text
	lbl.add_theme_font_size_override("font_size", 40)
	lbl.add_theme_color_override("font_color", color)
	lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	lbl.add_theme_constant_override("outline_size", 7)
	lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	toast_layer.add_child(lbl)
	lbl.anchor_left = 0.0
	lbl.anchor_right = 1.0
	lbl.anchor_top = 1.0
	lbl.anchor_bottom = 1.0
	lbl.offset_left = 0
	lbl.offset_right = 0
	lbl.offset_top = -200
	lbl.offset_bottom = -140
	lbl.modulate = Color(color.r, color.g, color.b, 0.0)
	var tw := lbl.create_tween()
	tw.tween_property(lbl, "modulate:a", 1.0, 0.2)
	tw.tween_interval(1.1)
	tw.tween_property(lbl, "modulate:a", 0.0, 0.4)
	tw.tween_callback(func():
		if is_instance_valid(lbl):
			lbl.queue_free()
	)

func _make_button(text: String, min_size: Vector2) -> Button:
	var btn := Button.new()
	btn.text = text
	btn.custom_minimum_size = min_size
	btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	btn.focus_mode = Control.FOCUS_NONE
	btn.add_theme_font_size_override("font_size", 22)
	btn.add_theme_color_override("font_color", Color(1, 1, 1))
	btn.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	btn.add_theme_constant_override("outline_size", 5)
	var sb := StyleBoxFlat.new()
	sb.bg_color = Color(0.05, 0.07, 0.16, 0.95)
	sb.border_color = Color(0.0, 0.90, 1.0)
	sb.set_border_width_all(2)
	sb.set_corner_radius_all(10)
	btn.add_theme_stylebox_override("normal", sb)
	btn.add_theme_stylebox_override("hover", sb)
	btn.add_theme_stylebox_override("pressed", sb)
	btn.add_theme_stylebox_override("focus", sb)
	btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -12.0))
	return btn

func _on_back_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CLICK, -8.0)
	LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.4)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_back"):
		_on_back_pressed()


# --- Preview swatch primitive ---

class _PreviewDraw extends Control:
	var kind: String = "frame"
	var accent: Color = Color.WHITE
	var colors: Array = []

	func _draw() -> void:
		var rect := Rect2(Vector2.ZERO, size)
		if kind == "banner" and colors.size() >= 2:
			var c1: Color = colors[0]
			var c2: Color = colors[1]
			var bands := 18
			for i in bands:
				var t: float = float(i) / float(bands)
				draw_rect(Rect2(0, t * rect.size.y, rect.size.x, rect.size.y / float(bands) + 1.0), c1.lerp(c2, t), true)
		else:
			draw_rect(rect, Color(0.05, 0.07, 0.15, 0.96), true)
		# Border — thicker, multi-layer for frames to show off the frame style.
		if kind == "frame":
			for i in 4:
				var inset: float = -float(i) * 2.5
				draw_rect(rect.grow(inset), Color(accent.r, accent.g, accent.b, 0.4 - float(i) * 0.08), false, 2.5)
			draw_rect(rect, accent, false, 4.0)
		else:
			draw_rect(rect, accent, false, 2.5)
