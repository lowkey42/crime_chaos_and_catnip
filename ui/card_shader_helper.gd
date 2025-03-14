extends TextureRect

var is_mouse_inside = false

var mouse_pos := Vector2.ZERO
var intensity := 0

func _process(delta: float) -> void:
	var mouse_position = get_global_mouse_position()
	var relative_mouse_position = mouse_position - global_position
	var s = scale * get_parent().scale
	if get_global_rect().expand(get_global_rect().size*2.0).has_point(mouse_position):
		mouse_pos = relative_mouse_position/s - size/2.0
		intensity = 1.0
	else:
		intensity = max(0.0, intensity - delta/4.0)

	material.set_shader_parameter("_mousePos", mouse_pos)
	material.set_shader_parameter("_intensity", intensity)
