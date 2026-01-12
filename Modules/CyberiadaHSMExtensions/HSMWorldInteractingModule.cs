using Godot;
using System;

public class HSMWorldInteractingModule
{
    InteractiveObject _object;

    const string ModuleName = "ВзаимодействиеСМиром";

    // Event keys
    const string ChangeSurfaceTypeKey = $"{ModuleName}.СменаТипаПоверхности";
    const string DayEventKey = $"{ModuleName}.НаступилДень";
    const string NightEventKey = $"{ModuleName}.НаступилаНочь";

    // Variable keys
    const string DayTimeVariableKey = $"{ModuleName}.ВремяСуток";
    const string IsDayVariableKey = $"{ModuleName}.ФлагДня";
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

        DayNightCycle.instance.DayNightCycleChanged += () =>
        {
            if (DayNightCycle.instance.IsDay)
            {
                logic.localBus.InvokeEvent(DayEventKey);
            }
            else
            {
                logic.localBus.InvokeEvent(NightEventKey);
            }
        };

        // Variables
        logic.localBus.AddVariableGetter(DayTimeVariableKey, () => _object.move.timesOfDay.Value);
        logic.localBus.AddVariableGetter(IsDayVariableKey, () => _object.move.isDay.Value);
        logic.localBus.AddVariableGetter(SurfaceTypeVariableKey, () => _object.move.surfaceType.Value);
        logic.localBus.AddVariableGetter(HeightAboveSurfaceVariableKey, () => _object.move.heightWorld.Value);
    }

}
