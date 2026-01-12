using Godot;
using System.Collections.Generic;
using System;

public partial class HSMInitializationModule : Node
{
    InteractiveObject _object;

    const string ModuleName = "МодульИнициализации";

    // Command keys
    const string SetPosPointCommandKey = $"{ModuleName}.ТелепортироватьПоКоординатам";
    const string SetPosLineCommandKey = $"{ModuleName}.ТелепортироватьМеждуТочками";
    const string SetPosAreaCommandKey = $"{ModuleName}.ТелепортироватьВОбласть";
    const string SetRotationLookAtCommandKey = $"{ModuleName}.ПовернутьВНаправлении";

    public HSMInitializationModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Commands
        logic.localBus.AddCommandListener(SetPosPointCommandKey, SetPosPoint);
        logic.localBus.AddCommandListener(SetPosLineCommandKey, SetPosLine);
        logic.localBus.AddCommandListener(SetPosAreaCommandKey, SetPosArea);
        logic.localBus.AddCommandListener(SetRotationLookAtCommandKey, SetRotationLookAt);
    }

    bool SetPosPoint(List<Tuple<string, string>> value)
    {
        float x = HSMUtils.GetValue<float>(value[0]);
        float z = HSMUtils.GetValue<float>(value[1]);
        _object.initialization.SetPosPoint(x, z);

        return true;
    }

    bool SetPosLine(List<Tuple<string, string>> value)
    {
        float x1 = HSMUtils.GetValue<float>(value[0]);
        float z1 = HSMUtils.GetValue<float>(value[1]);
        float x2 = HSMUtils.GetValue<float>(value[2]);
        float z2 = HSMUtils.GetValue<float>(value[3]);
        _object.initialization.SetPosLine(x1, z1, x2, z2);

        return true;
    }

    bool SetPosArea(List<Tuple<string, string>> value)
    {
        float x1 = HSMUtils.GetValue<float>(value[0]);
        float z1 = HSMUtils.GetValue<float>(value[1]);
        float x2 = HSMUtils.GetValue<float>(value[2]);
        float z2 = HSMUtils.GetValue<float>(value[3]);
        _object.initialization.SetPosArea(x1, z1, x2, z2);

        return true;
    }

    bool SetRotationLookAt(List<Tuple<string, string>> value)
    {
        float x = HSMUtils.GetValue<float>(value[0]);
        float z = HSMUtils.GetValue<float>(value[1]);
        _object.initialization.SetRotationLookAt(x, z);

        return true;
    }
}

