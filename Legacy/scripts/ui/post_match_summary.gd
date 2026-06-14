class_name PostMatchSummary extends Control

# Reusable post-match summary overlay shown at the end of a match in BOTH
# arenas (4-Player Bomb FFA and Brain VS Brain). It dims the arena, shows a
# VICTORY / DEFEAT headline, a row of neon stat cards (with count-up
# animation), the rank-point breakdown, and an updated level / rank bar.
# A CONTINUE button drives the scene change; a safety timer auto-continues
# if the player walks away.
#
# Driven entirely by present(cfg). The caller supplies a config Dictionary so
# each arena can show its own meaningful stats:
#   {
#     "won": bool,
#     "headline": String,           # e.g. "VICTORY", "DEFEAT", "MATCH OVER"
#     "subtitle": String,           # e.g. "You outlasted the field"
#     "accent": Color,              # headline + chrome accent
#     "stats": [                    # row of stat cards (left to right)
#       {"name": "SOLVES", "value": 7, "color": Color(...)},        # int -> count-up
#       {"name": "PLACEMENT", "text": "1st of 4", "color": Color(...)},  # static text
#     ],
#     "rp_lines": [ {"label": "Base", "amount": 60, "color": Color(...)} ],
#     "rp_total": int,
#     "levels_gained": int,
#   }

signal continue_pressed

const AUTO_CONTINUE_SECONDS: float = 45.0

var _continued: bool = false
var _continue_btn: Button = null
var _panel: PanelContainer = null

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

func present(cfg: Dictionary) -> void:
	var accent: Color = cfg.get("accent", Color(0.0, 0.90, 1.0))
	var won: bool = bool(cfg.get("won", false))

	# Dim backdrop — swallows clicks so the arena underneath is inert.
	var dim := ColorRect.new()
	dim.color = Color(0.02, 0.03, 0.08, 0.0)
	dim.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(dim)
	dim.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	dim.create_tween().tween_property(dim, "color:a", 0.78, 0.35)

	# Centered card.
	var center := CenterContainer.new()
	center.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(center)
	center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)

	_panel = PanelContainer.new()
	_panel.theme_type_variation = "DialogPanel"
	_panel.mouse_filter = Control.MOUSE_FILTER_STOP
	# Fallback chrome in case the theme has no DialogPanel variation.
	var pstyle := StyleBoxFlat.new()
	pstyle.bg_color = Color(0.04, 0.06, 0.14, 0.98)
	pstyle.border_color = Color(accent.r, accent.g, accent.b, 0.9)
	pstyle.set_border_width_all(3)
	pstyle.set_corner_radius_all(18)
	pstyle.set_content_margin_all(0)
	pstyle.shadow_color = Color(accent.r, accent.g, accent.b, 0.35)
	pstyle.shadow_size = 18
	_panel.add_theme_stylebox_override("panel", pstyle)
	center.add_child(_panel)

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 44)
	margin.add_theme_constant_override("margin_right", 44)
	margin.add_theme_constant_override("margin_top", 32)
	margin.add_theme_constant_override("margin_bottom", 32)
	_panel.add_child(margin)

	var vbox := VBoxContainer.new()
	vbox.alignment = BoxContainer.ALIGNMENT_CENTER
	vbox.add_theme_constant_override("separation", 14)
	vbox.custom_minimum_size = Vector2(640, 0)
	margin.add_child(vbox)

	# Headline.
	var headline := Label.new()
	headline.text = str(cfg.get("headline", "MATCH OVER"))
	headline.add_theme_font_size_override("font_size", 64)
	headline.add_theme_color_override("font_color", accent)
	headline.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	headline.add_theme_constant_override("outline_size", 8)
	headline.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	headline.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(headline)

	# Subtitle.
	var subtitle_text: String = str(cfg.get("subtitle", ""))
	if subtitle_text != "":
		var subtitle := Label.new()
		subtitle.text = subtitle_text
		subtitle.add_theme_font_size_override("font_size", 24)
		subtitle.add_theme_color_override("font_color", Color(0.85, 0.95, 1.0))
		subtitle.add_theme_color_override("font_outline_color", Color(0, 0, 0))
		subtitle.add_theme_constant_override("outline_size", 5)
		subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		subtitle.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.add_child(subtitle)

	# Stat cards row.
	var stats: Array = cfg.get("stats", [])
	if not stats.is_empty():
		var spacer_a := Control.new()
		spacer_a.custom_minimum_size = Vector2(0, 6)
		spacer_a.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.add_child(spacer_a)

		var row := HBoxContainer.new()
		row.alignment = BoxContainer.ALIGNMENT_CENTER
		row.add_theme_constant_override("separation", 14)
		vbox.add_child(row)
		for entry in stats:
			row.add_child(_make_stat_card(entry as Dictionary))

	# Divider.
	var divider := ColorRect.new()
	divider.color = Color(accent.r, accent.g, accent.b, 0.35)
	divider.custom_minimum_size = Vector2(0, 2)
	divider.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(divider)

	# RP breakdown.
	var rp_total: int = int(cfg.get("rp_total", 0))
	var rp_header := Label.new()
	rp_header.text = "+0 RP"
	rp_header.add_theme_font_size_override("font_size", 38)
	rp_header.add_theme_color_override("font_color", Color(1.0, 0.92, 0.18))
	rp_header.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	rp_header.add_theme_constant_override("outline_size", 6)
	rp_header.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	rp_header.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(rp_header)
	_animate_count(rp_header, rp_total, "+", " RP")

	var rp_lines: Array = cfg.get("rp_lines", [])
	for line_entry in rp_lines:
		var line_dict: Dictionary = line_entry as Dictionary
		var amt: int = int(line_dict.get("amount", 0))
		if amt == 0:
			continue
		var line := Label.new()
		line.text = "%s   +%d RP" % [str(line_dict.get("label", "")), amt]
		line.add_theme_font_size_override("font_size", 20)
		line.add_theme_color_override("font_color", line_dict.get("color", Color(1, 1, 1)))
		line.add_theme_color_override("font_outline_color", Color(0, 0, 0))
		line.add_theme_constant_override("outline_size", 4)
		line.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		line.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.add_child(line)

	# Level + rank line.
	var tier: Dictionary = PlayerProfile.get_rank_tier()
	var rank_line := Label.new()
	rank_line.text = "LEVEL %d  •  %s" % [PlayerProfile.level, str(tier.get("label", "NOVICE"))]
	rank_line.add_theme_font_size_override("font_size", 24)
	rank_line.add_theme_color_override("font_color", tier.get("color", Color(0.6, 0.7, 0.8)))
	rank_line.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	rank_line.add_theme_constant_override("outline_size", 5)
	rank_line.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	rank_line.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(rank_line)

	# Level-progress bar.
	var prog := ProgressBar.new()
	prog.min_value = 0.0
	prog.max_value = 100.0
	prog.value = PlayerProfile.rp_progress_in_level() * 100.0
	prog.show_percentage = false
	prog.custom_minimum_size = Vector2(0, 20)
	prog.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	var bar_bg := StyleBoxFlat.new()
	bar_bg.bg_color = Color(0.06, 0.08, 0.16, 0.95)
	bar_bg.border_color = tier.get("color", Color(0.6, 0.7, 0.8))
	bar_bg.set_border_width_all(2)
	bar_bg.set_corner_radius_all(10)
	var bar_fill := StyleBoxFlat.new()
	bar_fill.bg_color = tier.get("color", Color(0.0, 0.90, 1.0))
	bar_fill.set_corner_radius_all(8)
	prog.add_theme_stylebox_override("background", bar_bg)
	prog.add_theme_stylebox_override("fill", bar_fill)
	vbox.add_child(prog)

	var to_next := Label.new()
	to_next.text = "%d RP to next level" % maxi(0, PlayerProfile.rp_to_next_level())
	to_next.add_theme_font_size_override("font_size", 16)
	to_next.add_theme_color_override("font_color", Color(0.75, 0.83, 0.95, 0.9))
	to_next.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	to_next.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(to_next)

	# Level-up flourish.
	if int(cfg.get("levels_gained", 0)) > 0:
		var levelup := Label.new()
		levelup.text = "LEVEL UP!  (LVL %d)" % PlayerProfile.level
		levelup.add_theme_font_size_override("font_size", 26)
		levelup.add_theme_color_override("font_color", Color(0.61, 1.0, 0.18))
		levelup.add_theme_color_override("font_outline_color", Color(0, 0, 0))
		levelup.add_theme_constant_override("outline_size", 6)
		levelup.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		levelup.mouse_filter = Control.MOUSE_FILTER_IGNORE
		vbox.add_child(levelup)
		_pulse(levelup)

	# Continue button.
	var spacer_b := Control.new()
	spacer_b.custom_minimum_size = Vector2(0, 8)
	spacer_b.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(spacer_b)

	_continue_btn = _make_continue_button(accent)
	vbox.add_child(_continue_btn)

	# Win/loss tint on the headline chrome.
	if won:
		AudioManager.play_sfx(GameState.SFX_UI_CONFIRM, -4.0)

	# Pop-in + safety auto-continue.
	_play_intro()
	_arm_auto_continue()

func _make_stat_card(entry: Dictionary) -> Control:
	var color: Color = entry.get("color", Color(0.0, 0.90, 1.0))
	var card := PanelContainer.new()
	card.custom_minimum_size = Vector2(132, 116)
	card.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.06, 0.08, 0.16, 0.96)
	style.border_color = Color(color.r, color.g, color.b, 0.85)
	style.set_border_width_all(2)
	style.set_corner_radius_all(12)
	style.set_content_margin_all(10)
	card.add_theme_stylebox_override("panel", style)

	var inner := VBoxContainer.new()
	inner.alignment = BoxContainer.ALIGNMENT_CENTER
	inner.add_theme_constant_override("separation", 4)
	inner.mouse_filter = Control.MOUSE_FILTER_IGNORE
	card.add_child(inner)

	var value_lbl := Label.new()
	value_lbl.add_theme_font_size_override("font_size", 42)
	value_lbl.add_theme_color_override("font_color", color)
	value_lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	value_lbl.add_theme_constant_override("outline_size", 6)
	value_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	value_lbl.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	value_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	value_lbl.size_flags_vertical = Control.SIZE_EXPAND_FILL
	inner.add_child(value_lbl)

	if entry.has("text"):
		value_lbl.text = str(entry["text"])
		# Smaller font for multi-character text values.
		if str(entry["text"]).length() > 3:
			value_lbl.add_theme_font_size_override("font_size", 30)
	else:
		value_lbl.text = "0"
		_animate_count(value_lbl, int(entry.get("value", 0)), "", "")

	var name_lbl := Label.new()
	name_lbl.text = str(entry.get("name", ""))
	name_lbl.add_theme_font_size_override("font_size", 16)
	name_lbl.add_theme_color_override("font_color", Color(0.82, 0.9, 1.0))
	name_lbl.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	name_lbl.add_theme_constant_override("outline_size", 3)
	name_lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	name_lbl.mouse_filter = Control.MOUSE_FILTER_IGNORE
	inner.add_child(name_lbl)

	return card

func _make_continue_button(accent: Color) -> Button:
	var btn := Button.new()
	btn.text = "CONTINUE"
	btn.custom_minimum_size = Vector2(280, 64)
	btn.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	btn.focus_mode = Control.FOCUS_NONE
	btn.add_theme_font_size_override("font_size", 28)
	btn.add_theme_color_override("font_color", Color(0.03, 0.05, 0.12))
	btn.add_theme_color_override("font_hover_color", Color(0.03, 0.05, 0.12))
	btn.add_theme_color_override("font_pressed_color", Color(0.03, 0.05, 0.12))
	var sb := StyleBoxFlat.new()
	sb.bg_color = accent
	sb.set_corner_radius_all(14)
	sb.set_content_margin_all(8)
	sb.border_color = Color(1, 1, 1, 0.4)
	sb.set_border_width_all(2)
	btn.add_theme_stylebox_override("normal", sb)
	btn.add_theme_stylebox_override("hover", sb)
	btn.add_theme_stylebox_override("pressed", sb)
	btn.add_theme_stylebox_override("focus", sb)
	btn.mouse_entered.connect(func(): _animate_btn_hover(btn, true))
	btn.mouse_exited.connect(func(): _animate_btn_hover(btn, false))
	btn.pressed.connect(_on_continue)
	return btn

func _animate_btn_hover(btn: Button, hovering: bool) -> void:
	if not is_instance_valid(btn):
		return
	btn.pivot_offset = btn.size / 2.0
	var target_scale := Vector2(1.05, 1.05) if hovering else Vector2(1.0, 1.0)
	var target_mod := Color(0.9, 0.9, 0.9) if hovering else Color(1, 1, 1)
	var tw := btn.create_tween().set_parallel(true)
	tw.tween_property(btn, "scale", target_scale, 0.12).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	tw.tween_property(btn, "modulate", target_mod, 0.10)

func _animate_count(label: Label, target: int, prefix: String, suffix: String) -> void:
	if not is_instance_valid(label):
		return
	if target <= 0:
		label.text = "%s%d%s" % [prefix, target, suffix]
		return
	var tw := create_tween()
	tw.tween_method(func(v: float):
		if is_instance_valid(label):
			label.text = "%s%d%s" % [prefix, int(round(v)), suffix]
	, 0.0, float(target), 0.7).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)

func _pulse(node: Control) -> void:
	if not is_instance_valid(node):
		return
	var tw := node.create_tween().set_loops(4)
	tw.tween_property(node, "modulate", Color(1.3, 1.3, 1.3, 1.0), 0.4)
	tw.tween_property(node, "modulate", Color(1.0, 1.0, 1.0, 1.0), 0.4)

func _play_intro() -> void:
	await get_tree().process_frame
	if not is_instance_valid(_panel):
		return
	_panel.pivot_offset = _panel.size / 2.0
	_panel.scale = Vector2(0.9, 0.9)
	_panel.modulate = Color(1, 1, 1, 0.0)
	var tw := _panel.create_tween().set_parallel(true)
	tw.tween_property(_panel, "modulate:a", 1.0, 0.28)
	tw.tween_property(_panel, "scale", Vector2(1.0, 1.0), 0.38).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)

func _arm_auto_continue() -> void:
	await get_tree().create_timer(AUTO_CONTINUE_SECONDS).timeout
	if is_instance_valid(self) and not _continued:
		_on_continue()

func _on_continue() -> void:
	if _continued:
		return
	_continued = true
	AudioManager.play_sfx(GameState.SFX_UI_CLICK, -6.0)
	continue_pressed.emit()
