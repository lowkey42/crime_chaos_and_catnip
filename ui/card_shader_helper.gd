extends TextureRect

var is_mouse_inside = false

var mouse_pos : Vector2

func _process(delta: float) -> void:
	var mouse_position = get_global_mouse_position()
	var relative_mouse_position = mouse_position - global_position
	var s = scale * get_parent().scale
	mouse_pos = lerp(mouse_pos, relative_mouse_position/s - size/2.0 if get_global_rect().has_point(mouse_position) else Vector2(0.0,0.0), 0.2)
	material.set_shader_parameter("_mousePos", relative_mouse_position/s - size/2.0)
