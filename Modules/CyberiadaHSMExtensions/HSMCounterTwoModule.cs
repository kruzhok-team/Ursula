using Godot;
using System;
using System.Collections.Generic;

public class HSMCounterTwoModule
{
    InteractiveObject _object;

    const string ModuleName = "�������2";

    public HSMCounterTwoModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.counter2.onValueChanged += () => logic.localBus.InvokeEvent($"{ModuleName}.������������������");

        // Commands
        logic.localBus.AddCommandListener($"{ModuleName}.�����������������", AddValue);
        logic.localBus.AddCommandListener($"{ModuleName}.��������������", SubValue);
        logic.localBus.AddCommandListener($"{ModuleName}.����������������", ResetValue);

        // Variables
        logic.localBus.AddVariableGetter($"{ModuleName}.���������������", () => _object.counter2.variable.Value);
    }

    bool AddValue(List<Tuple<string, string>> value)
    {
        _object.counter2.AddValue(HSMUtils.GetValue<int>(value[0]));

        return true;
    }
    bool SubValue(List<Tuple<string, string>> value)
    {
        _object.counter2.SubValue(HSMUtils.GetValue<int>(value[0]));

        return true;
    }
    bool ResetValue(List<Tuple<string, string>> value)
    {
        _object.counter2.ResetValue();

        return true;
    }
}
