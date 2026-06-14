extends Control

# 4-Player Bomb FFA. Random player gets the bomb, a puzzle appears,
# the active player must solve before the timer expires. If they solve,
# the bomb passes to a random remaining player and a new (harder) puzzle
# appears. If they fail, that player is eliminated. Last one standing wins.

const PuzzlePipe := preload("res://scripts/puzzles/pipe_pulse.gd")
const PuzzleCircuit := preload("res://scripts/puzzles/circuit_scramble.gd")
const PuzzleCollapse := preload("res://scripts/puzzles/collapse_grid.gd")
const PuzzleComboRush := preload("res://scripts/puzzles/combo_rush.gd")
const PuzzleCrystal := preload("res://scripts/puzzles/crystal_rush.gd")
const PuzzleMaze := preload("res://scripts/puzzles/maze_rush.gd")
const PuzzleMirror := preload("res://scripts/puzzles/mirror_beam.gd")
const PuzzleKOTH := preload("res://scripts/puzzles/king_of_hill.gd")
const PuzzleScoreAttack := preload("res://scripts/puzzles/score_attack.gd")
const PuzzleShardStack := preload("res://scripts/puzzles/shard_stack.gd")
const PuzzleSpatialSpin := preload("res://scripts/puzzles/spatial_spin.gd")
const PuzzleRuleBreaker := preload("res://scripts/puzzles/rule_breaker.gd")
const Bomb := preload("res://scripts/bomb.gd")
const AIBotClass := preload("res://scripts/ai_bot.gd")
const PlayerCardClass := preload("res://scripts/player_card.gd")
const SummaryClass := preload("res://scripts/ui/post_match_summary.gd")

const TIMER_BASE: float = 22.0
const TIMER_MIN: float = 10.0
const TIMER_DECAY_PER_ROUND: float = 1.2

const LOCAL_PLAYER_INDEX: int = 0

const PUZZLE_NAMES: Array = [
	"Pipe Pulse",
	"Circuit Scramble",
	"Collapse Grid",
	"Combo Rush",
	"Crystal Rush",
	"Maze Rush",
	"Mirror Beam Mayhem",
	"King of the Hill",
	"Score Attack",
	"Shard Stack",
	"Spatial Spin",
	"Rule Breaker",
]

# Podium positions are EXPLICIT viewport-anchored offsets from center.
var podium_offsets: Array = [
	Vector2(-820, -400),  # top-left
	Vector2(820, -400),   # top-right
	Vector2(-820, 400),   # bottom-left
	Vector2(820, 400),    # bottom-right
]
var podium_size: Vector2 = Vector2(200, 280)
var center_panel_size: Vector2 = Vector2(700, 700)
var bomb_size: Vector2 = Vector2(150, 150)

var puzzle_classes: Array = []
var players_alive: Array[bool] = [true, true, true, true]
var elimination_order: Array[int] = []   # records players as they are eliminated
var podium_nodes: Array = []
var bomb: Control = null
var puzzle_slot: Control = null
var current_puzzle: PuzzleBase = null
var current_holder: int = -1
var round_number: int = 0
var ai_bots: Array = []
var timer_remaining: float = TIMER_BASE
var timer_running: bool = false
var game_over: bool = false

# Clutch tracking — fires when the LOCAL player solves with the bomb timer
# below this threshold. Counted toward bonus RP at match end.
const CLUTCH_TIMER_THRESHOLD: float = 3.0
const CLUTCH_RP_BONUS_EACH: int = 10
var clutch_count: int = 0
var local_solve_count: int = 0

var hud_label: Label = null
var puzzle_title_label: Label = null
var timer_bar: ProgressBar = null
var status_label: Label = null
var puzzle_panel: PanelContainer = null

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	if ResourceLoader.exists("res://assets/ui/theme.tres"):
		theme = load("res://assets/ui/theme.tres")
	puzzle_classes = [
		PuzzlePipe, PuzzleCircuit, PuzzleCollapse, PuzzleComboRush,
		PuzzleCrystal, PuzzleMaze, PuzzleMirror, PuzzleKOTH,
		PuzzleScoreAttack, PuzzleShardStack, PuzzleSpatialSpin, PuzzleRuleBreaker,
	]
	_build_background()
	_build_podiums()
	_build_center()
	_build_hud()
	_build_bomb()
	_build_ai_bots()
	_build_player_card()
	if NetManager.is_multiplayer:
		NetManager.message_received.connect(_on_net_message)
		NetManager.player_disconnected.connect(_on_net_disconnect)
	AudioManager.play_music(GameState.MUSIC_GAMEPLAY, -8.0, 0.6)
	await get_tree().create_timer(0.6).timeout
	# Host (or solo) starts the first round. Joiners wait for round_start.
	if not NetManager.is_multiplayer or NetManager.is_host:
		_start_next_round(randi() % 4)
	else:
		_show_status("WAITING FOR HOST...", Color(0.85, 0.95, 1.0), 2.0)

func _set_centered(node: Control, w: float, h: float, dx: float = 0.0, dy: float = 0.0) -> void:
	# Anchor at viewport center, then size via offsets. Works even when
	# the node is a non-Container child of this Control root.
	node.anchor_left = 0.5
	node.anchor_right = 0.5
	node.anchor_top = 0.5
	node.anchor_bottom = 0.5
	node.offset_left = dx - w / 2.0
	node.offset_right = dx + w / 2.0
	node.offset_top = dy - h / 2.0
	node.offset_bottom = dy + h / 2.0

func _build_background() -> void:
	var bg := TextureRect.new()
	if ResourceLoader.exists("res://assets/textures/backgrounds/arena_ffa_bg.png"):
		bg.texture = load("res://assets/textures/backgrounds/arena_ffa_bg.png")
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var overlay := ColorRect.new()
	overlay.color = Color(0.02, 0.04, 0.10, 0.55)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _build_podiums() -> void:
	podium_nodes.clear()
	for i in 4:
		var node := _make_podium(i)
		add_child(node)
		_set_centered(node, podium_size.x, podium_size.y, podium_offsets[i].x, podium_offsets[i].y)
		podium_nodes.append(node)

func _make_podium(idx: int) -> Control:
	var holder := Control.new()
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE

	# Glow halo
	var halo := _PodiumGlow.new()
	halo.tint = GameState.RIVAL_COLORS[idx]
	halo.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.add_child(halo)
	halo.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	# Avatar holder
	var avatar_size := Vector2(168, 168)
	var avatar_holder := Control.new()
	avatar_holder.custom_minimum_size = avatar_size
	avatar_holder.size = avatar_size
	avatar_holder.position = Vector2((podium_size.x - avatar_size.x) / 2.0, 16)
	avatar_holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.add_child(avatar_holder)

	if ResourceLoader.exists("res://assets/ui/frame_avatar.png"):
		var frame := TextureRect.new()
		frame.texture = load("res://assets/ui/frame_avatar.png")
		frame.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		frame.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		frame.mouse_filter = Control.MOUSE_FILTER_IGNORE
		frame.modulate = GameState.RIVAL_COLORS[idx].lerp(Color.WHITE, 0.3)
		avatar_holder.add_child(frame)
		frame.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	var sprite := TextureRect.new()
	if ResourceLoader.exists(GameState.RIVAL_SPRITES[idx]):
		sprite.texture = load(GameState.RIVAL_SPRITES[idx])
	sprite.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	sprite.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	sprite.mouse_filter = Control.MOUSE_FILTER_IGNORE
	avatar_holder.add_child(sprite)
	sprite.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	sprite.offset_left = 14
	sprite.offset_top = 14
	sprite.offset_right = -14
	sprite.offset_bottom = -14

	# Name label
	var name_label := Label.new()
	name_label.text = GameState.RIVAL_NAMES[idx]
	if idx == LOCAL_PLAYER_INDEX:
		name_label.text += " (YOU)"
	name_label.add_theme_font_size_override("font_size", 26)
	name_label.add_theme_color_override("font_color", GameState.RIVAL_COLORS[idx])
	name_label.add_theme_color_override("font_outline_color", Color(0, 0, 0, 0.9))
	name_label.add_theme_constant_override("outline_size", 6)
	name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.add_child(name_label)
	name_label.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	name_label.offset_top = -76
	name_label.offset_bottom = -42

	# Title label (subtitle)
	var title := Label.new()
	title.text = GameState.RIVAL_TITLES[idx]
	title.add_theme_font_size_override("font_size", 16)
	title.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0, 0.85))
	title.add_theme_color_override("font_outline_color", Color(0, 0, 0, 0.9))
	title.add_theme_constant_override("outline_size", 4)
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.mouse_filter = Control.MOUSE_FILTER_IGNORE
	holder.add_child(title)
	title.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	title.offset_top = -38
	title.offset_bottom = -12

	return holder

func _build_center() -> void:
	puzzle_panel = PanelContainer.new()
	puzzle_panel.theme_type_variation = "DialogPanel"
	puzzle_panel.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(puzzle_panel)
	_set_centered(puzzle_panel, center_panel_size.x, center_panel_size.y)

	puzzle_slot = CenterContainer.new()
	puzzle_slot.mouse_filter = Control.MOUSE_FILTER_PASS
	puzzle_panel.add_child(puzzle_slot)

func _build_hud() -> void:
	# Timer bar holder, anchored to top
	var bar_holder := Control.new()
	bar_holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bar_holder)
	bar_holder.anchor_left = 0.5
	bar_holder.anchor_right = 0.5
	bar_holder.anchor_top = 0.0
	bar_holder.anchor_bottom = 0.0
	bar_holder.offset_left = -480
	bar_holder.offset_right = 480
	bar_holder.offset_top = 24
	bar_holder.offset_bottom = 140

	timer_bar = ProgressBar.new()
	timer_bar.min_value = 0.0
	timer_bar.max_value = TIMER_BASE
	timer_bar.value = TIMER_BASE
	timer_bar.show_percentage = false
	var pb_bg := StyleBoxFlat.new()
	pb_bg.bg_color = Color(0.05, 0.06, 0.12, 0.9)
	pb_bg.border_color = Color(0.0, 0.90, 1.0, 0.7)
	pb_bg.set_border_width_all(3)
	pb_bg.set_corner_radius_all(10)
	var pb_fill := StyleBoxFlat.new()
	pb_fill.bg_color = Color(0.0, 0.90, 1.0)
	pb_fill.set_corner_radius_all(8)
	timer_bar.add_theme_stylebox_override("background", pb_bg)
	timer_bar.add_theme_stylebox_override("fill", pb_fill)
	bar_holder.add_child(timer_bar)
	timer_bar.anchor_left = 0
	timer_bar.anchor_right = 1
	timer_bar.anchor_top = 0
	timer_bar.anchor_bottom = 0
	timer_bar.offset_left = 0
	timer_bar.offset_right = 0
	timer_bar.offset_top = 0
	timer_bar.offset_bottom = 36

	hud_label = Label.new()
	hud_label.text = "ROUND 1"
	hud_label.add_theme_font_size_override("font_size", 28)
	hud_label.add_theme_color_override("font_color", Color(1, 1, 1))
	hud_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	hud_label.add_theme_constant_override("outline_size", 6)
	hud_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	bar_holder.add_child(hud_label)
	hud_label.anchor_left = 0
	hud_label.anchor_right = 1
	hud_label.anchor_top = 0
	hud_label.anchor_bottom = 0
	hud_label.offset_left = 0
	hud_label.offset_right = 0
	hud_label.offset_top = 44
	hud_label.offset_bottom = 80

	puzzle_title_label = Label.new()
	puzzle_title_label.text = ""
	puzzle_title_label.add_theme_font_size_override("font_size", 22)
	puzzle_title_label.add_theme_color_override("font_color", Color(0.0, 0.90, 1.0))
	puzzle_title_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	puzzle_title_label.add_theme_constant_override("outline_size", 5)
	puzzle_title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	bar_holder.add_child(puzzle_title_label)
	puzzle_title_label.anchor_left = 0
	puzzle_title_label.anchor_right = 1
	puzzle_title_label.anchor_top = 0
	puzzle_title_label.anchor_bottom = 0
	puzzle_title_label.offset_left = 0
	puzzle_title_label.offset_right = 0
	puzzle_title_label.offset_top = 82
	puzzle_title_label.offset_bottom = 114

	# Status banner
	status_label = Label.new()
	status_label.text = ""
	status_label.add_theme_font_size_override("font_size", 52)
	status_label.add_theme_color_override("font_color", Color(1, 0.95, 0.2))
	status_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	status_label.add_theme_constant_override("outline_size", 8)
	status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	status_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(status_label)
	status_label.anchor_left = 0
	status_label.anchor_right = 1
	status_label.anchor_top = 0
	status_label.anchor_bottom = 0
	status_label.offset_left = 0
	status_label.offset_right = 0
	status_label.offset_top = 170
	status_label.offset_bottom = 250

	# Back hint
	var back_hint := Label.new()
	back_hint.text = "ESC to leave round"
	back_hint.add_theme_font_size_override("font_size", 18)
	back_hint.add_theme_color_override("font_color", Color(0.7, 0.78, 0.95, 0.7))
	back_hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	add_child(back_hint)
	back_hint.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	back_hint.offset_left = 0
	back_hint.offset_right = -32
	back_hint.offset_top = -52
	back_hint.offset_bottom = -20

func _build_bomb() -> void:
	bomb = Bomb.new()
	add_child(bomb)
	_set_centered(bomb, bomb_size.x, bomb_size.y, 0.0, -center_panel_size.y / 2.0 - 80.0)
	bomb.set_lit(false)

func _build_ai_bots() -> void:
	ai_bots.clear()
	for i in 4:
		var bot: AIBot = AIBotClass.new()
		bot.skill = 0.7
		add_child(bot)
		ai_bots.append(bot)

func _build_player_card() -> void:
	var card: PlayerCard = PlayerCardClass.new()
	card.compact = true
	card.custom_minimum_size = Vector2(360, 110)
	card.size = card.custom_minimum_size
	add_child(card)
	card.anchor_left = 0.0
	card.anchor_right = 0.0
	card.anchor_top = 1.0
	card.anchor_bottom = 1.0
	card.offset_left = 24
	card.offset_right = 384
	card.offset_top = -134
	card.offset_bottom = -24
	card.bind_local()

func _move_bomb_to(holder: int) -> void:
	if not is_instance_valid(bomb):
		return
	var dx := 0.0
	var dy := -center_panel_size.y / 2.0 - 80.0
	if holder >= 0 and holder < podium_offsets.size():
		dx = podium_offsets[holder].x
		# Position above the podium
		dy = podium_offsets[holder].y - podium_size.y / 2.0 - 40.0
	# Snap centered position
	_set_centered(bomb, bomb_size.x, bomb_size.y, dx, dy)

func _start_next_round(holder: int) -> void:
	if game_over:
		return
	# Joiners must wait for the host to drive new rounds.
	if NetManager.is_multiplayer and not NetManager.is_host:
		return
	if not players_alive[holder]:
		holder = _pick_random_alive(holder)
		if holder < 0:
			_declare_winner()
			return
	var difficulty := 1 + round_number / 2
	var puzzle_idx: int = randi() % puzzle_classes.size()
	if NetManager.is_multiplayer:
		NetManager.send("round_start", {
			"holder": holder,
			"puzzle_idx": puzzle_idx,
			"difficulty": difficulty,
		})
	_begin_next_round(holder, puzzle_idx, difficulty)

func _begin_next_round(holder: int, puzzle_idx: int, difficulty: int) -> void:
	if game_over:
		return
	round_number += 1
	hud_label.text = "ROUND %d  -  %s HOLDS THE BOMB" % [round_number, GameState.RIVAL_NAMES[holder]]
	_move_bomb_to(holder)
	bomb.set_lit(true)
	AudioManager.play_sfx(GameState.SFX_BOMB_PASS, -6.0)
	current_holder = holder

	var puzzle_cls = puzzle_classes[puzzle_idx]
	puzzle_title_label.text = PUZZLE_NAMES[puzzle_idx].to_upper()
	_spawn_puzzle(puzzle_cls, difficulty)

	timer_remaining = maxf(TIMER_MIN, TIMER_BASE - float(round_number - 1) * TIMER_DECAY_PER_ROUND)
	timer_bar.max_value = timer_remaining
	timer_bar.value = timer_remaining
	# Only the host runs the authoritative timer in MP.
	timer_running = (not NetManager.is_multiplayer) or NetManager.is_host

	for i in 4:
		# In MP, only the holder's own client gets to interact.
		var local_is_holder := holder == LOCAL_PLAYER_INDEX
		if NetManager.is_multiplayer:
			local_is_holder = (holder == NetManager.local_player_id)
		if i == holder and players_alive[holder]:
			if local_is_holder:
				current_puzzle.input_enabled = true
			else:
				current_puzzle.input_enabled = false
				# Only host runs AI bots for AI-controlled slots.
				if NetManager.is_player_ai(i) or not NetManager.is_multiplayer:
					(ai_bots[i] as AIBot).bind_puzzle(current_puzzle, 0.65 + float(difficulty) * 0.06)
					(ai_bots[i] as AIBot).start()
				else:
					(ai_bots[i] as AIBot).stop()
		else:
			(ai_bots[i] as AIBot).stop()
	_show_status("ROUND %d" % round_number, GameState.RIVAL_COLORS[holder], 0.9)

func _spawn_puzzle(puzzle_cls: Resource, difficulty: int) -> void:
	if current_puzzle != null and is_instance_valid(current_puzzle):
		current_puzzle.queue_free()
		current_puzzle = null
	var p: PuzzleBase = puzzle_cls.new()
	puzzle_slot.add_child(p)
	p.setup(difficulty)
	p.solved.connect(_on_puzzle_solved)
	current_puzzle = p

func _on_puzzle_solved(_score: int) -> void:
	if game_over:
		return
	timer_running = false
	for bot in ai_bots:
		(bot as AIBot).stop()
	# Detect clutch — local player solved with bomb timer almost expired.
	var was_clutch: bool = false
	if current_holder == LOCAL_PLAYER_INDEX:
		local_solve_count += 1
		if timer_remaining <= CLUTCH_TIMER_THRESHOLD:
			clutch_count += 1
			was_clutch = true
	if was_clutch:
		_announce_clutch()
	_show_status("%s SOLVED IT!" % GameState.RIVAL_NAMES[current_holder], GameState.RIVAL_COLORS[current_holder], 1.0)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -3.0)
	# In MP, the holder's client tells the host they solved. Host advances.
	if NetManager.is_multiplayer and not NetManager.is_host:
		NetManager.send("ffa_solved", { "holder": current_holder })
		return
	await get_tree().create_timer(1.0).timeout
	if game_over or not is_instance_valid(self):
		return
	var next := _pick_random_alive(current_holder)
	if next < 0:
		_declare_winner()
		return
	_start_next_round(next)

func _explode_holder() -> void:
	if game_over:
		return
	timer_running = false
	for bot in ai_bots:
		(bot as AIBot).stop()
	var loser := current_holder
	players_alive[loser] = false
	if not elimination_order.has(loser):
		elimination_order.append(loser)
	AudioManager.play_sfx(GameState.SFX_BOMB_EXPLOSION, -2.0)
	_show_status("%s ELIMINATED!" % GameState.RIVAL_NAMES[loser], Color(1, 0.3, 0.2), 1.5)
	if NetManager.is_multiplayer and NetManager.is_host:
		NetManager.send("ffa_explode", { "victim": loser })
	var podium: Control = podium_nodes[loser]
	if is_instance_valid(podium):
		var tw := podium.create_tween()
		tw.tween_property(podium, "modulate", Color(0.3, 0.3, 0.3, 0.45), 0.4)
	_spawn_explosion_at_podium(loser)
	await get_tree().create_timer(1.6).timeout
	if game_over or not is_instance_valid(self):
		return
	if _count_alive() <= 1:
		_declare_winner()
		return
	var next := _pick_random_alive(loser)
	if next < 0:
		_declare_winner()
		return
	_start_next_round(next)

func _spawn_explosion_at_podium(idx: int) -> void:
	var podium: Control = podium_nodes[idx]
	if not is_instance_valid(podium):
		return
	var sprite := TextureRect.new()
	if ResourceLoader.exists("res://assets/sprites/effects/explosion.png"):
		sprite.texture = load("res://assets/sprites/effects/explosion.png")
	sprite.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	sprite.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	sprite.mouse_filter = Control.MOUSE_FILTER_IGNORE
	sprite.custom_minimum_size = Vector2(320, 320)
	sprite.size = sprite.custom_minimum_size
	add_child(sprite)
	sprite.global_position = podium.global_position + podium.size / 2.0 - sprite.size / 2.0
	sprite.pivot_offset = sprite.size / 2.0
	sprite.scale = Vector2(0.3, 0.3)
	sprite.modulate = Color(1, 1, 1, 1)
	var tw := sprite.create_tween().set_parallel(true)
	tw.tween_property(sprite, "scale", Vector2(1.6, 1.6), 0.5).set_trans(Tween.TRANS_CUBIC)
	tw.tween_property(sprite, "modulate:a", 0.0, 0.7).set_delay(0.2)
	tw.chain().tween_callback(func():
		if is_instance_valid(sprite):
			sprite.queue_free()
	)

func _count_alive() -> int:
	var n := 0
	for a in players_alive:
		if a:
			n += 1
	return n

func _pick_random_alive(exclude: int) -> int:
	var pool: Array = []
	for i in 4:
		if players_alive[i] and i != exclude:
			pool.append(i)
	if pool.is_empty():
		for i in 4:
			if players_alive[i]:
				return i
		return -1
	return pool[randi() % pool.size()]

func _declare_winner() -> void:
	game_over = true
	timer_running = false
	for bot in ai_bots:
		(bot as AIBot).stop()
	var winner_idx := -1
	for i in 4:
		if players_alive[i]:
			winner_idx = i
			break
	if winner_idx < 0:
		_show_status("DRAW", Color.WHITE, 4.0)
	else:
		_show_status("%s WINS!" % GameState.RIVAL_NAMES[winner_idx], GameState.RIVAL_COLORS[winner_idx], 4.0)
	var local_won: bool = winner_idx == LOCAL_PLAYER_INDEX
	if local_won:
		AudioManager.play_music(GameState.JINGLE_WIN, -4.0, 0.3)
	else:
		AudioManager.play_music(GameState.JINGLE_LOSE, -4.0, 0.3)
	# Award rank points based on placement plus clutch bonuses.
	var local_alive: bool = players_alive[LOCAL_PLAYER_INDEX]
	var base_rp: int = 0
	if local_won:
		base_rp = 90
	elif local_alive:
		base_rp = 50
	else:
		base_rp = 25
	var clutch_bonus: int = clutch_count * CLUTCH_RP_BONUS_EACH
	var total_rp: int = base_rp + clutch_bonus
	var prev_level: int = PlayerProfile.level
	PlayerProfile.award_rp(total_rp, local_won)
	var levels_gained: int = PlayerProfile.level - prev_level

	var placement: int = _local_placement()
	var place_color: Color = GameState.RIVAL_COLORS[0] if placement == 1 else Color(0.85, 0.95, 1.0)
	var accent: Color = GameState.RIVAL_COLORS[0]
	var headline: String = "VICTORY"
	var subtitle: String = "Last brain standing"
	if not local_won:
		if winner_idx >= 0:
			accent = GameState.RIVAL_COLORS[winner_idx]
			headline = "DEFEAT"
			subtitle = "%s wins  •  you placed %s" % [GameState.RIVAL_NAMES[winner_idx], _ordinal(placement)]
		else:
			accent = Color(0.85, 0.95, 1.0)
			headline = "MATCH OVER"
			subtitle = ""
	var rp_lines: Array = [{"label": "Base", "amount": base_rp, "color": Color(1, 1, 1)}]
	if clutch_bonus > 0:
		rp_lines.append({"label": "Clutch x%d" % clutch_count, "amount": clutch_bonus, "color": Color(1.0, 0.4, 0.1)})
	_show_summary({
		"won": local_won,
		"headline": headline,
		"subtitle": subtitle,
		"accent": accent,
		"stats": [
			{"name": "PLACEMENT", "text": _ordinal(placement), "color": place_color},
			{"name": "SOLVES", "value": local_solve_count, "color": Color(0.0, 0.90, 1.0)},
			{"name": "CLUTCHES", "value": clutch_count, "color": Color(1.0, 0.54, 0.12)},
			{"name": "ROUNDS", "value": round_number, "color": Color(0.61, 1.0, 0.18)},
		],
		"rp_lines": rp_lines,
		"rp_total": total_rp,
		"levels_gained": levels_gained,
	})

func _local_placement() -> int:
	# 1st = survived to the end; otherwise derived from elimination order
	# (first eliminated places last).
	if players_alive[LOCAL_PLAYER_INDEX]:
		return 1
	var idx: int = elimination_order.find(LOCAL_PLAYER_INDEX)
	if idx >= 0:
		return 4 - idx
	return maxi(2, _count_alive() + 1)

func _ordinal(n: int) -> String:
	match n:
		1: return "1st"
		2: return "2nd"
		3: return "3rd"
		_: return "%dth" % n

func _show_summary(cfg: Dictionary) -> void:
	var summary: PostMatchSummary = SummaryClass.new()
	add_child(summary)
	summary.continue_pressed.connect(_on_summary_continue)
	summary.present(cfg)

func _on_summary_continue() -> void:
	if not is_instance_valid(self):
		return
	LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.6)

func _announce_clutch() -> void:
	var toast := Label.new()
	toast.text = "CLUTCH SOLVE!"
	toast.add_theme_font_size_override("font_size", 56)
	toast.add_theme_color_override("font_color", Color(1.0, 0.4, 0.1))
	toast.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	toast.add_theme_constant_override("outline_size", 9)
	toast.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	toast.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(toast)
	toast.anchor_left = 0
	toast.anchor_right = 1
	toast.anchor_top = 0.5
	toast.anchor_bottom = 0.5
	toast.offset_left = 0
	toast.offset_right = 0
	toast.offset_top = -240
	toast.offset_bottom = -160
	toast.modulate = Color(1, 1, 1, 0)
	toast.pivot_offset = Vector2(960, 40)
	toast.scale = Vector2(0.7, 0.7)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -1.5, 0.05)
	var tw := toast.create_tween().set_parallel(true)
	tw.tween_property(toast, "modulate:a", 1.0, 0.18)
	tw.tween_property(toast, "scale", Vector2(1.0, 1.0), 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.chain().tween_interval(1.2)
	tw.chain().tween_property(toast, "modulate:a", 0.0, 0.4)
	tw.chain().tween_callback(func():
		if is_instance_valid(toast):
			toast.queue_free()
	)

func _show_status(text: String, color: Color, duration: float = 1.0) -> void:
	if status_label == null or not is_instance_valid(status_label):
		return
	status_label.text = text
	status_label.modulate = Color(color.r, color.g, color.b, 0.0)
	var tw := status_label.create_tween()
	tw.tween_property(status_label, "modulate:a", 1.0, 0.2)
	tw.tween_interval(duration)
	tw.tween_property(status_label, "modulate:a", 0.0, 0.25)

func _process(delta: float) -> void:
	if timer_running and not game_over:
		timer_remaining = maxf(0.0, timer_remaining - delta)
		if timer_bar != null and is_instance_valid(timer_bar):
			timer_bar.value = timer_remaining
			var fill := timer_bar.get_theme_stylebox("fill") as StyleBoxFlat
			if fill != null:
				var t: float = timer_remaining / maxf(timer_bar.max_value, 0.01)
				fill.bg_color = Color(0.0, 0.90, 1.0).lerp(Color(1.0, 0.2, 0.2), 1.0 - t)
			if timer_remaining <= 5.0 and int(timer_remaining * 10.0) % 10 == 0:
				AudioManager.play_sfx(GameState.SFX_BOMB_TICK, -14.0, 0.1)
		if timer_remaining <= 0.0:
			timer_running = false
			_explode_holder()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_back"):
		game_over = true
		timer_running = false
		for bot in ai_bots:
			(bot as AIBot).stop()
		LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.4)


# --- Network handlers (multiplayer) ---

func _on_net_message(type: String, msg: Dictionary, from_player: int) -> void:
	if from_player == NetManager.local_player_id:
		return
	match type:
		"round_start":
			if NetManager.is_host:
				return
			_begin_next_round(int(msg.get("holder", 0)), int(msg.get("puzzle_idx", 0)), int(msg.get("difficulty", 1)))
		"ffa_solved":
			if not NetManager.is_host:
				return
			# Host advances the round after a brief beat.
			timer_running = false
			for bot in ai_bots:
				(bot as AIBot).stop()
			_show_status("%s SOLVED IT!" % GameState.RIVAL_NAMES[current_holder], GameState.RIVAL_COLORS[current_holder], 1.0)
			await get_tree().create_timer(1.0).timeout
			if game_over or not is_instance_valid(self):
				return
			var next := _pick_random_alive(current_holder)
			if next < 0:
				_declare_winner()
				return
			_start_next_round(next)
		"ffa_explode":
			if NetManager.is_host:
				return
			_apply_remote_explode(int(msg.get("victim", 0)))
		"ffa_winner":
			if NetManager.is_host:
				return
			game_over = true
			_show_status("MATCH OVER", Color.WHITE, 4.0)
			await get_tree().create_timer(4.0).timeout
			if not is_instance_valid(self):
				return
			LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.6)

func _on_net_disconnect(pid: int) -> void:
	# Convert dropped player to an AI slot so the match can continue.
	if game_over or not NetManager.is_multiplayer:
		return
	if pid >= 0 and pid < 4 and not NetManager.ai_slots.has(pid):
		NetManager.ai_slots.append(pid)
		_show_status("%s LEFT — AI TAKES OVER" % GameState.RIVAL_NAMES[pid], Color(1.0, 0.92, 0.18), 1.4)

func _apply_remote_explode(victim: int) -> void:
	if game_over or victim < 0 or victim >= 4:
		return
	players_alive[victim] = false
	if not elimination_order.has(victim):
		elimination_order.append(victim)
	AudioManager.play_sfx(GameState.SFX_BOMB_EXPLOSION, -2.0)
	_show_status("%s ELIMINATED!" % GameState.RIVAL_NAMES[victim], Color(1, 0.3, 0.2), 1.5)
	var podium: Control = podium_nodes[victim]
	if is_instance_valid(podium):
		var tw := podium.create_tween()
		tw.tween_property(podium, "modulate", Color(0.3, 0.3, 0.3, 0.45), 0.4)
	_spawn_explosion_at_podium(victim)


# --- Podium glow primitive ---

class _PodiumGlow extends Control:
	var tint: Color = Color.WHITE

	func _ready() -> void:
		var tw := create_tween().set_loops()
		tw.tween_method(func(v: float):
			if is_instance_valid(self):
				modulate = Color(1, 1, 1, 0.85 + 0.15 * v)
				queue_redraw()
		, 0.0, 1.0, 1.2)
		tw.tween_method(func(v: float):
			if is_instance_valid(self):
				modulate = Color(1, 1, 1, 1.0 - 0.15 * v)
				queue_redraw()
		, 0.0, 1.0, 1.2)

	func _draw() -> void:
		var rect := Rect2(Vector2.ZERO, size)
		for i in 4:
			var inset := -8.0 + i * 4.0
			draw_rect(rect.grow(inset), Color(tint.r, tint.g, tint.b, 0.18 - i * 0.04), false, 3.0)
		draw_rect(rect.grow(-2.0), Color(0.04, 0.05, 0.12, 0.85), true)
		draw_rect(rect.grow(-2.0), tint, false, 2.5)
