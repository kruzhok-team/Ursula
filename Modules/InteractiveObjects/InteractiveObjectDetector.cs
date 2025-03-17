using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;

public partial class InteractiveObjectDetector : Area3D
{
    public Node detectedObject; // заданныйОбъект

    private bool isScanning = false;

    private enum ScanType { Player, Object, Sound }
    private ScanType currentScanType; 
    private string targetObjectName;
    private int targetObjectNameHash;
    private string targetSoundName;
    private float scanRadius;  

    private float timeAccumulator = 0f; 
    private const float SCAN_INTERVAL = 0.25f;

    public GMLActionHolder onObjectDetected = new();
    public GMLActionHolder onPlayerDetected = new();
    public GMLActionHolder onSoundDetected = new();
    public GMLActionHolder onPlayerInteractionObject = new();

    public GMLActionHolder onAnyObjectsNotDetected = new();

    public string playerName = "Player";

    public object StartPlayerScan(float radius)
    {
        StartScanning(ScanType.Player, radius);

        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        targetObjectNameHash = objectName.GetHashCode();
        StartScanning(ScanType.Object, radius);

        return null;
    }

    public object StartPlayerObjectInteractionScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        targetObjectNameHash = objectName.GetHashCode();
        scanRadius = radius;
        GameManager.onPlayerInteractionObjectAction += PlayerInteractionObject;

        return null;
    }   

    public object StartSoundScan(string soundName, float radius)
    {
        targetSoundName = soundName;
        StartScanning(ScanType.Sound, radius);

        return null;
    }

    private void StartScanning(ScanType scanType, float radius)
    {
        isScanning = true;
        currentScanType = scanType;
        scanRadius = radius;
        GD.Print($"Scanning for {scanType} started...");
    }
    public object StopScanning()
    {
        isScanning = false;
        GD.Print("Scanning stopped.");
        return null;
    }

    public override void _Process(double delta)
    {
        if (isScanning)
        {
            timeAccumulator += (float)delta;

            if (timeAccumulator >= SCAN_INTERVAL)
            {
                timeAccumulator = 0f;
                PerformScan();
            }
        }
    }

    private void PerformScan()
    {
        switch (currentScanType)
        {
            case ScanType.Player:
                //detectedObject = FindNodeInRadius<Node>(scanRadius, node => node.Name == playerName);
                //if (detectedObject != null)
                //{
                //    //ContextMenu.ShowMessageS($"Модуль сканирования. {onPlayerDetected} Выполнен поиск игрока по радиусу {scanRadius} -> обнаружен игрок {detectedObject.Name}");
                //    onPlayerDetected.Invoke();
                //}
                //else
                //    onAnyObjectsNotDetected.Invoke();
                Node3D player = PlayerScript.instance as Node3D;
                if (player != null)
                {
                    float distance = GlobalTransform.Origin.DistanceSquaredTo(player.GlobalTransform.Origin);
                    if (distance < scanRadius * scanRadius)
                    {
                        detectedObject = player;
                        onPlayerDetected.Invoke();
                    }
                    else
                        onAnyObjectsNotDetected.Invoke();
                }
                break;
            case ScanType.Object:
                detectedObject = FindNodeInRadius<ItemPropsScript>(scanRadius, ips => ips.GameObjectSampleHash == targetObjectNameHash);
                if (detectedObject != null)
                {
                    //ContextMenu.ShowMessageS($"Модуль сканирования. {onObjectDetected} Выполнен поиск объекта по радиусу {scanRadius} -> обнаружен объект {targetObjectName}");
                    onObjectDetected.Invoke();
                }
                else
                    onAnyObjectsNotDetected.Invoke();
                break;
            case ScanType.Sound:
                detectedObject = FindNodeInRadius<InteractiveObjectAudio>(scanRadius, IOAudio => IOAudio.currentAudioKey == targetSoundName && IOAudio.isPlaying)?.GetParent();
                if (detectedObject != null)
                {
                    //ContextMenu.ShowMessageS($"Модуль сканирования. {onSoundDetected} Выполнен поиск звука по радиусу {scanRadius} -> обнаружен звук {targetSoundName}");
                    onSoundDetected.Invoke();
                }
                break;
        }
    }

    private T FindInRadius<T>(float radius, Func<T, bool> condition) where T : Node
    {
        var collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        if (collisionShape.Shape is SphereShape3D sphereShape)
        {
            sphereShape.Radius = radius;
        }

        var bodiesInArea = GetOverlappingBodies();

        foreach (var body in bodiesInArea)
        {
            if (body is T node && condition(node))
            {
                return node;
            }
        }

        return null;
    }

    private T FindNodeInRadius<T>(float radius, Func<T, bool> condition) where T : Node
    {
        Node root = GetTree().Root;

        //var nodes = GetAllNodes(root).ToList();

        var nodes = GetItemsNodes().ToList();

        if (typeof(T) == typeof(InteractiveObjectAudio))
        {
            foreach (Node node in nodes)
            {
                ItemPropsScript item = (ItemPropsScript)node;
                if (item != null)
                {
                    Node IOaudio = (Node)item.IO.audio;

                    if (IOaudio is T targetNode && condition(targetNode))
                    {
                        if (node is Node3D targetNode3D)
                        {
                            float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
                            if (distance <= radius * radius)
                            {
                                return targetNode;
                            }
                        }
                    }
                }
            }
        }
        else if (typeof(T) == typeof(ItemPropsScript))
        {
            foreach (Node node in nodes)
            {
                ItemPropsScript item = (ItemPropsScript)node;
                if (item == null) continue;

                if (item is T targetNode && condition(targetNode))
                {
                    if (node is Node3D targetNode3D)
                    {
                        float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
                        if (distance <= radius * radius)
                        {
                            return targetNode;
                        }
                    }
                }
            }
        }
        else
        {
            Node player = PlayerScript.instance as Node;
            if (player != null) nodes.Add(player);

            foreach (Node node in nodes)
            {
                if (node is T targetNode && condition(targetNode))
                {
                    if (targetNode is Node3D targetNode3D)
                    {
                        float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
                        if (distance <= radius * radius)
                        {
                            return targetNode;
                        }
                    }
                }
            }
        }

        //foreach (Node node in nodes)
        //{
        //    if (node is T targetNode && condition(targetNode))
        //    {
        //        if (targetNode is InteractiveObjectAudio IOAudio)
        //        {
        //            Node3D node3D = targetNode.GetParent() as Node3D;
        //            if (node3D != null)
        //            {
        //                float distance = GlobalTransform.Origin.DistanceSquaredTo(node3D.GlobalTransform.Origin);
        //                if (distance <= radius * radius) return targetNode;
        //            }
        //        }
        //        else if (targetNode is Node3D targetNode3D)
        //        {
        //            float distance = GlobalTransform.Origin.DistanceSquaredTo(targetNode3D.GlobalTransform.Origin);
        //            if (distance <= radius * radius)
        //            {
        //                return targetNode;
        //            }
        //        }
        //    }
        //}

        return null;
    }

    // Метод для получения всех узлов сцены
    private IEnumerable<Node> GetAllNodes(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            yield return child;

            // Рекурсивно проверяем детей
            foreach (Node grandChild in GetAllNodes(child))
            {
                yield return grandChild;
            }
        }
    }

    private IEnumerable<Node> GetItemsNodes()
    {
        foreach (ItemPropsScript ips in VoxLib.mapManager.gameItems)
        {
            Node node = (Node)ips; // (Node)ips.GetParent();
            yield return node;
        }
    }

    public void PlayerInteractionObject()
    {
        detectedObject = FindNodeInRadius<Node>(scanRadius, node => node.Name.ToString().Contains(targetObjectName));
        if (detectedObject != null)
        {
            //ContextMenu.ShowMessageS($"Модуль сканирования. {onObjectDetected} Выполнен поиск объекта по радиусу {scanRadius} -> обнаружен объект {targetObjectName}");
            onPlayerInteractionObject.Invoke();
        }
        else
            onAnyObjectsNotDetected.Invoke();
    }

    public override void _ExitTree()
    {
        GameManager.onPlayerInteractionObjectAction -= PlayerInteractionObject;
    }
}
