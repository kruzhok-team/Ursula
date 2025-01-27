shader_type spatial;

// текстуры
uniform sampler2D _SnowTex;
uniform sampler2D _RocksTex;
uniform sampler2D _GrassTex;
uniform sampler2D _GroundTex;
uniform sampler2D _SandTex;
uniform sampler2D _LavaTex;

//uniform float heightZero : hint_range(-100.0, 100.0) = 36.5;
uniform float heightSnow : hint_range(-100.0, 100.0) = 15;
uniform float heightRocks : hint_range(-100.0, 100.0) = 10;
uniform float heightGrassMax : hint_range(-100.0, 100.0) = 5;
uniform float heightGrassMin : hint_range(-100.0, 100.0) = -7;
uniform float heightGroundMax : hint_range(-100.0, 100.0) = -11;
uniform float heightGroundMin : hint_range(-100.0, 100.0) = -12;
uniform float heightSand : hint_range(-100.0, 100.0) = -15;
uniform float heightLava : hint_range(-100.0, 100.0) = -30;

uniform float tiling : hint_range(0.0, 10.0);

uniform vec4 light_color;
uniform vec3 light_direction;
uniform float ambient_strength : hint_range(0.0, 1.0);

varying vec3 pos;

void vertex() {
    // Получите координаты вершины
    pos = VERTEX;
}

void fragment() {

	float height = pos.y;

	vec4 snowColor = texture(_SnowTex, UV * tiling);
    vec4 rocksColor = texture(_RocksTex, UV * tiling);
	vec4 grassColor = texture(_GrassTex, UV * tiling);
	vec4 groundColor = texture(_GroundTex, UV * tiling);
	vec4 sandColor = texture(_SandTex, UV * tiling);
	vec4 lavaColor = texture(_LavaTex, UV * tiling);

	vec4 texLerp;

	float blend1 = (height > heightGrassMin) ? float(0) : float(1) - (height - heightGroundMax)/(heightGrassMin - heightGroundMax);
	texLerp = mix(grassColor, groundColor, blend1);

	// Only Grass
    texLerp = (height > heightGrassMin && height < heightGrassMax) ? grassColor : texLerp;

	float blend2 = (height < heightGroundMin) ? float(1) - (height - heightSand)/(heightGroundMin - heightSand) : float(0) ;
    blend2 = (height < heightSand) ? 1.0 : blend2;
    texLerp = (height < heightGroundMin) ? mix(texLerp, sandColor, blend2) : texLerp;

    // Only Ground
    texLerp = (height > heightGroundMin && height < heightGroundMax) ? groundColor : texLerp;

	float blend3 = (height < heightSand) ? 1.0 - (height - heightLava)/(heightSand - heightLava) : float(0) ;
    blend3 = (height < heightLava) ? 1.0 : blend3;
    texLerp = (height < heightSand) ? mix(texLerp, lavaColor, blend3) : texLerp;

	float blend4 = (height > heightGrassMax) ? 1.0 - (height - heightRocks)/(heightGrassMax - heightRocks) : blend1 ;
    texLerp = (height > heightGrassMax) ? mix(texLerp, rocksColor, blend4) : texLerp;

	float blend5 = (height > heightRocks) ? 1.0 - (height - heightSnow)/(heightRocks - heightSnow) : blend1 ;
    texLerp = (height > heightRocks) ? mix(texLerp, snowColor, blend5) : texLerp;

	// MAX
    texLerp = (height > heightSnow) ? snowColor : texLerp;
    // MIN
    texLerp = (height < heightLava) ? lavaColor : texLerp;

	//COLOR = finalColor;
	//COLOR = vec4(vec3(height), 1.0);
	//ALBEDO = vec3(texLerp.r, texLerp.g, texLerp.b);

	//texLerp = mix(grassColor, groundColor, 0.5f);
	//ALBEDO = texLerp.rgb;
	//ALBEDO = COLOR.rgb;

	vec3 normal = normalize(NORMAL);
	float diff = max(dot(normal, -light_direction), 0.0);
    vec3 diffuse = diff * light_color.rgb;
	vec3 ambient = ambient_strength * light_color.rgb;

	ALBEDO = texLerp.rgb + ambient;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
