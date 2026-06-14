extends Control

# Profile Screen — view your card, edit your username, equip unlockable
# frames, banners and emblems.

const PlayerCardClass := preload("res://scripts/player_card.gd")

var card_preview: PlayerCard = null
var username_input: LineEdit = null
var stats_label: Label = null
var item_columns: Dictionary = {}  # kind -> VBoxContainer

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	if ResourceLoader.exists("res://assets/ui/theme.tres"):
		theme = load("res://assets/ui/theme.tres")
	_build_background()
	_build_ui()
	if not PlayerProfile.profile_changed.is_connected(_refresh_all):
		PlayerProfile.profile_changed.connect(_refresh_all)
	_refresh_all()

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
	overlay.color = Color(0.04, 0.06, 0.12, 0.70)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _build_ui() -> void:
	# Title at top
	var title := Label.new()
	title.text = "PLAYER PROFILE"
	title.add_theme_font_size_override("font_size", 52)
	title.add_theme_color_override("font_color", Color(0.0, 0.90, 1.0))
	title.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	title.add_theme_constant_override("outline_size", 8)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(title)
	title.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	title.offset_top = 20
	title.offset_bottom = 80

	# Player card preview (centered horizontally, large size)
	card_preview = PlayerCardClass.new()
	card_preview.custom_minimum_size = Vector2(640, 180)
	card_preview.size = Vector2(640, 180)
	add_child(card_preview)
	card_preview.anchor_left = 0.5
	card_preview.anchor_right = 0.5
	card_preview.anchor_top = 0.0
	card_preview.anchor_bottom = 0.0
	card_preview.offset_left = -320
	card_preview.offset_right = 320
	card_preview.offset_top = 100
	card_preview.offset_bottom = 280

	# Username edit row
	var username_row := HBoxContainer.new()
	username_row.add_theme_constant_override("separation", 16)
	username_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(username_row)
	username_row.anchor_left = 0.5
	username_row.anchor_right = 0.5
	username_row.anchor_top = 0.0
	username_row.anchor_bottom = 0.0
	username_row.offset_left = -340
	username_row.offset_right = 340
	username_row.offset_top = 296
	username_row.offset_bottom = 360

	var name_label := Label.new()
	name_label.text = "USERNAME"
	name_label.add_theme_font_size_override("font_size", 22)
	name_label.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	name_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	name_label.add_theme_constant_override("outline_size", 4)
	name_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	name_label.custom_minimum_size = Vector2(140, 50)
	username_row.add_child(name_label)

	username_input = LineEdit.new()
	username_input.max_length = 14
	username_input.placeholder_text = "Your call-sign"
	username_input.custom_minimum_size = Vector2(280, 50)
	username_input.add_theme_font_size_override("font_size", 24)
	username_row.add_child(username_input)

	var save_btn := _make_button("SAVE", Vector2(140, 50))
	save_btn.pressed.connect(_on_save_username)
	username_row.add_child(save_btn)

	# Stats label
	stats_label = Label.new()
	stats_label.text = ""
	stats_label.add_theme_font_size_override("font_size", 20)
	stats_label.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0, 0.92))
	stats_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	stats_label.add_theme_constant_override("outline_size", 4)
	stats_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	stats_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(stats_label)
	stats_label.set_anchors_and_offsets_preset(Control.PRESET_TOP_WIDE)
	stats_label.offset_top = 364
	stats_label.offset_bottom = 396

	# Three columns of unlockables
	var columns_row := HBoxContainer.new()
	columns_row.add_theme_constant_override("separation", 24)
	columns_row.alignment = BoxContainer.ALIGNMENT_CENTER
	columns_row.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(columns_row)
	columns_row.anchor_left = 0.5
	columns_row.anchor_right = 0.5
	columns_row.anchor_top = 0.0
	columns_row.anchor_bottom = 0.0
	columns_row.offset_left = -680
	columns_row.offset_right = 680
	columns_row.offset_top = 408
	columns_row.offset_bottom = 956

	item_columns["frame"] = _build_column(columns_row, "FRAMES", "frame")
	item_columns["banner"] = _build_column(columns_row, "BANNERS", "banner")
	item_columns["emblem"] = _build_column(columns_row, "EMBLEMS", "emblem")

	# Back button
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

func _build_column(parent: Container, header_text: String, kind: String) -> VBoxContainer:
	var panel := PanelContainer.new()
	panel.theme_type_variation = "DialogPanel"
	panel.custom_minimum_size = Vector2(420, 540)
	panel.mouse_filter = Control.MOUSE_FILTER_STOP
	parent.add_child(panel)
	var inner := VBoxContainer.new()
	inner.add_theme_constant_override("separation", 6)
	inner.mouse_filter = Control.MOUSE_FILTER_PASS
	panel.add_child(inner)

	var header := Label.new()
	header.text = header_text
	header.add_theme_font_size_override("font_size", 26)
	header.add_theme_color_override("font_color", Color(1, 0.92, 0.18))
	header.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	header.add_theme_constant_override("outline_size", 5)
	header.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	header.mouse_filter = Control.MOUSE_FILTER_IGNORE
	inner.add_child(header)

	var list := VBoxContainer.new()
	list.add_theme_constant_override("separation", 4)
	inner.add_child(list)
	list.name = "ItemList"
	return list

func _populate_column(list: VBoxContainer, items: Array, kind: String, equipped_id: String) -> void:
	for c in list.get_children():
		c.queue_free()
	for entry in items:
		var unlocked: bool = PlayerProfile.is_unlocked(str(entry.id), kind)
		var row := _make_item_row(entry, kind, unlocked, str(entry.id) == equipped_id)
		list.add_child(row)

func _make_item_row(entry: Dictionary, kind: String, unlocked: bool, equipped: bool) -> Control:
	var btn := Button.new()
	btn.focus_mode = Control.FOCUS_NONE
	btn.custom_minimum_size = Vector2(400, 56)
	btn.add_theme_font_size_override("font_size", 18)
	btn.add_theme_color_override("font_color", Color(1, 1, 1))
	btn.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	btn.add_theme_constant_override("outline_size", 4)

	var status: String
	if equipped:
		status = "  EQUIPPED"
	elif unlocked:
		status = "  TAP TO EQUIP"
	else:
		var src: String = str(entry.source)
		if src == "level":
			status = "  LVL %d" % int(entry.level)
		elif src == "season":
			status = "  SEASON %d" % PlayerProfile.CURRENT_SEASON
		elif src == "store":
			status = "  STORE"
		else:
			status = ""
	btn.text = "%s%s" % [str(entry.label), status]

	var sb := StyleBoxFlat.new()
	sb.bg_color = Color(0.05, 0.07, 0.16, 0.95)
	if equipped:
		sb.border_color = Color(1, 0.92, 0.18)
		sb.bg_color = Color(0.10, 0.10, 0.05, 0.95)
	elif unlocked:
		sb.border_color = Color(0.0, 0.90, 1.0, 0.85)
	else:
		sb.border_color = Color(0.45, 0.45, 0.55, 0.5)
		btn.modulate = Color(0.55, 0.55, 0.65)
	sb.set_border_width_all(2)
	sb.set_corner_radius_all(8)
	btn.add_theme_stylebox_override("normal", sb)
	btn.add_theme_stylebox_override("hover", sb)
	btn.add_theme_stylebox_override("pressed", sb)
	btn.add_theme_stylebox_override("focus", sb)
	btn.add_theme_stylebox_override("disabled", sb)
	btn.disabled = not unlocked
	if unlocked and not equipped:
		btn.pressed.connect(func():
			AudioManager.play_sfx(GameState.SFX_UI_CLICK, -8.0)
			PlayerProfile.equip(kind, str(entry.id))
		)
		btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -14.0))
	return btn

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

func _refresh_all() -> void:
	if is_instance_valid(card_preview):
		card_preview.bind_local()
	if is_instance_valid(username_input) and username_input.text == "":
		username_input.text = PlayerProfile.username
	if is_instance_valid(stats_label):
		var to_next: int = PlayerProfile.rp_to_next_level()
		stats_label.text = "Season %d  •  RP: %d  •  Next level in %d RP  •  Matches: %d  •  Wins: %d" % [
			PlayerProfile.season_id,
			PlayerProfile.rp,
			to_next,
			PlayerProfile.matches_played,
			PlayerProfile.matches_won,
		]
	_populate_column(item_columns["frame"] as VBoxContainer, PlayerProfile.FRAMES, "frame", PlayerProfile.equipped_frame)
	_populate_column(item_columns["banner"] as VBoxContainer, PlayerProfile.BANNERS, "banner", PlayerProfile.equipped_banner)
	_populate_column(item_columns["emblem"] as VBoxContainer, PlayerProfile.EMBLEMS, "emblem", PlayerProfile.equipped_emblem)

func _on_save_username() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -6.0)
	PlayerProfile.set_username(username_input.text)

func _on_back_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CLICK, -8.0)
	LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.4)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_back"):
		_on_back_pressed()
