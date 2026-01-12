using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using Talent.Logic.HSM;

public class HSMLogger
{
    private readonly static int timerInterval = 1000;
    private static Timer timer;
    private static bool timerInitialized = false;
   
    private static bool compressionEnabled = false;
    public static bool CompressionEnabled
    {
        get => compressionEnabled;
        set
        {
            compressionEnabled = value;
            if (value)
            {
                if (!timerInitialized) 
                    InitTimer();
                timer.Enabled = true;
            }
            else
            {
                timer.Enabled = false;
            }
        }
    }

    private static Dictionary<InteractiveObject, string> parentNameCache = new Dictionary<InteractiveObject, string>();
    private static ConcurrentDictionary<string, int> compressedMessages = new ConcurrentDictionary<string, int>();

    InteractiveObject _interactiveObject;

    private static void InitTimer()
    {
        timer = new Timer(timerInterval);
        timer.Elapsed += OnTimerElapsed;
        timer.AutoReset = true;
        timer.Start();
    }

    public HSMLogger(InteractiveObject interactiveObject)
    {
        _interactiveObject = interactiveObject;
    }

    public static void Print(InteractiveObject senderInteractiveObject, string message)
    {
        string prefix = GetPrefixClear(senderInteractiveObject);
        ShowMessageS(prefix, message);
        //ContextMenu.ShowMessageS($"{GetPrefixClear(senderInteractiveObject)} {message}");
    }

    public static void PrintMoveScriptError(InteractiveObject interactiveObject)
    {
        HSMLogger.Print(interactiveObject, "Объект установлен статичным, обработка функций перемещения отключена");
    }

    public static string GetPrefixClear(InteractiveObject interactiveObject)
    {
        return $"[HSM {GetParentName(interactiveObject)}]";
    }

    public static string GetPrefix(InteractiveObject interactiveObject, string stateLabel)
    {
        return $"[HSM {GetParentName(interactiveObject)} | {stateLabel}]";
    }

    public string GetPrefix(string stateLabel)
    {
        return GetPrefix(_interactiveObject, stateLabel);
    }

    public void OnStateEnter(object? sender, EventArgs args)
    {
        if (sender is State state)
        {
            string prefix = GetPrefix(state.Label);
            string message = "Выполнен вход в состояние";
            ShowMessageS(prefix, message);
            //ContextMenu.ShowMessageS($"{prefix} Выполнен вход в состояние");
        }
    }

    public void OnStateExit(object? sender, EventArgs args)
    {
        if (sender is State state)
        {
            string prefix = GetPrefix(state.Label);
            string message = "Выполнен выход из состояния";
            ShowMessageS(prefix, message);
            //ContextMenu.ShowMessageS($"{prefix} Выполнен выход из состояния");
        }
    }

    public void OnTransitionTriggered(object? sender, Transition transition)
    {
        if (sender is State state)
        {
            string prefix = GetPrefix(state.Label);
            string message = $"Вызван переход по событию {transition.EventName}";
            ShowMessageS(prefix, message);
            //ContextMenu.ShowMessageS($"{prefix} Вызван переход по событию {transition.EventName}");
        }
    }

    public void OnCommandMaked(object? sender, Command command)
    {
        var parameters = string.Join(", ", command.GetParameters().Select(t => t.Item2));

        if (sender is State state)
        {
            string prefix = GetPrefix(state.Label);
            string message = $"В состоянии выполнена команда {command.GetCommandName()}({parameters})";
            ShowMessageS(prefix, message);
            //ContextMenu.ShowMessageS($"{prefix} В состоянии выполнена команда {command.GetCommandName()}({parameters})");
        }

        if (sender is Transition transition)
        {
            string prefix = GetPrefix(transition.EventName);
            string message = $"В переходе выполнена команда {command.GetCommandName()}({parameters})";
            ShowMessageS(prefix, message);
            //ContextMenu.ShowMessageS($"{prefix} В переходе выполнена команда {command.GetCommandName()}({parameters})");
        }
    }

    public void OnConditionCheck(object? sender, ConditionEventArgs args)
    {
        if (sender is Transition transition)
        {
            string prefix = GetPrefix(transition.EventName);
            string message;
            if (compressionEnabled)
                message = $"В переходе вызвана проверка условия ({args.leftParameter.Key}) {args.CompareSymbol} {args.rightParameter.Value} ({args.rightParameter.Key})  {args.Result}";
            else
                message = $"В переходе вызвана проверка условия {args.leftParameter.Value} ({args.leftParameter.Key}) {args.CompareSymbol} {args.rightParameter.Value} ({args.rightParameter.Key})  {args.Result}";
            ShowMessageS(prefix, message);
            //ContextMenu.ShowMessageS($"{prefix} В переходе вызвана проверка условия {args.leftParameter.Value} ({args.leftParameter.Key}) {args.CompareSymbol} {args.rightParameter.Value} ({args.rightParameter.Key})  {args.Result}");
        }
    }

    private static void ShowMessageS(string prefix, string message)
    {
        if (CompressionEnabled)
            CompressMessage(message);
        else
            ContextMenu.ShowMessageS($"{prefix} {message}");
    }

    private static void CompressMessage(string message)
    {
        if (compressedMessages.TryGetValue(message, out int num))
        {
            compressedMessages[message] = num + 1;
        }
        else
        {
            compressedMessages[message] = 1;
        }
    }

    private static void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        PrintCompressedMessages();
    }

    private static void PrintCompressedMessages()
    {
        if (compressedMessages.Count == 0)
            return;

        foreach ((string msg, int count) in compressedMessages)
        {
            ContextMenu.ShowMessageS($"[{count} объектов] {msg}");
        }

        compressedMessages.Clear();
    }

    private static string GetParentName(InteractiveObject interactiveObject)
    {
        if (parentNameCache.TryGetValue(interactiveObject, out string parentName))
        {
            return parentName;
        }
        else
        {
            parentName = interactiveObject.GetParent().Name;
            parentNameCache[interactiveObject] = parentName;
            return parentName;
        }
    }
}
