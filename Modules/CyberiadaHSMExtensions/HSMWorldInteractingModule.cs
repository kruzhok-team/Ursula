using Godot;
using System;

public class HSMWorldInteractingModule
{
    InteractiveObject _object;

    const string ModuleName = "ВзаимодействиеСМиром";

    // Event keys
    const string ChangeSurfaceTypeKey = $"{ModuleName}.СменаТипаПоверхности";

    // Variable keys
    const string DayTimeVariableKey = $"{ModuleName}.ВремяСуток";
    const string SurfaceTypeVariableKey = $"{ModuleName}.ТипПоверхности";
    const string HeightAboveSurfaceVariableKey = $"{ModuleName}.ВысотаНадПоверхностью";

    public HSMWorldInteractingModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        if (_object.move.moveScript != null)
        {
            _object.move.moveScript.onChangeSurfaceType += () => logic.localBus.InvokeEvent(ChangeSurfaceTypeKey);
        }
        else
            HSMLogger.PrintMoveScriptError(interactiveObject);

        // Variables
        logic.localBus.AddVariableGetter(DayTimeVariableKey, () => _object.move.timesOfDay.Value);
        logic.localBus.AddVariableGetter(SurfaceTypeVariableKey, () => _object.move.surfaceType.Value);
        logic.localBus.AddVariableGetter(HeightAboveSurfaceVariableKey, () => _object.move.heightWorld.Value);
    }

}
