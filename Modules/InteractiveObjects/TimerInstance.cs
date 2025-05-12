using Godot;

public partial class TimerInstance : GodotObject
{
    //private VariableHolder<float> _currentTimerValue;

    //private Timer _timer;

    //private GMLActionHolder _timerCompleted;
    //private GMLActionHolder _tick;
    //private GMLActionHolder _tickOneSecond;

    //public TimerInstance(Node parent, VariableHolder<float> _currentTimerValue, GMLActionHolder timerCompleted, GMLActionHolder tick, GMLActionHolder tickOneSecond)
    //{
    //    _timerCompleted = timerCompleted;
    //    _tick = tick;
    //    _tickOneSecond = tickOneSecond;

    //    Init(parent);
    //}

    //private const float GameTick = 0.05f;
    //private float _accumulatedTime = 0.0f;

    //private bool liveFlag = false;

    //private void Init(Node parent)
    //{
    //    _timer = new Timer();
    //    _timer.WaitTime = GameTick;
    //    _timer.OneShot = false;
    //    _timer.Timeout += OnTick;
    //    parent.AddChild(_timer);
    //}

    //public object StartTimer(float time)
    //{
    //    ResetTimer();

    //    _currentTimerValue.Value = time;
    //    _accumulatedTime = 0.0f;
    //    _timer.Start();

    //    liveFlag = true;

    //    return null;
    //}

    //public object StopTimer()
    //{
    //    _timer.Stop();

    //    liveFlag = false;

    //    return null;
    //}

    //public object ResetTimer()
    //{
    //    _currentTimerValue.Value = 0.0f;
    //    _accumulatedTime = 0.0f;
    //    _timer.Stop();

    //    return null;
    //}

    //private void OnTick()
    //{
    //    if (_currentTimerValue.Value > 0)
    //    {
    //        _currentTimerValue.Value -= GameTick;
    //        _accumulatedTime += GameTick;
    //        _tick.Invoke();
    //        //GD.Print("Tick");


    //        if (_accumulatedTime >= 1.0f)
    //        {
    //            _accumulatedTime -= 1.0f;
    //            _tickOneSecond.Invoke();
    //            //GD.Print("TickSecond");
    //        }
    //    }
    //    else
    //    {
    //        _timer.Stop();
    //        _timerCompleted.Invoke();
    //        ResetTimer();
    //        //GD.Print("TimerCompleted");
    //    }
    //}
}
