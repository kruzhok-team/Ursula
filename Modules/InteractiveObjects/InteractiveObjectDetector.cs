using bearloga.addons.Ursula.Modules.InteractiveObjects.DetectorShapes.DetectorShapeVisualiation;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class InteractiveObjectDetector : Node
{
    public static bool IsDrawDebug = false;

    private bool prevDrawDebug;

    public Node detectedObject; // заданныйОбъект
    public Node previousDetectedObject;

    private bool isScanning = false;

    private Action scanAction;
    private string targetObjectName;
    private string targetSoundName;
    private IDetectorShape detectorShape;

    private DetectorShapeVisualization visualization = new DetectorShapeVisualization();

    private float timeAccumulator = 0f;
    private const float SCAN_INTERVAL = 0.25f;

    public Action onObjectDetected;
    public Action onPlayerDetected;
    public Action onSoundDetected;
    public Action onPlayerInteractionObject;

    public Action onAnyObjectsNotDetected;

    public string playerName = "Player";

    private MoveScript moveScriptCache;
    private static Dictionary<Node3D, MoveScript> moveScriptMap = new Dictionary<Node3D, MoveScript>();
    private static Dictionary<Node3D, Vector3> staticObjectPositionMap = new Dictionary<Node3D, Vector3>();

    public MoveScript moveScript
    {
        get
        {
            if (moveScriptCache == null)
            {
                var moveScript = GetParent() as MoveScript;
                moveScriptCache = moveScript;
            }
            return moveScriptCache;
        }
    }

    public bool ObjectToTheRight()
    {
        Node3D detectedObject3D = detectedObject as Node3D;
        if (detectedObject3D == null)
            return false;

        Node3D parent = GetParent() as Node3D;
        if (parent == null)
            return false;

        Vector3 detectedPos = detectedObject3D.GlobalPosition;
        Vector3 pos = moveScript.GlobalPosition;
        Vector3 forward = parent.Quaternion * Vector3.Forward;
        Vector3 right = Vector3.Up.Cross(forward);
        float cross = right.Dot(detectedPos - pos);
        if (cross > 0.7f) // угол < 45 градусов
            return true;
        return false;
    }

    public bool ObjectAhead()
    {
        Node3D detectedObject3D = detectedObject as Node3D;
        if (detectedObject3D == null)
            return false;

        Node3D parent = GetParent() as Node3D;
        if (parent == null)
            return false;

        Vector3 detectedPos = detectedObject3D.GlobalPosition;
        Vector3 pos = moveScript.GlobalPosition;
        Vector3 forward = parent.Quaternion * -Vector3.Forward;
        float dot = forward.Dot(detectedPos - pos);
        if (dot > 0.7f) // угол < 45 градусов
            return true;
        return false;
    }

    public bool ObjectCodirectional()
    {
        if (detectedObject == null)
            return false;

        Node3D detectedObject3D = detectedObject.GetParent() as Node3D;
        if (detectedObject3D == null)
            return false;

        Node3D parent = GetParent() as Node3D;
        if (parent == null)
            return false;

        Vector3 detectedForward = detectedObject3D.Quaternion * Vector3.Forward;
        Vector3 forward = parent.Quaternion * Vector3.Forward;

        float dot = forward.Dot(detectedForward);
        if (dot > 0.7f) // угол < 45 градусов
            return true;
        return false;
    }

    public bool ObjectCounterdirectional()
    {
        if (detectedObject == null)
            return false;

        Node3D detectedObject3D = detectedObject.GetParent() as Node3D;
        if (detectedObject3D == null)
            return false;

        Node3D parent = GetParent() as Node3D;
        if (parent == null)
            return false;

        Vector3 detectedForward = detectedObject3D.Quaternion * Vector3.Forward;
        Vector3 back = parent.Quaternion * Vector3.Back;

        float dot = back.Dot(detectedForward);
        if (dot > 0.7f) // угол < 45 градусов
            return true;
        return false;
    }

    public bool ObjectCloserToIntersection()
    {
        if (detectedObject == null)
            return false;

        Node3D detectedObject3D = detectedObject.GetParent() as Node3D;
        if (detectedObject3D == null)
            return false;

        Node3D parent = GetParent() as Node3D;
        if (parent == null)
            return false;

        Vector3 detectedForward = detectedObject3D.Quaternion * Vector3.Forward;
        detectedForward.Y = 0;
        Vector3 detectedPos = detectedObject3D.GlobalPosition;
        detectedPos.Y = 0;

        Vector3 forward = parent.Quaternion * Vector3.Forward;
        forward.Y = 0;
        Vector3 pos = parent.GlobalPosition;
        pos.Y = 0;

        float det = forward.X * detectedForward.Z - forward.Z * detectedForward.X;
        if (Mathf.Abs(det) < Mathf.Epsilon)
            return false; // Параллельны

        Vector3 diff = detectedPos - pos;
        float t = (diff.X * detectedForward.Z - diff.Z * detectedForward.X) / det;
        Vector3 intersection = pos + forward * t;

        float dist1 = pos.DistanceSquaredTo(intersection);
        float dist2 = detectedPos.DistanceSquaredTo(intersection);

        return dist2 < dist1;
    }

    public bool ObjectIndexBigger()
    {
        if (detectedObject == null)
            return false;

        ulong detectedIdx = detectedObject.GetInstanceId();
        ulong idx = GetInstanceId();
        if (detectedIdx > idx)
            return true;
        return false;
    }

    public override void _Ready()
    {
        CSharpBridgeRegistry.Process += CSProcess;
        Random rnd = new Random();
        timeAccumulator = rnd.NextSingle() * SCAN_INTERVAL;
    }

    public object StartPlayerScan(float radius)
    {
        StartScanning();
        scanAction += FindPlayer;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        DrawDebug();
        return null;
    }

    public object StartObjectScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        StartScanning();
        scanAction += FindObject;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        DrawDebug();
        return null;
    }

    public object StartObjectScanSquare(string objectName, float width, float offsetX, float offsetZ)
    {
        targetObjectName = objectName;
        StartScanning();
        scanAction += FindObject;
        detectorShape = new RectangleDetectorShape(moveScript, width, width, new Vector3(offsetX, 0, offsetZ));
        DrawDebug();
        return null;
    }

    public object StartObjectScanRectangle(string objectName, float width, float heihgt, float offsetX, float offsetZ)
    {
        targetObjectName = objectName;
        StartScanning();
        scanAction += FindObject;
        detectorShape = new RectangleDetectorShape(moveScript, width, heihgt, new Vector3(offsetX, 0, offsetZ));
        DrawDebug();
        return null;
    }

    public object StartPlayerObjectInteractionScan(string objectName, float radius)
    {
        targetObjectName = objectName;
        GameManager.onPlayerInteractionObjectAction += PlayerInteractionObject;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        DrawDebug();
        return null;
    }

    public object StartSoundScan(string soundName, float radius)
    {
        targetSoundName = soundName;
        StartScanning();
        scanAction += FindSound;
        detectorShape = new SphereDetectorShape(moveScript, radius, Vector3.Zero);
        DrawDebug();
        return null;
    }

    public object StartSoundScanOffset(string soundName, float radius, float offsetX, float offsetZ)
    {
        targetSoundName = soundName;
        StartScanning();
        scanAction += FindSound;
        detectorShape = new SphereDetectorShape(moveScript, radius, new Vector3(offsetX, 0, offsetZ));
        DrawDebug();
        return null;
    }

    private void StartScanning()
    {
        isScanning = true;
        //GD.Print($"Scanning started...");
    }

    public object StopScanning()
    {
        isScanning = false;
        visualization.Hide();
        //GD.Print("Scanning stopped.");
        return null;
    }

    public void CSProcess(double delta)
    {
        if (prevDrawDebug != IsDrawDebug)
        {
            prevDrawDebug = IsDrawDebug;
            DrawDebug();
        }
        if (isScanning)
        {
            timeAccumulator += (float)delta;

            if (timeAccumulator >= SCAN_INTERVAL)
            {
                timeAccumulator = 0f;
                scanAction();
            }
        }
    }

    private IEnumerable<Node> GetItemsNodes()
    {
        foreach (ItemPropsScript ips in VoxLib.mapManager.gameItems)
        {
            yield return ips;
        }
    }

    private void PlayerInteractionObject()
    {
        Node currentDetectedObject = null;

        var nodes = GetItemsNodes().ToList();
        Node player = PlayerScript.instance;
        if (player != null) nodes.Add(player);
        foreach (Node node in nodes)
        {
            if (!IsInstanceValid(node))
            {
                continue;
            }

            float distance;
            if (node is Node3D targetNode3D && detectorShape.IsDetected(targetNode3D.GlobalPosition, out distance) && node.Name.ToString().Contains(targetObjectName))
            {
                currentDetectedObject = node;
                break;
            }
        }

        detectedObject = currentDetectedObject;
        if (currentDetectedObject != null)
        {
            onPlayerInteractionObject?.Invoke();
            previousDetectedObject = currentDetectedObject;
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }

    public override void _ExitTree()
    {
        GameManager.onPlayerInteractionObjectAction -= PlayerInteractionObject;
        CSharpBridgeRegistry.Process -= CSProcess;
    }

    private void FindPlayer()
    {
        float distance;

        Node3D player = PlayerScript.instance;
        if (player != null && Node.IsInstanceValid(player) && detectorShape.IsDetected(player.GlobalPosition, out distance))
        {
            previousDetectedObject = player;
            detectedObject = player;
        }
        else
        {
            detectedObject = null;
        }

        if (detectedObject != null)
        {
            onPlayerDetected?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }

    private void FindObject()
    {
        Node currentDetectedObject = null;
        float min_distance = float.MaxValue;
        SphereDetectorShape staticSphere = detectorShape.ToStaticSphere();

        var nodes = VoxLib.mapManager.spatialGrid.GetItemsNodes(staticSphere.center, staticSphere.radius);
        foreach (Node node in nodes)
        {
            if (!IsInstanceValid(node))
            {
                continue;
            }

            float distance;

            Vector3 nodePos;
            if (!TryGetPosition(node, out nodePos))
                continue;

            if (node is ItemPropsScript item &&
               detectorShape.IsDetected(nodePos, out distance)
               && item.GameObjectSample.StartsWith(targetObjectName))
            {
                if (min_distance > distance)
                {
                    currentDetectedObject = node;
                    min_distance = distance;
                }
            }
        }

        detectedObject = currentDetectedObject;
        if (currentDetectedObject != null && Node.IsInstanceValid(currentDetectedObject))
        {
            previousDetectedObject = currentDetectedObject;
            onObjectDetected?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }

    private void FindSound()
    {
        Node currentDetectedObject = null;

        float min_distance = float.MaxValue;
        SphereDetectorShape staticSphere = detectorShape.ToStaticSphere();

        var nodes = VoxLib.mapManager.spatialGrid.GetItemsNodes(staticSphere.center, staticSphere.radius);
        foreach (Node node in nodes)
        {
            if (!IsInstanceValid(node))
            {
                continue;
            }

            float distance;

            Vector3 nodePos;
            if (!TryGetPosition(node, out nodePos))
                continue;

            if (node is ItemPropsScript item && item.IO.audio.isPlaying && detectorShape.IsDetected(nodePos, out distance) && item.IO.audio.currentAudioKey.StartsWith(targetSoundName))
            {
                if (min_distance > distance)
                {
                    currentDetectedObject = node;
                    min_distance = distance;
                }
            }
        }

        detectedObject = currentDetectedObject;
        if (currentDetectedObject != null && Node.IsInstanceValid(currentDetectedObject))
        {
            previousDetectedObject = currentDetectedObject;
            onSoundDetected?.Invoke();
        }
        else
        {
            onAnyObjectsNotDetected?.Invoke();
        }
    }

    private MoveScript GetChachedMoveScript(Node3D node)
    {
        if (moveScriptMap.TryGetValue(node, out MoveScript moveScript))
        {
            return moveScript;
        }
        else
        {
            MoveScript ms = node.GetParent() as MoveScript;
            moveScriptMap[node] = ms;
            return ms;
        }
    }

    private Vector3 GetStaticObjectCachedPosition(Node3D node)
    {
        if (staticObjectPositionMap.TryGetValue(node, out Vector3 position))
        {
            return position;
        }
        else
        {
            position = node.GlobalPosition;
            staticObjectPositionMap[node] = position;
            return position;
        }
    }

    private bool TryGetPosition(Node node, out Vector3 vector)
    {
        if (node is Node3D node3D)
        {
            MoveScript ms = GetChachedMoveScript(node3D);
            if (ms == null)
            {
                vector = GetStaticObjectCachedPosition(node3D);
                return true;
            }
            else
            {
                vector = ms.GlobalPosition;
                return true;
            }
        }
        vector = Vector3.Zero;
        return false;
    }

    private void DrawDebug()
    {
        visualization.Hide();
        if (IsDrawDebug)
        {
            visualization.Draw(detectorShape, this);
        }
    }
}
