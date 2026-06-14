class_name PuzzleBase
extends Control

# PuzzleBase — common interface every puzzle implements.
# Arenas spawn one of these, hook the signals, and tear it down on round end.

signal solved(progress_score: int)
signal piece_placed  # fired on every meaningful interaction (used for combo/score-trickle)

@export var difficulty: int = 1
@export var input_enabled: bool = true

var _solved: bool = false

func setup(d: int = 1) -> void:
	difficulty = max(1, d)

func is_solved() -> bool:
	return _solved

# Returns 0..1 — how close the puzzle is to being solved.
# Used by the Brain VS Brain score meter.
func get_progress() -> float:
	return 0.0

# Apply a partial auto-solve step (defensive move).
func auto_solve_step() -> void:
	pass

# Called by the AI bot once per "think" tick. Default: random valid move.
func ai_step() -> void:
	pass

# Convenience for subclasses.
func _emit_solved(score: int = 100) -> void:
	if _solved:
		return
	_solved = true
	input_enabled = false
	solved.emit(score)
