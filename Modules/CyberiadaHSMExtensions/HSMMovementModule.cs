using Godot;
using System;
using System.Collections.Generic;

public class HSMMovementModule
{
    InteractiveObject _object;

    const string ModuleName = "МодульДвижения";

    // Event keys
    const string StuckMovingEventKey = $"{ModuleName}.ЗастреваниеПриДвижении";
    const string MovementFinishedEventKey = $"{ModuleName}.ПеремещениеВыполнено";
    const string CollisionEventKey = $"{ModuleName}.СтолкновениеСПрепятствием";
    const string MovingDistanceFinishedEventKey = $"{ModuleName}.ПеремещениеВыполнено";
    const string AnimationCompletedEventKey = $"{ModuleName}.АнимацияЗавершилась";
    const string AnimationCycleCompletedEventKey = $"{ModuleName}.ЦиклАнимацийЗавершился";

    // Command keys
    const string MoveRandomCommandKey = $"{ModuleName}.ДвигатьсяСлучайно";
    const string MoveToTargetCommandKey = $"{ModuleName}.ДвигатьсяКОбъекту";
    const string MoveFromTargetCommandKey = $"{ModuleName}.ДвигатьсяОтОбъекта";
    const string MoveToPositionCommandKey = $"{ModuleName}.ДвигатьсяПоКоординатам";
    const string SetPositionCommandKey = $"{ModuleName}.ЗадатьКоординаты";
    const string SetPositionUpCommandKey = $"{ModuleName}.ЗадатьКоординатуВверх";
    const string SetPositionDownCommandKey = $"{ModuleName}.ЗадатьКоординатуВниз";
    const string SetPositionRightCommandKey = $"{ModuleName}.ЗадатьКоординатуВправо";
    const string SetPositionLeftCommandKey = $"{ModuleName}.ЗадатьКоординатуВлево";
    const string SetRandomPositionCommandKey = $"{ModuleName}.ЗадатьСлучайныеКоординаты";
    const string ResetCoordinatesCommandKey = $"{ModuleName}.СбросКоординат";
    const string StopMovingCommandKey = $"{ModuleName}.Стоп";
    const string SetObjectNameCommandKey = $"{ModuleName}.ЗадатьИмяОбъекта";
    const string SetMoveDistanceCommandKey = $"{ModuleName}.ЗадатьПройденноеРасстояние";

    // Variable keys
    const string DistanceVariableKey = $"{ModuleName}.ПройденноеРасстояние";

    public HSMMovementModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        _object.move.moveScript.onStuckMoving += () => logic.localBus.InvokeEvent(StuckMovingEventKey);
        _object.move.moveScript.onMovementFinished += () => logic.localBus.InvokeEvent(MovementFinishedEventKey);
        _object.move.moveScript.onCollision += () => logic.localBus.InvokeEvent(CollisionEventKey);
        _object.move.moveScript.onMovingDistanceFinished += () => logic.localBus.InvokeEvent(MovingDistanceFinishedEventKey);
        _object.move.animationCompleted += () => logic.localBus.InvokeEvent(AnimationCompletedEventKey);
        _object.move.animationCompleted += () => logic.localBus.InvokeEvent(AnimationCycleCompletedEventKey);

        // Commands
        logic.localBus.AddCommandListener(MoveRandomCommandKey, MoveToRandom);
        logic.localBus.AddCommandListener(MoveToTargetCommandKey, MoveToTarget);
        logic.localBus.AddCommandListener(MoveFromTargetCommandKey, MoveFromTarget);
        logic.localBus.AddCommandListener(MoveToPositionCommandKey, MoveToPosition);
        logic.localBus.AddCommandListener(SetPositionCommandKey, SetPosition);
        logic.localBus.AddCommandListener(SetPositionUpCommandKey, SetPositionUp);
        logic.localBus.AddCommandListener(SetPositionDownCommandKey, SetPositionDown);
        logic.localBus.AddCommandListener(SetPositionRightCommandKey, SetPositionRight);
        logic.localBus.AddCommandListener(SetPositionLeftCommandKey, SetPositionLeft);
        logic.localBus.AddCommandListener(SetRandomPositionCommandKey, SetRandomPosition);
        logic.localBus.AddCommandListener(ResetCoordinatesCommandKey, ResetCoordinates);
        logic.localBus.AddCommandListener(StopMovingCommandKey, StopMoving);
        logic.localBus.AddCommandListener(SetObjectNameCommandKey, SetObjectName);
        logic.localBus.AddCommandListener(SetMoveDistanceCommandKey, SetMoveDistance);

        // Variables
        logic.localBus.AddVariableGetter(DistanceVariableKey, () => _object.move.moveDistance.Value);
    }


    bool MoveToRandom(List<Tuple<string, string>> value)
    {
        _object.move.MoveToRandom();

        return true;
    }

    bool MoveToTarget(List<Tuple<string, string>> value)
    {
        _object.move.MoveToTarget();

        return true;
    }

    bool MoveFromTarget(List<Tuple<string, string>> value)
    {
        _object.move.MoveFromTarget();

        return true;
    }

    bool MoveToPosition(List<Tuple<string, string>> value)
    {
        _object.move.MoveFromTarget();

        return true;
    }

    bool SetPosition(List<Tuple<string, string>> value)
    {
        _object.move.SetPosition(
            HSMUtils.GetValue<float>(value[0]),
            HSMUtils.GetValue<float>(value[1]));

        return true;
    }

    bool SetPositionUp(List<Tuple<string, string>> value)
    {
        _object.move.movePosition.X = HSMUtils.GetValue<float>(value[0]);

        return true;
    }

    bool SetPositionDown(List<Tuple<string, string>> value)
    {
        _object.move.movePosition.X = -HSMUtils.GetValue<float>(value[0]);

        return true;
    }

    bool SetPositionRight(List<Tuple<string, string>> value)
    {
        _object.move.SetPositionRight(
            HSMUtils.GetValue<float>(value[0]));

        return true;
    }

    bool SetPositionLeft(List<Tuple<string, string>> value)
    {
        _object.move.SetPositionLeft(
            HSMUtils.GetValue<float>(value[0]));

        return true;
    }

    bool SetRandomPosition(List<Tuple<string, string>> value)
    {
        _object.move.SetRandomPosition(
            HSMUtils.GetValue<float>(value[0]));

        return true;
    }

    bool ResetCoordinates(List<Tuple<string, string>> value)
    {
        _object.move.ResetCoordinates();

        return true;
    }

    bool StopMoving(List<Tuple<string, string>> value)
    {
        _object.move.StopMoving();

        return true;
    }

    bool SetObjectName(List<Tuple<string, string>> value)
    {
        _object.selectedObjectName = HSMUtils.GetValue<string>(value[0]);

        return true;
    }

    bool SetMoveDistance(List<Tuple<string, string>> value)
    {
        _object.move.SetMoveDistance(
            HSMUtils.GetValue<float>(value[0]));

        return true;
    }
}
