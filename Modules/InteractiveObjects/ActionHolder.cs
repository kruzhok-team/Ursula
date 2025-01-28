using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public class ActionHolder
{
    public string guid { get; private set; }

    public ActionHolder()
    {
        guid = Guid.NewGuid().ToString();
    }

    private Action _action;

    public List<MethodInfo> registeredMethods { get; } = new();

    public void Invoke()
    {
        //GD.Print($"*** Invoke _action: {_action}");
        _action?.Invoke();
    }

    public void Subscribe(Action action)
    {
        _action += action;
        //GD.Print($"*** Subscribed and now _action: {_action} with {action}");
        registeredMethods.Add(action.Method);
    }

    public void ClearSubscriptions()
    {
        _action = null;
        registeredMethods.Clear();
    }

    public override string ToString()
    {
        return $"{guid} [{registeredMethods.Count}] : {_action}";
    }
}
