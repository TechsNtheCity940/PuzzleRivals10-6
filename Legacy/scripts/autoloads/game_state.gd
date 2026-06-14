extends Node

# GameState autoload — tracks chosen mode, current round, settings.

enum Mode { NONE, FFA, BVB }

const MUSIC_MENU := "res://assets/audio/music/music_menu_menu_theme.mp3"
const MUSIC_GAMEPLAY := "res://assets/audio/music/music_gameplay_gameplay_theme.mp3"
const JINGLE_WIN := "res://assets/audio/music/jingle_level_complete_round_win.mp3"
const JINGLE_LOSE := "res://assets/audio/music/jingle_game_over_round_lose.mp3"

const SFX_BOMB_TICK := "res://assets/audio/sfx/environment/environment_bomb_tick.mp3"
const SFX_BOMB_PASS := "res://assets/audio/sfx/environment/environment_bomb_pass.mp3"
const SFX_BOMB_EXPLOSION := "res://assets/audio/sfx/environment/environment_bomb_explosion.mp3"
const SFX_PUZZLE_SOLVE := "res://assets/audio/sfx/ui/ui_puzzle_solve.mp3"
const SFX_PUZZLE_PLACE := "res://assets/audio/sfx/ui/ui_puzzle_piece_place.mp3"
const SFX_PUZZLE_ROTATE := "res://assets/audio/sfx/ui/ui_puzzle_piece_rotate.mp3"
const SFX_UI_CLICK := "res://assets/audio/sfx/ui/ui_ui_click.mp3"
const SFX_UI_HOVER := "res://assets/audio/sfx/ui/ui_ui_hover.mp3"
const SFX_UI_CONFIRM := "res://assets/audio/sfx/ui/ui_ui_confirm.mp3"

const RIVAL_COLORS: Array = [
	Color(1.0, 0.18, 0.78),   # magenta
	Color(0.0, 0.90, 1.0),    # cyan
	Color(0.61, 1.0, 0.18),   # lime
	Color(1.0, 0.54, 0.12),   # orange
]

# Puzzle-themed rival call-signs.
const RIVAL_NAMES: Array = ["GLITCH", "CIPHER", "VERTEX", "AXIOM"]

const RIVAL_TITLES: Array = [
	"The Glitchsmith",
	"The Code Breaker",
	"The Geometer",
	"The Strategist",
]

const RIVAL_SPRITES: Array = [
	"res://assets/sprites/characters/rival_magenta_idle.png",
	"res://assets/sprites/characters/rival_cyan_idle.png",
	"res://assets/sprites/characters/rival_lime_idle.png",
	"res://assets/sprites/characters/rival_orange_idle.png",
]

var current_mode: int = Mode.NONE
var local_player_index: int = 0  # which podium is the human player in FFA / BVB

func reset() -> void:
	current_mode = Mode.NONE
	local_player_index = 0
