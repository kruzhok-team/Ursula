using Godot;
using System;
using System.Collections.Generic;

public class HSMMovementModule
{
    InteractiveObject _object;

    const string ModuleName = "��������������";

    // Event keys
    const string StuckMovingEventKey = $"{ModuleName}.����������������������";
    const string MovementFinishedEventKey = $"{ModuleName}.��������������������";
    const string CollisionEventKey = $"{ModuleName}.�������������������������";
    const string MovingDistanceFinishedEventKey = $"{ModuleName}.��������������������";
    const string AnimationCompletedEventKey = $"{ModuleName}.�������������������";
    const string AnimationCycleCompletedEventKey = $"{ModuleName}.����������������������";

    // Command keys
    const string MoveRandomCommandKey = $"{ModuleName}.�����������������";
    const string MoveToTargetCommandKey = $"{ModuleName}.�����������������";
    const string MoveFromTargetCommandKey = $"{ModuleName}.������������������";
    const string MoveToPositionCommandKey = $"{ModuleName}.����������������������";
    const string SetPositionCommandKey = $"{ModuleName}.����������������";
    const string SetPositionUpCommandKey = $"{ModuleName}.���������������������";
    const string SetPositionDownCommandKey = $"{ModuleName}.��������������������";
    const string SetPositionRightCommandKey = $"{ModuleName}.����������������������";
    const string SetPositionLeftCommandKey = $"{ModuleName}.���������������������";
    const string SetRandomPositionCommandKey = $"{ModuleName}.�������������������������";
    const string ResetCoordinatesCommandKey = $"{ModuleName}.��������������";
    const string StopMovingCommandKey = $"{ModuleName}.����";
    const string SetObjectNameCommandKey = $"{ModuleName}.����������������";
    const string SetMoveDistanceCommandKey = $"{ModuleName}.��������������������������";

    // Variable keys
    const string DistanceVariableKey = $"{ModuleName}.��������������������";

    public HSMMovementModule(CyberiadaLogic logic, InteractiveObject interactiveObject)
    {
        _object = interactiveObject;

        // Events
        if (_object.move.moveScript != null)
        {
            _object.move.moveScript.onStuckMoving += () => logic.localBus.InvokeEvent(StuckMovingEventKey);
            _object.move.moveScript.onMovementFinished += () => logic.localBus.InvokeEvent(MovementFinishedEventKey);
            _object.move.moveScript.onCollision += () => logic.localBus.InvokeEvent(CollisionEventKey);
            _object.move.moveScript.onMovingDistanceFinished += () => logic.localBus.InvokeEvent(MovingDistanceFinishedEventKey);
            _object.move.animationCompleted += () => logic.localBus.InvokeEvent(AnimationCompletedEventKey);
            _object.move.animationCompleted += () => logic.localBus.InvokeEvent(AnimationCycleCompletedEventKey);
        }
        else
            HSMLogger.PrintMoveScriptError(interactiveObject);

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
        _object.move.MoveToPosition();

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
