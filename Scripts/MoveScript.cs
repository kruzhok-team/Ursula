using Godot;
using System.Reflection;
using static Godot.TextServer;
using static MoveScript;

public partial class MoveScript : CharacterBody3D
{
    public InteractiveObjectMove interactiveObjectMove;

    private const int IgnoredLayers = 2; // Игнорируемые слои для коллизий

    [Export]
    private RayCast3D raycastForward;

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

    private AnimationTree _animationTree;
    private NavigationAgent3D _navigationAgent;

    private float _movementSpeed = 2.0f;
    private Vector3 _TargetPosition;
    private float radiusSearh;

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

    private Variant gravity = ProjectSettings.GetSetting("physics/3d/default_gravity", -9.8f);
    public GMLActionHolder onTargetLost = new();
    public GMLActionHolder onCollision = new();

    public GMLActionHolder onMovementFinished = new();
    public GMLActionHolder onStuckMoving = new();
    public GMLActionHolder onMovingDistanceFinished = new();
    public GMLActionHolder onChangeSurfaceType = new();

    float waterLevel = -1;

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

    private float timeAccumulator = 0f;
    private float timeCheckPath = 0f;
    private const float INTERVAL = 2f;

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

    public override void _PhysicsProcess(double delta)
    {
        if (isBlocked) return;

        base._PhysicsProcess(delta);

        bool isOnFloor = IsOnFloor();
        bool isOnFloorOnly = IsOnFloorOnly();
        bool isOnCeiling = IsOnCeiling();
        bool isOnWall = IsOnWall();
        bool isWater = GlobalTransform.Origin.Y < waterLevel;

        VoxDrawTypes TypeSurfaceNew = isOnFloor ? VoxDrawTypes.solid : TypeSurface;
        TypeSurfaceNew = isWater ? VoxDrawTypes.water : TypeSurface;
        if (TypeSurface != TypeSurfaceNew && onChangeSurfaceType != null)
        {
            onChangeSurfaceType?.Invoke();
            onChangeSurfaceType = null;
            TypeSurface = TypeSurfaceNew;
        }

        if (_navigationAgent.IsNavigationFinished())
        {
            //onMovementFinished.Invoke();

            if (stateMashine == StateMashine.moveToRandom)
                MoveToRandomSetup();

            if (stateMashine == StateMashine.moveToTarget || stateMashine == StateMashine.moveFromTarget)
            {
                if (MovementTarget != null)
                {
                    Vector3 targetPosition = MovementTarget.GlobalPosition;
                    float distanceToTarget = GlobalTransform.Origin.DistanceTo(targetPosition);
                    if (distanceToTarget > _navigationAgent.TargetDesiredDistance * 2)
                    {
                        onStuckMoving?.Invoke();
                    }
                }
            }
            else if (stateMashine == StateMashine.moveToPosition)
            {
                stateMashine = StateMashine.idle;
                PlayIdleAnimation();
                onMovementFinished?.Invoke();

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
                    ContextMenu.ShowMessageS($"{onMovementFinished.guid} Достигнут предел карты: перемещение остановлено.");
                }
            }
            else return;
        }

        timeAccumulator += (float)delta;
        if (timeAccumulator >= INTERVAL)
        {
            timeAccumulator = 0f;

            if (stateMashine == StateMashine.moveToTarget)
                if (MovementTarget != null)
                    _navigationAgent.TargetPosition = MovementTarget.GlobalPosition;
        }

        if (stateMashine == StateMashine.idle)
        {
            return;
        }
        else if (stateMashine == StateMashine.moveToRandom)
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
        else if (stateMashine == StateMashine.moveToPosition)
        {
            Vector3 currentAgentPosition = GlobalTransform.Origin;
            float distanceToTarget = currentAgentPosition.DistanceTo(MovementPosition);// currentAgentPosition.DistanceTo(MovementPosition);
            if (distanceToTarget >= _navigationAgent.TargetDesiredDistance)
            {
                //var nextPathPosition = _navigationAgent.GetNextPathPosition();
                //nextPathPosition.Y = 0;
                //currentAgentPosition.Y = 0;
                //float distanceToPosition = currentAgentPosition.DistanceTo(nextPathPosition);
                //if (distanceToPosition <= _navigationAgent.TargetDesiredDistance)
                //{
                //    //_navigationAgent.SetProcessInternal
                //    int current = _navigationAgent.GetCurrentNavigationPathIndex();
                //    //int pathLenght = _navigationAgent.
                //    Vector3[] navPath = _navigationAgent.GetCurrentNavigationPath();
                //    GD.Print("Промежуточная точка");
                //}
                //_targetVelocity = GlobalPosition.DirectionTo(nextPathPosition) * _movementSpeed;

                if (currentPath < navPath.Length)
                {
                    Vector3 currentPathPos = navPath[currentPath];
                    currentPathPos.Y = currentAgentPosition.Y;
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
        else if (stateMashine == StateMashine.moveToTarget)
        {
            //if (MovementTarget == null || !IsInstanceValid(MovementTarget))
            //{
            //    stateMashine = StateMashine.idle;
            //    onTargetLost.Invoke();
            //    return;
            //}

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
        else if (stateMashine == StateMashine.moveFromTarget)
        {
            if (MovementTarget == null || !IsInstanceValid(MovementTarget))
            {
                stateMashine = StateMashine.idle;
                onTargetLost.Invoke();
                return;
            }

            Vector3 currentAgentPosition = GlobalTransform.Origin;
            float distanceToTarget = currentAgentPosition.DistanceTo(MovementTarget.GlobalPosition);

            Vector3 direction = (currentAgentPosition - MovementTarget.Position).Normalized();
            _targetVelocity = direction * _movementSpeed;

            //if (distanceToTarget < _navigationAgent.TargetDesiredDistance * 10)
            //{
            //    Vector3 direction = (currentAgentPosition - MovementTarget.Position).Normalized();
            //    Velocity = direction * _movementSpeed;
            //}
            //else
            //{
            //    Velocity = Vector3.Zero;
            //}
        }
        else if (stateMashine == StateMashine.findTarget)
        {
            if (MovementTarget == null)
            {
                stateMashine = StateMashine.idle;
                onTargetLost.Invoke();
                return;
            }

            Vector3 currentAgentPosition = GlobalTransform.Origin;
            float distanceToTarget = currentAgentPosition.DistanceTo(MovementTarget.GlobalPosition);
            if (distanceToTarget < radiusSearh) stateMashine = StateMashine.moveToTarget;
        }

        if (signalMashine == SignalMashine.checkMoveDistance)
        {
            //moveDistance
        }

        bool isMoving = (stateMashine == StateMashine.moveToRandom || stateMashine == StateMashine.moveToTarget
            || stateMashine == StateMashine.moveToPosition || stateMashine == StateMashine.moveFromTarget);

        if (oldPosition != Vector3.Zero && isMoving)
        {
            Vector3 vel = new Vector3(Velocity.X, 0, Velocity.Z);
            float moveDistDelta = oldPosition.DistanceTo(GlobalTransform.Origin);
            if (moveDistDelta > 0.01f && vel.Length() > 0.01f)
            {
                moveDistance += oldPosition.DistanceTo(GlobalTransform.Origin);
                moveDistanceOld = moveDistance;
                timeStuckMoving = 0;
            }
        }
        oldPosition = GlobalTransform.Origin;

        if (moveDistanceOld == moveDistance && isMoving)
        {
            if (timeStuckMoving > 5)
            {
                onStuckMoving?.Invoke();
                //onStuckMoving = null;
                timeStuckMoving = 0;
            }
            else
                timeStuckMoving += delta;
        }

        //if (signalMashine == SignalMashine.checkMoveDistance)
        if (checkMoveDistance > 0)
        {
            if (moveDistance > checkMoveDistance)
            {
                if (interactiveObjectMove != null) interactiveObjectMove.moveDistance.Value = checkMoveDistance;
                onMovingDistanceFinished?.Invoke();        
                signalMashine = SignalMashine.none;
                checkMoveDistance = 0;
            }
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

        // Движение

        if (Velocity.Length() > 0)
        {
            Vector3 forwardDir = Velocity.Normalized();
            float targetAngle = Mathf.Atan2(forwardDir.X, forwardDir.Z);
            Rotation = new Vector3(0, targetAngle, 0);
        }

        _targetVelocity.Y = 0;
        //_targetVelocity = _targetVelocity.Normalized();
        //_targetVelocity *= _movementSpeed;

        if (!isOnFloor)
        {
            Vector3 vel = Velocity;
            _targetVelocity.Y -= (float)9.8 * (float)delta * 20;
        }

        Velocity = _targetVelocity;
        MoveAndSlide();
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
        float posY = VoxLib.createTerrain.mapHeight[posX, posZ] + VoxLib.createTerrain.positionOffset.Y;

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

        int X = (int)newPosition.X;
        float Y = VoxLib.createTerrain.positionOffset.Y;
        int Z = (int)newPosition.Z;

        if (X > 0 && Z > 0)
            Y = VoxLib.createTerrain.mapHeight[(int)newPosition.X, (int)newPosition.Z] + VoxLib.createTerrain.positionOffset.Y;

        MovementPosition = new Vector3(X, Y, Z);

        PlayRunAnimation();

        SetupPath();
    }

    public Vector3 SetPositionRight(float n)
    {
        int X = (int)(GlobalPosition.X + n);
        float Y = VoxLib.createTerrain.positionOffset.Y;
        int Z = (int)GlobalPosition.Z;

        if (X > 0 && Z > 0)
            Y = VoxLib.createTerrain.mapHeight[(int)X, (int)Z] + VoxLib.createTerrain.positionOffset.Y;

        PlayRunAnimation();

        MovementPosition = new Vector3(X, Y, Z);
        return MovementPosition;
    }

    public Vector3 SetPositionLeft(float n)
    {
        int X = (int)(GlobalPosition.X - n);
        float Y = VoxLib.createTerrain.positionOffset.Y;
        int Z = (int)(GlobalPosition.Z);

        if (X > 0 && Z > 0)
            Y = VoxLib.createTerrain.mapHeight[(int)X, (int)Z] + VoxLib.createTerrain.positionOffset.Y;

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

    public void ReloadAlgorithm()
    {
        isBlocked = false;
    }

    bool isCollisionDetected = false;
    string nameCollisionDetected;
    private void CollisionDetectedSetup(string nameCollisionDetected)
    {
        this.nameCollisionDetected = nameCollisionDetected;
        isCollisionDetected = true;
    }

    private void _on_Player_body_entered(Node3D node)
    {
        Node3D my = this as Node3D;
        if (my == node) return;

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

    private void SetupPath()
    {
        var nextPathPosition = _navigationAgent.GetNextPathPosition();
        navPath = _navigationAgent.GetCurrentNavigationPath();
        currentPath = _navigationAgent.GetCurrentNavigationPathIndex();
    }
}
