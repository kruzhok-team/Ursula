using System;
using System.Collections.Generic;
using Godot;

public class HSMInteractiveObjectModule
{
    InteractiveObject _object;

    const string ModuleName = "МодульДляРаботыСИнтерактивнымОбъектами"; //Вероятно надо изменить

    // Command keys
    const string DuplicateObjectCommandKey = $"{ModuleName}.ДублироватьОбъект";
    const string RemoveObjectCommandKey = $"{ModuleName}.УдалитьОбъект";

    public HSMInteractiveObjectModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Commands
        logic.localBus.AddCommandListener(DuplicateObjectCommandKey, DuplicateObject);
        logic.localBus.AddCommandListener(RemoveObjectCommandKey, RemoveObject);
    }


    bool DuplicateObject(List<Tuple<string, string>> value)
    {
        InteractiveObjectsManager.Instance.DuplicateObject(_object);
        return true;
    }

    bool RemoveObject(List<Tuple<string, string>> value)
    {
        InteractiveObjectsManager.Instance.RemoveObject(_object);
        return true;
    }
}
