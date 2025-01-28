using Godot;
using System;
using static GMLAlgorithm;

public partial class InteractiveObjectCounter : Node
{
    public VariableHolder<float> variable = new(0);

    public GMLActionHolder onValueChanged = new();

    public object AddValue(int val)
    {
        variable.Value += val;
        onValueChanged.Invoke();
        GD.Print($"InteractiveObjectCounter: {variable.Value}");
        return null;
    }

    public object SubValue(int val)
    {
        variable.Value -= val;
        onValueChanged.Invoke();
        GD.Print($"InteractiveObjectCounter: {variable.Value}");
        return null;
    }

    public object ResetValue()
    {
        variable.Value = 0;
        onValueChanged.Invoke();
        GD.Print($"InteractiveObjectCounter: {variable.Value}");
        return null;
    }

}
