using System.Collections.Generic;
using System;
using Godot;

public partial class HSMRandomnessModule : Node
{
    InteractiveObject _object;

    const string ModuleName = "МодульСлучайности";

    // Command keys
    const string GenerateRangeCommandKey = $"{ModuleName}.СгенерироватьИзПромежутка";

    // Variable keys
    const string CurrentValueVariableKey = $"{ModuleName}.ТекущееЗначение";
    const string RandomValueVariableKey = $"{ModuleName}.СлучайноеЗначение";

    public HSMRandomnessModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Commands
        logic.localBus.AddCommandListener(GenerateRangeCommandKey, GenerateRange);

        // Variables
        logic.localBus.AddVariableGetter(CurrentValueVariableKey, () => _object.random.CurrentValue.Value);
        logic.localBus.AddVariableGetter(RandomValueVariableKey, () => _object.random.RandomFloat);
    }

    bool GenerateRange(List<Tuple<string, string>> value)
    {
        int a = HSMUtils.GetValue<int>(value[0]);
        int b = HSMUtils.GetValue<int>(value[1]);
        // Включительно
        _object.random.GenerateRange(a, b + 1);

        return true;
    }
}

