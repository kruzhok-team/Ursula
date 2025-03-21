using Godot;
using System;
using Modules.HSM;

public partial class ProjectTimer : Node
{
    private static ProjectTimer _instance;

    public static ProjectTimer Instance
    {
        get
        {
            return _instance;
        }
    }

    public Action Tick;
    public Action TickOneSecond;

    private Timer _timer;

    private const float GameTick = 0.05f;
    private float _accumulatedTime = 0.0f;

    public override void _Ready()
    {
        _instance = this;

        _timer = new Timer();
        _timer.WaitTime = GameTick;
        _timer.OneShot = false;
        _timer.Timeout += OnTick;
        AddChild(_timer);

        _timer.Start();
    }

    private void OnTick()
    {
        _accumulatedTime += GameTick;
        Tick?.Invoke();

        if (_accumulatedTime >= 1.0f)
        {
            _accumulatedTime -= 1.0f;
            TickOneSecond?.Invoke();
        }
    }
}
