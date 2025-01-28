using Godot;
using System;
using System.IO;
using static MoveScript;

// Тут будет происходить парсинг XML файла, поэтапное выполнение алгоритма и связь элементов с соответствующими узлами
public partial class InteractiveObject : Node
{
    public GMLActionHolder onThisInteraction = new();

    public string selectedObjectName;

    public InteractiveObjectDetector detector;
    public InteractiveObjectAudio audio;
    public InteractiveObjectMove move;
    public InteractiveObjectTimer timer;
    public InteractiveObjectCounter counter1;
    public InteractiveObjectCounter counter2;

    [Export]
    public string xmlPath;

    string workFolderPath = "res://addons/Ursula/Modules/InteractiveObjects";

    public GMLAlgorithm gml;

    public override void _Ready()
	{
        InteractiveObjectsManager.Register(this);

        detector = LinkOrLoadPrefabComponent<InteractiveObjectDetector>("InteractiveObjectDetector", $"{workFolderPath}/Prefabs/interactive_object_detector.tscn");
        audio = LinkOrLoadPrefabComponent<InteractiveObjectAudio>("InteractiveObjectAudio", $"{workFolderPath}/Prefabs/interactive_object_audio.tscn");
        move = LinkOrLoadPrefabComponent<InteractiveObjectMove>("InteractiveObjectMove", $"{workFolderPath}/Prefabs/interactive_object_move.tscn");
        timer = LinkOrLoadPrefabComponent<InteractiveObjectTimer>("InteractiveObjectTimer", $"{workFolderPath}/Prefabs/interactive_object_timer.tscn");

        counter1 = LinkOrLoadPrefabComponent<InteractiveObjectCounter>("InteractiveObjectCounter1", $"{workFolderPath}/Prefabs/interactive_object_counter.tscn");
        counter2 = LinkOrLoadPrefabComponent<InteractiveObjectCounter>("InteractiveObjectCounter2", $"{workFolderPath}/Prefabs/interactive_object_counter.tscn");

        ReloadAlgorithm();
        //StartAlgorithm();

        //ItemPropsScript ips = (ItemPropsScript)this.GetParent().FindChild("ItemPropsScript", true, true);
        //if (ips != null) ips.IO = this;
    }

    public void ReloadAlgorithm()
    {
        StopAlgorithm();
        gml = null;

        LoadAlgorithm(xmlPath);
        //if (move != null) move.ReloadAlgorithm();
    }

    public void LoadAlgorithm(string path)
    {
        xmlPath = path; //$"{workFolderPath}/Graphs/{interactiveObjectName}.graphml"

        if (xmlPath != null)
        {
            if (xmlPath.Replace(" ", "") != "")
            {
                if (File.Exists(ProjectSettings.GlobalizePath(xmlPath)))
                    gml = GMLAlgorithm.Load(xmlPath, this);
                else
                    ContextMenu.ShowMessageS($"[GML {this.GetParent().Name}] Ошибка: файл алгоритма не найден по пути {xmlPath}");
            }
        }
    }

    public void StartAlgorithm()
    {
        if (gml != null)
        {
            if (gml.currentState != null)
            {
                gml.Start();
                return;
            }
        }

        //GD.PrintErr("GML cannot be started, it is not loaded");       
    }

    public void StopAlgorithm()
    {
        if (gml != null)
        {
            gml.Stop();
        }

        detector?.StopScanning();
        audio?.Stop();
        move?.StopMoving();
        timer?.StopTimer();

        counter1?.ResetValue();
        counter2?.ResetValue();
    }

    public Node3D GetCurrentTargetObject()
    {
        return detector.detectedObject as Node3D;
    }

    public void Interaction() //Метод запуска взаимодействия с текущим объектом
    {
        onThisInteraction?.Invoke();
    }

    private T LinkOrLoadPrefabComponent<T>(string nodeName, string prefabPath) where T : Node
    {
        Node parentNode = GetParent();

        if (parentNode.HasNode(nodeName))
        {
            Node foundNode = parentNode.GetNode(nodeName);
            if (foundNode is T targetNode)
            {
                return targetNode;
            }
        }

        // Если узел не найден, загружаем префаб
        PackedScene prefab = ResourceLoader.Load<PackedScene>(prefabPath);
        if (prefab == null)
        {
            GD.PrintErr($"Не удалось загрузить префаб по пути: {prefabPath}");
            return null;
        }

        // Инстанцируем префаб
        T newNode = prefab.Instantiate<T>();
        if (newNode == null)
        {
            GD.PrintErr($"Префаб по пути {prefabPath} не соответствует типу {typeof(T)}");
            return null;
        }

        newNode.Name = nodeName;

        parentNode.CallDeferred("add_child", newNode);

        return newNode;
    }

    public async void AsyncReloadAlgorithm()
    {
        await ToSignal(GetTree().CreateTimer(1), "timeout");

        ReloadAlgorithm();
    }
}
