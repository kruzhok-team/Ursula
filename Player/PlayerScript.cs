using Fractural.Tasks;
using Godot;
using System;
using System.Drawing;
using Ursula.Core.DI;
using Ursula.GameObjects.View;
using static Godot.TextServer;


public partial class PlayerScript : CharacterBody3D, IInjectable
{
    public static PlayerScript instance;

    CharacterBody3D characterBody3D;

    [Export]
    private RayCast3D rayCast;

    [Export] public Camera3D _camera;

    [Export]
    public int Speed { get; set; } = 6;
    [Export]
    public int FallAcceleration { get; set; } = 75;

    private float sensitivity{ get { return VoxLib.Sensitivity(); } }

    private Vector3 _targetVelocity = Vector3.Zero;

    private Vector2 _rotation = Vector2.Zero;
    private float JumpSpeed = 20f;

    private Variant gravity = ProjectSettings.GetSetting("physics/3d/default_gravity", -9.8f);

    private bool isRun = false;
    private bool isJumpDown = false;

    private float timer = 0f;
    private const float INTERVAL = 10.2f;

    [Inject]
    private ISingletonProvider<HUDViewModel> _hudManager;

    void IInjectable.OnDependenciesInjected()
    {
    }

    public override void _Ready()
    {
        //if (instance != null) instance.Free();
        instance = this;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _rotation[0] = RotationDegrees.X;
        _rotation[1] = RotationDegrees.Y;

        CursorShow(false);

        waterLevel = VoxLib.mapManager.WaterLevel;

        characterBody3D = this as CharacterBody3D;

        instance.Name = "Player";
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

    bool isVivibleInfo = false;
    bool isNeedUse = false;
    bool isUsed = false;

    public override void _Input(InputEvent @event)
    {
        if (Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.X) && @event.IsPressed())
        {
            VoxLib.mapManager.playMode = PlayMode.testMode;
            VoxLib.mapManager.PlayTest();
        }

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

                _rotation[0] -= mouseMovement[1] * sensitivity;
                _rotation[1] -= mouseMovement[0] * sensitivity;

                _rotation[0] = Mathf.Clamp(_rotation[0], -90f, 90f);

                RotationDegrees = new Vector3(_rotation[0], _rotation[1], 0);
            }
        }

        isRun = (Input.IsKeyPressed(Key.Shift) && @event.IsPressed());

        isJumpDown = (Input.IsKeyPressed(Key.Space) && @event.IsPressed());

        isVivibleInfo = (Input.IsKeyPressed(Key.Alt) && @event.IsPressed());

        isUsed = (Input.IsKeyPressed(Key.E) && @event.IsPressed());

        VoxLib.hud._labelCoordinates.Visible = isVivibleInfo || isNeedUse;
        VoxLib.hud._cross.Visible = isVivibleInfo;


        //if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        //{
        //    if (Raycaster.Hit(_camera, eventMouseButton.Position, out Node collider, out Vector3 pos))
        //    {
        //        VoxLib.hud.SetCoordinate(pos);
        //    }
        //}
    }



    public override async void _Process(double delta)
    {
        isNeedUse = false;

        float RayLength = isVivibleInfo ? 1000f : 3f;

        //if (isVivibleInfo)
        {
            if (Raycaster.HitFromCenterOfScreen(_camera, out Node collider, out Vector3 pos, false, RayLength))
            {
                HUD.ItemProcessing(collider, pos, out var ips, out var parent);

                if (isVivibleInfo)
                {
                    //VoxLib.hud.SetInfo(HUD.GetCordsInfo(collider, pos, ips, parent));
                    await SetHudInfo(HUD.GetCordsInfo(collider, pos, ips, parent));
                }

                if (ips != null && !isVivibleInfo)
                {
                    isNeedUse = true;

                    string info = $"Нажмите Е для взаимодействия";
                    //VoxLib.hud.SetInfo(info);
                    await SetHudInfo(info);

                    if (isUsed)
                    {
                        ips.Use();
                        GameManager.onPlayerInteractionObjectAction?.Invoke();
                    }
                }
            }
            else
            {
                VoxLib.hud._labelCoordinates.Visible = false;
            }
        }
    }

    private async GDTask SetHudInfo(string info)
    {
        var model = _hudManager != null ? await _hudManager.GetAsync() : null;
        model?.SetInfo(info);
    }

    float waterLevel = -1;
    VoxDrawTypes TypeSurface = VoxDrawTypes.solid;
    public override void _PhysicsProcess(double delta)
    {
        //_targetVelocity = Vector3.Zero;

        bool isOnFloor = IsOnFloor();
        bool isOnFloorOnly = IsOnFloorOnly();
        bool isOnCeiling = IsOnCeiling();
        bool isOnWall = IsOnWall();
        bool isWater = GlobalTransform.Origin.Y + 0.5f < waterLevel;

        VoxDrawTypes TypeSurfaceNew = isOnFloor ? VoxDrawTypes.solid : TypeSurface;
        TypeSurfaceNew = isWater ? VoxDrawTypes.water : TypeSurface;

        var direction = Vector3.Zero;

        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
        {
            direction.X += 1.0f;
            Input.ActionRelease("ui_right");
        }
        if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
        {
            direction.X -= 1.0f;
            Input.ActionRelease("ui_left");
        }
        if (Input.IsActionPressed("ui_down") || Input.IsKeyPressed(Key.S))
        {
            direction.Z += 1.0f;
            Input.ActionRelease("ui_down");
        }
        if (Input.IsActionPressed("ui_up") || Input.IsKeyPressed(Key.W))
        {
            direction.Z -= 1.0f;
            Input.ActionRelease("ui_up");
        }

        bool isSlide = characterBody3D.SlideOnCeiling;

        if (direction != Vector3.Zero)
        {
            direction = direction.Normalized();
            direction = (Transform.Basis * direction).Normalized();
        }

        // Ground velocity
        float speed = isRun ? Speed * 1.5f : Speed;

        _targetVelocity.X = direction.X * speed;
        _targetVelocity.Z = direction.Z * speed;

        if (isOnFloor) _targetVelocity.Y = 0;

        if (isJumpDown && isOnFloor && !isWater)
        {
            _targetVelocity.Y += JumpSpeed;
        }
        // Vertical velocity
        if (!isOnFloor && !isWater) // If in the air, fall towards the floor. Literally gravity
        {
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }
        else if (/*!isOnFloor && */isWater)
        {
            _targetVelocity.Y += FallAcceleration * 0.15f * (float)delta;
            _targetVelocity.X = direction.X * speed * 0.3f;
            _targetVelocity.Z = direction.Z * speed * 0.3f;
        }

        // Moving the character
        if (!LogScript.isLogEntered && !VoxLib.mapManager.isDialogsOpen && !VoxLib.log.isDialogOpen)
        {
            Velocity = _targetVelocity;
            MoveAndSlide();

        }
    }

}
