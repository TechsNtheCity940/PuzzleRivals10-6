class_name BombSprite
extends Control

# Visual bomb that floats above a player podium. Animates the lit fuse + scale pulse
# when active, and travels (tween) to the next player on pass.

const SPRITE_IDLE := "res://assets/sprites/effects/bomb_idle.png"
const SPRITE_LIT := "res://assets/sprites/effects/bomb_lit.png"

@export var bomb_size: Vector2 = Vector2(180, 180)

var _texture_rect: TextureRect = null
var _glow_rect: TextureRect = null
var _lit: bool = false

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	custom_minimum_size = bomb_size
	size = bomb_size
	_build()

func _build() -> void:
	# Glow halo behind bomb
	_glow_rect = TextureRect.new()
	if ResourceLoader.exists(SPRITE_LIT):
		_glow_rect.texture = load(SPRITE_LIT)
	_glow_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_glow_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	_glow_rect.modulate = Color(1, 0.4, 0.1, 0.5)
	_glow_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_glow_rect.scale = Vector2(1.4, 1.4)
	_glow_rect.pivot_offset = bomb_size / 2.0
	_glow_rect.visible = false
	add_child(_glow_rect)
	_glow_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	_texture_rect = TextureRect.new()
	if ResourceLoader.exists(SPRITE_IDLE):
		_texture_rect.texture = load(SPRITE_IDLE)
	_texture_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_texture_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	_texture_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_texture_rect)
	_texture_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_texture_rect.pivot_offset = bomb_size / 2.0

func set_lit(lit: bool) -> void:
	_lit = lit
	if _texture_rect != null and is_instance_valid(_texture_rect):
		if lit and ResourceLoader.exists(SPRITE_LIT):
			_texture_rect.texture = load(SPRITE_LIT)
		elif not lit and ResourceLoader.exists(SPRITE_IDLE):
			_texture_rect.texture = load(SPRITE_IDLE)
	if _glow_rect != null and is_instance_valid(_glow_rect):
		_glow_rect.visible = lit
		if lit:
			_pulse_glow()

func _pulse_glow() -> void:
	if not is_instance_valid(_glow_rect):
		return
	var tw := _glow_rect.create_tween().set_loops()
	tw.tween_property(_glow_rect, "scale", Vector2(1.7, 1.7), 0.45)
	tw.tween_property(_glow_rect, "scale", Vector2(1.3, 1.3), 0.45)

func travel_to(target_global_pos: Vector2, duration: float = 0.55) -> void:
	# Move from current global_position to target_global_pos over duration.
	var start_pos := global_position
	var tw := create_tween()
	tw.tween_method(func(t: float):
		if not is_instance_valid(self):
			return
		var eased: float = ease(t, 0.5)
		# Arc: peak at midpoint
		var lerp_pos: Vector2 = start_pos.lerp(target_global_pos, eased)
		lerp_pos.y -= sin(t * PI) * 80.0
		global_position = lerp_pos
	, 0.0, 1.0, duration)
	tw.tween_callback(func(): pass)
