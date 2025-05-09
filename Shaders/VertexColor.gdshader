shader_type spatial;
render_mode vertex_lighting, skip_vertex_transform,
            specular_schlick_ggx, diffuse_lambert_wrap, shadows_disabled;

uniform vec4 modulate: source_color = vec4(1.0);
uniform sampler2D albedo: hint_default_black, filter_nearest;
uniform vec2 uv_scrolling_velocity = vec2(0.0);

const float vertex_snapping_offset = 0.8;


varying float vertex_distance;

uniform float corruption_amount : hint_range(0.0, 1.0) = 0.0; // Adjust the amount of corruption
uniform float corruption_seed : hint_range(0.1, 10.0) = 1.0; // Adjusts a random component of the corruption
uniform bool corrupt_red = true;
uniform bool corrupt_green = true;
uniform bool corrupt_blue = true;
uniform bool checker = true;
uniform vec2 checker_size = vec2(16.0, 16.0);
uniform float checker_offset : hint_range(2.0, 10.0) = 2.0;

void vertex() {
    UV += uv_scrolling_velocity * TIME;

    VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;

    float z_origin = VERTEX.z;
    float snap_i = (1.0 - vertex_snapping_offset) * min(VIEWPORT_SIZE.x, VIEWPORT_SIZE.y) / 2.0;

    float projection_w = (PROJECTION_MATRIX * vec4(VERTEX, 1.0)).w;
    VERTEX = round(VERTEX / projection_w * snap_i) / snap_i * projection_w;
    VERTEX = VERTEX;

    NORMAL = (MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz;

    vertex_distance = length((MODEL_MATRIX * vec4(VERTEX, 1.0)));

    UV *= VERTEX.z;
}

float get_dither_brightness(vec3 given_color, vec4 fragcoord) {

    int x = int(fragcoord.x) % 4;
    int y = int(fragcoord.y) % 4;
    const vec3 luminance = vec3(0.2126, 0.7152, 0.0722);
    float calculated_brightness = (luminance.r * given_color.r) + (luminance.g * given_color.g) + (luminance.b * given_color.b);


    float thresholdMatrix[16] = float[16] (
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    );

    float dithering = thresholdMatrix[x * 4 + y];

    if ((calculated_brightness - 0.2 < dithering) && (vertex_distance < 60.)) {
        return ((dithering - 0.5) * 0.25) + 1.0;
    } else {
        return 1.;
    }
}

//these functions are all magic to me, trust me, they generate noise

float randomr(vec2 uv) {
	return fract(sin(dot(uv, vec2(corruption_seed, 78.233))) * 216.5453);
}

float randomg(vec2 uv) {
	return fract(sin(dot(uv, vec2(corruption_seed + 12.9898, 78.233))) * 123.5453);
}

float randomb(vec2 uv) {
	return fract(sin(dot(uv, vec2(corruption_seed - 12.9898, 78.233))) * 246.5453);
}

void fragment() {
    vec2 uv = UV / VERTEX.z;

    vec3 texture_rgb = (texture(albedo, uv).rgb * COLOR.rgb) * modulate.rgb;
    vec4 texture_rgba = vec4(texture_rgb.r, texture_rgb.g, texture_rgb.b, 1.0);

    ALPHA = texture(albedo, uv).a * modulate.a;
    ALPHA_SCISSOR_THRESHOLD = 0.1;


	// Get size of texture in pixels
	float size_x = float(textureSize(albedo, 0).x);
	float size_y = float(textureSize(albedo, 0).y);

	float checker_pattern;
	// Add a checker pattern to the image
	if (checker){
		checker_pattern = mod(floor(uv.x * checker_size.x) + floor(uv.y * checker_size.y), checker_offset);
	} else {
		//disables the checker pattern
		checker_pattern = 1.0;
	}
	// gets the current pixel's color
	vec4 pixelColor = texture(albedo, uv);
	// Create a new "UV" which remaps every UV value to a snapped pixel value, makes things align to the pixels in the texture
	vec2 UVr = vec2(floor(uv.x*size_x)/size_x, floor(uv.y*size_y)/size_y);
	//this is the offset, honestly, could potentially be useless
	vec2 corruption_offset = vec2(corruption_amount * checker_pattern, corruption_seed );

	vec3 corrupt = vec3(pixelColor.r,pixelColor.g,pixelColor.b);

	if (corrupt_red){
		corrupt.r = step(corruption_amount, randomr(UVr * corruption_offset));
	}

	if (corrupt_green){
		corrupt.g = step(corruption_amount, randomg(UVr * corruption_offset));
	}

	if (corrupt_blue){
		corrupt.b = step(corruption_amount, randomb(UVr * corruption_offset));
	}

    // applies the vertex colors and corruption effects.
	vec3 corrupt_texture_rgba = ((vec3(min(corrupt.r, pixelColor.r), min(corrupt.g, pixelColor.g), min(corrupt.b, pixelColor.b)) * COLOR.rgb) * modulate.rgb) * get_dither_brightness(texture_rgba.rgb, FRAGCOORD);
    ALBEDO = corrupt_texture_rgba.rgb;
}