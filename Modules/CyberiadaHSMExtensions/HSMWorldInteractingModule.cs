using Godot;
using System;

public class HSMWorldInteractingModule
{
    InteractiveObject _object;

    const string ModuleName = "��������������������";

    // Variable keys
    const string DayTimeVariableKey = $"{ModuleName}.����������";
    const string SurfaceTypeVariableKey = $"{ModuleName}.��������������";
    const string HeightAboveSurfaceVariableKey = $"{ModuleName}.���������������������";

    public HSMWorldInteractingModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Variables
        logic.localBus.AddVariableGetter(DayTimeVariableKey, () => _object.move.timesOfDay.Value);
        logic.localBus.AddVariableGetter(SurfaceTypeVariableKey, () => _object.move.surfaceType.Value);
        logic.localBus.AddVariableGetter(HeightAboveSurfaceVariableKey, () => _object.move.heightWorld.Value);
    }

}
