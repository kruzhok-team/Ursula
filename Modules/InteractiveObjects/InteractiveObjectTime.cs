using Godot;
using System;
using Modules.HSM;


public partial class InteractiveObjectTime : Node
{
    public VariableHolder<int> CurrentTicksValue = new(0);
    public VariableHolder<int> CurrentSecondsValue = new(0);

    private int moduloSeconds = 0;
    private int moduloTicks = 0;

    public override void _Ready()
    {
        ProjectTimer.Instance.Tick += () => ProcessTicks();
        ProjectTimer.Instance.TickOneSecond += () => ProcessSeconds();
    }

    private void ProcessTicks()
    {
        if (moduloTicks > 0)
        {
            CurrentTicksValue.Value = (CurrentTicksValue.Value + 1) % moduloTicks;
        }
        else
        {
            CurrentTicksValue.Value = 0;
        }
    }

    private void ProcessSeconds()
    {
        if (moduloSeconds > 0)
        {
            CurrentSecondsValue.Value = (CurrentSecondsValue.Value + 1) % moduloSeconds;
        }
        else
        {
            CurrentSecondsValue.Value = 0;
        }
    }

    public object SetModuloSeconds(int modulo)
    {
        moduloSeconds = modulo > 0 ? modulo : 0;
        ProcessSeconds();

        return null;
    }

    public object SetModuloTicks(int modulo)
    {
        moduloTicks = modulo > 0 ? modulo : 0;
        ProcessTicks();

        return null;
    }
}

