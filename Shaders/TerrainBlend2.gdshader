
shader_type spatial;

uniform sampler2D _SnowTex;
uniform sampler2D _RocksTex;
uniform sampler2D _GrassTex;
uniform sampler2D _GroundTex;
uniform sampler2D _SandTex;
uniform sampler2D _LavaTex;


//uniform float heightZero : hint_range(-100.0, 100.0) = 36.5;
uniform float heightSnow : hint_range(-100.0, 100.0) = 15;
uniform float heightRocks : hint_range(-100.0, 100.0) = 10;
uniform float heightGrass : hint_range(-100.0, 100.0) = 5;
//uniform float heightGrassMin : hint_range(-100.0, 100.0) = -7;
uniform float heightGround : hint_range(-100.0, 100.0) = -11;
//uniform float heightGroundMin : hint_range(-100.0, 100.0) = -12;
uniform float heightSand : hint_range(-100.0, 100.0) = -15;
uniform float heightLava : hint_range(-100.0, 100.0) = -30;

uniform float tiling : hint_range(0.0, 1.0);

varying vec3 pos;
void vertex() {
    // Получите координаты вершины
    pos = VERTEX;
}

void fragment( )
{
    vec3 worldNormal = normalize(NORMAL); // Нормали
    vec3 worldPos = pos; // NODE_POSITION_WORLD; // Позиция вершины в мировых координатах

	vec4 final_color;

	float height = pos.y;

	if (height < heightLava) {
        final_color = texture(_LavaTex, UV * tiling);
    } else if (height < heightSand) {
        final_color = mix(texture(_SandTex, UV * tiling), texture(_LavaTex, UV * tiling), height/heightSand);
    }else if (height < heightGround) {
        final_color = mix(texture(_GroundTex, UV * tiling), texture(_SandTex, UV * tiling), height/heightSand);
    }else if (height < heightGrass) {
        final_color = mix(texture(_GrassTex, UV * tiling), texture(_GroundTex, UV * tiling), height/heightSand);
    }else if (height < heightRocks) {
        final_color = mix(texture(_RocksTex, UV * tiling), texture(_GrassTex, UV * tiling), height/heightSand);
    }
	else{
		final_color = texture(_SnowTex, UV * tiling);
	}


	ALBEDO = final_color.rgb;
}
