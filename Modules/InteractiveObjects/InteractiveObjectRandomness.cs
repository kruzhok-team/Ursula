using Godot;
using Modules.HSM;
using System;

public partial class InteractiveObjectRandomness : Node
{
    public VariableHolder<int> CurrentValue = new(0);
    public VariableHolder<float> RandomValue = new(0);

    static Random random;
    private bool valueChanged;

    public Random Random
    {
        get
        {
            if (random is null)
            {
                random = new Random(42);
            }
            return random;
        }
    }

    public float RandomFloat
    {
        get
        {
            float value = (float)Random.NextDouble();
            RandomValue.Value = value;
            GD.Print($"[HSMRandomnessModule] Сгенерировано случайное число в диапазоне [{0}, {1}]: {value}");
            return value;
        }
    }


    public object GenerateRange(int a, int b)
    {
        if (a > b)
        {
            int t = a;
            a = b;
            b = t;
        }
        int value = Random.Next(a, b);
        CurrentValue.Value = value;
        valueChanged = true;

        return null;
    }
}

