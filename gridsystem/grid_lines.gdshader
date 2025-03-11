shader_type spatial;

uniform float thickness = 0.02;
uniform float fade_distance_outer = 0.02;
uniform float fade_exp_outer = 10.0;
uniform float fade_distance_inner = 0.1;
uniform float fade_exp_inner = 10.0;
uniform float margin = 0.04;
uniform vec3 grid_color : source_color = vec3(1.0, 1.0, 1.0);

uniform float animation_amount = 0.04;
uniform float animation_speed = 4.0;

void fragment() {
	float distance_to_border = max(abs(UV.x - 0.5) * 2.0, abs(UV.y - 0.5) * 2.0);
	
	distance_to_border *= mix(1.0-animation_amount, 1.0, sin(TIME*animation_speed)*0.5+0.5);
	
	float fade_inner = smoothstep(1.0-margin-fade_distance_outer-thickness-fade_distance_inner, 1.0-margin-fade_distance_outer-thickness, distance_to_border);
	fade_inner = pow(fade_inner, fade_exp_inner);
	
	float fade_outer = (1.0 - smoothstep(1.0-margin-fade_distance_outer, 1.0-margin, distance_to_border));
	fade_inner = pow(fade_inner, fade_exp_outer);
	
	float grid = fade_inner * fade_outer;
	
	ALBEDO = grid_color;
	ALPHA = grid;
}