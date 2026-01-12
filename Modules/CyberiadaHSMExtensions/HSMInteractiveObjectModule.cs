using System;
using System.Collections.Generic;
using Godot;

public class HSMInteractiveObjectModule
{
    InteractiveObject _object;

    const string ModuleName = "МодульИнтерактивныхОбъектов";

    // Command keys
    const string DuplicateObjectCommandKey = $"{ModuleName}.ДублироватьОбъект";
    const string BornObjectCommandKey = $"{ModuleName}.Рождение";
    const string RemoveCurrentObjectCommandKey = $"{ModuleName}.УдалитьЭтотОбъект";
    const string RemoveFoundedObjectCommandKey = $"{ModuleName}.УдалитьНайденныйОбъект";
    const string RemoveFoundedObject2CommandKey = $"{ModuleName}.УдалитьНайденныйОбъект2";
    
    public HSMInteractiveObjectModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Commands
        logic.localBus.AddCommandListener(DuplicateObjectCommandKey, DuplicateObject);
        logic.localBus.AddCommandListener(BornObjectCommandKey, BornObject);
        logic.localBus.AddCommandListener(RemoveCurrentObjectCommandKey, RemoveCurrentObject);
        logic.localBus.AddCommandListener(RemoveFoundedObjectCommandKey, RemoveFoundedObject);
        logic.localBus.AddCommandListener(RemoveFoundedObject2CommandKey, RemoveFounded2Object);
    }
    
    bool BornObject(List<Tuple<string, string>> value)
    {
        Random random = new Random();
        
        int num_child = HSMUtils.GetValue<int>(value[0]);
        for (int i = 0; i < num_child; i++)
        {
            int randomNumber = random.Next(0, 2);
            if (randomNumber == 0)
            {
                InteractiveObjectsManager.Instance.DuplicateObject(_object);
            }
            else
            {
                var target = _object.GetCurrentTargetObject2();
                var interactiveObject = GetChildByType<InteractiveObject>(target.GetParent());
                InteractiveObjectsManager.Instance.DuplicateObject(interactiveObject);
            }
        }
        
        return true;
    }
    
    bool DuplicateObject(List<Tuple<string, string>> value)
    {
        InteractiveObjectsManager.Instance.DuplicateObject(_object);
        return true;
    }

    bool RemoveCurrentObject(List<Tuple<string, string>> value)
    {
        InteractiveObjectsManager.Instance.RemoveObject(_object);
        return true;
    }
    
    bool RemoveFoundedObject(List<Tuple<string, string>> value)
    {
        var target = _object.GetCurrentTargetObject();
        if (target != null)
        {
            var interactiveObject = GetChildByType<InteractiveObject>(target.GetParent());
            InteractiveObjectsManager.Instance.RemoveObject(interactiveObject);
        }
        return true;
    }
    
    bool RemoveFounded2Object(List<Tuple<string, string>> value)
    {
        var target = _object.GetCurrentTargetObject2();
        if (target != null)
        {
            var interactiveObject = GetChildByType<InteractiveObject>(target.GetParent());
            InteractiveObjectsManager.Instance.RemoveObject(interactiveObject);
        }
        return true;
    }
    
    private T GetChildByType<T>(Node parent) where T : Node
    {
        foreach (Node child in parent.GetChildren())
        {
            if (child is T typedChild)
            {
                return typedChild;
            }
        }
        return null;
    }
}
