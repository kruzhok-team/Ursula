using Fractural.Tasks;
using Godot;
using System;
using System.Linq;

public partial class MoveScript : CharacterBody3D
{
    public static bool IsPhysicsOn = false;

    #region cache
    private Vector3 globalPositionCache;
    private Vector3 velocityCache;
    private Vector3 rotationCache;

    private bool globalPositionCacheUpdated;
    private bool velocityCacheUpdated;
    private bool rotationCacheUpdated;

    public new Vector3 GlobalPosition
    {
        get
        {
            if (!globalPositionCacheUpdated)
            {
                globalPositionCache = base.GlobalPosition;
                globalPositionCacheUpdated = true;
            }
            return globalPositionCache;
        }
        set
        {
            globalPositionCache = value;
        }
    }

    public new Vector3 Velocity
    {
        get
        {
            if (!velocityCacheUpdated)
            {
                velocityCache = new Vector3(0, 0, 0);
                velocityCacheUpdated = true;
            }
            return velocityCache;
        }
        set
        {
            if (velocityCache != value)
            {
                base.Velocity = value;
                velocityCache = value;
            }
        }
    }

    public new Quaternion Quaternion { get; private set; }
    public new Transform3D Transform { get; private set; }

    public new Vector3 Rotation
    {
        get
        {
            if (!rotationCacheUpdated)
            {
                rotationCache = base.Rotation;
                rotationCacheUpdated = true;
            }
            return rotationCache;
        }
        set
        {
            rotationCache = value;
        }
    }

    private Vector3 oldGlobalPosition = Vector3.Zero;
    private Vector3 oldRotation = Vector3.Zero;

    private ItemPropsScript itemPropsScriptCache = null;
    private ItemPropsScript ItemPropsScript
    {
        get
        {
            if (itemPropsScriptCache == null)
            {
                ItemPropsScript value = FindChild("ItemPropsScript") as ItemPropsScript;
                itemPropsScriptCache = value;
            }
            return itemPropsScriptCache;
        }
    }

    #endregion

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

    private NavigationAgent3D _navigationAgent;

    private float _movementSpeed = 2.0f;
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
    private CollisionShape3D collisionShape;

    Vector3 oldPosition = Vector3.Zero;
    VoxDrawTypes TypeSurface = VoxDrawTypes.solid;

    bool isBlocked => GlobalPosition.X < 0 || GlobalPosition.Z < 0 || GlobalPosition.X > VoxLib.mapManager.sizeX || GlobalPosition.Z > VoxLib.mapManager.sizeZ;

    private Vector3 _targetVelocity = Vector3.Zero;

    #endregion

    #region Init
    public override void _Ready()
    {
        base._Ready();
        stateMashine = StateMashine.idle;

        _navigationAgent = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");

        // These values need to be adjusted for the actor's speed
        // and the navigation layout.
        _navigationAgent.PathDesiredDistance = 3.0f;
        _navigationAgent.TargetDesiredDistance = 3.0f;

        waterLevel = VoxLib.mapManager.WaterLevel;

        var children = GetChildren();
        collisionShape = children.First(x => x is CollisionShape3D) as CollisionShape3D;
        collisionShape.Disabled = true;

        CSharpBridgeRegistry.Process += CSProcess;
        CSharpBridgeRegistry.PhysicsProcess += CSPhysicsProcess;
    }

    #endregion

    #region NextPosition

    Vector3 NextPositionRandomMove()
    {
        Vector3 currentAgentPosition = GlobalTransform.Origin;
        float distanceToPathPosition = currentAgentPosition.DistanceTo(oldPathPosition);
        if (oldPathPosition == Vector3.Zero || distanceToPathPosition < _navigationAgent.TargetDesiredDistance)
        {
            Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
            oldPathPosition = nextPathPosition;
        }

        _targetVelocity = currentAgentPosition.DirectionTo(oldPathPosition) * _movementSpeed;
        return _targetVelocity;
    }

    Vector3 NextPositionInPositionMove()
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
                if (currentAgentPosition.DistanceTo(currentPathPos) < _navigationAgent.TargetDesiredDistance)
                {
                    currentPath++;
                }

                _targetVelocity = GlobalPosition.DirectionTo(currentPathPos) * _movementSpeed;
                return _targetVelocity;
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
        return Vector3.Zero;
    }

    Vector3 NextPositionToTargetMove()
    {
        if (MovementTarget == null || !IsInstanceValid(MovementTarget))
        {
            stateMashine = StateMashine.idle;
            PlayIdleAnimation();
            onTargetLost.Invoke();
            return Vector3.Zero;
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
            return _targetVelocity;
        }
        else
        {
            GD.Print("Moved to point");
            onMovementFinished?.Invoke();
            stateMashine = StateMashine.idle;
            _targetVelocity = Vector3.Zero;
            oldPathPosition = Vector3.Zero;
            return Vector3.Zero;
        }
    }

    Vector3 FromTargetMoveState()
    {
        if (MovementTarget == null || !IsInstanceValid(MovementTarget))
        {
            stateMashine = StateMashine.idle;
            PlayIdleAnimation();
            onTargetLost.Invoke();
            return Vector3.Zero;
        }

        Vector3 currentAgentPosition = GlobalTransform.Origin;
        float distanceToTarget = currentAgentPosition.DistanceTo(MovementTarget.GlobalPosition);

        Vector3 direction = (currentAgentPosition - MovementTarget.GlobalPosition).Normalized();
        _targetVelocity = direction * _movementSpeed;
        return _targetVelocity;
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
        onMovementFinished?.Invoke();

        Vector3 targetPosition = MovementPosition;
        float distanceToTarget = GlobalPosition.DistanceTo(targetPosition);
        if (distanceToTarget > _navigationAgent.TargetDesiredDistance * 2)
        {
            onStuckMoving?.Invoke();
        }

        if (MovementPosition.X < 0 || MovementPosition.Z < 0 || MovementPosition.X > VoxLib.mapManager.sizeX || MovementPosition.Z > VoxLib.mapManager.sizeZ)
        {
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

    public void CSProcess(double delta)
    {
        base._Process(delta);
        transformProcessTimer += (float)delta;

        if (transformProcessTimer >= 1f)
        {
            transformProcessTimer = 0;

            // PhysicsLod

            inFrustum = Frustum.In(this);
            var distance = Frustum.GetDistance(this);
            float targetDistance = inFrustum ? 200 : 50;

            if (distance < targetDistance)
            {
                SetPhysicsLod0();
            }
            else
            {
                SetPhysicsLod1();
            }
        }

        if (oldGlobalPosition != Vector3.Zero)
        {
            float interpolationFactor = (float)Engine.GetPhysicsInterpolationFraction();
            Vector3 globalPos = oldGlobalPosition.Lerp(GlobalPosition, interpolationFactor);
            base.GlobalPosition = globalPos;
            base.Rotation = oldRotation.Lerp(Rotation, interpolationFactor);
            Quaternion = base.Quaternion;
            Transform = base.Transform;
            VoxLib.mapManager.spatialGrid.Update(ItemPropsScript, globalPos);
        }
        else
        {
            base.GlobalPosition = GlobalPosition;
            base.Rotation = Rotation;
            Quaternion = base.Quaternion;
            Transform = base.Transform;
            VoxLib.mapManager.spatialGrid.Update(ItemPropsScript, GlobalPosition);
        }
    }

    private float navigationProcessTimer = 0f;

    public void CSPhysicsProcess(double delta)
    {
        // Достигнута граница карты
        if (isBlocked)
            return;

        // Обновление NA при следовании за сущностью
        UpdateNavAgentTargetPosition(delta);

        Vector3 position = GlobalPosition;
        Vector3 velocity = Velocity;

        oldGlobalPosition = position;
        oldRotation = Rotation;

        navigationProcessTimer += (float)delta;

        if (navigationProcessTimer >= 0.3f)
        {
            navigationProcessTimer = 0f;

            SetFlags();

            UpdateSurfaceType();

            var isNavigationFinished = _navigationAgent.IsNavigationFinished();

            if (isNavigationFinished)
            {
                if (stateMashine == StateMashine.moveToTarget || stateMashine == StateMashine.moveFromTarget)
                {
                    FinishTargetMoveState();
                }
                else if (stateMashine == StateMashine.moveToPosition)
                {
                    FinishPositionMoveState();
                }
                else
                {
                    return;
                }
            }

            if (stateMashine == StateMashine.idle)
            {
                velocity = Vector3.Zero;
            }
            else if (stateMashine == StateMashine.moveToRandom)
            {
                velocity = NextPositionRandomMove();
            }
            else if (stateMashine == StateMashine.moveToPosition)
            {
                velocity = NextPositionInPositionMove();
            }
            else if (stateMashine == StateMashine.moveToTarget)
            {
                velocity = NextPositionToTargetMove();
            }
            else if (stateMashine == StateMashine.moveFromTarget)
            {
                velocity = FromTargetMoveState();
            }
            else if (stateMashine == StateMashine.findTarget)
            {
                FindTargetState();
            }

        }

        if (onStuckMoving != null || onMovingDistanceFinished != null)
        {
            MovingCheck(delta);
        }

        velocity = SetVelocity(velocity, delta);

        if (IsPhysicsOn)
        {
            Velocity = velocity;

            MoveAndSlide();
            globalPositionCacheUpdated = false;
            rotationCacheUpdated = false;

            Rotation = SetRotation(Rotation, velocity, delta);
            GlobalPosition = CorrectPositionByTerrainHeight(GlobalPosition, 0.5f);
        }
        else
        {
            position += velocity * (float)delta;
            position = CorrectPositionByTerrainHeight(position);

            GlobalPosition = position;
            Velocity = velocity;
            Rotation = SetRotation(Rotation, velocity, delta);
        }
    }

    #endregion

    #region Transform

    void SetPhysicsLod0()
    {
        physicsLod = PhysicsLod.Lod0;
        if (IsInstanceValid(collisionShape))
        {
            collisionShape.Disabled = false;
        }
        else
        {
            var children = GetChildren();
            collisionShape = children.First(x => x is CollisionShape3D) as CollisionShape3D;
            collisionShape.Disabled = false;
        }
    }

    void SetPhysicsLod1()
    {
        physicsLod = PhysicsLod.Lod1;
        if (IsInstanceValid(collisionShape))
        {
            collisionShape.Disabled = false;
        }
        else
        {
            var children = GetChildren();
            collisionShape = children.First(x => x is CollisionShape3D) as CollisionShape3D;
            collisionShape.Disabled = false;
        }
    }

    void MovingCheck(double delta)
    {
        float moveDistDelta = oldPosition.DistanceTo(GlobalPosition);

        if (oldPosition != Vector3.Zero && isMoving)
        {
            Vector3 vel = new Vector3(Velocity.X, 0, Velocity.Z);

            if (moveDistDelta > 0.01f && vel.Length() > 0.01f)
            {
                moveDistance += oldPosition.DistanceTo(GlobalPosition);
                moveDistanceOld = moveDistance;
                timeStuckMoving = 0;
            }
        }
        oldPosition = GlobalPosition;

        if (onStuckMoving != null && Math.Abs(moveDistanceOld - moveDistance) < 1 && isMoving || (moveDistDelta == 0 && isMoving))
        {
            if (timeStuckMoving > 0.5f)
            {
                onStuckMoving?.Invoke();
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

    Vector3 SetRotation(Vector3 rotation, Vector3 velocity, double delta)
    {
        if (velocity.Length() > 0)
        {
            Vector3 dir = velocity.Normalized();
            float targetAngle = Mathf.Atan2(dir.X, dir.Z);
            float currentAngle = rotation.Y;

            // плавная интерполяция угла
            var t = 1f - Math.Exp(-TurnSharpness * delta);
            var newAngle = Mathf.LerpAngle(currentAngle, targetAngle, t);

            return new Vector3(0, (float)newAngle, 0);
        }
        return rotation;
    }

    Vector3 SetVelocity(Vector3 velocity, double delta)
    {
        if (!isMoving)
            velocity = Vector3.Zero;

        if (physicsLod == PhysicsLod.Lod0)
            velocity = AddGravity(velocity, delta);

        return velocity;
    }

    Vector3 AddGravity(Vector3 velocity, double delta)
    {
        velocity.Y = 0;
        //if (!isOnFloor)
        //{
        _targetVelocity.Y -= (float)9.8 * (float)delta * 20;
        //}

        return velocity;
    }

    public void SetPosition(float x, float z)
    {
        float y = VoxLib.terrainManager.GetTerrainHeight(x, z);
        y = MathF.Max(waterLevel, y);
        GlobalPosition = new Vector3(x, y, z);
    }

    public void SetRotationLookAt(float x, float z)
    {
        Vector3 targetPos = new Vector3(x, GlobalPosition.Y, z);
        Vector3 dir = GlobalPosition.DirectionTo(targetPos);
        float targetAngle = Mathf.Atan2(dir.X, dir.Z);
        Rotation = new Vector3(0, targetAngle, 0);
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

        if (currentPath == 0)
        {
            GD.Print("Path is null");
        }
    }

    public async void MoveToRandomSetup()
    {
        if (!IsInstanceValid(this))
        {
            return;
        }

        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveToRandom;
        moveDistance = 0;

        var _rng = new RandomNumberGenerator();

        int posX = _rng.RandiRange(0, VoxLib.mapManager.sizeX);
        int posZ = _rng.RandiRange(0, VoxLib.mapManager.sizeZ);
        float posY = VoxLib.terrainManager.GetTerrainHeight(posX, posZ);

        // Now that the navigation map is no longer empty, set the movement target.
        MovementPosition = new Vector3(posX, posY, posZ);

        PlayRunAnimation();
    }

    public async void MoveToPositionSetup(Vector3 newPosition)
    {
        if (!IsInstanceValid(this))
        {
            return;
        }

        ResetCoordinates();
        // TODO: requst to change + optimistic movement.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        int X = Math.Clamp((int)newPosition.X, 0, VoxLib.mapManager.sizeX);
        float Y = VoxLib.terrainManager.positionOffset.Y;
        int Z = Math.Clamp((int)newPosition.Z, 0, VoxLib.mapManager.sizeX);

        Y = GetTerrainHeight(X, Z);

        MovementPosition = new Vector3(X, Y, Z);

        await SetupPath();

        PlayRunAnimation();
        stateMashine = StateMashine.moveToPosition;
        moveDistance = 0;
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
        if (!IsInstanceValid(this))
        {
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveToTarget;
        moveDistance = 0;

        MovementTarget = node;

        PlayRunAnimation();
    }

    public async void MoveFromTargetSetup(Node3D node)
    {
        if (!IsInstanceValid(this))
        {
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        stateMashine = StateMashine.moveFromTarget;
        moveDistance = 0;

        MovementTarget = node;

        PlayRunAnimation();
    }

    private async void FindToTargetSetup(Node3D node, float radiusSearh)
    {
        if (!IsInstanceValid(this))
        {
            return;
        }
        
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

    public void ResetCoordinates()
    {
        MovementPosition = Vector3.Zero;
        oldPathPosition = Vector3.Zero;
        _navigationAgent.TargetPosition = Vector3.Zero;
        MovementTarget = null;

        PlayIdleAnimation();
    }

    #endregion

    #region Utils

    Vector3 CorrectPositionByTerrainHeight(Vector3 pos, float upOffset = 0.0f)
    {
        //if (pos.Y < target_y)
        return new Vector3(pos.X, GetTerrainHeightByCurrentPos(pos) + upOffset, pos.Z);
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

    public float GetTerrainHeightByCurrentPos(Vector3 pos)
    {
        return VoxLib.terrainManager.GetTerrainHeight(pos.X, pos.Z);
    }

    public void ReloadAlgorithm()
    {

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
    public override void _ExitTree()
    {
        CSharpBridgeRegistry.Process -= CSProcess;
        CSharpBridgeRegistry.PhysicsProcess -= CSPhysicsProcess;
    }
}
