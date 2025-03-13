shader_type spatial;

uniform float distance_fade_start = 15.0;
uniform float distance_fade_end = 30.0;

uniform float thickness = 1.0;
uniform float fade_distance = 0.1;
uniform float fade_exp = 4.0;
uniform vec3 grid_color : source_color = vec3(1.0, 1.0, 1.0);

uniform float animation_amount = 0.04;
uniform float animation_overshoot = 0.0;
uniform float animation_speed = 4.0;

float gridFactor (vec2 parameter, float width) {
	vec2 d = fwidth(parameter);
	vec2 looped = 0.5 - abs(parameter - 0.5);
	vec2 a3 = smoothstep(d * (width - 0.5), d * (width + 0.5), looped);
	return min(a3.x, a3.y);
}

void vertex() {
	float alpha = sin(TIME*animation_speed + length(NODE_POSITION_WORLD) * 0.01)/2.0-0.5;
	VERTEX *= animation_overshoot + mix(1.0-animation_amount, 1.0, alpha);
}

void fragment() {
	vec2 uvCentered = abs(UV-0.5) * 2.0;
	float distance_to_border = max(uvCentered.x, uvCentered.y);
	float fade = smoothstep(1.0-fade_distance, 1.0, distance_to_border);
	fade = pow(fade, fade_exp);

	float distance_fade = 1.0 - smoothstep(distance_fade_start, distance_fade_end, length(VERTEX));

	ALBEDO = grid_color * COLOR.rgb;
	ALPHA = max(fade, 1.0 - gridFactor(UV, thickness)) * distance_fade * COLOR.a;
}