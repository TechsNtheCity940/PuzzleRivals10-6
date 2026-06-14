extends Node

# NetManager — handles the WebSocket connection to the tesana.ai multiplayer
# relay. Supports 2-4 player rooms with host authority.

signal message_received(type: String, data: Dictionary, from_player: int)
signal connected_to_room
signal room_created(code: String)
signal player_joined(player_id: int, total_players: int)
signal player_disconnected(player_id: int)
signal all_players_joined
signal lobby_full
signal connection_failed(reason: String)

var is_multiplayer: bool = false
var is_host: bool = false
var room_code: String = ""
var max_players: int = 2
var local_player_id: int = 0
var connected_players: int = 0

# Phase 4 — host-marked AI slots (filled by AI fallback when humans don't
# arrive in time). The host owns the AI bots for these slots.
var ai_slots: Array[int] = []

var _ws: WebSocketPeer = null
var _ping_timer: float = 0.0
const PING_INTERVAL: float = 5.0

const MP_RELAY_HOST: String = "mp.tesana.ai"

func get_server_base_url() -> String:
	if OS.has_feature("web"):
		return "wss://" + MP_RELAY_HOST
	return "wss://" + MP_RELAY_HOST

func reset() -> void:
	disconnect_from_room()
	ai_slots.clear()

func create_room(player_count: int = 2) -> void:
	max_players = clampi(player_count, 2, 4)
	is_host = true
	local_player_id = 0
	var http := HTTPRequest.new()
	Engine.get_main_loop().root.add_child(http)
	http.request_completed.connect(func(_result: int, _code: int, _headers: PackedStringArray, body: PackedByteArray):
		http.queue_free()
		var json: Variant = JSON.parse_string(body.get_string_from_utf8())
		if json == null or not (json is Dictionary):
			connection_failed.emit("Could not contact matchmaker")
			return
		var d: Dictionary = json
		if not d.has("code"):
			connection_failed.emit("Matchmaker returned no code")
			return
		room_code = str(d["code"])
		room_created.emit(room_code)
		connect_to_room(room_code)
	)
	var base := get_server_base_url().replace("ws://", "http://").replace("wss://", "https://")
	http.request(base + "/lobby/create?max_players=" + str(max_players))

func connect_to_room(code: String) -> void:
	room_code = code
	_ws = WebSocketPeer.new()
	var err: int = _ws.connect_to_url(get_server_base_url() + "/ws/room/" + code)
	if err != OK:
		connection_failed.emit("Could not connect to room")

func disconnect_from_room() -> void:
	if _ws != null:
		_ws.close()
	_ws = null
	is_multiplayer = false
	is_host = false
	room_code = ""
	local_player_id = 0
	connected_players = 0
	max_players = 2

func send(type: String, data: Dictionary = {}, from_override: int = -1) -> void:
	if _ws == null:
		return
	if _ws.get_ready_state() != WebSocketPeer.STATE_OPEN:
		return
	var payload: Dictionary = data.duplicate()
	payload["t"] = type
	payload["from"] = from_override if from_override >= 0 else local_player_id
	_ws.send_text(JSON.stringify(payload))

func is_player_ai(player_id: int) -> bool:
	return ai_slots.has(player_id)

func _process(delta: float) -> void:
	if _ws == null:
		return
	_ws.poll()
	var state: int = _ws.get_ready_state()
	if state == WebSocketPeer.STATE_OPEN:
		_ping_timer += delta
		if _ping_timer >= PING_INTERVAL:
			_ping_timer = 0.0
			send("ping")
		while _ws.get_available_packet_count() > 0:
			var raw: String = _ws.get_packet().get_string_from_utf8()
			var msg_v: Variant = JSON.parse_string(raw)
			if msg_v == null or not (msg_v is Dictionary):
				continue
			var msg: Dictionary = msg_v
			var msg_type: String = str(msg.get("t", ""))
			match msg_type:
				"joined":
					local_player_id = int(msg.get("player_id", 0))
					connected_players = int(msg.get("total", 1))
					connected_to_room.emit()
					player_joined.emit(local_player_id, connected_players)
					if connected_players >= max_players:
						all_players_joined.emit()
				"player_disconnected":
					var dc_id: int = int(msg.get("player_id", -1))
					player_disconnected.emit(dc_id)
				"pong":
					pass
				_:
					var from_player: int = int(msg.get("from", -1))
					message_received.emit(msg_type, msg, from_player)
	elif state == WebSocketPeer.STATE_CLOSED:
		if is_multiplayer:
			player_disconnected.emit(-1)
		_ws = null
