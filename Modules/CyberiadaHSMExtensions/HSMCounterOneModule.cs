using System;
using System.Collections.Generic;

public class HSMCounterOneModule
{
    InteractiveObject _object;
    const string ModuleName = "Счётчик1";

    // Event keys
    const string ValueChangedModuleKey = $"{ModuleName}.ЗначениеИзменилось";

    // Command keys
    const string AddValueCommandKey = $"{ModuleName}.ПрибавитьЗначение";
    const string SubValueCommandKey = $"{ModuleName}.ОтнятьЗначение";
    const string ResetValueCommandKey = $"{ModuleName}.ОбнулитьЗначение";

    // Variable keys
    const string CurrentValueVariableKey = $"{ModuleName}.ТекущееЗначение";

    public HSMCounterOneModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.counter1.onValueChanged += () => logic.localBus.InvokeEvent(ValueChangedModuleKey);

        // Commands
        logic.localBus.AddCommandListener(AddValueCommandKey, AddValue);
        logic.localBus.AddCommandListener(SubValueCommandKey, SubValue);
        logic.localBus.AddCommandListener(ResetValueCommandKey, ResetValue);

        // Variables
        logic.localBus.AddVariableGetter(CurrentValueVariableKey, () => _object.counter1.variable.Value);
    }

    bool AddValue(List<Tuple<string, string>> value)
    {
        _object.counter1.AddValue(HSMUtils.GetValue<int>(value[0]));

        return true;
    }
    bool SubValue(List<Tuple<string, string>> value)
    {
        _object.counter1.SubValue(HSMUtils.GetValue<int>(value[0]));

        return true;
    }
    bool ResetValue(List<Tuple<string, string>> value)
    {
        _object.counter1.ResetValue();

        return true;
    }
}
