extends Control

# game_world.gd — Mode selection screen. The engine-provisioned main menu's
# Play button transitions here. From here the player picks FFA or Brain VS Brain.

const ARENA_FFA_PATH := "res://scenes/arena_ffa.tscn"
const ARENA_BVB_PATH := "res://scenes/arena_bvb.tscn"
const LOBBY_PATH := "res://scenes/lobby.tscn"
const PROFILE_PATH := "res://scenes/profile_screen.tscn"
const STORE_PATH := "res://scenes/store_screen.tscn"
const PlayerCardClass := preload("res://scripts/player_card.gd")

var mini_card: PlayerCard = null
var username_modal: Control = null
var coins_label: Label = null

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	if ResourceLoader.exists("res://assets/ui/theme.tres"):
		theme = load("res://assets/ui/theme.tres")
	_build_background()
	_build_ui()
	_build_mini_card()
	AudioManager.play_music(GameState.MUSIC_MENU, -8.0, 0.4)
	if not PlayerProfile.profile_changed.is_connected(_refresh_mini_card):
		PlayerProfile.profile_changed.connect(_refresh_mini_card)
	# First-run prompt for a username.
	if PlayerProfile.first_run or PlayerProfile.username == "":
		await get_tree().process_frame
		_show_username_modal()

func _build_background() -> void:
	var bg := TextureRect.new()
	if ResourceLoader.exists("res://assets/textures/backgrounds/menu_bg.png"):
		bg.texture = load("res://assets/textures/backgrounds/menu_bg.png")
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Dark overlay for legibility
	var overlay := ColorRect.new()
	overlay.color = Color(0.04, 0.06, 0.12, 0.55)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _build_ui() -> void:
	# Wordmark at top center
	var center := CenterContainer.new()
	center.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(center)
	center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var vbox := VBoxContainer.new()
	vbox.add_theme_constant_override("separation", 28)
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	center.add_child(vbox)

	# Wordmark
	if ResourceLoader.exists("res://assets/ui/wordmark_title.png"):
		var wordmark := TextureRect.new()
		wordmark.texture = load("res://assets/ui/wordmark_title.png")
		wordmark.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		wordmark.expand_mode = TextureRect.EXPAND_FIT_WIDTH_PROPORTIONAL
		wordmark.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		wordmark.custom_minimum_size = Vector2(900, 280)
		wordmark.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.add_child(wordmark)

	# Sub-title label
	var subtitle := Label.new()
	subtitle.text = "SELECT A MODE"
	subtitle.add_theme_font_size_override("font_size", 36)
	subtitle.add_theme_color_override("font_color", Color(0.0, 0.90, 1.0))
	subtitle.add_theme_color_override("font_outline_color", Color(0, 0, 0, 0.9))
	subtitle.add_theme_constant_override("outline_size", 6)
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	vbox.add_child(subtitle)

	# Spacer
	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 24)
	vbox.add_child(spacer)

	# Mode buttons row
	var hbox := HBoxContainer.new()
	hbox.alignment = BoxContainer.ALIGNMENT_CENTER
	hbox.add_theme_constant_override("separation", 40)
	hbox.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	vbox.add_child(hbox)

	hbox.add_child(_make_mode_button("res://assets/ui/mode_ffa.tres", _on_ffa_pressed))
	hbox.add_child(_make_mode_button("res://assets/ui/mode_bvb.tres", _on_bvb_pressed))

	# Spacer + online section
	var spacer_online := Control.new()
	spacer_online.custom_minimum_size = Vector2(0, 12)
	vbox.add_child(spacer_online)

	var online_label := Label.new()
	online_label.text = "OR PLAY ONLINE"
	online_label.add_theme_font_size_override("font_size", 24)
	online_label.add_theme_color_override("font_color", Color(0.61, 1.0, 0.18))
	online_label.add_theme_color_override("font_outline_color", Color(0, 0, 0, 0.9))
	online_label.add_theme_constant_override("outline_size", 5)
	online_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	online_label.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	vbox.add_child(online_label)

	var online_hbox := HBoxContainer.new()
	online_hbox.alignment = BoxContainer.ALIGNMENT_CENTER
	online_hbox.add_theme_constant_override("separation", 28)
	online_hbox.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	vbox.add_child(online_hbox)

	var online_ffa_btn := _make_online_button("ONLINE 4P FFA", Color(1.0, 0.54, 0.12))
	online_ffa_btn.pressed.connect(_on_online_ffa_pressed)
	online_hbox.add_child(online_ffa_btn)

	var online_bvb_btn := _make_online_button("ONLINE 1v1", Color(0.0, 0.90, 1.0))
	online_bvb_btn.pressed.connect(_on_online_bvb_pressed)
	online_hbox.add_child(online_bvb_btn)

	# Back button
	var spacer2 := Control.new()
	spacer2.custom_minimum_size = Vector2(0, 24)
	vbox.add_child(spacer2)

	var back := _make_kit_button("res://assets/ui/btn_quit.tres", Vector2(360, 130))
	back.text = ""
	back.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -10.0))
	back.pressed.connect(_on_back_pressed)
	var back_wrap := CenterContainer.new()
	back_wrap.add_child(back)
	vbox.add_child(back_wrap)

	# Hint label
	var hint := Label.new()
	hint.text = "Press ESC to return to main menu"
	hint.add_theme_font_size_override("font_size", 18)
	hint.add_theme_color_override("font_color", Color(0.7, 0.78, 0.95, 0.7))
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hint.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	vbox.add_child(hint)

	# Profile button anchored to the top-right corner.
	var profile_btn := Button.new()
	profile_btn.text = "PROFILE"
	profile_btn.custom_minimum_size = Vector2(220, 60)
	profile_btn.focus_mode = Control.FOCUS_NONE
	profile_btn.add_theme_font_size_override("font_size", 22)
	profile_btn.add_theme_color_override("font_color", Color(1, 1, 1))
	profile_btn.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	profile_btn.add_theme_constant_override("outline_size", 5)
	var psb := StyleBoxFlat.new()
	psb.bg_color = Color(0.05, 0.07, 0.16, 0.95)
	psb.border_color = Color(1.0, 0.92, 0.18)
	psb.set_border_width_all(2)
	psb.set_corner_radius_all(10)
	profile_btn.add_theme_stylebox_override("normal", psb)
	profile_btn.add_theme_stylebox_override("hover", psb)
	profile_btn.add_theme_stylebox_override("pressed", psb)
	profile_btn.add_theme_stylebox_override("focus", psb)
	profile_btn.pressed.connect(_on_profile_pressed)
	profile_btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -12.0))
	add_child(profile_btn)
	profile_btn.anchor_left = 1.0
	profile_btn.anchor_right = 1.0
	profile_btn.anchor_top = 0.0
	profile_btn.anchor_bottom = 0.0
	profile_btn.offset_left = -240
	profile_btn.offset_right = -20
	profile_btn.offset_top = 24
	profile_btn.offset_bottom = 84

func _build_mini_card() -> void:
	# Compact player card anchored to the top-left so the user always sees
	# their rank and progress.
	mini_card = PlayerCardClass.new()
	mini_card.compact = true
	mini_card.custom_minimum_size = Vector2(360, 110)
	mini_card.size = mini_card.custom_minimum_size
	add_child(mini_card)
	mini_card.anchor_left = 0.0
	mini_card.anchor_right = 0.0
	mini_card.anchor_top = 0.0
	mini_card.anchor_bottom = 0.0
	mini_card.offset_left = 24
	mini_card.offset_right = 384
	mini_card.offset_top = 24
	mini_card.offset_bottom = 134
	_refresh_mini_card()

func _refresh_mini_card() -> void:
	if is_instance_valid(mini_card):
		mini_card.bind_local()

func _show_username_modal() -> void:
	if is_instance_valid(username_modal):
		return
	username_modal = Control.new()
	username_modal.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(username_modal)
	username_modal.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	# Block clicks behind it.
	var dim := ColorRect.new()
	dim.color = Color(0, 0, 0, 0.75)
	dim.mouse_filter = Control.MOUSE_FILTER_STOP
	username_modal.add_child(dim)
	dim.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var panel := PanelContainer.new()
	panel.theme_type_variation = "DialogPanel"
	panel.mouse_filter = Control.MOUSE_FILTER_STOP
	username_modal.add_child(panel)
	panel.anchor_left = 0.5
	panel.anchor_right = 0.5
	panel.anchor_top = 0.5
	panel.anchor_bottom = 0.5
	panel.offset_left = -340
	panel.offset_right = 340
	panel.offset_top = -180
	panel.offset_bottom = 180

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 36)
	margin.add_theme_constant_override("margin_right", 36)
	margin.add_theme_constant_override("margin_top", 28)
	margin.add_theme_constant_override("margin_bottom", 28)
	panel.add_child(margin)

	var vbox := VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	vbox.add_theme_constant_override("separation", 18)
	margin.add_child(vbox)

	var title := Label.new()
	title.text = "WELCOME, RIVAL"
	title.add_theme_font_size_override("font_size", 42)
	title.add_theme_color_override("font_color", Color(0.0, 0.90, 1.0))
	title.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	title.add_theme_constant_override("outline_size", 7)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "Pick a call-sign for your player card."
	subtitle.add_theme_font_size_override("font_size", 22)
	subtitle.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	subtitle.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	subtitle.add_theme_constant_override("outline_size", 4)
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(subtitle)

	var input := LineEdit.new()
	input.placeholder_text = "Up to 14 characters"
	input.max_length = 14
	input.custom_minimum_size = Vector2(420, 56)
	input.add_theme_font_size_override("font_size", 28)
	vbox.add_child(input)
	input.grab_focus()

	var confirm := Button.new()
	confirm.text = "LOCK IT IN"
	confirm.custom_minimum_size = Vector2(280, 64)
	confirm.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	confirm.focus_mode = Control.FOCUS_NONE
	confirm.add_theme_font_size_override("font_size", 26)
	confirm.add_theme_color_override("font_color", Color(1, 1, 1))
	confirm.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	confirm.add_theme_constant_override("outline_size", 5)
	var sb := StyleBoxFlat.new()
	sb.bg_color = Color(0.05, 0.07, 0.16, 0.95)
	sb.border_color = Color(0.61, 1.0, 0.18)
	sb.set_border_width_all(3)
	sb.set_corner_radius_all(12)
	confirm.add_theme_stylebox_override("normal", sb)
	confirm.add_theme_stylebox_override("hover", sb)
	confirm.add_theme_stylebox_override("pressed", sb)
	confirm.add_theme_stylebox_override("focus", sb)
	confirm.pressed.connect(func():
		AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -4.0)
		PlayerProfile.set_username(input.text)
		if is_instance_valid(username_modal):
			username_modal.queue_free()
			username_modal = null
	)
	input.text_submitted.connect(func(_t: String):
		AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -4.0)
		PlayerProfile.set_username(input.text)
		if is_instance_valid(username_modal):
			username_modal.queue_free()
			username_modal = null
	)
	vbox.add_child(confirm)

func _on_profile_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -8.0)
	LoadingScreen.change_scene(PROFILE_PATH, 0.4)

func _make_mode_button(stylebox_path: String, on_pressed: Callable) -> Button:
	var btn := _make_kit_button(stylebox_path, Vector2(540, 180))
	btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -10.0))
	btn.pressed.connect(on_pressed)
	return btn

func _make_kit_button(stylebox_path: String, min_size: Vector2) -> Button:
	var btn := Button.new()
	btn.custom_minimum_size = min_size
	btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	btn.text = ""
	btn.focus_mode = Control.FOCUS_NONE
	if ResourceLoader.exists(stylebox_path):
		var sb := load(stylebox_path) as StyleBox
		if sb != null:
			btn.add_theme_stylebox_override("normal", sb)
			btn.add_theme_stylebox_override("hover", sb)
			btn.add_theme_stylebox_override("pressed", sb)
			btn.add_theme_stylebox_override("focus", sb)
			btn.add_theme_stylebox_override("disabled", sb)
	btn.mouse_entered.connect(func(): _animate_btn_hover(btn, true))
	btn.mouse_exited.connect(func(): _animate_btn_hover(btn, false))
	return btn

func _animate_btn_hover(btn: Button, hovering: bool) -> void:
	if not is_instance_valid(btn):
		return
	btn.pivot_offset = btn.size / 2.0
	var target_scale := Vector2(1.06, 1.06) if hovering else Vector2(1.0, 1.0)
	var target_modulate := Color(1.15, 1.15, 1.15) if hovering else Color(1, 1, 1)
	var tw := btn.create_tween().set_parallel(true)
	tw.tween_property(btn, "scale", target_scale, 0.12).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	tw.tween_property(btn, "modulate", target_modulate, 0.10)

func _on_ffa_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -6.0)
	GameState.current_mode = GameState.Mode.FFA
	NetManager.is_multiplayer = false
	LoadingScreen.preload_and_change_scene(ARENA_FFA_PATH, 0.8)

func _on_bvb_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -6.0)
	GameState.current_mode = GameState.Mode.BVB
	NetManager.is_multiplayer = false
	LoadingScreen.preload_and_change_scene(ARENA_BVB_PATH, 0.8)

func _on_online_ffa_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -6.0)
	GameState.current_mode = GameState.Mode.FFA
	LoadingScreen.preload_and_change_scene(LOBBY_PATH, 0.6)

func _on_online_bvb_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -6.0)
	GameState.current_mode = GameState.Mode.BVB
	LoadingScreen.preload_and_change_scene(LOBBY_PATH, 0.6)

func _make_online_button(text: String, tint: Color) -> Button:
	var btn := Button.new()
	btn.text = text
	btn.custom_minimum_size = Vector2(360, 80)
	btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	btn.focus_mode = Control.FOCUS_NONE
	btn.add_theme_font_size_override("font_size", 24)
	btn.add_theme_color_override("font_color", Color(1, 1, 1))
	btn.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	btn.add_theme_constant_override("outline_size", 6)
	var sb := StyleBoxFlat.new()
	sb.bg_color = Color(0.05, 0.07, 0.16, 0.95)
	sb.border_color = tint
	sb.set_border_width_all(3)
	sb.set_corner_radius_all(12)
	sb.shadow_color = Color(tint.r, tint.g, tint.b, 0.45)
	sb.shadow_size = 12
	var sb_hover := sb.duplicate() as StyleBoxFlat
	sb_hover.bg_color = Color(tint.r * 0.25, tint.g * 0.25, tint.b * 0.25, 0.95)
	btn.add_theme_stylebox_override("normal", sb)
	btn.add_theme_stylebox_override("hover", sb_hover)
	btn.add_theme_stylebox_override("pressed", sb_hover)
	btn.add_theme_stylebox_override("focus", sb)
	btn.add_theme_stylebox_override("disabled", sb)
	btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -12.0))
	btn.mouse_entered.connect(func(): _animate_btn_hover(btn, true))
	btn.mouse_exited.connect(func(): _animate_btn_hover(btn, false))
	return btn

func _on_back_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CLICK, -6.0)
	LoadingScreen.change_scene("res://scenes/main.tscn", 0.4)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_back"):
		_on_back_pressed()
