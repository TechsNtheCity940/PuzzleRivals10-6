extends Control

# Auto-matchmaking lobby — no codes, no host/join UI. The player clicks
# Online and immediately enters a match-finding screen. After a brief
# countdown the empty slots are filled with AI rivals so the match never
# stalls.

const AI_FALLBACK_SECONDS: float = 12.0

enum Mode { FFA, BVB }

var pending_mode: int = Mode.BVB
var pending_max_players: int = 2

var fallback_timer_remaining: float = AI_FALLBACK_SECONDS
var fallback_active: bool = false
var transitioning: bool = false

var status_label: Label = null
var player_count_label: Label = null
var countdown_label: Label = null
var pulse_label: Label = null
var spinner_dots: int = 0
var spinner_timer: float = 0.0

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	if ResourceLoader.exists("res://assets/ui/theme.tres"):
		theme = load("res://assets/ui/theme.tres")
	if GameState.current_mode == GameState.Mode.FFA:
		pending_mode = Mode.FFA
		pending_max_players = 4
	else:
		pending_mode = Mode.BVB
		pending_max_players = 2

	NetManager.reset()
	NetManager.max_players = pending_max_players
	NetManager.is_multiplayer = true
	NetManager.room_created.connect(_on_room_created)
	NetManager.player_joined.connect(_on_player_joined)
	NetManager.all_players_joined.connect(_on_all_players_joined)
	NetManager.connection_failed.connect(_on_connection_failed)

	_build_background()
	_build_ui()
	# Kick off matchmaking immediately.
	NetManager.create_room(pending_max_players)
	fallback_timer_remaining = AI_FALLBACK_SECONDS
	fallback_active = true

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
	overlay.color = Color(0.04, 0.06, 0.12, 0.68)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _build_ui() -> void:
	var center := CenterContainer.new()
	center.mouse_filter = Control.MOUSE_FILTER_PASS
	add_child(center)
	center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var vbox := VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	vbox.add_theme_constant_override("separation", 22)
	center.add_child(vbox)

	var title := Label.new()
	title.text = "ONLINE %s" % ("4-PLAYER FFA" if pending_mode == Mode.FFA else "1v1 DUEL")
	title.add_theme_font_size_override("font_size", 58)
	title.add_theme_color_override("font_color", Color(0.0, 0.90, 1.0))
	title.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	title.add_theme_constant_override("outline_size", 8)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(title)

	status_label = Label.new()
	status_label.text = "MATCHMAKING"
	status_label.add_theme_font_size_override("font_size", 42)
	status_label.add_theme_color_override("font_color", Color(0.61, 1.0, 0.18))
	status_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	status_label.add_theme_constant_override("outline_size", 6)
	status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(status_label)

	pulse_label = Label.new()
	pulse_label.text = "Finding rivals"
	pulse_label.add_theme_font_size_override("font_size", 30)
	pulse_label.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	pulse_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	pulse_label.add_theme_constant_override("outline_size", 5)
	pulse_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(pulse_label)

	player_count_label = Label.new()
	player_count_label.text = "Players: 1 / %d" % pending_max_players
	player_count_label.add_theme_font_size_override("font_size", 26)
	player_count_label.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0, 0.9))
	player_count_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(player_count_label)

	countdown_label = Label.new()
	countdown_label.text = "Filling empty slots with AI rivals in %ds..." % int(AI_FALLBACK_SECONDS)
	countdown_label.add_theme_font_size_override("font_size", 22)
	countdown_label.add_theme_color_override("font_color", Color(1.0, 0.92, 0.18))
	countdown_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	vbox.add_child(countdown_label)

	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(0, 24)
	vbox.add_child(spacer)

	# Cancel
	var cancel_btn := Button.new()
	cancel_btn.text = "CANCEL"
	cancel_btn.custom_minimum_size = Vector2(360, 70)
	cancel_btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	cancel_btn.focus_mode = Control.FOCUS_NONE
	cancel_btn.add_theme_font_size_override("font_size", 24)
	cancel_btn.add_theme_color_override("font_color", Color(1, 1, 1))
	cancel_btn.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	cancel_btn.add_theme_constant_override("outline_size", 5)
	if ResourceLoader.exists("res://assets/ui/btn_quit.tres"):
		var sb := load("res://assets/ui/btn_quit.tres") as StyleBox
		if sb != null:
			cancel_btn.add_theme_stylebox_override("normal", sb)
			cancel_btn.add_theme_stylebox_override("hover", sb)
			cancel_btn.add_theme_stylebox_override("pressed", sb)
			cancel_btn.add_theme_stylebox_override("focus", sb)
	cancel_btn.pressed.connect(_on_cancel_pressed)
	cancel_btn.mouse_entered.connect(func(): AudioManager.play_sfx(GameState.SFX_UI_HOVER, -12.0))
	vbox.add_child(cancel_btn)

func _on_cancel_pressed() -> void:
	AudioManager.play_sfx(GameState.SFX_UI_CLICK, -8.0)
	fallback_active = false
	NetManager.reset()
	NetManager.is_multiplayer = false
	LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.4)

func _on_room_created(_code: String) -> void:
	# Don't surface the code — auto-matchmaking hides it.
	NetManager.is_multiplayer = true

func _on_player_joined(_player_id: int, total_players: int) -> void:
	if player_count_label != null and is_instance_valid(player_count_label):
		player_count_label.text = "Players: %d / %d" % [total_players, pending_max_players]
	AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -8.0)

func _on_all_players_joined() -> void:
	NetManager.ai_slots.clear()
	if status_label != null and is_instance_valid(status_label):
		status_label.text = "MATCH FOUND!"
	if pulse_label != null and is_instance_valid(pulse_label):
		pulse_label.text = "Launching..."
	_transition_to_arena()

func _on_connection_failed(_reason: String) -> void:
	# Matchmaker unreachable — fall back to solo AI right now.
	if status_label != null and is_instance_valid(status_label):
		status_label.text = "OFFLINE — Playing vs AI"
	NetManager.ai_slots.clear()
	for pid in range(pending_max_players):
		if pid != NetManager.local_player_id:
			NetManager.ai_slots.append(pid)
	_transition_to_arena()

func _apply_ai_fallback_and_start() -> void:
	if transitioning:
		return
	# Mark every non-local slot as AI.
	NetManager.ai_slots.clear()
	for pid in range(pending_max_players):
		if pid >= NetManager.connected_players:
			NetManager.ai_slots.append(pid)
	# Broadcast for joiners (if any).
	NetManager.send("ai_slots", { "slots": NetManager.ai_slots })
	if status_label != null and is_instance_valid(status_label):
		status_label.text = "AI RIVALS DEPLOYED"
	if pulse_label != null and is_instance_valid(pulse_label):
		pulse_label.text = "Launching..."
	_transition_to_arena()

func _transition_to_arena() -> void:
	if transitioning:
		return
	transitioning = true
	fallback_active = false
	await get_tree().create_timer(0.5).timeout
	if not is_instance_valid(self):
		return
	var target := "res://scenes/arena_bvb.tscn" if pending_mode == Mode.BVB else "res://scenes/arena_ffa.tscn"
	LoadingScreen.preload_and_change_scene(target, 0.8)

func _process(delta: float) -> void:
	# Pulsing "Finding rivals..." dots
	spinner_timer += delta
	if spinner_timer >= 0.45:
		spinner_timer = 0.0
		spinner_dots = (spinner_dots + 1) % 4
		if pulse_label != null and is_instance_valid(pulse_label) and not transitioning:
			pulse_label.text = "Finding rivals" + ".".repeat(spinner_dots)

	if not fallback_active or transitioning:
		return
	fallback_timer_remaining = maxf(0.0, fallback_timer_remaining - delta)
	if countdown_label != null and is_instance_valid(countdown_label):
		if NetManager.connected_players >= pending_max_players:
			countdown_label.text = "All players ready!"
		else:
			countdown_label.text = "Filling empty slots with AI rivals in %ds..." % int(ceil(fallback_timer_remaining))
	if fallback_timer_remaining <= 0.0:
		_apply_ai_fallback_and_start()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_back"):
		_on_cancel_pressed()
