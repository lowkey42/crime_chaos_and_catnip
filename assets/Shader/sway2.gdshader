shader_type spatial;
render_mode blend_mix, cull_disabled, depth_prepass_alpha;

uniform vec3 wind_direction = vec3(1.0, 0.0, 0.0);
uniform float wind_intensity = 1;
uniform float wind_speed = 1.0; // Speed of the wind animation
uniform float wind_frequency = 1.0; // Frequency of wind oscillation
uniform sampler2D albedo_texture : source_color;
uniform float alpha_threshold = 0.5;
uniform float height_start = 1.0; // Height at which wind starts affecting the object
uniform float height_influence = 20.0; // How much height affects wind strength

void vertex() {
    // Calculate wind effect
    float time_factor = TIME * wind_speed;
    float wind_effect = sin(VERTEX.y * wind_frequency + time_factor) * wind_intensity;

    // Only apply wind to vertices above height_start
    // Use smoothstep for a gradual transition
    float height_factor = smoothstep(height_start, height_start + height_influence, VERTEX.y);

    // Apply wind only to the top part
    VERTEX += wind_direction * wind_effect * height_factor;
}

void fragment() {
    vec4 albedo_tex = texture(albedo_texture, UV);

    // Apply alpha scissoring for clean edges
    if (albedo_tex.a < alpha_threshold) {
        discard;
    }

    ALBEDO = albedo_tex.rgb;
    ALPHA = albedo_tex.a;

    // Add some subtle color variation based on wind
    float wind_color = sin(TIME * wind_speed * 0.5) * 0.05;
    ALBEDO += vec3(wind_color, wind_color * 0.7, 0.0);
}