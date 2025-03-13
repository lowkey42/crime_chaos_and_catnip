shader_type spatial;

uniform bool billboard_y = false;
uniform bool enable_details = true;
uniform vec3 detail_color:source_color = vec3(0.2);
uniform vec2 world_size = vec2(10.0);
uniform vec2 world_position = vec2(0);

void vertex() {
	// Called for every vertex the material is visible on.
		float root = (5.0 - UV.y);
	vec3 sway;
	sway.x = sin(NODE_POSITION_WORLD.x + TIME * 1.25 + UV.y) * root * 0.10;
	sway.y = sin(NODE_POSITION_WORLD.x + TIME * 0.6 + UV.y) * root * 0.08;
	sway.z = cos(NODE_POSITION_WORLD.z + TIME * 0.45 + UV.y) * root * 0.15;
	
	if(billboard_y){
		VERTEX += sway;
		MODELVIEW_MATRIX = VIEW_MATRIX * mat4(vec4(normalize(cross(vec3(0.0, 1.0, 0.0), INV_VIEW_MATRIX[2].xyz)), 0.0), vec4(0.0, 1.0, 0.0, 0.0), vec4(normalize(cross(INV_VIEW_MATRIX[0].xyz, vec3(0.0, 1.0, 0.0))), 0.0), MODEL_MATRIX[3]);
	}
	else{
		VERTEX += (vec4(sway, 1.0) * MODEL_MATRIX).xyz;
	}
}

void fragment() {
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
