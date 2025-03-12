shader_type canvas_item;

uniform bool highlight_active = false; // Steuert, ob der Glow-Effekt aktiv ist
uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 0.0, 1.0); // Farbe des Glow-Effekts (gelb)
uniform float glow_thickness : hint_range(0.0, 10.0) = 2.0; // Dicke des Glow-Effekts
uniform float glow_intensity : hint_range(0.0, 1.0) = 0.5; // Intensität des Glow-Effekts
uniform float pulse_speed : hint_range(0.0, 10.0) = 1.0; // Geschwindigkeit des Puls-Effekts

void fragment() {
    // Standardfarbe der Textur
    vec4 color = texture(TEXTURE, UV);

    // Nur den Glow-Effekt anwenden, wenn highlight_active true ist
    if (highlight_active) {
        // Puls-Effekt berechnen
        float pulse = sin(TIME * pulse_speed) * 0.5 + 0.5; // Wert zwischen 0 und 1

        // Glow-Effekt berechnen
        vec2 pixel_size = TEXTURE_PIXEL_SIZE * glow_thickness;
        vec4 glow = vec4(0.0);

        // Überprüfen Sie die umliegenden Pixel
        glow += texture(TEXTURE, UV + vec2(pixel_size.x, 0.0));
        glow += texture(TEXTURE, UV + vec2(-pixel_size.x, 0.0));
        glow += texture(TEXTURE, UV + vec2(0.0, pixel_size.y));
        glow += texture(TEXTURE, UV + vec2(0.0, -pixel_size.y));

        if (glow.a > 0.0) {
            COLOR = mix(color, glow_color, glow_intensity * pulse);
        } else {
            COLOR = color;
        }
    } else {
        // Kein Glow-Effekt, zeige die Standardtextur
        COLOR = color;
    }
}