using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Talent.Logic.HSM;

public class HSMLogger
{
    InteractiveObject _interactiveObject;

    public HSMLogger(InteractiveObject interactiveObject)
    {
        _interactiveObject = interactiveObject;
    }

    public static void Print(InteractiveObject senderInteractiveObject, string message)
    {
        ContextMenu.ShowMessageS($"{GetPrefix(senderInteractiveObject, "-")} {message}");
    }

    public static void PrintMoveScriptError(InteractiveObject interactiveObject)
    {
        HSMLogger.Print(interactiveObject, "Объект установлен статичным, обработка функций перемещения отключена");
    }

    public static string GetPrefix(InteractiveObject interactiveObject, string stateLabel)
    {
        return $"[HSM {interactiveObject.GetParent().Name} | {stateLabel}]";
    }

    public string GetPrefix(string stateLabel)
    {
        return GetPrefix(_interactiveObject, stateLabel);
    }

    public void OnStateEnter(object? sender, EventArgs args)
    {
        if (sender is State state)
        {
            var prefix = GetPrefix(state.Label);
            ContextMenu.ShowMessageS($"{prefix} Выполнен вход в состояние");
        }
    }

    public void OnStateExit(object? sender, EventArgs args)
    {
        if (sender is State state)
        {
            var prefix = GetPrefix(state.Label);
            ContextMenu.ShowMessageS($"{prefix} Выполнен выход из состояния");
        }
    }

    public void OnTransitionTriggered(object? sender, Transition transition)
    {
        if (sender is State state)
        {
            var prefix = GetPrefix(state.Label);
            ContextMenu.ShowMessageS($"{prefix} Вызван переход по событию {transition.EventName}");
        }
    }

    public void OnCommandMaked(object? sender, Command command)
    {
        var parameters = string.Join(", ", command.GetParameters().Select(t => t.Item2));

        if (sender is State state)
        {
            var prefix = GetPrefix(state.Label);
            ContextMenu.ShowMessageS($"{prefix} В состоянии выполнена команда {command.GetCommandName()}({parameters})");
        }

        if (sender is Transition transition)
        {
            var prefix = GetPrefix(transition.EventName);

            ContextMenu.ShowMessageS($"{prefix} В переходе выполнена команда {command.GetCommandName()}({parameters})");
        }
    }

    public void OnConditionCheck(object? sender, ConditionEventArgs args)
    {
        if (sender is Transition transition)
        {
            var prefix = GetPrefix(transition.EventName);
            ContextMenu.ShowMessageS($"{prefix} В переходе вызвана проверка условия {args.leftParameter.Value} ({args.leftParameter.Key}) {args.CompareSymbol} {args.rightParameter.Value} ({args.rightParameter.Key})  {args.Result}");
        }
    }
}
