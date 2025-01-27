shader_type spatial;

uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear_mipmap;

uniform vec4 water_color: source_color = vec4(0.0, 0.2, 1.0, 1.0);

uniform float force: hint_range(0.1, 5.0, 0.1) = 0.5;

uniform float speed: hint_range(0.1, 4.0, 0.1) = 1;

uniform float metalness = 0.6;
uniform float specular = 0.5;
uniform float roughness = 0.2;
uniform float alpha = 0.9;

float offset(float x, float z, float val1, float val2, float time) {
	float radX = ((mod(x + z * x * val1, force) / force) + (time * speed) * mod(x * 0.8 + z, 1.5)) * 2.0 * 3.14;
	float radZ = ((mod(val2 * (z * x + x * z), force) / force) + (time * speed) * 2.0 * mod(x, 2.0)) * 2.0 * 3.14;

	return force * 0.5 * (sin(radZ) * cos(radX));
}

vec3 wave(vec3 vertex, float time) {
	float xd = offset(vertex.x, vertex.z, 0.2, 0.1, time);
	float yd = offset(vertex.x, vertex.z, 0.1, 0.3, time);
	float zd = offset(vertex.x, vertex.z, 0.15, 0.2, time);

	return vertex + vec3(xd, yd, zd);
}

void vertex() {
	VERTEX = wave(VERTEX, TIME * 0.1);
}

void fragment() {
	NORMAL = normalize(cross(dFdx(VERTEX), dFdy(VERTEX)));
	METALLIC = metalness;
	SPECULAR = specular;
	ROUGHNESS = roughness;
	ALBEDO = water_color.rgb;
	ALPHA = alpha;
}