extends Node

# AudioManager autoload — handles music, SFX, web autoplay unlock, and bus setup.

const MAX_SFX: int = 12

var music_player: AudioStreamPlayer = null
var sfx_players: Array[AudioStreamPlayer] = []

var _unlocked: bool = false
var _pending_music: AudioStream = null
var _pending_music_vol: float = -6.0

var _cache: Dictionary = {}

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.has_feature("web"):
		_unlocked = true
	_setup_buses()
	_create_players()

func _input(event: InputEvent) -> void:
	if _unlocked:
		return
	if event is InputEventMouseButton or event is InputEventKey or event is InputEventScreenTouch:
		_unlocked = true
		if _pending_music != null:
			_play_music_now(_pending_music, _pending_music_vol)
			_pending_music = null

func _setup_buses() -> void:
	for bus_name in ["Music", "SFX"]:
		if AudioServer.get_bus_index(bus_name) == -1:
			AudioServer.add_bus()
			var idx := AudioServer.bus_count - 1
			AudioServer.set_bus_name(idx, bus_name)
			AudioServer.set_bus_send(idx, "Master")

func _create_players() -> void:
	music_player = AudioStreamPlayer.new()
	music_player.bus = "Music"
	music_player.playback_type = AudioServer.PLAYBACK_TYPE_STREAM
	add_child(music_player)
	for i in MAX_SFX:
		var p := AudioStreamPlayer.new()
		p.bus = "SFX"
		p.playback_type = AudioServer.PLAYBACK_TYPE_STREAM
		add_child(p)
		sfx_players.append(p)

func _load_stream(path: String) -> AudioStream:
	if _cache.has(path):
		return _cache[path]
	if not ResourceLoader.exists(path):
		return null
	var s := load(path) as AudioStream
	_cache[path] = s
	return s

func play_music(path: String, volume_db: float = -6.0, fade_in: float = 0.5) -> void:
	var stream := _load_stream(path)
	if stream == null:
		return
	if not _unlocked:
		_pending_music = stream
		_pending_music_vol = volume_db
		return
	_play_music_now(stream, volume_db, fade_in)

func _play_music_now(stream: AudioStream, volume_db: float, fade_in: float = 0.0) -> void:
	if music_player == null:
		return
	if music_player.stream == stream and music_player.playing:
		return
	music_player.stream = stream
	if fade_in > 0.0:
		music_player.volume_db = -40.0
		music_player.play()
		create_tween().tween_property(music_player, "volume_db", volume_db, fade_in)
	else:
		music_player.volume_db = volume_db
		music_player.play()

func stop_music(fade_out: float = 0.3) -> void:
	if music_player == null or not music_player.playing:
		return
	if fade_out > 0.0:
		var tw := create_tween()
		tw.tween_property(music_player, "volume_db", -40.0, fade_out)
		tw.tween_callback(func(): if is_instance_valid(music_player): music_player.stop())
	else:
		music_player.stop()

func play_sfx(path: String, volume_db: float = 0.0, pitch_variation: float = 0.0) -> void:
	var stream := _load_stream(path)
	if stream == null:
		return
	for p in sfx_players:
		if not p.playing:
			p.stream = stream
			p.volume_db = volume_db
			p.pitch_scale = 1.0
			if pitch_variation > 0.0:
				p.pitch_scale = randf_range(1.0 - pitch_variation, 1.0 + pitch_variation)
			p.play()
			return
