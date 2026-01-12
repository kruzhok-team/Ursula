using Godot;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

public partial class BaseAnimation : Node
{
    public static string LIBRARY = "Animations";

    [Export] public string IdleAnimationName = "Idle";
    [Export] public string WalkAnimationName = "Walk";
    [Export] public string RunAnimationName = "Run";
    [Export] public string JumpAnimationName = "Jump";
    [Export] public string EatingAnimationName = "Eating";
    [Export] public string HeadSpinAnimationName = "HeadSpin";

    [Export] public Animation HeadSpinAnimationOverride;

    public Action playAction { get; set; }

    AnimationLibrary animationLibrary;

    public enum State
    {
        Idle,
        Use,
        Walk,
        Run,
        Jump,
        Eating,
        HeadSpin
    }

    public State currentState;

    private StringName _currentAnim = default;
    private double _currentBlend = -1;
    private bool _loopCurrent = false;
    private StringName _queuedNext = default;

    private Node3D parentNode;

    private bool _subscribed = false;

    public AnimationPlayer animationPlayer;

    public virtual Action IdleAction { get; set; }
    public virtual Action UseAction { get; set; }
    public virtual Action WalkAction { get; set; }
    public virtual Action RunAction { get; set; }
    public virtual Action JumpAction { get; set; }
    public virtual Action EatingAction { get; set; }
    public virtual Action HeadSpinAction { get; set; }

    public override void _Ready()
    {
        AnimationSetup();
        AutoplaySetup();
        parentNode = GetParent<Node3D>();
        CSharpBridgeRegistry.Process += CSProcess;
    }

    public void CSProcess(double delta)
    {
        if (Frustum.In(parentNode))
        {
            if (animationPlayer.CurrentAnimation != "")
            {
                animationPlayer?.Play();
            }
        }
        else
        {
            animationPlayer?.Pause();
        }
    }

    public virtual void AutoplaySetup()
    {
        //animationPlayer.Autoplay = IdleAnimationName;
        PlayIdleAnimation();
    }

    public virtual void AnimationSetup()
    {
        animationPlayer = GetNodeOrNull("AnimationPlayer") as AnimationPlayer;
        if (animationPlayer == null) animationPlayer = GetParent().GetNodeOrNull("AnimationPlayer") as AnimationPlayer;

        IdleAction = PlayIdleAnimation;
        UseAction = PlayUseAnimation;
        WalkAction = PlayWalkAnimation;
        RunAction = PlayRunAnimation;
        JumpAction = PlayJumpAnimation;
        EatingAction = PlayEatingAnimation;
        HeadSpinAction = PlayHeadSpinAnimation;

        animationLibrary = new AnimationLibrary();
        string[] animations = animationPlayer.GetAnimationList();
        for (int i = 0; i < animations.Length; i++)
        {
            animationLibrary.AddAnimation(animations[i], animationPlayer.GetAnimation(animations[i]));
        }

        if (HeadSpinAnimationOverride != null && !animationLibrary.HasAnimation(HeadSpinAnimationName))
        {
            animationLibrary.AddAnimation(HeadSpinAnimationName, HeadSpinAnimationOverride);
            animationPlayer.AddAnimationLibrary(LIBRARY, animationLibrary);
        }
    }

    public override void _ExitTree()
    {
        IdleAction -= PlayIdleAnimation;
        UseAction -= PlayUseAnimation;
        WalkAction -= PlayWalkAnimation;
        RunAction -= PlayRunAnimation;
        JumpAction -= PlayJumpAnimation;
        EatingAction -= PlayEatingAnimation;
        HeadSpinAction -= PlayHeadSpinAnimation;
        CSharpBridgeRegistry.Process -= CSProcess;
    }

    private void EnsureSubscribed()
    {
        if (_subscribed || animationPlayer == null)
            return;
        animationPlayer.AnimationFinished += HandleAnimationFinished;
        _subscribed = true;
    }

    protected void Play(string name, float blend)
    {
        if (animationPlayer == null) return;

        EnsureSubscribed();

        _loopCurrent = true;
        _queuedNext = default;
        _currentBlend = blend;

        var target = (StringName)name;

        StartAnimation(target, _currentBlend);
    }

    protected void PlayOnce(string name, float blend, string next = null)
    {
        if (animationPlayer == null) return;

        EnsureSubscribed();

        _loopCurrent = false;
        _queuedNext = string.IsNullOrEmpty(next) ? default : (StringName)next;
        _currentBlend = blend;

        var target = (StringName)name;

        StartAnimation(target, _currentBlend);
    }

    private void StartAnimation(StringName anim, double blend)
    {
        if (!animationPlayer.HasAnimation(anim)) return;

        animationPlayer.Play(anim, 0, 1.0f, false);
        _currentAnim = anim;
    }

    private void HandleAnimationFinished(StringName finishedAnim)
    {
        if (finishedAnim != _currentAnim) return;
        if (_loopCurrent)
        {
            animationPlayer.Play(_currentAnim, 0, 1.0f, false);
        }
        else if (_queuedNext != default)
        {
            Play(_queuedNext, (float)_currentBlend);
        }
    }

    public virtual void PlayIdleAnimation()
    {
        if (animationPlayer == null) return;

        currentState = State.Idle;
        Play(IdleAnimationName, 0);
    }

    public virtual void PlayUseAnimation()
    {
        PlayEatingAnimation();
        currentState = State.Use;
    }

    public virtual void PlayWalkAnimation()
    {
        if (animationPlayer == null) return;

        currentState = State.Walk;
        Play(WalkAnimationName, 0);
    }

    public virtual void PlayRunAnimation()
    {
        if (animationPlayer == null) return;

        currentState = State.Run;
        Play(RunAnimationName, 0);
    }

    public virtual void PlayJumpAnimation()
    {
        if (animationPlayer == null) return;

        currentState = State.Jump;
        PlayOnce(JumpAnimationName, 0, IdleAnimationName);
    }

    public virtual void PlayEatingAnimation()
    {
        if (animationPlayer == null) return;

        currentState = State.Eating;
        PlayOnce(EatingAnimationName, 0, IdleAnimationName);
    }

    public virtual void PlayHeadSpinAnimation()
    {
        if (animationPlayer == null) return;

        string nameAnim = HeadSpinAnimationName;
        if (!animationPlayer.HasAnimation(nameAnim)) nameAnim = LIBRARY + "/" + HeadSpinAnimationName;

        currentState = State.HeadSpin;
        PlayOnce(nameAnim, 0, IdleAnimationName);
    }
}
