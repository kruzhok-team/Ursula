using Godot;
using System.Collections.Generic;
using System;


public partial class HSMTimeModule : Node
{
    InteractiveObject _object;

    const string ModuleName = "МодульВремени";

    // Event keys
    const string TickEventKey = $"{ModuleName}.Тик";
    const string TickOneSecondEventKey = $"{ModuleName}.Тик1Секунда";

    // Command keys
    const string SetMaxTicksCommandKey = $"{ModuleName}.УстановитьМаксимумТиков";
    const string SetMaxSecondsCommandKey = $"{ModuleName}.УстановитьМаксимумСекунд";

    // Variable keys
    const string CurrentTicksVariableKey = $"{ModuleName}.КоличествоТиков";
    const string CurrentSecondsVariableKey = $"{ModuleName}.КоличествоСекунд";

    public HSMTimeModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        ProjectTimer.Instance.Tick += () => logic.localBus.InvokeEvent(TickEventKey);
        ProjectTimer.Instance.TickOneSecond += () => logic.localBus.InvokeEvent(TickOneSecondEventKey);

        // Commands
        logic.localBus.AddCommandListener(SetMaxTicksCommandKey, SetMaxTicks);
        logic.localBus.AddCommandListener(SetMaxSecondsCommandKey, SetMaxSeconds);

        // Variables
        logic.localBus.AddVariableGetter(CurrentTicksVariableKey, () => _object.time.CurrentTicksValue.Value);
        logic.localBus.AddVariableGetter(CurrentSecondsVariableKey, () => _object.time.CurrentSecondsValue.Value);
    }

    private bool SetMaxTicks(List<Tuple<string, string>> value)
    {
         int moduloTicks = HSMUtils.GetValue<int>(value[0]);
        _object.time.SetModuloTicks(moduloTicks);

        return true;
    }

    private bool SetMaxSeconds(List<Tuple<string, string>> value)
    {
        int moduloSeconds = HSMUtils.GetValue<int>(value[0]);
        _object.time.SetModuloSeconds(moduloSeconds);

        return true;
    }
}

