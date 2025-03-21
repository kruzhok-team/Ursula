using Godot;
using System;
using System.Collections.Generic;

public partial class HSMTimerModule : Node
{
    InteractiveObject _object;

    const string ModuleName = "Таймер";

    // Event keys
    const string TimerCompletedEventKey = $"{ModuleName}.ТаймерВыполнен";
    const string TickEventKey = $"{ModuleName}.Тик";
    const string TickOneSecondEventKey = $"{ModuleName}.Тик1Секунда";

    // Command keys
    const string TimerStartCommandKey = $"{ModuleName}.ТаймерЗапуск";
    const string TimerStartRangeCommandKey = $"{ModuleName}.ТаймерЗапускДиапазон";
    const string TimerStopCommandKey = $"{ModuleName}.ТаймерСтоп";
    const string TimerResetCommandKey = $"{ModuleName}.ТаймерСброс";

    // Variable keys
    const string CurrentTimerValueVariableKey = $"{ModuleName}.ТекущееЗначениеТаймера";

    public HSMTimerModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.timer.TimerCompleted += () => logic.localBus.InvokeEvent(TimerCompletedEventKey);
        ProjectTimer.Instance.Tick += () => logic.localBus.InvokeEvent(TickEventKey);
        ProjectTimer.Instance.TickOneSecond += () => logic.localBus.InvokeEvent(TickOneSecondEventKey);

        // Commands
        logic.localBus.AddCommandListener(TimerStartCommandKey, StartTimer);
        logic.localBus.AddCommandListener(TimerStartRangeCommandKey, StartRandom);
        logic.localBus.AddCommandListener(TimerStopCommandKey, StopTimer);
        logic.localBus.AddCommandListener(TimerResetCommandKey, ResetTimer);

        // Variables
        logic.localBus.AddVariableGetter(CurrentTimerValueVariableKey, () => _object.timer.CurrentTimerValue.Value);
    }

    bool StartTimer(List<Tuple<string, string>> value)
    {
        _object.timer.StartTimer(HSMUtils.GetValue<float>(value[0]));

        return true;
    }

    bool StartRandom(List<Tuple<string, string>> value)
    {
        _object.timer.StartRandom(HSMUtils.GetValue<float>(value[0]), HSMUtils.GetValue<float>(value[1]));

        return true;
    }

    bool StopTimer(List<Tuple<string, string>> value)
    {
        _object.timer.StopTimer();

        return true;
    }

    bool ResetTimer(List<Tuple<string, string>> value)
    {
        _object.timer.ResetTimer();

        return true;
    }
}
