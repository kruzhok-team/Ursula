using System;
using System.Collections.Generic;

public class HSMModelsModule
{
    InteractiveObject _object;
    const string ModuleName = "МодульМоделей";
    
    const string ChangeModelCommandKey = $"{ModuleName}.ИзменитьМодель";
    
    public HSMModelsModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        logic.localBus.AddCommandListener(ChangeModelCommandKey, ChangeModel);
    }

    bool ChangeModel(List<Tuple<string, string>> value)
    {
        _object.models.ChangeModel(HSMUtils.GetValue<string>(value[0]));

        return true;
    }
}
