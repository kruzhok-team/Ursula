using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Setters;

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
    private MapManagerItemSetter _mapManagerItemSetter;
    private GameObjectLibraryManager _gameObjectLibraryManager;
    private GameObjectCollectionModel _gameObjectCollectionModel;
    private GameObjectCreateItemsModel _gameObjectCreateItemsModel;
        
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
            obj.StopAlgorithm();
            var parent = obj.GetParent();
            var itemPropsScript = parent.GetChildren().OfType<ItemPropsScript>().FirstOrDefault();
            VoxLib.mapManager.spatialGrid.Remove(itemPropsScript);
            parent.QueueFree();
            objects.Remove(obj);
        }
        mutex.Unlock();
    }

    public void DuplicateObject(InteractiveObject obj)
    {
        mutex.Lock();

        _mapManagerItemSetter = VoxLib.mapManager._mapManagerItemSetter;
        _gameObjectCollectionModel = VoxLib.mapManager._gameObjectCollectionModel;
        _gameObjectCreateItemsModel = VoxLib.mapManager._gameObjectCreateItemsModel;
        _gameObjectLibraryManager = VoxLib.mapManager._gameObjectLibraryManager;
        
        var parent = obj.GetParent();
        var itemPropsScript = parent.GetChildren().OfType<ItemPropsScript>().FirstOrDefault();
        var assetInfo = _gameObjectLibraryManager.GetItemInfo(itemPropsScript.AssetInfoId);
        
        
        Node3D parentNode3D = parent as Node3D;
        Vector3 position = parentNode3D.GlobalPosition + new Vector3(2, 0, 0);
        VoxLib.mapManager.spatialGrid.Add(itemPropsScript, position);

        _gameObjectCollectionModel.SetGameObjectAssetSelected(assetInfo);
        _gameObjectCreateItemsModel.SetGameObjectCreateItem(position, 1f, 0, false);
                
        Vector3 positionNode = _gameObjectCreateItemsModel.PositionNode;
        float scaleNode = _gameObjectCreateItemsModel.ScaleNode;
        byte rotationNode = _gameObjectCreateItemsModel.RotationNode;

        int _x = Mathf.RoundToInt(positionNode.X);
        int _y = Mathf.RoundToInt(positionNode.Y);
        int _z = Mathf.RoundToInt(positionNode.Z);

        int id = _x + _y * 256 + _z * 256 * 256;
        
        Node item = _mapManagerItemSetter.CreateGameItem(assetInfo, rotationNode, scaleNode, positionNode.X, positionNode.Y, positionNode.Z, 0, id, false, null, itemPropsScript.LogicInjector);
        
        var interactiveObject = item.GetChildren().OfType<InteractiveObject>().FirstOrDefault();
        if (interactiveObject != null)
        {
            interactiveObject.audio.audios = obj.audio.audios.ToDictionary(entry => entry.Key, entry => entry.Value);
            interactiveObject.StartAlgorithm();
        }
        
        mutex.Unlock();
    }
}
