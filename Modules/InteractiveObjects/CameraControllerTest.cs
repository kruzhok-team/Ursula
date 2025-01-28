using Godot;
using System;

public partial class CameraControllerTest : Camera3D
{
    public static CameraControllerTest instance;

	[Export] private float sensitivity = 1f; // Чувствительность мыши
	private Vector2 _rotation = Vector2.Zero;
	private float MoveSpeed = 100f;

	public override void _Ready()
	{
        instance ??= this;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private bool _isEscapePressed = false;
    private int isEscapePressed = 0;

    public static bool isLock
    {
        get { return Input.MouseMode != Input.MouseModeEnum.Captured; }
    }

    public override void _Input(InputEvent @event)
	{
        // Проверка на нажатие клавиши Escape для выхода из режима захвата
        if (Input.IsKeyPressed(Key.Escape) && @event.IsPressed())
        {
            if (CameraControllerTest.instance == this)
            {
                if (Input.MouseMode == Input.MouseModeEnum.Captured)
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                else
                    Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }

        // Вращение камеры
        if (!CameraControllerTest.isLock) {
            // Обработка события движения мыши
            if (@event is InputEventMouseMotion mouseMotion)
            {
                // Получаем движение мыши
                Vector2 mouseMovement = mouseMotion.Relative;

                // Обновляем углы вращения
                _rotation[0] -= mouseMovement[1] * sensitivity; // Вертикальное вращение
                _rotation[1] -= mouseMovement[0] * sensitivity; // Горизонтальное вращение

                // Ограничиваем вертикальное вращение
                _rotation[0] = Mathf.Clamp(_rotation[0], -90f, 90f);

                // Применяем вращение к камере
                RotationDegrees = new Vector3(_rotation[0], _rotation[1], 0); // Обновляем вращение камеры
            }
        }
	}

	public override void _ExitTree()
	{

	}
	
	public override void _Process(double delta)
	{
        // Движение камеры
        {
            Vector3 direction = new Vector3();
            if (Input.IsActionPressed("ui_up")) direction[2] -= 1;
            if (Input.IsActionPressed("ui_down")) direction[2] += 1;
            if (Input.IsActionPressed("ui_left")) direction[0] -= 1;
            if (Input.IsActionPressed("ui_right")) direction[0] += 1;

            // Нормализуем направление
            direction = (Transform.Basis * direction).Normalized();
            Position += direction * MoveSpeed * (float)delta; // Перемещаем камеру
        }

    }
}
