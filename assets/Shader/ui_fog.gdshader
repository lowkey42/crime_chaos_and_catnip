shader_type canvas_item;

uniform vec4 color_light_bottom : source_color = vec4(1,1,1,1);
uniform vec4 color_light_top : source_color = vec4(1,1,1,1);
uniform vec4 color_dark_bottom : source_color = vec4(0,0,0,0);
uniform vec4 color_dark_top : source_color = vec4(0,0,0,0);

uniform vec2 speed_1 = vec2(-0.051, 0.0324);
uniform vec2 speed_2 = vec2(0.00508, 0.039);
uniform float turbulence : hint_range(0.0, 1.0) = 0.13;

void fragment() {
	vec3 noiseUV = texture(TEXTURE, SCREEN_UV* 2.0 + TIME*speed_1).rgb;
	vec2 noise = texture(TEXTURE, SCREEN_UV* 2.0 + TIME*speed_2 + noiseUV.rg*turbulence).rg;

	float fog = noise.r*0.8 + noise.g*0.4;
	fog = mix(fog, (1.0-fog) * noiseUV.b, 0.4);

	float cutoff = smoothstep(0.1, 1.0, UV.y);

	vec3 color = mix(vec3(0.8, 0.2, 0.8), vec3(1.0, 0.6, 1.0), cutoff);

	float alpha = fog * mix(0.0, 1.0, cutoff) + mix(0.0, 0.7, cutoff)*1.8;

	alpha *= min(1.0, (1.0-smoothstep(0.2, 0.5, 1.0-length(SCREEN_UV*2.0-1.0))) + 0.2);

	vec4 color_light = mix(color_light_top, color_light_bottom, cutoff);
	vec4 color_dark = mix(color_dark_top, color_dark_bottom, cutoff);
	fog = mix(1.0, (1.1-fog), 1.5);
	COLOR = mix(color_dark, color_light, fog) * vec4(1.0, 1.0, 1.0, alpha * 0.8);
}
