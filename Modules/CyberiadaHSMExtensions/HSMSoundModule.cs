using Godot;
using System;
using System.Collections.Generic;

public class HSMSoundModule
{
    InteractiveObject _object;

    const string ModuleName = "ВоспроизведениеЗвука";

    // Command keys
    const string SetMaxDistanceCommandKey = $"{ModuleName}.УстановитьРадиусСлышимости";
    const string PlaySoundCommandKey = $"{ModuleName}.ВоспроизвестиЗвук";
    const string PlayRandomSoundCommandKey = $"{ModuleName}.ВоспроизвестиСлучайныйЗвук";
    const string StopSoundCommandKey = $"{ModuleName}.Стоп";
    const string PauseSoundCommandKey = $"{ModuleName}.Пауза";

    public HSMSoundModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Commands
        logic.localBus.AddCommandListener(SetMaxDistanceCommandKey, SetMaxDistance);
        logic.localBus.AddCommandListener(PlaySoundCommandKey, Play2D);
        logic.localBus.AddCommandListener(PlayRandomSoundCommandKey, PlayRandom2D);
        logic.localBus.AddCommandListener(StopSoundCommandKey, Stop);
        logic.localBus.AddCommandListener(PauseSoundCommandKey, Pause);
    }

    bool SetMaxDistance(List<Tuple<string, string>> value)
    {
        _object.audio.SetMaxDistance(HSMUtils.GetValue<float>(value[0]));

        return true;
    }

    bool Play2D(List<Tuple<string, string>> value)
    {
        _object.audio.Play2D(HSMUtils.GetValue<string>(value[0]), HSMUtils.GetValue<string>(value[1]));

        return true;
    }

    bool PlayRandom2D(List<Tuple<string, string>> value)
    {
        _object.audio.PlayRandom2D(HSMUtils.GetValue<string>(value[0]));

        return true;
    }

    bool Stop(List<Tuple<string, string>> value)
    {
        _object.audio.Stop();

        return true;
    }

    bool Pause(List<Tuple<string, string>> value)
    {
        _object.audio.Pause();

        return true;
    }
}
