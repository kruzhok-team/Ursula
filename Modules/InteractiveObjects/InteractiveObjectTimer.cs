using Godot;
using System;
using Modules.HSM;

public partial class InteractiveObjectTimer : Node
{
    public VariableHolder<float> CurrentTimerValue = new(0.0f);

    private Timer _timer;

    public Action TimerCompleted;
    //public GMLActionHolder Tick = new();
    //public GMLActionHolder TickOneSecond = new();

    private const float GameTick = 0.05f;
    private float _accumulatedTime = 0.0f;

    private bool _isTimerActive = false;

    public override void _Ready()
    {
        _timer = new Timer();
        _timer.WaitTime = GameTick;
        _timer.OneShot = false;
        _timer.Timeout += OnTick;
        AddChild(_timer);
    }

    public object StartTimer(float time)
    {
        ResetTimer();

        CurrentTimerValue.Value = time;
        _accumulatedTime = 0.0f;

        _isTimerActive = true;
        _timer.Start();

        return null;
    }

    public static Random _random = new Random();
    public static float RandomBetween(float a, float b)
    {
        if (a > b)
        {
            var t = a;
            a = b;
            b = t;
        }

        var rndVal = (float)_random.NextDouble();

        var delta = b - a;

        var rndOffset = delta * rndVal;

        return a + rndOffset;
    }

    public object StartRandom(float time0, float time1)
    {
        StartTimer(RandomBetween(time0, time1));

        return null;
    }

    public object StopTimer()
    {
        if (_timer == null) 
            return null;

        _timer.Stop();
        _isTimerActive = false;

        return null;
    }

    public object ResetTimer()
    {
        CurrentTimerValue.Value = 0.0f;
        _accumulatedTime = 0.0f;

        _timer.Stop();
        _isTimerActive = false;

        return null;
    }

    private void OnTick()
    {
        if (!_isTimerActive)
            return;

        if (CurrentTimerValue.Value > 0)
        {
            CurrentTimerValue.Value -= GameTick;
            _accumulatedTime += GameTick;
            //Tick.Invoke();
            // GD.Print("Tick");

            if (_accumulatedTime >= 1.0f)
            {
                _accumulatedTime -= 1.0f;
                //TickOneSecond.Invoke();
                // GD.Print("TickOneSecond");
            }
        }
        else
        {
            _timer.Stop();
            _isTimerActive = false;
            TimerCompleted.Invoke();
            // GD.Print("TimerCompleted");
        }
    }
}