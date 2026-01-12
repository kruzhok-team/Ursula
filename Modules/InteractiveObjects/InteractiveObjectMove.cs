using Godot;
using System;
using System.Reflection;
using System.Xml.Linq;
using Modules.HSM;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller;
using System.Collections.Generic;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization;

public partial class InteractiveObjectMove : Node
{
    private InteractiveObject _interactiveObject;

    public VariableHolder<float> moveDistance = new(0.0f);
    public VariableHolderLazy<float> heightWorld;
    public VariableHolder<float> surfaceType = new(0.0f);
    public VariableHolder<float> timesOfDay = new(0.0f);
    public VariableHolder<bool> isDay = new(false);

    public Action moveDistanceStart;
    public Action moveDistanceCompleted;
    public Action animationCompleted;
    public Action animationCycleCompleted;

    public Vector3 movePosition;
    private MoveScript moveScriptCache;
    public Queue<Vector3> movePath;

    private NavGraphVisualization visualization = new NavGraphVisualization();

    public MoveScript moveScript 
    { 
        get
        {
            if (moveScriptCache == null)
            {
                var moveScript = GetParent() as MoveScript;
                if (moveScript != null) moveScript.interactiveObjectMove = this;
                moveScriptCache = moveScript;
            }
            return moveScriptCache;
        } 
    }

    public InteractiveObject interactiveObject
    {
        get
        {
            if (_interactiveObject == null )
                _interactiveObject = GetParent().GetNode("InteractiveObject") as InteractiveObject;

            return _interactiveObject;
        }
    }

    public override void _Ready()
    {
        CSharpBridgeRegistry.Process += CSProcess;
        if (moveScript != null)
            heightWorld = new VariableHolderLazy<float>(moveScript.GetHeightWorld);
        else
            heightWorld = new VariableHolderLazy<float>(() =>
            {
                Node3D parent = GetParent() as Node3D;
                if (parent == null)
                    return 0;
                return parent.GlobalPosition.Y;
            });
    }

    public object MoveToTarget()
    {
        var movementTarget = interactiveObject.GetCurrentTargetObject();
        moveScript?.MoveToTargetSetup(movementTarget);

        return null;
    }

    public object MoveFromTarget()
    {
        var movementTarget = interactiveObject.GetCurrentTargetObject();
        moveScript?.MoveFromTargetSetup(movementTarget);

        return null;
    }

    public object MoveToTarget2()
    {
        var movementTarget = interactiveObject.GetCurrentTargetObject2();
        moveScript?.MoveToTargetSetup(movementTarget);

        return null;
    }

    public object MoveFromTarget2()
    {
        var movementTarget = interactiveObject.GetCurrentTargetObject2();
        moveScript?.MoveFromTargetSetup(movementTarget);

        return null;
    }
    
    public object MoveToRandom()
    {
        moveScript?.MoveToRandomSetup();
        return null;
    }

    public object MoveToPosition()
    {
        moveScript?.MoveToPositionSetup(movePosition);
        return null;
    }

    public object SetRandomPosition(float radius)
    {
        var random = new Random();

        float x = (float)(random.NextDouble() * 2 - 1) * radius;
        float z = (float)(random.NextDouble() * 2 - 1) * radius;

        if (x * x + z * z > radius * radius)
        {
            Vector3 direction = new Vector3(x, 0, z).Normalized();
            x = direction.X * radius;
            z = direction.Z * radius;
        }

        // Установка позиции с учетом только X и Z координат
        movePosition = moveScript.GlobalTransform.Origin + new Vector3(x, 0, z);
        return null;
    }

    public object SetPosition(float x, float y)
    {
        movePosition = new Vector3(x, 0, y);
        return null;
    }

    public object SetPositionRight(float n)
    {
        movePosition = moveScript.SetPositionRight(n);
        return null;
    }

    public object SetPositionLeft(float n)
    {
        movePosition = moveScript.SetPositionLeft(n);
        return null;
    }

    public object StopMoving()
    {
        moveScript?.StopMoveSetup();

        return null;
    }

    public object ResetCoordinates()
    {
        movePosition = Vector3.Zero;
        moveScript?.ResetCoordinates();

        return null;
    }

    public object BuildRandomPath()
    {
        NavGraphManager navGraph = NavGraphManager.Instance;
        if (navGraph == null)
            return null;

        Vector3 nextPoint = navGraph.GetRandomPoint();
        Vector3? position = moveScript?.Position;
        if (!position.HasValue)
            return null;

        movePath = NavGraphManager.Instance.BuildPath(position.Value, nextPoint);

        //visualization.Clear();
        //visualization.DrawPath(movePath, NavGraphManager.Instance, 0.03f);

        return null;
    }

    public object GetNextPathPoint()
    {
        if (movePath == null || movePath.Count == 0)
            return null;

        movePosition = movePath.Dequeue();
        return null;
    }

    public void CSProcess(double delta)
    {
        if (moveScript != null)
        {
            moveDistance.Value = moveScript.GetMoveDistance();
            surfaceType.Value = moveScript.GetSurfaceType();

            //if (moveScript.GetMoveDistance() > moveDistance.Value) moveScript.onMovingDistanceFinished.Invoke();
        }

        if (DayNightCycle.instance != null)
        {
            timesOfDay.Value = DayNightCycle.instance.TimesOfDay();
            isDay.Value = DayNightCycle.instance.IsDay;
        }
    }

    public object SetMoveDistance(float distance)
    {
        moveScript?.CheckMoveDistance(distance);
        return null;
    }

    public void ReloadAlgorithm()
    {
        moveScript?.ReloadAlgorithm();
    }

    AnimationPlayer animationPlayer;

    private void InitAnimator()
    {
        if (animationPlayer == null)
        {
            animationPlayer = GetNodeOrNull("AnimationPlayer") as AnimationPlayer;

            if (animationPlayer == null)
                animationPlayer = GetParent().GetNodeOrNull("AnimationPlayer") as AnimationPlayer;

            if (animationPlayer == null)
                animationPlayer = FindChild(GetParent(), "AnimationPlayer") as AnimationPlayer;
        }


    }

    private static Node FindChild(Node current, string name)
    {
        foreach (var child in current.GetChildren())
        {
            if (child.Name == name)
            {
                return child;
            }

            var result = FindChild(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    public object PlayRandomAnimation(int cyclesCount)
    {
        InitAnimator();

        var animations = animationPlayer.GetAnimationList();

        var _rng = new RandomNumberGenerator();
        int id = _rng.RandiRange(0, animations.Length);

        PlayAnimation(animations[id], cyclesCount);

        return null;
    }


    public object PlayAnimation(string id, int cyclesCount)
    {
        InitAnimator();

        if (animationPlayer == null)
            return null;

        string nameAnim = id;
        if (!animationPlayer.HasAnimation(nameAnim)) 
            nameAnim = BaseAnimation.LIBRARY + "/" + nameAnim;

        if (!animationPlayer.HasAnimation(nameAnim))
        {
            HSMLogger.Print(interactiveObject, $"Ошибка: анимация с именем '{nameAnim}' не найдена");
            return null;
        }

        animationPlayer.Play(nameAnim);
        OnAnimationFinishedWithCallBack(animationPlayer.CurrentAnimationLength, () =>
        {
            animationPlayer.Stop();

            animationCompleted?.Invoke();

            cyclesCount--;

            if (cyclesCount > 0)
            {
                PlayAnimation(id, cyclesCount);
            }
            else
            {
                animationCycleCompleted?.Invoke();
            }

        });

        async void OnAnimationFinishedWithCallBack(double delay, Action callback)
        {
            await ToSignal(GetTree().CreateTimer(delay), "timeout");
            callback?.Invoke();
        }

        return null;
    }

    public object PlayCycleAnimation(string id)
    {
        InitAnimator();

        if (animationPlayer == null)
            return null;

        string nameAnim = id;
        if (!animationPlayer.HasAnimation(nameAnim))
            nameAnim = BaseAnimation.LIBRARY + "/" + nameAnim;

        var animation = animationPlayer.GetAnimation(nameAnim);
        if (animation != null) animation.LoopMode = Animation.LoopModeEnum.Linear;

        if (animation == null)
        {
            HSMLogger.Print(interactiveObject, $"Ошибка: анимация с именем '{nameAnim}' не найдена");
            return null;
        }

        animation.LoopMode = Animation.LoopModeEnum.Linear;
        animationPlayer.Play(nameAnim);

        return null;
    }

    public object StopAnimation()
    {
        return null;
    }

    public override void _ExitTree()
    {
        CSharpBridgeRegistry.Process -= CSProcess;
    }
}
