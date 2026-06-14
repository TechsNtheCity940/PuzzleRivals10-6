extends Node

# PlayerProfile — persisted player data: username, rank points (RP),
# level, season, and unlocked + equipped cosmetics (frames, banners,
# emblems). Saved to user://profile.cfg.

const PROFILE_PATH := "user://profile.cfg"
const CURRENT_SEASON: int = 1
const STARTING_COINS: int = 300

signal profile_changed
signal level_up(new_level: int)
signal item_unlocked(item_id: String, kind: String)
signal username_set

# --- Profile data ---
var username: String = ""
var rp: int = 0
var level: int = 1
var matches_played: int = 0
var matches_won: int = 0
var season_id: int = CURRENT_SEASON
var first_run: bool = true
var coins: int = 0

var unlocked_frames: Array[String] = []
var unlocked_banners: Array[String] = []
var unlocked_emblems: Array[String] = []
var equipped_frame: String = "frame_basic"
var equipped_banner: String = "banner_neon"
var equipped_emblem: String = "emblem_bolt"

# --- Catalog ---
const FRAMES: Array = [
	{"id": "frame_basic",   "label": "Cyan Edge",     "source": "default", "level": 1,  "color": Color(0.0, 0.90, 1.0)},
	{"id": "frame_lime",    "label": "Lime Spark",    "source": "level",   "level": 3,  "color": Color(0.61, 1.0, 0.18)},
	{"id": "frame_amber",   "label": "Amber Halo",    "source": "level",   "level": 6,  "color": Color(1.0, 0.54, 0.12)},
	{"id": "frame_magenta", "label": "Magenta Pulse", "source": "level",   "level": 10, "color": Color(1.0, 0.18, 0.78)},
	{"id": "frame_violet",  "label": "Violet Aura",   "source": "season",  "level": 1,  "color": Color(0.78, 0.35, 1.0)},
	{"id": "frame_gold",    "label": "Gold Crown",    "source": "level",   "level": 18, "color": Color(1.0, 0.92, 0.18)},
	{"id": "frame_legend",  "label": "Legend",        "source": "level",   "level": 30, "color": Color(1.0, 1.0, 1.0)},
	{"id": "frame_chrome",  "label": "Chrome Edge",   "source": "store",   "level": 1,  "price": 600, "color": Color(0.78, 0.86, 0.96)},
]

const BANNERS: Array = [
	{"id": "banner_neon",    "label": "Neon Wave",      "source": "default", "level": 1,  "colors": [Color(0.05, 0.06, 0.18), Color(0.0, 0.65, 0.85)]},
	{"id": "banner_storm",   "label": "Storm Front",    "source": "level",   "level": 4,  "colors": [Color(0.05, 0.08, 0.18), Color(0.78, 0.35, 1.0)]},
	{"id": "banner_aurora",  "label": "Aurora",         "source": "level",   "level": 8,  "colors": [Color(0.0, 0.30, 0.35), Color(0.61, 1.0, 0.18)]},
	{"id": "banner_inferno", "label": "Inferno",        "source": "level",   "level": 14, "colors": [Color(0.30, 0.06, 0.0), Color(1.0, 0.40, 0.10)]},
	{"id": "banner_void",    "label": "Void Drift",     "source": "season",  "level": 1,  "colors": [Color(0.04, 0.02, 0.10), Color(0.45, 0.0, 0.80)]},
	{"id": "banner_legend",  "label": "Legend Banner",  "source": "level",   "level": 25, "colors": [Color(1.0, 0.70, 0.10), Color(1.0, 1.0, 1.0)]},
	{"id": "banner_aqua",    "label": "Hyperflow",      "source": "store",   "level": 1,  "price": 500, "colors": [Color(0.05, 0.40, 0.55), Color(0.0, 0.90, 1.0)]},
	{"id": "banner_solar",   "label": "Solar Flare",    "source": "store",   "level": 1,  "price": 750, "colors": [Color(0.30, 0.08, 0.0), Color(1.0, 0.58, 0.10)]},
]

const EMBLEMS: Array = [
	{"id": "emblem_bolt",    "label": "Lightning Bolt", "source": "default", "level": 1,  "color": Color(1.0, 0.92, 0.18)},
	{"id": "emblem_diamond", "label": "Diamond",        "source": "level",   "level": 2,  "color": Color(0.0, 0.90, 1.0)},
	{"id": "emblem_skull",   "label": "Puzzle Skull",   "source": "level",   "level": 5,  "color": Color(0.78, 0.35, 1.0)},
	{"id": "emblem_crown",   "label": "Crown",          "source": "level",   "level": 9,  "color": Color(1.0, 0.92, 0.18)},
	{"id": "emblem_eye",     "label": "All-Seeing Eye", "source": "level",   "level": 15, "color": Color(1.0, 0.18, 0.78)},
	{"id": "emblem_star",    "label": "Five-Star",      "source": "season",  "level": 1,  "color": Color(1.0, 1.0, 1.0)},
	{"id": "emblem_burst",   "label": "Sun Burst",      "source": "store",   "level": 1,  "price": 400, "color": Color(1.0, 0.54, 0.12)},
	{"id": "emblem_orbit",   "label": "Orbit Ring",     "source": "store",   "level": 1,  "price": 550, "color": Color(0.0, 0.90, 1.0)},
	{"id": "emblem_legend",  "label": "Legend Crest",   "source": "level",   "level": 22, "color": Color(1.0, 0.40, 0.10)},
]

const RANK_TIERS: Array = [
	{"min_level": 1,  "label": "NOVICE",      "color": Color(0.6, 0.7, 0.8)},
	{"min_level": 4,  "label": "APPRENTICE",  "color": Color(0.0, 0.90, 1.0)},
	{"min_level": 8,  "label": "ADEPT",       "color": Color(0.61, 1.0, 0.18)},
	{"min_level": 14, "label": "ELITE",       "color": Color(1.0, 0.54, 0.12)},
	{"min_level": 22, "label": "MASTER",      "color": Color(1.0, 0.18, 0.78)},
	{"min_level": 32, "label": "GRANDMASTER", "color": Color(1.0, 0.92, 0.18)},
]

func _ready() -> void:
	load_profile()

func load_profile() -> void:
	var cfg := ConfigFile.new()
	var err: int = cfg.load(PROFILE_PATH)
	if err != OK:
		first_run = true
		coins = STARTING_COINS
		_grant_defaults()
		_recompute_level()
		return
	first_run = false
	coins = int(cfg.get_value("profile", "coins", STARTING_COINS))
	username = str(cfg.get_value("profile", "username", ""))
	if username == "":
		first_run = true
	rp = int(cfg.get_value("profile", "rp", 0))
	matches_played = int(cfg.get_value("profile", "matches_played", 0))
	matches_won = int(cfg.get_value("profile", "matches_won", 0))
	season_id = int(cfg.get_value("profile", "season_id", CURRENT_SEASON))
	equipped_frame = str(cfg.get_value("profile", "equipped_frame", "frame_basic"))
	equipped_banner = str(cfg.get_value("profile", "equipped_banner", "banner_neon"))
	equipped_emblem = str(cfg.get_value("profile", "equipped_emblem", "emblem_bolt"))
	unlocked_frames = _string_array(cfg.get_value("profile", "unlocked_frames", []))
	unlocked_banners = _string_array(cfg.get_value("profile", "unlocked_banners", []))
	unlocked_emblems = _string_array(cfg.get_value("profile", "unlocked_emblems", []))
	_grant_defaults()
	_recompute_level()
	_auto_unlock_for_level(level)

func _string_array(v: Variant) -> Array[String]:
	var out: Array[String] = []
	if v is Array:
		for item in v:
			out.append(str(item))
	return out

func _grant_defaults() -> void:
	for f in FRAMES:
		if f.source == "default" and not unlocked_frames.has(f.id):
			unlocked_frames.append(f.id)
	for b in BANNERS:
		if b.source == "default" and not unlocked_banners.has(b.id):
			unlocked_banners.append(b.id)
	for e in EMBLEMS:
		if e.source == "default" and not unlocked_emblems.has(e.id):
			unlocked_emblems.append(e.id)

func save_profile() -> void:
	var cfg := ConfigFile.new()
	cfg.set_value("profile", "username", username)
	cfg.set_value("profile", "rp", rp)
	cfg.set_value("profile", "coins", coins)
	cfg.set_value("profile", "matches_played", matches_played)
	cfg.set_value("profile", "matches_won", matches_won)
	cfg.set_value("profile", "season_id", season_id)
	cfg.set_value("profile", "equipped_frame", equipped_frame)
	cfg.set_value("profile", "equipped_banner", equipped_banner)
	cfg.set_value("profile", "equipped_emblem", equipped_emblem)
	cfg.set_value("profile", "unlocked_frames", unlocked_frames)
	cfg.set_value("profile", "unlocked_banners", unlocked_banners)
	cfg.set_value("profile", "unlocked_emblems", unlocked_emblems)
	cfg.save(PROFILE_PATH)

func _recompute_level() -> void:
	level = rp_to_level(rp)

func rp_to_level(amount: int) -> int:
	var n: int = 1
	var cum: int = 0
	while true:
		var needed: int = int(50.0 * pow(float(n), 1.35))
		if cum + needed > amount:
			return n
		cum += needed
		n += 1
		if n > 99:
			return 99
	return n

func rp_for_level(lvl: int) -> int:
	var cum: int = 0
	for i in range(1, lvl):
		cum += int(50.0 * pow(float(i), 1.35))
	return cum

func rp_to_next_level() -> int:
	return rp_for_level(level + 1) - rp

func rp_progress_in_level() -> float:
	var current_floor: int = rp_for_level(level)
	var next_floor: int = rp_for_level(level + 1)
	if next_floor - current_floor <= 0:
		return 1.0
	return clampf(float(rp - current_floor) / float(next_floor - current_floor), 0.0, 1.0)

func award_rp(amount: int, won: bool) -> int:
	# Returns levels gained (0 if none).
	matches_played += 1
	if won:
		matches_won += 1
	var prev_level: int = level
	rp += amount
	_recompute_level()
	var newly_unlocked: Array = _auto_unlock_for_level(level)
	save_profile()
	profile_changed.emit()
	if level > prev_level:
		level_up.emit(level)
	for item in newly_unlocked:
		item_unlocked.emit(item.id, item.kind)
	return level - prev_level

func _auto_unlock_for_level(lvl: int) -> Array:
	var newly: Array = []
	for f in FRAMES:
		if f.source in ["default", "level"] and int(f.level) <= lvl and not unlocked_frames.has(f.id):
			unlocked_frames.append(f.id)
			newly.append({"id": f.id, "kind": "frame"})
	for b in BANNERS:
		if b.source in ["default", "level"] and int(b.level) <= lvl and not unlocked_banners.has(b.id):
			unlocked_banners.append(b.id)
			newly.append({"id": b.id, "kind": "banner"})
	for e in EMBLEMS:
		if e.source in ["default", "level"] and int(e.level) <= lvl and not unlocked_emblems.has(e.id):
			unlocked_emblems.append(e.id)
			newly.append({"id": e.id, "kind": "emblem"})
	return newly

func get_rank_tier() -> Dictionary:
	var tier: Dictionary = RANK_TIERS[0]
	for t in RANK_TIERS:
		if level >= int(t.min_level):
			tier = t
	return tier

func get_frame_def(id: String) -> Dictionary:
	for f in FRAMES:
		if str(f.id) == id:
			return f
	return FRAMES[0]

func get_banner_def(id: String) -> Dictionary:
	for b in BANNERS:
		if str(b.id) == id:
			return b
	return BANNERS[0]

func get_emblem_def(id: String) -> Dictionary:
	for e in EMBLEMS:
		if str(e.id) == id:
			return e
	return EMBLEMS[0]

func is_unlocked(id: String, kind: String) -> bool:
	match kind:
		"frame":  return unlocked_frames.has(id)
		"banner": return unlocked_banners.has(id)
		"emblem": return unlocked_emblems.has(id)
	return false

func set_username(name: String) -> void:
	var clean: String = name.strip_edges()
	if clean.length() > 14:
		clean = clean.substr(0, 14)
	if clean == "":
		clean = "RIVAL"
	username = clean
	first_run = false
	save_profile()
	profile_changed.emit()
	username_set.emit()

func equip(kind: String, item_id: String) -> bool:
	if not is_unlocked(item_id, kind):
		return false
	match kind:
		"frame":  equipped_frame = item_id
		"banner": equipped_banner = item_id
		"emblem": equipped_emblem = item_id
		_: return false
	save_profile()
	profile_changed.emit()
	return true

func award_coins(amount: int) -> void:
	if amount <= 0:
		return
	coins += amount
	save_profile()
	profile_changed.emit()

func _def_for(kind: String, item_id: String) -> Dictionary:
	match kind:
		"frame":  return get_frame_def(item_id)
		"banner": return get_banner_def(item_id)
		"emblem": return get_emblem_def(item_id)
	return {}

func get_store_price(kind: String, item_id: String) -> int:
	return int(_def_for(kind, item_id).get("price", 0))

func can_afford(kind: String, item_id: String) -> bool:
	var price: int = get_store_price(kind, item_id)
	return price > 0 and coins >= price

func purchase(kind: String, item_id: String) -> bool:
	# Buy a store-source cosmetic with coins. Returns true on success.
	if is_unlocked(item_id, kind):
		return false
	var def: Dictionary = _def_for(kind, item_id)
	if str(def.get("source", "")) != "store":
		return false
	var price: int = int(def.get("price", 0))
	if price <= 0 or coins < price:
		return false
	coins -= price
	match kind:
		"frame":
			if not unlocked_frames.has(item_id):
				unlocked_frames.append(item_id)
		"banner":
			if not unlocked_banners.has(item_id):
				unlocked_banners.append(item_id)
		"emblem":
			if not unlocked_emblems.has(item_id):
				unlocked_emblems.append(item_id)
		_:
			return false
	save_profile()
	profile_changed.emit()
	item_unlocked.emit(item_id, kind)
	return true

func current_card_data() -> Dictionary:
	var tier: Dictionary = get_rank_tier()
	return {
		"username": username if username != "" else "RIVAL",
		"level": level,
		"rp": rp,
		"rp_progress": rp_progress_in_level(),
		"tier": str(tier.label),
		"tier_color": tier.color,
		"frame_color": get_frame_def(equipped_frame).color,
		"banner_colors": get_banner_def(equipped_banner).colors,
		"emblem_color": get_emblem_def(equipped_emblem).color,
		"emblem_id": equipped_emblem,
	}
