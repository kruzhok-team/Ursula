using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

// МодульОбнаружения
public class HSMDetectorTwoModule
{
    InteractiveObject _object;

    const string ModuleName = "МодульОбнаружения2";

    const string ObjectDetectedModuleKey = $"{ModuleName}.ОбъектОбнаружен";
    const string PlayerDetectedModuleKey = $"{ModuleName}.ИгрокОбнаружен";

    const string SoundDetectedModuleKey = $"{ModuleName}.ЗвукОбнаружен";

    const string TargetLostModuleKey = $"{ModuleName}.ЦельПотеряна";
    const string ThisInteractionModuleKey = $"{ModuleName}.ВзаимодействиеИгрока";
    const string PlayerInteractionObjectModuleKey = $"{ModuleName}.ВзаимодействиеИгрокаСОбъектом";
    const string AnyObjectsNotDetectedObjectDetectedModuleKey = $"{ModuleName}.ОбъектовНеОбнаружено";

    const string PlayerScanCommandKey = $"{ModuleName}.ПоискИгрокаВРадиусе";
    const string ObjectScanCommandKey = $"{ModuleName}.ПоискОбъектаВРадиусеПоИмени";
    const string ObjectScanSquareCommandKey = $"{ModuleName}.ПоискОбъектаВКвадратеПоИмени";
    const string ObjectScanRectangleCommandKey = $"{ModuleName}.ПоискОбъектаВПрямоугольникеПоИмени";
    const string SoundScanCommandKey = $"{ModuleName}.ОбнаружениеВоспроизведенияЗвука";
    const string SoundScanOffsetCommandKey = $"{ModuleName}.ПоискЗвукаВРадиусеСоСмещением";
    const string StopScanningCommandKey = $"{ModuleName}.ОстановкаПоиска";
    const string PlayerObjectInteractionScanCommandKey = $"{ModuleName}.ВзаимодействиеИгрокаСОбъектом";

    const string SoundDetectionVariableKey = $"{ModuleName}.ЗначениеОбнаруженияЗвука";
    const string ObjectToTheRightVariableKey = $"{ModuleName}.ОбъектСправа";
    const string ObjectAheadVariableKey = $"{ModuleName}.ОбъектСпереди";
    const string ObjectCodirectional = $"{ModuleName}.ОбъектСонаправлен";
    const string ObjectCounterdirectional = $"{ModuleName}.ОбъектПротивонаправлен";
    const string ObjectCloserToIntersection = $"{ModuleName}.ОбъектБлижеКТочкеПересечения";

    public HSMDetectorTwoModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.detector2.onObjectDetected += () => logic.localBus.InvokeEvent(ObjectDetectedModuleKey);
        _object.detector2.onPlayerDetected += () => logic.localBus.InvokeEvent(PlayerDetectedModuleKey);
        _object.detector2.onSoundDetected += () => logic.localBus.InvokeEvent(SoundDetectedModuleKey);

        if (_object.move.moveScript != null)
            _object.move.moveScript.onTargetLost += () => logic.localBus.InvokeEvent(TargetLostModuleKey);
        else
            HSMLogger.PrintMoveScriptError(interactiveObject);

        _object.onThisInteraction += () => logic.localBus.InvokeEvent(ThisInteractionModuleKey);
        _object.detector2.onPlayerInteractionObject += () => logic.localBus.InvokeEvent(PlayerInteractionObjectModuleKey);
        _object.detector2.onAnyObjectsNotDetected += () => logic.localBus.InvokeEvent(AnyObjectsNotDetectedObjectDetectedModuleKey);

        // Commands
        logic.localBus.AddCommandListener(PlayerScanCommandKey, StartPlayerScan);
        logic.localBus.AddCommandListener(ObjectScanCommandKey, StartObjectScan);
        logic.localBus.AddCommandListener(ObjectScanSquareCommandKey, StartObjectScanSquare);
        logic.localBus.AddCommandListener(ObjectScanRectangleCommandKey, StartObjectScanRectangle);
        logic.localBus.AddCommandListener(SoundScanCommandKey, StartSoundScan);
        logic.localBus.AddCommandListener(SoundScanOffsetCommandKey, StartSoundScanOffset);
        logic.localBus.AddCommandListener(StopScanningCommandKey, StopScanning);
        logic.localBus.AddCommandListener(PlayerObjectInteractionScanCommandKey, StartPlayerObjectInteractionScan);

        logic.localBus.AddVariableGetter(ObjectToTheRightVariableKey, () => _object.detector2.ObjectToTheRight());
        logic.localBus.AddVariableGetter(ObjectAheadVariableKey, () => _object.detector2.ObjectAhead());
        logic.localBus.AddVariableGetter(ObjectCodirectional, () => _object.detector2.ObjectCodirectional());
        logic.localBus.AddVariableGetter(ObjectCounterdirectional, () => _object.detector2.ObjectCounterdirectional());
        logic.localBus.AddVariableGetter(ObjectCloserToIntersection, () => _object.detector2.ObjectCloserToIntersection());
    }

    bool StartPlayerScan(List<Tuple<string, string>> values)
    {
        _object.detector2.StartPlayerScan(HSMUtils.GetValue<float>(values[0]));

        return true;
    }

    bool StartObjectScan(List<Tuple<string, string>> values)
    {
        _object.detector2.StartObjectScan(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]));

        return true;
    }

    bool StartObjectScanSquare(List<Tuple<string, string>> values)
    {
        _object.detector2.StartObjectScanSquare(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]),
            HSMUtils.GetValue<float>(values[2]),
            HSMUtils.GetValue<float>(values[3]));

        return true;
    }

    bool StartObjectScanRectangle(List<Tuple<string, string>> values)
    {
        float width = 0;
        float height = 0;
        float offsetX = 0;
        float offsetZ = 0;

        string widthHeight = HSMUtils.GetValue<string>(values[1]);
        string offsetXZ = HSMUtils.GetValue<string>(values[2]);
        if (TryParseFloatArray(widthHeight.Split(";"), out float[] widthHeightArray) && widthHeightArray.Length == 2)
        {
            width = widthHeightArray[0];
            height = widthHeightArray[1];
        }
        if (TryParseFloatArray(offsetXZ.Split(";"), out float[] offsetXZArray) && offsetXZArray.Length == 2)
        {
            offsetX = offsetXZArray[0];
            offsetZ = offsetXZArray[1];
        }

        _object.detector.StartObjectScanRectangle(
            HSMUtils.GetValue<string>(values[0]),
            width,
            height,
            offsetX,
            offsetZ
            );

        return true;
    }

    bool StartSoundScan(List<Tuple<string, string>> values)
    {
        _object.detector2.StartSoundScan(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]));

        return true;
    }

    bool StartSoundScanOffset(List<Tuple<string, string>> values)
    {
        _object.detector2.StartSoundScanOffset(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]),
            HSMUtils.GetValue<float>(values[2]),
            HSMUtils.GetValue<float>(values[3]));

        return true;
    }

    bool StopScanning(List<Tuple<string, string>> values)
    {
        _object.detector2.StopScanning();

        return true;
    }

    bool StartPlayerObjectInteractionScan(List<Tuple<string, string>> values)
    {
        _object.detector2.StartPlayerObjectInteractionScan(
            HSMUtils.GetValue<string>(values[0]),
            HSMUtils.GetValue<float>(values[1]));

        return true;
    }

    private static bool TryParseFloatArray(IList<string> strings, out float[] floats)
    {
        float[] result = new float[strings.Count];
        for (int i = 0; i < strings.Count; i++)
        {
            if (!float.TryParse(strings[i].StripEdges(), NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                floats = null;
                return false;
            }
            result[i] = value;
        }
        floats = result;
        return true;
    }
}
