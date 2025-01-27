
shader_type spatial;

uniform sampler2D texture1; // Текстура для низких высот
uniform sampler2D texture2; // Текстура для средних высот
uniform sampler2D texture3; // Текстура для высоких высот

uniform float heightScale : hint_range(0.0, 1.0); // Масштаб высоты
uniform float midHeight : hint_range(0.0, 1.0); // Высота, где начинается переход от texture1 к texture2
uniform float highHeight : hint_range(0.0, 1.0); // Высота, где начинается переход от texture2 к texture3

uniform float heightSnow : hint_range(-100.0, 100.0) = 15;
uniform float heightRocks : hint_range(-100.0, 100.0) = 10;
uniform float heightGrass : hint_range(-100.0, 100.0) = 5;

varying vec3 pos;
void vertex() {
    // Получите координаты вершины
    pos = VERTEX;
}

void fragment() {
    vec3 worldNormal = normalize(NORMAL); // Нормали
    vec3 worldPos = pos; // NODE_POSITION_WORLD; // Позиция вершины в мировых координатах

    // Нормализуем высоту в диапазоне 0-1
    float height = clamp(worldPos.y * heightScale, 0.0, 1.0);

    // Определяем, какую текстуру использовать по высоте
    vec4 final_color;

    if (height < midHeight) {
        final_color = texture(texture1, UV);
    } else if (height < highHeight) {
        float blendFactor = (height - midHeight) / (highHeight - midHeight);
        final_color = mix(texture(texture2, UV), texture(texture1, UV), blendFactor);
    } else {
        float blendFactor = (height - highHeight) / (1.0 - highHeight);
        final_color = mix(texture(texture3, UV), texture(texture2, UV), blendFactor);
    }

    // Устанавливаем финальный цвет
    ALBEDO = final_color.rgb;
}
