using Godot;
using System;
using Modules.HSM;

public partial class InteractiveObjectCounter : Node
{
    public VariableHolder<float> variable = new(0);

    public Action onValueChanged;

    public object AddValue(int val)
    {
        variable.Value += val;
        onValueChanged?.Invoke();
        GD.Print($"InteractiveObjectCounter: {variable.Value}");
        return null;
    }

    public object SubValue(int val)
    {
        variable.Value -= val;
        onValueChanged?.Invoke();
        GD.Print($"InteractiveObjectCounter: {variable.Value}");
        return null;
    }

    public object ResetValue()
    {
        variable.Value = 0;
        onValueChanged?.Invoke();
        GD.Print($"InteractiveObjectCounter: {variable.Value}");
        return null;
    }

}
