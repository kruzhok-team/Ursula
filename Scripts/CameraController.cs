using Godot;
using System;

public partial class CameraController : Camera3D
{
    public static CameraController instance;

    [Export] private Camera3D _camera;
    private float sensitivity { get { return VoxLib.Sensitivity; } }
    private Vector2 _rotation = Vector2.Zero;
    private float MoveSpeed = 40f;
    private bool isRun;
    private bool isVivibleInfo;

    bool IsShiftPressed = false;

    public override void _Ready()
    {
        if (instance != null)
        {
            if (IsInstanceValid(instance))
                instance.Free();
            else
                instance = null;
        }

        instance = this;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        CursorShow(true);
    }

    private bool _isEscapePressed = false;
    private int isEscapePressed = 0;

    public static bool isLock
    {
        get { return Input.MouseMode != Input.MouseModeEnum.Captured; }
    }

    public void CursorShow(bool state)
    {
        if (state)
            Input.MouseMode = Input.MouseModeEnum.Visible;
        else
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }


    public override void _Input(InputEvent @event)
    {
        if (Input.IsKeyPressed(Key.Escape) && @event.IsPressed())
        {
            //if (CameraController.instance == this)
            {
                if (Input.MouseMode == Input.MouseModeEnum.Captured)
                    CursorShow(true);
                else
                    CursorShow(false);
            }
        }

        if (!isLock)
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                Vector2 mouseMovement = mouseMotion.Relative;

                _rotation[0] -= mouseMovement[1] * sensitivity; // Вертикальное вращение
                _rotation[1] -= mouseMovement[0] * sensitivity; // Горизонтальное вращение

                _rotation[0] = Mathf.Clamp(_rotation[0], -90f, 90f);

                RotationDegrees = new Vector3(_rotation[0], _rotation[1], 0);
            }
        }

        IsShiftPressed = Input.IsKeyPressed(Key.Shift);
        isRun = (IsShiftPressed && @event.IsPressed());
        isVivibleInfo = (Input.IsKeyPressed(Key.Alt) && @event.IsPressed());
        VoxLib.hud._labelCoordinates.Visible = isVivibleInfo;
        VoxLib.hud._cross.Visible = true;

    }

    public override void _ExitTree()
    {

    }

    
    private void ApplyIpsScale(ItemPropsScript ips, Node parent)
    {
        VoxLib.mapManager.tempScale = ips.scale;
        (parent as Node3D).Scale = Vector3.One * ips.scale;
    }

    private const float DelayBetweenClicks = 0.25f;
    double lastClickTime = 0;

    public override async void _Process(double delta)
    {
        lastClickTime -= delta;

        bool isRotate = false;
        if (Input.IsKeyPressed(Key.R) && lastClickTime < 0)
        {
            isRotate = true;
            lastClickTime = DelayBetweenClicks;
        }
        bool isNormalize = false;
        if (Input.IsKeyPressed(Key.N) && lastClickTime < 0)
        {
            isNormalize = true;
            lastClickTime = DelayBetweenClicks;
        }
        bool isScalePlus = false;
        if ((Input.IsKeyPressed(Key.Equal) || Input.IsKeyPressed(Key.KpAdd)) && lastClickTime < 0)
        {
            isScalePlus = true;
            lastClickTime = DelayBetweenClicks;
        }
        bool isScaleMinus = false;
        if ((Input.IsKeyPressed(Key.Minus) || Input.IsKeyPressed(Key.KpSubtract)) && lastClickTime < 0)
        {
            isScaleMinus = true;
            lastClickTime = DelayBetweenClicks;
        }

        // Движение камеры
        {
            Vector3 direction = new Vector3();

            if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.W)) direction[2] -= 1;
            if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.S)) direction[2] += 1;
            if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A)) direction[0] -= 1;
            if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D)) direction[0] += 1;

            // Нормализуем направление
            direction = (Transform.Basis * direction).Normalized();
            float speed = isRun ? MoveSpeed * 2 : MoveSpeed;

            if (Position.X < 0 && direction.X < 0) direction.X = 0;
            if (Position.X > VoxLib.mapManager.sizeX && direction.X > 0) direction.X = 0;
            if (Position.Z < 0 && direction.Z < 0) direction.Z = 0;
            if (Position.Z > VoxLib.mapManager.sizeZ && direction.Z > 0) direction.Z = 0;
            if (Position.Y < 0 && direction.Y < 0) direction.Y = 0;
            if (Position.Y > VoxLib.mapManager.sizeY && direction.Y > 0) direction.Y = 0;

            if (!Raycaster.HoverUI(_camera))
                Position += direction * speed * (float)delta; // Перемещаем камеру
        }


        if (Raycaster.HitFromCenterOfScreen(_camera, out Node collider, out Vector3 pos, false))
        {
            HUD.ItemProcessing(collider, pos, out var ips, out var parent);

            if (isVivibleInfo)
            {
                VoxLib.hud.SetInfo(HUD.GetCordsInfo(collider, pos, ips, parent));
            }

            if (ips != null)
            {
                if (isRotate)
                {
                    ips.rotation++;

                    if (ips.rotation > 3) ips.rotation = 0;

                    VoxLib.mapManager.tempRotation = ips.rotation;

                    (parent as Node3D).Quaternion = VoxLib.mapManager.GetRotation(ips.rotation);
                }

                if (isNormalize)
                {
                    ips.scale = 1;
                    ApplyIpsScale(ips, parent);
                }

                //if (Input.IsActionJustPressed("ed_scale_plus_slow"))
                //{
                //    ips.scale++;

                //    ApplyIpsScale(ips, parent);
                //}
                //else 
                if (isScalePlus)
                {
                    ips.scale += 0.1f;
                    ApplyIpsScale(ips, parent);
                }

                //if (Input.IsActionJustPressed("ed_scale_minus_slow"))
                //{
                //    if (ips.scale > 1)
                //        ips.scale--;
                //    else
                //        ips.scale -= 0.1f;


                //    ApplyIpsScale(ips, parent);
                //}
                //else 
                if (isScaleMinus)
                {
                    ips.scale -= 0.1f;
                    ApplyIpsScale(ips, parent);
                }
            }
        }
        else
        {
            VoxLib.hud._labelCoordinates.Visible = false;
        }

    }
}
