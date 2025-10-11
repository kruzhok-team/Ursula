using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InteractiveObjectsManager : Node
{
    #region Singleton

    public static InteractiveObjectsManager Instance { get; private set; }

    public static InteractiveObjectDetector detectorPrefab;
    public static InteractiveObjectAudio audioPrefab;
    public static InteractiveObjectMove movePrefab;
    public static InteractiveObjectTimer timerPrefab;
    public static InteractiveObjectCounter counter1Prefab;
    public static InteractiveObjectCounter counter2Prefab;

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr($"An instance of InteractiveObjectsManager already exists.");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion

    public List<InteractiveObject> objects = new();
    private Mutex mutex = new();

    public static void Register(InteractiveObject obj)
    {
        if (!Instance.objects.Contains(obj))
        {
            Instance.objects.Add(obj);
        }
    }

    public void RunAllObjects()
    {
        ForEach(o => o.StartAlgorithm());
    }

    public void StopAllObjects()
    {
        ForEach(o => o.StopAlgorithm());
    }

    public void RestartAllObjects()
    {
        ForEach(o => { o.ReloadAlgorithm(); o.StartAlgorithm(); });
    }

    private void ForEach(Action<InteractiveObject> action)
    {
        var snapshot = objects.ToArray();

        foreach (InteractiveObject obj in snapshot)
        {
            if (obj != null && IsInstanceValid(obj))
            {
                action.Invoke(obj);
            }
        }
    }

    public void RemoveObject(InteractiveObject obj)
    {
        mutex.Lock();
        if (objects.Contains(obj))
        {
            var parent = obj.GetParent();
            parent.QueueFree();
            objects.Remove(obj);
        }
        mutex.Unlock();
    }

    public void DuplicateObject(InteractiveObject obj)
    {
        mutex.Lock();
        
        var parent = obj.GetParent();
        Node duplicatedObject = parent.Duplicate();
        GetTree().CurrentScene.AddChild(duplicatedObject);
        
        var interactiveObject = duplicatedObject.GetChildren().OfType<InteractiveObject>().FirstOrDefault();
        if (interactiveObject != null)
        {
            interactiveObject.StartAlgorithm();
        }
        
        mutex.Unlock();
    }
}
