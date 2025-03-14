shader_type canvas_item;

void fragment() {
	vec3 noiseUV = texture(TEXTURE, SCREEN_UV* 2.0 + TIME*vec2(0.342, 0.54)*0.06).rgb;
	vec2 noise = texture(TEXTURE, SCREEN_UV* 2.0 + TIME*vec2(0.127, 0.975)*0.04 + noiseUV.rg*0.13).rg;

	float fog = noise.r*0.8 + noise.g*0.4;
	fog = mix(fog, (1.0-fog) * noiseUV.b, 0.4);

	float cutoff = smoothstep(0.1, 0.9, UV.y);

	vec3 color = mix(vec3(0.2, 0.2, 0.3), vec3(0.6, 0.6, 0.6), cutoff);

	float alpha = fog * mix(0.0, 1.0, cutoff) + mix(0.0, 0.7, cutoff)*1.8;

	alpha *= min(1.0, (1.0-smoothstep(0.2, 0.5, 1.0-length(SCREEN_UV*2.0-1.0))) + 0.2);

	COLOR = vec4(mix(color, color*(1.1-fog), 1.5), alpha * 0.8);
	//COLOR = vec4(vec3(1.0,1.0,1.0) * (1.0-smoothstep(0.2, 0.5, 1.0-length(SCREEN_UV*2.0-1.0))), 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
