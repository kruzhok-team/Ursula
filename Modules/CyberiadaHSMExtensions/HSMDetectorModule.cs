using System;
using System.Collections.Generic;

// МодульОбнаружения
public class HSMDetectorModule
{
    InteractiveObject _object;

    const string ModuleName = "МодульОбнаружения";

    const string ObjectDetectedModuleKey = $"{ModuleName}.ОбъектОбнаружен";
    const string PlayerDetectedModuleKey = $"{ModuleName}.ИгрокОбнаружен";
    const string SoundDetectedModuleKey = $"{ModuleName}.ЗвукОбнаружен";
    const string TargetLostModuleKey = $"{ModuleName}.ЦельПотеряна";
    const string ThisInteractionModuleKey = $"{ModuleName}.ВзаимодействиеИгрока";
    const string PlayerInteractionObjectModuleKey = $"{ModuleName}.ВзаимодействиеИгрокаСОбъектом";
    const string AnyObjectsNotDetectedObjectDetectedModuleKey = $"{ModuleName}.ОбъектовНеОбнаружено";

    const string PlayerScanCommandKey = $"{ModuleName}.ПоискИгрокаВРадиусе";
    const string ObjectScanCommandKey = $"{ModuleName}.ПоискОбъектаВРадиусеПоИмени";
    const string SoundScanCommandKey = $"{ModuleName}.ОбнаружениеВоспроизведенияЗвука";
    const string StopScanningCommandKey = $"{ModuleName}.ОстановкаПоиска";
    const string PlayerObjectInteractionScanCommandKey = $"{ModuleName}.ВзаимодействиеИгрокаСОбъектом";

    public HSMDetectorModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.detector.onObjectDetected += () => logic.localBus.InvokeEvent(ObjectDetectedModuleKey);
        _object.detector.onPlayerDetected += () => logic.localBus.InvokeEvent(PlayerDetectedModuleKey);
        _object.detector.onSoundDetected += () => logic.localBus.InvokeEvent(SoundDetectedModuleKey);
        _object.move.moveScript.onTargetLost += () => logic.localBus.InvokeEvent(TargetLostModuleKey);
        _object.onThisInteraction += () => logic.localBus.InvokeEvent(ThisInteractionModuleKey);
        _object.detector.onPlayerInteractionObject += () => logic.localBus.InvokeEvent(PlayerInteractionObjectModuleKey);
        _object.detector.onAnyObjectsNotDetected += () => logic.localBus.InvokeEvent(AnyObjectsNotDetectedObjectDetectedModuleKey);

        // Commands
        logic.localBus.AddCommandListener(PlayerScanCommandKey, StartPlayerScan);
        logic.localBus.AddCommandListener(ObjectScanCommandKey, StartObjectScan);
        logic.localBus.AddCommandListener(SoundScanCommandKey, StartSoundScan);
        logic.localBus.AddCommandListener(StopScanningCommandKey, StopScanning);
        logic.localBus.AddCommandListener(PlayerObjectInteractionScanCommandKey, StartPlayerObjectInteractionScan);
    }

    bool StartPlayerScan(List<Tuple<string, string>> values)
    {
        _object.detector.StartPlayerScan(HSMUtils.GetValue<float>(values[0]));

        return true;
    }

    bool StartObjectScan(List<Tuple<string, string>> values)
    {
        _object.detector.StartObjectScan(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]));

        return true;
    }

    bool StartSoundScan(List<Tuple<string, string>> values)
    {
        _object.detector.StartSoundScan(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[ 1]));

        return true;
    }

    bool StopScanning(List<Tuple<string, string>> values)
    {
        _object.detector.StopScanning();

        return true;
    }

    bool StartPlayerObjectInteractionScan(List<Tuple<string, string>> values)
    {
        _object.detector.StartPlayerObjectInteractionScan(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]));

        return true;
    }

}
