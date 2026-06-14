extends Control

# Brain VS Brain — 1v1 puzzle duel with live attack and defense.
#
# Each side has an Attack Meter that fills as they solve pieces / rounds.
# When it hits 100, a random sabotage attack fires at the opponent — screen
# shake, freeze, rescramble, fog, or dazzle. The opponent can block or mirror
# the attack if they have a defense charge, which they earn while behind on
# score. First to TARGET_SCORE solves wins the match.

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
const AIBotClass := preload("res://scripts/ai_bot.gd")
const PlayerCardClass := preload("res://scripts/player_card.gd")
const SummaryClass := preload("res://scripts/ui/post_match_summary.gd")

const TARGET_SCORE: int = 5
const LOCAL_PLAYER_INDEX: int = 0
const PANEL_SIZE: Vector2 = Vector2(800, 760)
const PANEL_X: float = 470.0

const ATTACK_METER_MAX: float = 100.0
const ATTACK_GAIN_PER_PIECE: float = 5.0
const ATTACK_GAIN_PER_SOLVE: float = 35.0

const DEFENSE_GAIN_PER_PIECE_BEHIND: float = 0.22
const DEFENSE_GAIN_PER_SOLVE_BEHIND: float = 0.5
const DEFENSE_MAX: float = 3.0

const ATTACK_SHAKE: int = 0
const ATTACK_FREEZE: int = 1
const ATTACK_RESCRAMBLE: int = 2
const ATTACK_FOG: int = 3
const ATTACK_DAZZLE: int = 4
const ATTACK_NAMES: Array = ["SHAKE", "FREEZE", "RESCRAMBLE", "FOG", "DAZZLE"]

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

var scores: Array[int] = [0, 0]
var attack_meters: Array[float] = [0.0, 0.0]
var defense_charges: Array[float] = [0.0, 0.0]
var current_puzzles: Array = [null, null]
var puzzle_slots: Array = [null, null]
var puzzle_panels: Array = [null, null]
var current_puzzle_class: Resource = null
var current_difficulty: int = 1

var puzzle_classes: Array = []
var ai_bot: AIBot = null
var round_number: int = 0
var game_over: bool = false

# --- Combo + Clutch tracking (LOCAL player only) ---
const COMBO_WINDOW: float = 9.0        # back-to-back solves within this many seconds extend combo
const CLUTCH_THRESHOLD: float = 6.0    # solve within this many seconds of puzzle spawn = clutch
const COMBO_MAX: int = 5
const COMBO_RP_BONUS_PER_TIER: int = 6
const CLUTCH_RP_BONUS_EACH: int = 8

var combo_count: int = 0
var max_combo: int = 0
var last_solve_time: float = -1000.0
var puzzle_spawn_time: float = 0.0
var clutch_count: int = 0
var combo_label: Label = null

# Match stat counters (LOCAL player) for the post-match summary.
var attacks_fired: int = 0
var defenses_made: int = 0

var hud_label: Label = null
var puzzle_title_label: Label = null
var score_meter_bar: Control = null
var left_score_label: Label = null
var right_score_label: Label = null
var status_label: Label = null
var attack_bars: Array = [null, null]
var defense_labels: Array = [null, null]
var attack_meter_labels: Array = [null, null]

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
	_build_layout()
	_build_hud()
	_build_ai()
	_build_player_card()
	if NetManager.is_multiplayer:
		NetManager.message_received.connect(_on_net_message)
		NetManager.player_disconnected.connect(_on_net_disconnect)
	AudioManager.play_music(GameState.MUSIC_GAMEPLAY, -8.0, 0.6)
	await get_tree().create_timer(0.6).timeout
	# Host (or solo) starts the first round. Joiners wait for round_start broadcast.
	if not NetManager.is_multiplayer or NetManager.is_host:
		_start_round()
	else:
		_show_status("WAITING FOR HOST...", Color(0.85, 0.95, 1.0), 2.0)

func _set_centered(node: Control, w: float, h: float, dx: float = 0.0, dy: float = 0.0) -> void:
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
	if ResourceLoader.exists("res://assets/textures/backgrounds/arena_bvb_bg.png"):
		bg.texture = load("res://assets/textures/backgrounds/arena_bvb_bg.png")
	bg.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	bg.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(bg)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	var overlay := ColorRect.new()
	overlay.color = Color(0.02, 0.04, 0.10, 0.60)
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func _build_layout() -> void:
	for side in 2:
		puzzle_slots[side] = _make_side_panel(side)

func _make_side_panel(side: int) -> Control:
	var x_offset: float = -PANEL_X if side == 0 else PANEL_X
	var panel := PanelContainer.new()
	panel.theme_type_variation = "DialogPanel"
	panel.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(panel)
	_set_centered(panel, PANEL_SIZE.x, PANEL_SIZE.y, x_offset, 80.0)
	puzzle_panels[side] = panel

	# Title strip ABOVE the panel
	var color: Color = GameState.RIVAL_COLORS[0] if side == 0 else GameState.RIVAL_COLORS[1]
	var label := Label.new()
	label.text = ("%s (YOU)" % GameState.RIVAL_NAMES[0]) if side == LOCAL_PLAYER_INDEX else "RIVAL: %s" % GameState.RIVAL_NAMES[1]
	label.add_theme_font_size_override("font_size", 32)
	label.add_theme_color_override("font_color", color)
	label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	label.add_theme_constant_override("outline_size", 6)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(label)
	_set_centered(label, PANEL_SIZE.x, 50.0, x_offset, 80.0 - PANEL_SIZE.y / 2.0 - 42.0)

	var center := CenterContainer.new()
	center.mouse_filter = Control.MOUSE_FILTER_PASS
	panel.add_child(center)
	return center

func _build_hud() -> void:
	var top_band := Control.new()
	top_band.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(top_band)
	top_band.anchor_left = 0.5
	top_band.anchor_right = 0.5
	top_band.anchor_top = 0.0
	top_band.anchor_bottom = 0.0
	top_band.offset_left = -640
	top_band.offset_right = 640
	top_band.offset_top = 10
	top_band.offset_bottom = 260

	# Score meter
	var bar := _ScoreMeter.new()
	bar.left_color = GameState.RIVAL_COLORS[0]
	bar.right_color = GameState.RIVAL_COLORS[1]
	bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top_band.add_child(bar)
	bar.anchor_left = 0.5
	bar.anchor_right = 0.5
	bar.offset_left = -440
	bar.offset_right = 440
	bar.offset_top = 0
	bar.offset_bottom = 38
	score_meter_bar = bar

	left_score_label = _make_score_label(GameState.RIVAL_COLORS[0])
	top_band.add_child(left_score_label)
	left_score_label.anchor_left = 0.5
	left_score_label.anchor_right = 0.5
	left_score_label.offset_left = -580
	left_score_label.offset_right = -440
	left_score_label.offset_top = -10
	left_score_label.offset_bottom = 60

	right_score_label = _make_score_label(GameState.RIVAL_COLORS[1])
	top_band.add_child(right_score_label)
	right_score_label.anchor_left = 0.5
	right_score_label.anchor_right = 0.5
	right_score_label.offset_left = 440
	right_score_label.offset_right = 580
	right_score_label.offset_top = -10
	right_score_label.offset_bottom = 60

	# Attack meter bars + labels
	for side in 2:
		var color: Color = GameState.RIVAL_COLORS[side]
		var bar_holder := Control.new()
		bar_holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
		top_band.add_child(bar_holder)
		bar_holder.anchor_left = 0.5
		bar_holder.anchor_right = 0.5
		bar_holder.offset_top = 50
		bar_holder.offset_bottom = 80
		if side == 0:
			bar_holder.offset_left = -440
			bar_holder.offset_right = -40
		else:
			bar_holder.offset_left = 40
			bar_holder.offset_right = 440

		var atk_bar := _AttackBar.new()
		atk_bar.tint = color
		atk_bar.fill_from_right = (side == 1)
		atk_bar.mouse_filter = Control.MOUSE_FILTER_IGNORE
		bar_holder.add_child(atk_bar)
		atk_bar.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		attack_bars[side] = atk_bar

		# ATK label inside the bar
		var atk_label := Label.new()
		atk_label.text = "ATK 0%"
		atk_label.add_theme_font_size_override("font_size", 18)
		atk_label.add_theme_color_override("font_color", Color(1, 1, 1))
		atk_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
		atk_label.add_theme_constant_override("outline_size", 4)
		atk_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		atk_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		atk_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		bar_holder.add_child(atk_label)
		atk_label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		attack_meter_labels[side] = atk_label

	# Defense charge indicators
	for side in 2:
		var color: Color = GameState.RIVAL_COLORS[side]
		var def_label := Label.new()
		def_label.text = "DEF: 0"
		def_label.add_theme_font_size_override("font_size", 22)
		def_label.add_theme_color_override("font_color", color)
		def_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
		def_label.add_theme_constant_override("outline_size", 5)
		def_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		def_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		def_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
		top_band.add_child(def_label)
		def_label.anchor_left = 0.5
		def_label.anchor_right = 0.5
		def_label.offset_top = 86
		def_label.offset_bottom = 118
		if side == 0:
			def_label.offset_left = -440
			def_label.offset_right = -40
		else:
			def_label.offset_left = 40
			def_label.offset_right = 440
		defense_labels[side] = def_label

	# First-to-N hint
	hud_label = Label.new()
	hud_label.text = "FIRST TO %d SOLVES WINS  -  ROUND 1" % TARGET_SCORE
	hud_label.add_theme_font_size_override("font_size", 22)
	hud_label.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
	hud_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	hud_label.add_theme_constant_override("outline_size", 5)
	hud_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hud_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top_band.add_child(hud_label)
	hud_label.anchor_left = 0
	hud_label.anchor_right = 1
	hud_label.offset_left = 0
	hud_label.offset_right = 0
	hud_label.offset_top = 128
	hud_label.offset_bottom = 158

	puzzle_title_label = Label.new()
	puzzle_title_label.text = ""
	puzzle_title_label.add_theme_font_size_override("font_size", 22)
	puzzle_title_label.add_theme_color_override("font_color", Color(0.0, 0.90, 1.0))
	puzzle_title_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	puzzle_title_label.add_theme_constant_override("outline_size", 5)
	puzzle_title_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	puzzle_title_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top_band.add_child(puzzle_title_label)
	puzzle_title_label.anchor_left = 0
	puzzle_title_label.anchor_right = 1
	puzzle_title_label.offset_left = 0
	puzzle_title_label.offset_right = 0
	puzzle_title_label.offset_top = 160
	puzzle_title_label.offset_bottom = 188

	# Status banner
	status_label = Label.new()
	status_label.text = ""
	status_label.add_theme_font_size_override("font_size", 56)
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
	status_label.offset_top = 235
	status_label.offset_bottom = 315

	# Back hint
	var back_hint := Label.new()
	back_hint.text = "ESC to leave duel"
	back_hint.add_theme_font_size_override("font_size", 18)
	back_hint.add_theme_color_override("font_color", Color(0.7, 0.78, 0.95, 0.7))
	back_hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	add_child(back_hint)
	back_hint.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
	back_hint.offset_left = 0
	back_hint.offset_right = -32
	back_hint.offset_top = -52
	back_hint.offset_bottom = -20

func _make_score_label(color: Color) -> Label:
	var label := Label.new()
	label.text = "0"
	label.add_theme_font_size_override("font_size", 52)
	label.add_theme_color_override("font_color", color)
	label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	label.add_theme_constant_override("outline_size", 6)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	return label

func _build_ai() -> void:
	ai_bot = AIBotClass.new()
	ai_bot.skill = 0.85
	add_child(ai_bot)

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

func _start_round() -> void:
	if game_over:
		return
	if NetManager.is_multiplayer and not NetManager.is_host:
		# Joiners wait for the host's round_start broadcast.
		return
	round_number += 1
	var puzzle_idx: int = randi() % puzzle_classes.size()
	var difficulty: int = 1 + (round_number - 1) / 2
	if NetManager.is_multiplayer:
		NetManager.send("round_start", {
			"puzzle_idx": puzzle_idx,
			"difficulty": difficulty,
			"round": round_number,
		})
	_begin_round(puzzle_idx, difficulty)

func _begin_round(puzzle_idx: int, difficulty: int) -> void:
	if game_over:
		return
	if not NetManager.is_host:
		# Joiner uses the host-supplied round number.
		round_number += 1
	current_puzzle_class = puzzle_classes[puzzle_idx]
	current_difficulty = difficulty
	if puzzle_title_label != null and is_instance_valid(puzzle_title_label):
		puzzle_title_label.text = "PUZZLE: %s" % PUZZLE_NAMES[puzzle_idx].to_upper()
	if hud_label != null and is_instance_valid(hud_label):
		hud_label.text = "FIRST TO %d SOLVES WINS  -  ROUND %d" % [TARGET_SCORE, round_number]
	_spawn_puzzle(0, current_puzzle_class, current_difficulty)
	if _opponent_is_local_ai():
		# Solo or AI fallback — local AI plays the right side.
		_spawn_puzzle(1, current_puzzle_class, current_difficulty)
		ai_bot.bind_puzzle(current_puzzles[1], 0.7 + float(current_difficulty) * 0.05)
		ai_bot.start()
	else:
		# Live human opponent — they solve on their own machine. Show a
		# placeholder where the AI puzzle used to be.
		_spawn_rival_placeholder()
		ai_bot.stop()
	_show_status("ROUND %d" % round_number, Color.WHITE, 0.7)

func _opponent_is_local_ai() -> bool:
	if not NetManager.is_multiplayer:
		return true
	var opp_id: int = 1 - NetManager.local_player_id
	return NetManager.ai_slots.has(opp_id)

func _spawn_rival_placeholder() -> void:
	if current_puzzles[1] != null and is_instance_valid(current_puzzles[1]):
		current_puzzles[1].queue_free()
		current_puzzles[1] = null
	# Clear the slot's children and drop a "thinking" label.
	for c in (puzzle_slots[1] as Control).get_children():
		c.queue_free()
	var label := Label.new()
	label.text = "RIVAL IS\nSOLVING..."
	label.add_theme_font_size_override("font_size", 56)
	label.add_theme_color_override("font_color", Color(0.7, 0.78, 0.95))
	label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	label.add_theme_constant_override("outline_size", 6)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.custom_minimum_size = Vector2(480, 420)
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	puzzle_slots[1].add_child(label)

func _spawn_puzzle(side: int, puzzle_cls: Resource, difficulty: int) -> void:
	if current_puzzles[side] != null and is_instance_valid(current_puzzles[side]):
		current_puzzles[side].queue_free()
		current_puzzles[side] = null
	var p: PuzzleBase = puzzle_cls.new()
	puzzle_slots[side].add_child(p)
	p.setup(difficulty)
	p.input_enabled = (side == LOCAL_PLAYER_INDEX)
	p.solved.connect(func(_score: int): _on_side_solved(side))
	p.piece_placed.connect(func(): _on_side_piece_placed(side))
	current_puzzles[side] = p
	# Track when the local puzzle appears for the clutch threshold.
	if side == LOCAL_PLAYER_INDEX:
		puzzle_spawn_time = Time.get_ticks_msec() / 1000.0

func _on_side_piece_placed(side: int) -> void:
	if game_over:
		return
	attack_meters[side] = minf(ATTACK_METER_MAX, attack_meters[side] + ATTACK_GAIN_PER_PIECE)
	# If this side is BEHIND on score, earn defense charge.
	if scores[side] < scores[1 - side]:
		defense_charges[side] = minf(DEFENSE_MAX, defense_charges[side] + DEFENSE_GAIN_PER_PIECE_BEHIND)
	_refresh_meter_ui()
	# Broadcast in MP vs human so the opponent's HUD shows our meter.
	if NetManager.is_multiplayer and side == LOCAL_PLAYER_INDEX and not _opponent_is_local_ai():
		NetManager.send("piece", {})
	if attack_meters[side] >= ATTACK_METER_MAX:
		attack_meters[side] = 0.0
		_fire_attack(side, 1 - side)

func _on_side_solved(side: int) -> void:
	if game_over:
		return
	scores[side] += 1
	# Combo + clutch multiplier only applies to the LOCAL player's solves;
	# the rival's stats are tracked separately on their own machine.
	var multiplier: float = 1.0
	var was_clutch: bool = false
	if side == LOCAL_PLAYER_INDEX:
		var now: float = Time.get_ticks_msec() / 1000.0
		var since_last: float = now - last_solve_time
		var since_spawn: float = now - puzzle_spawn_time
		if since_last <= COMBO_WINDOW:
			combo_count = mini(combo_count + 1, COMBO_MAX)
		else:
			combo_count = 1
		was_clutch = since_spawn <= CLUTCH_THRESHOLD
		multiplier = 1.0 + 0.35 * float(combo_count - 1)
		if was_clutch:
			multiplier += 0.5
			clutch_count += 1
		last_solve_time = now
		max_combo = maxi(max_combo, combo_count)
		_announce_combo(combo_count, was_clutch, multiplier)
	attack_meters[side] = minf(ATTACK_METER_MAX, attack_meters[side] + ATTACK_GAIN_PER_SOLVE * multiplier)
	if scores[side] < scores[1 - side]:
		defense_charges[side] = minf(DEFENSE_MAX, defense_charges[side] + DEFENSE_GAIN_PER_SOLVE_BEHIND)
	_update_score_labels()
	_refresh_meter_ui()
	if side == LOCAL_PLAYER_INDEX:
		_show_status("YOU SCORED!", GameState.RIVAL_COLORS[0], 0.6)
		if NetManager.is_multiplayer and not _opponent_is_local_ai():
			NetManager.send("solved", {})
	else:
		_show_status("RIVAL SCORED!", GameState.RIVAL_COLORS[1], 0.6)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -4.0 if side == LOCAL_PLAYER_INDEX else -8.0)
	# Fire attack from solve gain if we crossed threshold
	if attack_meters[side] >= ATTACK_METER_MAX:
		attack_meters[side] = 0.0
		_fire_attack(side, 1 - side)
	if scores[side] >= TARGET_SCORE:
		if NetManager.is_multiplayer:
			NetManager.send("winner", { "player": side })
		_declare_winner(side)
		return
	ai_bot.stop()
	# Only host advances rounds in MP.
	if NetManager.is_multiplayer and not NetManager.is_host:
		return
	await get_tree().create_timer(0.9).timeout
	if game_over or not is_instance_valid(self):
		return
	_start_round()

func _refresh_meter_ui() -> void:
	for side in 2:
		if is_instance_valid(attack_bars[side]):
			(attack_bars[side] as _AttackBar).progress = attack_meters[side] / ATTACK_METER_MAX
			(attack_bars[side] as _AttackBar).queue_redraw()
		if is_instance_valid(attack_meter_labels[side]):
			(attack_meter_labels[side] as Label).text = "ATK %d%%" % int(attack_meters[side])
		if is_instance_valid(defense_labels[side]):
			(defense_labels[side] as Label).text = "DEF: %d" % int(defense_charges[side])

func _update_score_labels() -> void:
	if is_instance_valid(left_score_label):
		left_score_label.text = str(scores[0])
	if is_instance_valid(right_score_label):
		right_score_label.text = str(scores[1])
	if score_meter_bar != null and is_instance_valid(score_meter_bar):
		(score_meter_bar as _ScoreMeter).left_progress = float(scores[0]) / float(TARGET_SCORE)
		(score_meter_bar as _ScoreMeter).right_progress = float(scores[1]) / float(TARGET_SCORE)
		score_meter_bar.queue_redraw()

# --- Attack system ---

func _fire_attack(from_side: int, target_side: int, mirrored: bool = false) -> void:
	if game_over:
		return
	# Check for defense first.
	if not mirrored and defense_charges[target_side] >= 1.0:
		defense_charges[target_side] -= 1.0
		_refresh_meter_ui()
		if target_side == LOCAL_PLAYER_INDEX:
			defenses_made += 1
		# 50% block, 50% mirror.
		if randi() % 2 == 0:
			_show_status("%s BLOCKED!" % _side_name(target_side), GameState.RIVAL_COLORS[target_side], 1.2)
			AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -4.0)
			return
		else:
			_show_status("%s MIRRORED!" % _side_name(target_side), GameState.RIVAL_COLORS[target_side], 1.2)
			AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -4.0, 0.15)
			# Recurse on the original attacker with mirrored=true.
			await get_tree().create_timer(0.4).timeout
			if game_over or not is_instance_valid(self):
				return
			_fire_attack(target_side, from_side, true)
			return
	if from_side == LOCAL_PLAYER_INDEX:
		attacks_fired += 1
	var attack: int = randi() % 5
	var label := "%s -> %s : %s" % [_side_name(from_side), _side_name(target_side), ATTACK_NAMES[attack]]
	_show_status(label, GameState.RIVAL_COLORS[from_side], 1.2)
	AudioManager.play_sfx(GameState.SFX_BOMB_PASS, -6.0)
	# In MP vs human, broadcast the attack so it lands on the opponent's
	# machine. Otherwise apply locally (solo or AI fallback).
	if NetManager.is_multiplayer and target_side != LOCAL_PLAYER_INDEX and not _opponent_is_local_ai():
		NetManager.send("attack", { "kind": attack })
		return
	_apply_attack_local(target_side, attack)

func _apply_attack_local(target_side: int, kind: int) -> void:
	match kind:
		ATTACK_SHAKE: _attack_shake(target_side)
		ATTACK_FREEZE: _attack_freeze(target_side)
		ATTACK_RESCRAMBLE: _attack_rescramble(target_side)
		ATTACK_FOG: _attack_fog(target_side)
		ATTACK_DAZZLE: _attack_dazzle(target_side)

func _side_name(side: int) -> String:
	if side == LOCAL_PLAYER_INDEX:
		return GameState.RIVAL_NAMES[0]
	return GameState.RIVAL_NAMES[1]

func _attack_shake(side: int) -> void:
	var panel: PanelContainer = puzzle_panels[side] as PanelContainer
	if not is_instance_valid(panel):
		return
	var orig_pos: Vector2 = panel.position
	var tw := panel.create_tween()
	for i in 14:
		var off := Vector2(randf_range(-18, 18), randf_range(-18, 18))
		tw.tween_property(panel, "position", orig_pos + off, 0.05)
	tw.tween_property(panel, "position", orig_pos, 0.12)

func _attack_freeze(side: int) -> void:
	var p: PuzzleBase = current_puzzles[side] as PuzzleBase
	if not is_instance_valid(p):
		return
	var was_input: bool = p.input_enabled
	p.input_enabled = false
	var was_paused: bool = false
	if side != LOCAL_PLAYER_INDEX and is_instance_valid(ai_bot):
		was_paused = ai_bot.paused
		ai_bot.paused = true
	# Visual: cyan frost overlay
	var overlay := _spawn_overlay(side, Color(0.7, 0.95, 1.0, 0.45))
	var freeze_text := _spawn_attack_text(side, "FROZEN", Color(0.7, 0.95, 1.0))
	await get_tree().create_timer(2.5).timeout
	if not is_instance_valid(self):
		return
	if is_instance_valid(p) and not p.is_solved():
		p.input_enabled = (side == LOCAL_PLAYER_INDEX)
	if side != LOCAL_PLAYER_INDEX and is_instance_valid(ai_bot):
		ai_bot.paused = was_paused
	if is_instance_valid(overlay):
		overlay.queue_free()
	if is_instance_valid(freeze_text):
		freeze_text.queue_free()

func _attack_rescramble(side: int) -> void:
	if current_puzzle_class == null:
		return
	var burst := _spawn_attack_text(side, "RESCRAMBLED!", Color(1.0, 0.4, 0.4))
	_spawn_overlay_burst(side, Color(1.0, 0.3, 0.3, 0.55), 0.6)
	_spawn_puzzle(side, current_puzzle_class, current_difficulty)
	if side != LOCAL_PLAYER_INDEX and is_instance_valid(ai_bot):
		ai_bot.bind_puzzle(current_puzzles[side], 0.7 + float(current_difficulty) * 0.05)
		ai_bot.start()
	await get_tree().create_timer(1.2).timeout
	if is_instance_valid(burst):
		burst.queue_free()

func _attack_fog(side: int) -> void:
	var overlay := _spawn_overlay(side, Color(0.04, 0.05, 0.12, 0.85))
	var fog_text := _spawn_attack_text(side, "FOG OF WAR", Color(0.8, 0.7, 1.0))
	await get_tree().create_timer(3.0).timeout
	if not is_instance_valid(self):
		return
	if is_instance_valid(overlay):
		overlay.queue_free()
	if is_instance_valid(fog_text):
		fog_text.queue_free()

func _attack_dazzle(side: int) -> void:
	var panel: PanelContainer = puzzle_panels[side] as PanelContainer
	if not is_instance_valid(panel):
		return
	var orig: Color = panel.modulate
	var dazzle_text := _spawn_attack_text(side, "DAZZLED!", Color(1.0, 0.5, 1.0))
	# Hue-shift loop
	var tw := panel.create_tween().set_loops(8)
	tw.tween_property(panel, "modulate", Color(1.5, 0.4, 1.5), 0.18)
	tw.tween_property(panel, "modulate", Color(0.4, 1.5, 1.5), 0.18)
	tw.tween_property(panel, "modulate", Color(1.5, 1.5, 0.4), 0.18)
	tw.tween_property(panel, "modulate", Color(0.6, 0.6, 1.5), 0.18)
	await get_tree().create_timer(4.0).timeout
	if not is_instance_valid(self):
		return
	if is_instance_valid(panel):
		panel.modulate = orig
	if is_instance_valid(dazzle_text):
		dazzle_text.queue_free()

func _spawn_overlay(side: int, color: Color) -> ColorRect:
	var x_offset: float = -PANEL_X if side == 0 else PANEL_X
	var fog := ColorRect.new()
	fog.color = color
	fog.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(fog)
	_set_centered(fog, PANEL_SIZE.x - 16, PANEL_SIZE.y - 16, x_offset, 80.0)
	fog.modulate = Color(1, 1, 1, 0.0)
	var tw := fog.create_tween()
	tw.tween_property(fog, "modulate:a", 1.0, 0.15)
	return fog

func _spawn_overlay_burst(side: int, color: Color, duration: float) -> void:
	var overlay := _spawn_overlay(side, color)
	var tw := overlay.create_tween()
	tw.tween_interval(duration)
	tw.tween_property(overlay, "modulate:a", 0.0, 0.25)
	tw.tween_callback(func():
		if is_instance_valid(overlay):
			overlay.queue_free()
	)

func _spawn_attack_text(side: int, text: String, color: Color) -> Label:
	var x_offset: float = -PANEL_X if side == 0 else PANEL_X
	var label := Label.new()
	label.text = text
	label.add_theme_font_size_override("font_size", 60)
	label.add_theme_color_override("font_color", color)
	label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	label.add_theme_constant_override("outline_size", 8)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(label)
	_set_centered(label, 600.0, 100.0, x_offset, 80.0)
	label.modulate = Color(color.r, color.g, color.b, 0.0)
	label.pivot_offset = Vector2(300, 50)
	label.scale = Vector2(0.6, 0.6)
	var tw := label.create_tween().set_parallel(true)
	tw.tween_property(label, "modulate:a", 1.0, 0.18)
	tw.tween_property(label, "scale", Vector2(1.0, 1.0), 0.25).set_trans(Tween.TRANS_BACK)
	return label

func _declare_winner(side: int) -> void:
	game_over = true
	ai_bot.stop()
	var won_match: bool = side == LOCAL_PLAYER_INDEX
	if won_match:
		_show_status("YOU WIN!", GameState.RIVAL_COLORS[0], 4.0)
		AudioManager.play_music(GameState.JINGLE_WIN, -4.0, 0.3)
	else:
		_show_status("%s WINS" % GameState.RIVAL_NAMES[1], GameState.RIVAL_COLORS[1], 4.0)
		AudioManager.play_music(GameState.JINGLE_LOSE, -4.0, 0.3)
	# Award rank points + combo / clutch bonuses.
	var base_rp: int = 60 if won_match else 22
	var combo_bonus: int = maxi(0, max_combo - 1) * COMBO_RP_BONUS_PER_TIER
	var clutch_bonus: int = clutch_count * CLUTCH_RP_BONUS_EACH
	var total_rp: int = base_rp + combo_bonus + clutch_bonus
	var prev_level: int = PlayerProfile.level
	PlayerProfile.award_rp(total_rp, won_match)
	var levels_gained: int = PlayerProfile.level - prev_level

	var accent: Color = GameState.RIVAL_COLORS[0] if won_match else GameState.RIVAL_COLORS[1]
	var subtitle: String = "You won the duel %d-%d" % [scores[0], scores[1]] if won_match else "%s won the duel %d-%d" % [GameState.RIVAL_NAMES[1], scores[1], scores[0]]
	var rp_lines: Array = [{"label": "Base", "amount": base_rp, "color": Color(1, 1, 1)}]
	if combo_bonus > 0:
		rp_lines.append({"label": "Combo x%d" % max_combo, "amount": combo_bonus, "color": Color(1.0, 0.92, 0.18)})
	if clutch_bonus > 0:
		rp_lines.append({"label": "Clutch x%d" % clutch_count, "amount": clutch_bonus, "color": Color(1.0, 0.4, 0.1)})
	_show_summary({
		"won": won_match,
		"headline": "VICTORY" if won_match else "DEFEAT",
		"subtitle": subtitle,
		"accent": accent,
		"stats": [
			{"name": "SOLVES", "value": scores[LOCAL_PLAYER_INDEX], "color": GameState.RIVAL_COLORS[0]},
			{"name": "ATTACKS", "value": attacks_fired, "color": Color(1.0, 0.4, 0.4)},
			{"name": "DEFENSES", "value": defenses_made, "color": Color(0.0, 0.90, 1.0)},
			{"name": "PEAK COMBO", "value": max_combo, "color": Color(1.0, 0.92, 0.18)},
			{"name": "CLUTCHES", "value": clutch_count, "color": Color(1.0, 0.54, 0.12)},
		],
		"rp_lines": rp_lines,
		"rp_total": total_rp,
		"levels_gained": levels_gained,
	})

func _show_summary(cfg: Dictionary) -> void:
	var summary: PostMatchSummary = SummaryClass.new()
	add_child(summary)
	summary.continue_pressed.connect(_on_summary_continue)
	summary.present(cfg)

func _on_summary_continue() -> void:
	if not is_instance_valid(self):
		return
	LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.6)

func _announce_combo(combo: int, was_clutch: bool, multiplier: float) -> void:
	# Pop a transient COMBO / CLUTCH banner over the player's panel.
	var pieces: Array = []
	var color: Color = Color(1.0, 0.92, 0.18)
	if was_clutch:
		pieces.append("CLUTCH!")
		color = Color(1.0, 0.4, 0.1)
		AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -2.0, 0.05)
	if combo >= 2:
		pieces.append("COMBO x%d" % combo)
	if pieces.is_empty():
		return
	var text: String = " • ".join(pieces) + "   x%.1f" % multiplier
	var toast := Label.new()
	toast.text = text
	toast.add_theme_font_size_override("font_size", 36)
	toast.add_theme_color_override("font_color", color)
	toast.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	toast.add_theme_constant_override("outline_size", 7)
	toast.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	toast.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(toast)
	# Anchored over the LEFT panel (local player's puzzle).
	toast.anchor_left = 0.5
	toast.anchor_right = 0.5
	toast.anchor_top = 0.0
	toast.anchor_bottom = 0.0
	toast.offset_left = -PANEL_X - 320
	toast.offset_right = -PANEL_X + 320
	toast.offset_top = 320
	toast.offset_bottom = 400
	toast.modulate = Color(color.r, color.g, color.b, 0.0)
	toast.pivot_offset = Vector2(320, 40)
	toast.scale = Vector2(0.7, 0.7)
	var tw := toast.create_tween().set_parallel(true)
	tw.tween_property(toast, "modulate:a", 1.0, 0.18)
	tw.tween_property(toast, "scale", Vector2(1.0, 1.0), 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.chain().tween_interval(1.0)
	tw.chain().tween_property(toast, "modulate:a", 0.0, 0.35)
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

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_back"):
		game_over = true
		ai_bot.stop()
		LoadingScreen.change_scene("res://scenes/game_world.tscn", 0.4)


# --- Network handlers (multiplayer) ---

func _on_net_message(type: String, msg: Dictionary, from_player: int) -> void:
	if from_player == NetManager.local_player_id:
		return
	match type:
		"round_start":
			if NetManager.is_host:
				return
			_begin_round(int(msg.get("puzzle_idx", 0)), int(msg.get("difficulty", 1)))
		"piece":
			_apply_remote_piece_placed()
		"solved":
			_apply_remote_solve()
		"attack":
			_apply_incoming_attack(int(msg.get("kind", 0)))
		"winner":
			var pid: int = int(msg.get("player", 1))
			game_over = true
			# Translate the network-side player id to a local arena side.
			var local_side: int = 0 if pid == NetManager.local_player_id else 1
			_declare_winner(local_side)

func _on_net_disconnect(_pid: int) -> void:
	if game_over or not NetManager.is_multiplayer:
		return
	# Opponent left mid-match — fall back to a local AI rival.
	var opp_id: int = 1 - NetManager.local_player_id
	if not NetManager.ai_slots.has(opp_id):
		NetManager.ai_slots.append(opp_id)
	_show_status("OPPONENT LEFT — AI TAKES OVER", Color(1.0, 0.92, 0.18), 1.6)
	if current_puzzle_class != null:
		_spawn_puzzle(1, current_puzzle_class, current_difficulty)
		ai_bot.bind_puzzle(current_puzzles[1], 0.7 + float(current_difficulty) * 0.05)
		ai_bot.start()

func _apply_remote_piece_placed() -> void:
	if game_over:
		return
	attack_meters[1] = minf(ATTACK_METER_MAX, attack_meters[1] + ATTACK_GAIN_PER_PIECE)
	if scores[1] < scores[0]:
		defense_charges[1] = minf(DEFENSE_MAX, defense_charges[1] + DEFENSE_GAIN_PER_PIECE_BEHIND)
	_refresh_meter_ui()

func _apply_remote_solve() -> void:
	if game_over:
		return
	# Mirror the local _on_side_solved logic for the opponent (side 1) but
	# DON'T re-broadcast and DON'T let host attempt to start the next round
	# (the joiner's solve message reaches the host through this handler).
	scores[1] += 1
	attack_meters[1] = minf(ATTACK_METER_MAX, attack_meters[1] + ATTACK_GAIN_PER_SOLVE)
	if scores[1] < scores[0]:
		defense_charges[1] = minf(DEFENSE_MAX, defense_charges[1] + DEFENSE_GAIN_PER_SOLVE_BEHIND)
	_update_score_labels()
	_refresh_meter_ui()
	_show_status("RIVAL SCORED!", GameState.RIVAL_COLORS[1], 0.6)
	AudioManager.play_sfx(GameState.SFX_PUZZLE_SOLVE, -8.0)
	if scores[1] >= TARGET_SCORE:
		if NetManager.is_host:
			NetManager.send("winner", { "player": 1 })
		_declare_winner(1)
		return
	# Host pushes the next round after a brief beat.
	if NetManager.is_host:
		await get_tree().create_timer(0.9).timeout
		if game_over or not is_instance_valid(self):
			return
		_start_round()

func _apply_incoming_attack(kind: int) -> void:
	# Defense check on our side (left) — block / mirror as in solo.
	if defense_charges[0] >= 1.0:
		defense_charges[0] -= 1.0
		_refresh_meter_ui()
		defenses_made += 1
		if randi() % 2 == 0:
			_show_status("YOU BLOCKED!", GameState.RIVAL_COLORS[0], 1.2)
			AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -4.0)
			return
		else:
			_show_status("YOU MIRRORED!", GameState.RIVAL_COLORS[0], 1.2)
			AudioManager.play_sfx(GameState.SFX_PUZZLE_ROTATE, -4.0, 0.15)
			# Reflect the attack back at the opponent.
			NetManager.send("attack", { "kind": kind })
			return
	var label := "%s -> YOU : %s" % [_side_name(1), ATTACK_NAMES[kind]]
	_show_status(label, GameState.RIVAL_COLORS[1], 1.2)
	AudioManager.play_sfx(GameState.SFX_BOMB_PASS, -6.0)
	_apply_attack_local(0, kind)


# --- Score meter primitive ---

class _ScoreMeter extends Control:
	var left_color: Color = Color.MAGENTA
	var right_color: Color = Color.CYAN
	var left_progress: float = 0.0
	var right_progress: float = 0.0

	func _draw() -> void:
		var rect := Rect2(Vector2.ZERO, size)
		draw_rect(rect, Color(0.05, 0.06, 0.12, 0.92), true)
		draw_rect(rect, Color(0.85, 0.95, 1.0, 0.7), false, 3.0)
		var half := size.x * 0.5
		var left_width := half * clampf(left_progress, 0.0, 1.0)
		var left_rect := Rect2(Vector2(half - left_width, 4), Vector2(left_width, size.y - 8))
		draw_rect(left_rect, Color(left_color.r, left_color.g, left_color.b, 0.5), true)
		draw_rect(left_rect, left_color, false, 2.0)
		var right_width := half * clampf(right_progress, 0.0, 1.0)
		var right_rect := Rect2(Vector2(half, 4), Vector2(right_width, size.y - 8))
		draw_rect(right_rect, Color(right_color.r, right_color.g, right_color.b, 0.5), true)
		draw_rect(right_rect, right_color, false, 2.0)
		draw_line(Vector2(half, 0), Vector2(half, size.y), Color(1, 1, 1, 0.95), 3.0)


# --- Attack meter primitive ---

class _AttackBar extends Control:
	var tint: Color = Color.MAGENTA
	var progress: float = 0.0
	var fill_from_right: bool = false

	func _draw() -> void:
		var rect := Rect2(Vector2.ZERO, size)
		draw_rect(rect, Color(0.05, 0.06, 0.12, 0.92), true)
		draw_rect(rect, Color(tint.r, tint.g, tint.b, 0.75), false, 2.0)
		var p := clampf(progress, 0.0, 1.0)
		var fill_w := (size.x - 6.0) * p
		var fill_rect: Rect2
		if fill_from_right:
			fill_rect = Rect2(Vector2(size.x - 3.0 - fill_w, 3.0), Vector2(fill_w, size.y - 6.0))
		else:
			fill_rect = Rect2(Vector2(3.0, 3.0), Vector2(fill_w, size.y - 6.0))
		var fill_color := Color(tint.r, tint.g, tint.b, 0.85)
		if p >= 1.0:
			fill_color = Color(1.0, 0.95, 0.2)  # ready-to-fire flash
		draw_rect(fill_rect, fill_color, true)
		# Inner highlight
		if fill_w > 0.0:
			var hi := Color(1.0, 1.0, 1.0, 0.35)
			draw_rect(Rect2(fill_rect.position, Vector2(fill_rect.size.x, 2)), hi, true)
