class_name AIBot
extends Node

# AI bot — solves a puzzle by stepping it at a rate determined by skill.
# Higher skill = lower step interval = faster solver.

signal step_taken

@export var skill: float = 1.0  # 1.0 = average; 2.0 = very fast; 0.5 = sloppy
@export var min_step_time: float = 0.25
@export var max_step_time: float = 1.2

var paused: bool = false

var _puzzle: PuzzleBase = null
var _timer: Timer = null
var _active: bool = false

func _ready() -> void:
	_timer = Timer.new()
	_timer.one_shot = true
	add_child(_timer)
	_timer.timeout.connect(_on_timer_tick)

func bind_puzzle(puzzle: PuzzleBase, skill_factor: float = 1.0) -> void:
	# Stop any in-flight timer before re-binding so the new puzzle gets a
	# fresh schedule (this matters after a rescramble attack).
	stop()
	_puzzle = puzzle
	skill = clampf(skill_factor, 0.3, 3.0)

func start() -> void:
	if _puzzle == null:
		return
	_active = true
	_schedule_next()

func stop() -> void:
	_active = false
	if is_instance_valid(_timer):
		_timer.stop()

func _schedule_next() -> void:
	if not _active or _puzzle == null:
		return
	var base := randf_range(min_step_time, max_step_time)
	var interval: float = base / maxf(skill, 0.1)
	if is_instance_valid(_timer):
		_timer.start(interval)

func _on_timer_tick() -> void:
	if not _active or _puzzle == null:
		return
	if not is_instance_valid(_puzzle):
		stop()
		return
	if _puzzle.is_solved():
		stop()
		return
	if paused:
		# Freeze attack is active on this AI — skip this tick and re-poll.
		_schedule_next()
		return
	# Many puzzles route ai_step() through their own click handlers, which
	# bail when input_enabled is false. The arenas set input_enabled=false
	# on AI-owned puzzles to lock out player clicks, so the AI's synthetic
	# clicks bounce too. Temporarily flip input_enabled to true around the
	# step and restore it afterwards.
	var saved: bool = _puzzle.input_enabled
	_puzzle.input_enabled = true
	_puzzle.ai_step()
	step_taken.emit()
	if is_instance_valid(_puzzle) and not _puzzle.is_solved():
		_puzzle.input_enabled = saved
	_schedule_next()
