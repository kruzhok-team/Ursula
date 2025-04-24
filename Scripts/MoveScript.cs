using Godot;
using System.Reflection;
using static Godot.TextServer;
using static MoveScript;
using Modules.HSM;
using System;
using Fractural.Tasks;
using System.Diagnostics;
using static Godot.TileSet;

public partial class MoveScript : CharacterBody3D
{

    #region PublicFields

    public InteractiveObjectMove interactiveObjectMove;

    public enum StateMashine
    {
        idle,
        moveToRandom,
        moveToTarget,
        moveToPosition,
        findTarget,
        moveFromTarget,
    }
    public StateMashine stateMashine;

    public enum SignalMashine
    {
        none,
        checkMoveDistance,
    }
    public SignalMashine signalMashine = SignalMashine.none;

    public Vector3 MovementPosition
    {
        get { return _navigationAgent.TargetPosition; }
        set { _navigationAgent.TargetPosition = value; }
    }

    Node3D nodeTarget;
    public Node3D MovementTarget
    {
        get { return nodeTarget; }
        set { nodeTarget = value; }
    }

    public Action onTargetLost;
    public Action onCollision;

    public Action onMovementFinished;
    public Action onStuckMoving;
    public Action onMovingDistanceFinished;
    public Action onChangeSurfaceType;

    #endregion

    #region PrivateFields

    private const int IgnoredLayers = 2; // Игнорируемые слои для коллизий

    [Export]
    private RayCast3D raycastForward;

    private AnimationTree _animationTree;
    private NavigationAgent3D _navigationAgent;

    private float _movementSpeed = 2.0f;
    private Vector3 _TargetPosition;
    private float radiusSearh;


    private Variant gravity = ProjectSettings.GetSetting("physics/3d/default_gravity", -9.8f);

    float waterLevel = -1;

    private Random random = new Random();


    Vector3 oldPathPosition = Vector3.Zero;
    Vector3 oldTargetPosition = Vector3.Zero;
    private float moveDistanceOld;
    private float moveDistance;
    private float checkMoveDistance = 0;
    private double timeStuckMoving;

    Vector3 oldPosition = Vector3.Zero;
    VoxDrawTypes TypeSurface = VoxDrawTypes.solid;

    bool isBlocked = false;

    private Vector3 _targetVelocity = Vector3.Zero;

    #endregion

    #region Init
    public override void _Ready()
    {
        base._Ready();
        stateMashine = StateMashine.idle;

        //SetCollisionLayerValue(IgnoredLayers, false);
        //SetCollisionMaskValue(IgnoredLayers, false);

        _animationTree = GetNodeOrNull<AnimationTree>("AnimationTree");
        _navigationAgent = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");

        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        _navigationAgent.PathDesiredDistance = 1.5f;
        _navigationAgent.TargetDesiredDistance = 0.5f;

        // Make sure to not await during _Ready.
        //Callable.From(MoveToRandomSetup).CallDeferred();
        waterLevel = VoxLib.mapManager.WaterLevel;
    }

    #endregion

    #region NextPosition

    void NextPositionRandomMove()
    {
        Vector3 currentAgentPosition = GlobalTransform.Origin;
        {
            float distanceToPathPosition = currentAgentPosition.DistanceTo(oldPathPosition);
            if (oldPathPosition == Vector3.Zero || distanceToPathPosition < _navigationAgent.TargetDesiredDistance)
            {
                Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
                oldPathPosition = nextPathPosition;
            }

            _targetVelocity = currentAgentPosition.DirectionTo(oldPathPosition) * _movementSpeed;
        }
    }

    void NextPositionInPositionMove()
    {
        


        Vector3 currentAgentPosition = GlobalTransform.Origin;
        float distanceToTarget = currentAgentPosition.DistanceTo(MovementPosition);
        if (distanceToTarget >= _navigationAgent.TargetDesiredDistance && navPath != null)
        {
            if (currentPath < navPath.Length)
            {
                Vector3 currentPathPos = navPath[currentPath];
                //currentPathPos.Y = currentAgentPosition.Y;

                currentPathPos.Y = GetTerrainHeight((int)currentPathPos.X, (int)currentPathPos.Z);

                _targetVelocity = GlobalPosition.DirectionTo(currentPathPos) * _movementSpeed;
                if (currentAgentPosition.DistanceTo(currentPathPos) < _navigationAgent.TargetDesiredDistance)
                {
                    currentPath++;
                }
            }
            else
            {
                MovementFinished();
            }
        }
        else if (stateMashine == StateMashine.moveToPosition)
        {
            MovementFinished();
        }
    }

    void NextPositionToTargetMove()
    {
        if (MovementTarget == null || !IsInstanceValid(MovementTarget))
        {
            stateMashine = StateMashine.idle;
            PlayIdleAnimation();
            onTargetLost.Invoke();
            return;
        }

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        float distanceToTarget = currentAgentPosition.DistanceTo(MovementTarget.GlobalPosition);

        if (distanceToTarget > _navigationAgent.TargetDesiredDistance)
        {
            float distanceToPathPosition = currentAgentPosition.DistanceTo(oldPathPosition);
            if (oldPathPosition == Vector3.Zero || distanceToPathPosition < _navigationAgent.TargetDesiredDistance)
            {
                oldPathPosition = _navigationAgent.GetNextPathPosition();
            }
            else if (oldTargetPosition != MovementTarget.GlobalPosition)
            {
                oldTargetPosition = MovementTarget.GlobalPosition;
                oldPathPosition = _navigationAgent.GetNextPathPosition();
            }

            _targetVelocity = currentAgentPosition.DirectionTo(oldPathPosition) * _movementSpeed;
        }
        else
        {
            //GD.Print("Moved to point");
            onMovementFinished.Invoke();
            //stateMashine = StateMashine.idle;
            _targetVelocity = Vector3.Zero;
            oldPathPosition = Vector3.Zero;
        }
    }

    void FromTargetMoveState()
    {
        if (MovementTarget == null || !IsInstanceValid(MovementTarget))
        {
            stateMashine = StateMashine.idle;
            PlayIdleAnimation();
            onTargetLost.Invoke();
            return;
        }

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        float distanceToTarget = currentAgentPosition.DistanceTo(MovementTarget.GlobalPosition);

        Vector3 direction = (currentAgentPosition - MovementTarget.Position).Normalized();
        _targetVelocity = direction * _movementSpeed;
    }

    void FindTargetState()
    {
        if (MovementTarget == null)
        {
            stateMashine = StateMashine.idle;
            PlayIdleAnimation();
            onTargetLost.Invoke();
            return;
        }

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        float distanceToTarget = currentAgentPosition.DistanceTo(MovementTarget.GlobalPosition);
        if (distanceToTarget < radiusSearh)
            stateMashine = StateMashine.moveToTarget;
    }

    #endregion

    #region FinishMove

    void FinishTargetMoveState()
    {
        if (MovementTarget != null && Node.IsInstanceValid(MovementTarget))
        {
            Vector3 targetPosition = MovementTarget.GlobalPosition;
            float distanceToTarget = GlobalTransform.Origin.DistanceTo(targetPosition);
            if (distanceToTarget > _navigationAgent.TargetDesiredDistance * 2)
            {
                onStuckMoving?.Invoke();
            }
        }
    }

    void FinishPositionMoveState()
    {
        stateMashine = StateMashine.idle;
        PlayIdleAnimation();


        //Profiler.BeginSample("onMovementFinished?.Invoke()");
        onMovementFinished?.Invoke();  // ! 5 - 40 ms
        //Profiler.EndLastSample();


        Vector3 targetPosition = MovementPosition;
        float distanceToTarget = GlobalTransform.Origin.DistanceTo(targetPosition);
        if (distanceToTarget > _navigationAgent.TargetDesiredDistance * 2)
        {
            onStuckMoving?.Invoke();
        }

        if (MovementPosition.X < 0 || MovementPosition.Z < 0 || MovementPosition.X > VoxLib.mapManager.sizeX || MovementPosition.Z > VoxLib.mapManager.sizeZ)
        {
            //onMovementFinished = null;
            isBlocked = true;
            ContextMenu.ShowMessageS($"{/*onMovementFinished.guid*/""} Достигнут предел карты: перемещение остановлено.");
        }

    }

    #endregion

    #region Flags

    bool isOnFloor;
    bool isOnFloorOnly;
    bool isOnCeiling;
    bool isOnWall;
    bool isWater;

    bool isMoving;

    bool inFrustum = true;

    //bool useGravity = true;

    enum PhysicsLod { Lod0, Lod1 }

    PhysicsLod physicsLod = PhysicsLod.Lod0;

    void SetFlags()
    {
        isOnFloor = IsOnFloor();
        isOnFloorOnly = IsOnFloorOnly();
        isOnCeiling = IsOnCeiling();
        isOnWall = IsOnWall();
        isWater = GlobalTransform.Origin.Y < waterLevel;

        isMoving = (stateMashine == StateMashine.moveToRandom || stateMashine == StateMashine.moveToTarget
                    || stateMashine == StateMashine.moveToPosition || stateMashine == StateMashine.moveFromTarget);
    }

    void UpdateSurfaceType()
    {
        VoxDrawTypes TypeSurfaceNew = isOnFloor ? VoxDrawTypes.solid : TypeSurface;
        TypeSurfaceNew = isWater ? VoxDrawTypes.water : TypeSurface;
        if (TypeSurface != TypeSurfaceNew && onChangeSurfaceType != null)
        {
            onChangeSurfaceType?.Invoke();
            onChangeSurfaceType = null;
            TypeSurface = TypeSurfaceNew;
        }
    }

    #endregion

    #region Process

    private float transformProcessTimer = 0f;

    public override void _Process(double delta)
    {
        base._Process(delta);

        transformProcessTimer += (float)delta;

        if (transformProcessTimer >= 1f)
        {
            transformProcessTimer = 0;

            // PhysicsLod

            inFrustum = Frustum.In(this);

            var distance = Frustum.GetDistance(this);

            //GD.Print(distance);

            if (distance < 200 && inFrustum)
            {
                SetPhysicsLod0();
            }
            else
            {
                SetPhysicsLod1();
            }
        }
    }

    private float navigationProcessTimer = 0f;
    public override void _PhysicsProcess(double delta)
    {
        if (isBlocked) return;

        //Profiler.BeginSample("_PhysicsProcess");


        if (inFrustum)
        {
            base._PhysicsProcess(delta);
        }

        UpdateNavAgentTargetPosition(delta);

        navigationProcessTimer += (float)delta;
        if (navigationProcessTimer >= 0.3f)
        {

            if (physicsLod == PhysicsLod.Lod0)
                CheckAndFixFallen();

            navigationProcessTimer = 0f;

            SetFlags();

            UpdateSurfaceType();

            var isNavigationFinished = _navigationAgent.IsNavigationFinished();

            if (isNavigationFinished)
            {
                if (stateMashine == StateMashine.moveToRandom)
                {
                    MoveToRandomSetup();
                }


                if (stateMashine == StateMashine.moveToTarget || stateMashine == StateMashine.moveFromTarget)
                {
                    FinishTargetMoveState();
                }
                else if (stateMashine == StateMashine.moveToPosition)
                {
                    FinishPositionMoveState(); // ! 5 - 40 ms
                }
                else
                {
                    //Profiler.EndLastSample(false);
                    return;
                }
            }

            if (stateMashine == StateMashine.idle)
            {
                //Profiler.EndLastSample(false);

                //Profiler.EndLastSample(false);
                return;
            }
            else if (stateMashine == StateMashine.moveToRandom)
            {
                NextPositionRandomMove();
            }
            else if (stateMashine == StateMashine.moveToPosition)
            {
                NextPositionInPositionMove();
            }
            else if (stateMashine == StateMashine.moveToTarget)
            {
                NextPositionToTargetMove();
            }
            else if (stateMashine == StateMashine.moveFromTarget)
            {
                FromTargetMoveState();
            }
            else if (stateMashine == StateMashine.findTarget)
            {
                FindTargetState();
            }

        }

        if (signalMashine == SignalMashine.checkMoveDistance)
        {
            //moveDistance
        }

        if (onStuckMoving != null || onMovingDistanceFinished != null)
        {
            MovingCheck(delta);
        }

        //if (isCollisionDetected)
        //{
        //    for (int i = 0; i < GetSlideCollisionCount(); i++)
        //    {
        //        var collision = GetSlideCollision(i);
        //        string namec = ((Node)collision.GetCollider()).Name;

        //        GD.Print("Collision detected: " + namec);

        //        //if (namec.Contains(nameCollisionDetected))
        //        //{
        //        //    onCollisionFinished?.Invoke();
        //        //    isCollisionDetected = false;
        //        //}
        //    }
        //}

        SetRotation(delta);

        SetVelocity(delta);

        if (physicsLod == PhysicsLod.Lod1)
            CorrectPositionByTerrainHeight();

        MoveAndSlide();

        //Profiler.EndLastSample(false);
    }

    #endregion

    #region Transform

    void SetPhysicsLod0()
    {
        EnableTerrainCollision();

        physicsLod = PhysicsLod.Lod0;
    }

    void SetPhysicsLod1()
    {
        DisableTerrainCollision();

        physicsLod = PhysicsLod.Lod1;
    }

    void MovingCheck(double delta)
    {
        float moveDistDelta = oldPosition.DistanceTo(GlobalTransform.Origin);

        if (oldPosition != Vector3.Zero && isMoving)
        {
            Vector3 vel = new Vector3(Velocity.X, 0, Velocity.Z);

            if (moveDistDelta > 0.01f && vel.Length() > 0.01f)
            {
                moveDistance += oldPosition.DistanceTo(GlobalTransform.Origin);
                moveDistanceOld = moveDistance;
                timeStuckMoving = 0;
            }
        }
        oldPosition = GlobalTransform.Origin;

        if (onStuckMoving != null && Math.Abs(moveDistanceOld - moveDistance) < 1 && isMoving || (moveDistDelta == 0 && isMoving))
        {
            if (timeStuckMoving > 0.5f)
            {
                onStuckMoving?.Invoke();
                //onStuckMoving = null;
                timeStuckMoving = 0;
            }
            else
                timeStuckMoving += delta;
        }

        if (onMovingDistanceFinished != null && checkMoveDistance > 0)
        {
            if (moveDistance > checkMoveDistance)
            {
                if (interactiveObjectMove != null)
                    interactiveObjectMove.moveDistance.Value = checkMoveDistance;
                onMovingDistanceFinished?.Invoke();
                signalMashine = SignalMashine.none;
                checkMoveDistance = 0;
            }
        }
    }

    private float timeAccumulator = 0f;
    private float timeCheckPath = 0f;
    private const float INTERVAL = 2f;

    void UpdateNavAgentTargetPosition(double delta)
    {
        timeAccumulator += (float)delta;
        if (timeAccumulator >= INTERVAL)
        {
            timeAccumulator = 0f;

            if (stateMashine == StateMashine.moveToTarget)
                if (MovementTarget != null && Node.IsInstanceValid(MovementTarget))
                    _navigationAgent.TargetPosition = MovementTarget.GlobalPosition;
        }
    }


    public float TurnSpeed = 5f; // радиан в секунду
    public float TurnSharpness = 10f;

    void SetRotation(double delta)
    {
        if (Velocity.Length() > 0)
        {
            Vector3 dir = Velocity.Normalized();
            float targetAngle = Mathf.Atan2(dir.X, dir.Z);
            float currentAngle = Rotation.Y;

            // плавная интерполяция угла
            var t = 1f - Math.Exp(-TurnSharpness * delta);
            var newAngle = Mathf.LerpAngle(currentAngle, targetAngle, t);

            Rotation = new Vector3(0, (float)newAngle, 0);
        }
    }

    void SetVelocity(double delta)
    {
        if (!isMoving)
            _targetVelocity = Vector3.Zero;

        if (physicsLod == PhysicsLod.Lod0)
            AddGravity(delta);

        ////_targetVelocity.Y = 0;

        //////_targetVelocity = _targetVelocity.Normalized();
        //////_targetVelocity *= _movementSpeed;

        Velocity = _targetVelocity;
    }

    void AddGravity(double delta)
    {
        _targetVelocity.Y = 0;
        if (!isOnFloor)
        {
            _targetVelocity.Y -= (float)9.8 * (float)delta * 20;
        }

        Velocity = _targetVelocity;
    }

    #endregion

    #region Setups

    int currentPath;
    Vector3[] navPath;

    void MovementFinished()
    {
        onMovementFinished.Invoke();
        stateMashine = StateMashine.idle;
        _targetVelocity = Vector3.Zero;
        oldPathPosition = Vector3.Zero;
        PlayIdleAnimation();
    }

    private async GDTask SetupPath()
    {
        int delay = random.Next(5, 20);
        await GDTask.Delay(delay);
        var nextPathPosition = _navigationAgent.GetNextPathPosition();
        delay = random.Next(5, 20);
        await GDTask.Delay(delay);
        navPath = _navigationAgent.GetCurrentNavigationPath();
        delay = random.Next(5, 20);
        await GDTask.Delay(delay);
        currentPath = _navigationAgent.GetCurrentNavigationPathIndex();
    }


    bool isCollisionDetected = false;
    string nameCollisionDetected;
    private void CollisionDetectedSetup(string nameCollisionDetected)
    {
        this.nameCollisionDetected = nameCollisionDetected;
        isCollisionDetected = true;
    }

    public async void MoveToRandomSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveToRandom;
        moveDistance = 0;

        var _rng = new RandomNumberGenerator();

        int posX = _rng.RandiRange(0, VoxLib.mapManager.sizeX);
        int posZ = _rng.RandiRange(0, VoxLib.mapManager.sizeZ);
        float posY = GetTerrainHeight(posX, posZ);

        // Now that the navigation map is no longer empty, set the movement target.
        MovementPosition = new Vector3(posX, posY, posZ);

        PlayRunAnimation();
    }

    public async void MoveToPositionSetup(Vector3 newPosition)
    {
        ResetCoordinates();

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveToPosition;
        moveDistance = 0;

        int X = Math.Clamp((int)newPosition.X, 0, VoxLib.mapManager.sizeX);
        float Y = VoxLib.terrainManager.positionOffset.Y;
        int Z = Math.Clamp((int)newPosition.Z, 0, VoxLib.mapManager.sizeX);

        Y = GetTerrainHeight(X, Z);

        MovementPosition = new Vector3(X, Y, Z);

        PlayRunAnimation();

        _= SetupPath();

    }

    public Vector3 SetPositionRight(float n)
    {
        int X = (int)(GlobalPosition.X + n);
        float Y = VoxLib.terrainManager.positionOffset.Y;
        int Z = (int)GlobalPosition.Z;

        if (X > 0 && Z > 0)
            Y = GetTerrainHeight(X, Z);

        PlayRunAnimation();

        MovementPosition = new Vector3(X, Y, Z);
        return MovementPosition;
    }

    public Vector3 SetPositionLeft(float n)
    {
        int X = (int)(GlobalPosition.X - n);
        float Y = VoxLib.terrainManager.positionOffset.Y;
        int Z = (int)(GlobalPosition.Z);

        if (X > 0 && Z > 0)
            Y = GetTerrainHeight(X, Z);

        PlayRunAnimation();

        MovementPosition = new Vector3(X, Y, Z);
        return MovementPosition;
    }

    public async void MoveToTargetSetup(Node3D node)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveToTarget;
        moveDistance = 0;

        MovementTarget = node;

        PlayRunAnimation();
    }

    public async void MoveFromTargetSetup(Node3D node)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveFromTarget;
        moveDistance = 0;

        MovementTarget = node;

        PlayRunAnimation();
    }

    private async void FindToTargetSetup(Node3D node, float radiusSearh)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.findTarget;

        MovementTarget = node;

        PlayRunAnimation();
    }

    public async void StopMoveSetup()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.idle;

        MovementTarget = null;

        PlayIdleAnimation();
    }

    public async void ResetCoordinates()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        MovementPosition = Vector3.Zero;
        oldPathPosition = Vector3.Zero;
        _navigationAgent.TargetPosition = Vector3.Zero;
        MovementTarget = null;

        PlayIdleAnimation();
    }

    #endregion

    #region Collision

    const int TerrainLayerIndex = 0; // террейн — слой 1 -> бит 0
    const int FenceLayerIndex = 1; // забор   — слой 2 -> бит 1
    const int OtherLayerIndex = 2; // прочее  — слой 3 -> бит 2

    public void DisableTerrainCollision()
    {
        CollisionMask &= ~(1u << TerrainLayerIndex);

        CollisionMask |= (1u << FenceLayerIndex)
                       | (1u << OtherLayerIndex);
    }

    public void EnableTerrainCollision()
    {
        CollisionMask |= (1u << TerrainLayerIndex);
    }

    #endregion

    #region Utils

    void CheckAndFixFallen()
    {
        var terrainHeight = GetTerrainHeightByCurrentPos();
        if (Position.Y - terrainHeight < -0.3f)
        {
            Position = new Vector3(Position.X, terrainHeight + 0.1f, Position.Z);
        }
    }

    void CorrectPositionByTerrainHeight(float upOffset = 0.0f)
    {
        Position = new Vector3(Position.X, GetTerrainHeightByCurrentPos() + upOffset, Position.Z);
    }

    public void CheckMoveDistance(float distance)
    {
        signalMashine = SignalMashine.checkMoveDistance;
        moveDistance = 0;
        checkMoveDistance = distance;
    }

    public float GetMoveDistance()
    {
        return moveDistance;
    }

    public float GetHeightWorld()
    {
        return GlobalTransform.Origin.Y;
    }

    public float GetSurfaceType()
    {
        return (int)TypeSurface;
    }

    public float GetTerrainHeight(int X, int Z)
    {
        return VoxLib.terrainManager.mapHeight[X, Z] + VoxLib.terrainManager.positionOffset.Y;
    }

    public float GetTerrainHeightByCurrentPos()
    {
        // the whole part of the coordinates
        int x0 = Mathf.FloorToInt(GlobalPosition.X);
        int z0 = Mathf.FloorToInt(GlobalPosition.Z);
        // fractional part
        float fx = GlobalPosition.X - x0;
        float fz = GlobalPosition.Z - z0;

        // array boundaries
        int maxX = VoxLib.terrainManager.mapHeight.GetLength(0) - 1;
        int maxZ = VoxLib.terrainManager.mapHeight.GetLength(1) - 1;

        // if we go abroad, we return the offset
        if (x0 < 0 || z0 < 0 || x0 >= maxX || z0 >= maxZ)
            return VoxLib.terrainManager.positionOffset.Y;

        // four adjacent values
        float h00 = VoxLib.terrainManager.mapHeight[x0, z0];
        float h10 = VoxLib.terrainManager.mapHeight[x0 + 1, z0];
        float h01 = VoxLib.terrainManager.mapHeight[x0, z0 + 1];
        float h11 = VoxLib.terrainManager.mapHeight[x0 + 1, z0 + 1];

        // linear interpolation by X
        float h0 = Mathf.Lerp(h00, h10, fx);
        float h1 = Mathf.Lerp(h01, h11, fx);
        // linear interpolation by Z
        float h = Mathf.Lerp(h0, h1, fz);

        // adding offset
        return h + VoxLib.terrainManager.positionOffset.Y;
    }

    public void ReloadAlgorithm()
    {
        isBlocked = false;
    }

    #endregion

    #region MoveAnimations

    BaseAnimation _baseAnimation;
    private BaseAnimation baseAnimation 
    {
        get
        {
            if (_baseAnimation != null) return _baseAnimation;
            var baseAnimation = GetNodeOrNull("AnimationObject") as BaseAnimation;
            if (baseAnimation == null) baseAnimation = GetParent().GetNodeOrNull("AnimationObject") as BaseAnimation;
            return baseAnimation;
        }
    }

    public void PlayRunAnimation()
    {
        baseAnimation?.PlayRunAnimation();
    }

    public void PlayIdleAnimation()
    {
        baseAnimation?.PlayIdleAnimation();
    }

    public void PlayJumpAnimation()
    {
        baseAnimation?.PlayJumpAnimation();
    }

    #endregion

    #region Other

    private void _on_Player_body_entered(Node3D node)
    {
        Node3D my = this as Node3D;
        if (my == node)
            return;

        GD.Print("Столкновение с: " + node.Name);
        onCollision?.Invoke();

        //node.EmitSignal("ApplyImpulse");
    }

    public void ApplyImpulse()
    {
        GD.Print("");
        // Это может быть использовано для применения силы к персонажу
        //Velocity += impulse;
    }

    #endregion

}
