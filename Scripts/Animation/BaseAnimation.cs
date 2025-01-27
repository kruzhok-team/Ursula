using Godot;
using System;
using System.Xml.Linq;

public partial class BaseAnimation : Node3D
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

    public enum state 
    {
        Idle,
        Use,
        Walk,
        Run,
        Jump,
        Eating,
        HeadSpin
    }

    public state State;

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
    }

    public virtual async void AutoplaySetup()
    {
        animationPlayer.Autoplay = IdleAnimationName;
        PlayIdleAnimation();
    }

    public virtual async void AnimationSetup()
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
    }

    public virtual async void OnAnimationPlay(string name, float blend, double delay = 0, Action action = null)
    {
        if (animationPlayer == null) return;

        await ToSignal(GetTree().CreateTimer(delay), "timeout");
        animationPlayer.Play(name, blend);
        playAction = action;
        if (playAction != null) OnAnimationFinished(animationPlayer.CurrentAnimationLength);
    }

    public virtual async void OnAnimationFinished(double delay)
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");
        playAction?.Invoke();
    }

    public virtual async void PlayIdleAnimation()
    {
        if (animationPlayer == null) return;

        State = state.Idle;
        animationPlayer.Play(IdleAnimationName, 1, 1, false);
        playAction = PlayIdleAnimation;
        OnAnimationFinished(animationPlayer.CurrentAnimationLength);
    }

    public virtual async void PlayUseAnimation()
    {
        State = state.Use;
        PlayEatingAnimation();
    }

    public virtual async void PlayWalkAnimation()
    {
        if (animationPlayer == null) return;

        State = state.Walk;
        animationPlayer.Play(WalkAnimationName);
        playAction = PlayWalkAnimation;
        OnAnimationFinished(animationPlayer.CurrentAnimationLength);
    }

    public virtual async void PlayRunAnimation()
    {
        if (animationPlayer == null) return;

        State = state.Run;
        animationPlayer.Play(RunAnimationName);
        playAction = PlayRunAnimation;
        OnAnimationFinished(animationPlayer.CurrentAnimationLength);
    }

    public virtual async void PlayJumpAnimation()
    {
        if (animationPlayer == null) return;

        State = state.Jump;
        animationPlayer.Play(JumpAnimationName);
        OnAnimationPlay(IdleAnimationName, 0.3f, animationPlayer.CurrentAnimationLength);
    }

    public virtual async void PlayEatingAnimation()
    {
        if (animationPlayer == null) return;

        State = state.Eating;
        playAction = null;
        animationPlayer.Play(EatingAnimationName, 0.3f);
        OnAnimationPlay(IdleAnimationName, 0.3f, animationPlayer.CurrentAnimationLength, PlayIdleAnimation);
    }

    public virtual async void PlayHeadSpinAnimation()
    {
        if (animationPlayer == null) return;

        string nameAnim = HeadSpinAnimationName;
        if (!animationPlayer.HasAnimation(nameAnim)) nameAnim = LIBRARY + "/" + HeadSpinAnimationName;

        State = state.HeadSpin;
        playAction = null;
        animationPlayer.Play(nameAnim, 0.3f);
        OnAnimationPlay(IdleAnimationName, 0.3f, animationPlayer.CurrentAnimationLength, PlayIdleAnimation);
    }

}
