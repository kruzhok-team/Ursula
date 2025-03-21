using System;
using System.Collections.Generic;

public class HSMAnimationModule
{
    InteractiveObject _object;

    const string ModuleName = "��������������";

    // Event keys
    const string AnimationCompletedModuleKey = $"{ModuleName}.�������������������";
    const string AnimationCycleCompletedModuleKey = $"{ModuleName}.����������������������";

    // Command keys
    const string PlayAnimationCommandKey = $"{ModuleName}.���������������������";
    const string PlayRandomAnimationCommandKey = $"{ModuleName}.������������������������������";
    const string StopAnimationCommandKey = $"{ModuleName}.������������������";

    public HSMAnimationModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.move.animationCompleted += () => logic.localBus.InvokeEvent(AnimationCompletedModuleKey);
        _object.move.animationCompleted += () => logic.localBus.InvokeEvent(AnimationCycleCompletedModuleKey);

        // Commands
        logic.localBus.AddCommandListener(PlayAnimationCommandKey, PlayAnimation);
        logic.localBus.AddCommandListener(PlayRandomAnimationCommandKey, PlayRandomAnimation);
        logic.localBus.AddCommandListener(StopAnimationCommandKey, StopAnimation);
    }


    bool PlayAnimation(List<Tuple<string, string>> value)
    {
        _object.move.PlayAnimation(
            HSMUtils.GetValue<string>(value[0]), 
            HSMUtils.GetValue<int>(value[1]));

        return true;
    }

    bool PlayRandomAnimation(List<Tuple<string, string>> value)
    {
        _object.move.PlayRandomAnimation(HSMUtils.GetValue<int>(value[0]));

        return true;
    }

    bool StopAnimation(List<Tuple<string, string>> value)
    {
        _object.move.StopAnimation();

        return true;
    }
}
