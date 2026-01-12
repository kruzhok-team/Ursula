using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using static MoveScript;
using Modules.HSM;
using Fractural.Tasks;
using System.Threading;
using bearloga.addons.Ursula.Modules.LogicInjector;
using ursula.addons.Ursula.Modules.CyberiadaHSMExtensions;
using ursula.addons.Ursula.Modules.InteractiveObjects;

// Тут будет происходить парсинг XML файла, поэтапное выполнение алгоритма и связь элементов с соответствующими узлами
public partial class InteractiveObject : Node
{
    public Action onThisInteraction;

    public string selectedObjectName;

    public InteractiveObjectDetector detector;
    public InteractiveObjectDetector detector2;
    public InteractiveObjectAudio audio;
    public InteractiveObjectMove move;
    public InteractiveObjectTimer timer;
    public InteractiveObjectTimer timer2;
    public InteractiveObjectTime time;
    public InteractiveObjectCounter counter1;
    public InteractiveObjectCounter counter2;
    public InteractiveObjectModels models;
    public InteractiveObjectInitialization initialization;
    public InteractiveObjectRandomness random;
    public InteractiveObjectEpidemic epidemic;

    [Export]
    public string xmlPath;

    [Export]
    public string AudioFolderPath;

    string workFolderPath = "res://addons/Ursula/Modules/InteractiveObjects";

    public CyberiadaLogic hsmLogic;
    public Injector injector = null;

    public HSMDetectorModule hsmDetectorModule;
    public HSMDetectorTwoModule hsmDetectorTwoModule;
    public HSMMovementModule hsmMovementModule;
    public HSMAnimationModule hsmAnimationModule;
    public HSMSoundModule hsmSoundModule;
    public HSMTimerModule hsmTimerModule;
    public HSMTimerTwoModule hsmTimerTwoModule;
    public HSMTimeModule hsmTimeModule;
    public HSMCounterOneModule hsmCounterOneModule;
    public HSMCounterTwoModule hsmCounterTwoModule;
    public HSMWorldInteractingModule hsmWorldInteractingModule;
    public HSMModelsModule hswModelsModule;
    public HSMInitializationModule hsmInitializationModule;
    public HSMRandomnessModule hsmRandomnessModule;
    public HSMEpidemicModule hsmEpidemicModule;
    public HSMInteractiveObjectModule interactiveObjectModule;

    HSMLogger _logger;

    public ManualResetEvent resetEvent = new ManualResetEvent(false);

    public override void _Ready()
	{
        InteractiveObjectsManager.Register(this);

        InitInstances();

        ReloadAlgorithm();
    }

    private void InitInstances()
    {
        detector = LinkComponent<InteractiveObjectDetector>("InteractiveObjectDetector", VoxLib.mapAssets.InteractiveObjectDetectorPrefab);
        detector2 = LinkComponent<InteractiveObjectDetector>("InteractiveObjectDetector2", VoxLib.mapAssets.InteractiveObjectDetectorPrefab);
        audio = LinkComponent<InteractiveObjectAudio>("InteractiveObjectAudio", VoxLib.mapAssets.InteractiveObjectAudioPrefab);
        move = LinkComponent<InteractiveObjectMove>("InteractiveObjectMove", VoxLib.mapAssets.InteractiveObjectMovePrefab);
        timer = LinkComponent<InteractiveObjectTimer>("InteractiveObjectTimer", VoxLib.mapAssets.InteractiveObjectTimerPrefab);
        timer2 = LinkComponent<InteractiveObjectTimer>("InteractiveObjectTimer2", VoxLib.mapAssets.InteractiveObjectTimerPrefab);
        time = LinkComponent<InteractiveObjectTime>("InteractiveObjectTime", VoxLib.mapAssets.InteractiveObjectTimePrefab);
        counter1 = LinkComponent<InteractiveObjectCounter>("InteractiveObjectCounter1", VoxLib.mapAssets.InteractiveObjectCounterPrefab);
        counter2 = LinkComponent<InteractiveObjectCounter>("InteractiveObjectCounter2", VoxLib.mapAssets.InteractiveObjectCounterPrefab);
        models = LinkComponent<InteractiveObjectModels>("InteractiveObjectModels", VoxLib.mapAssets.InteractiveObjectModelsPrefab);
        initialization = LinkComponent<InteractiveObjectInitialization>("InteractiveObjectInitialization", VoxLib.mapAssets.InteractiveObjectInitializationPrefab);
        random = LinkComponent<InteractiveObjectRandomness>("InteractiveObjectRandomness", VoxLib.mapAssets.InteractiveObjectRandomnessPrefab);
        epidemic = LinkComponent<InteractiveObjectEpidemic>("InteractiveObjectEpidemic", VoxLib.mapAssets.InteractiveObjectEpidemicPrefab);
    }

    private async GDTask InitHsm()
    {
        await ToSignal(GetTree().CreateTimer(0.1), "timeout");
        
        hsmDetectorModule = new HSMDetectorModule(hsmLogic, this);        
        hsmDetectorTwoModule = new HSMDetectorTwoModule(hsmLogic, this);
        hsmMovementModule = new HSMMovementModule(hsmLogic, this);
        hsmAnimationModule = new HSMAnimationModule(hsmLogic, this);
        hsmSoundModule = new HSMSoundModule(hsmLogic, this);
        hsmTimerModule = new HSMTimerModule(hsmLogic, this);
        hsmTimerTwoModule = new HSMTimerTwoModule(hsmLogic, this);
        hsmTimeModule = new HSMTimeModule(hsmLogic, this);
        hsmCounterOneModule = new HSMCounterOneModule(hsmLogic, this);
        hsmCounterTwoModule = new HSMCounterTwoModule(hsmLogic, this);
        hsmWorldInteractingModule = new HSMWorldInteractingModule(hsmLogic, this);
        hswModelsModule = new HSMModelsModule(hsmLogic, this);
        hsmInitializationModule = new HSMInitializationModule(hsmLogic, this);
        hsmRandomnessModule = new HSMRandomnessModule(hsmLogic, this);
        hsmEpidemicModule = new HSMEpidemicModule(hsmLogic, this);
        interactiveObjectModule = new HSMInteractiveObjectModule(hsmLogic, this);

        resetEvent.Set();
    }

    public void ReloadAlgorithm()
    {
        StopAlgorithm();
        hsmLogic = null;

        LoadAlgorithm(xmlPath);
        //if (move != null) move.ReloadAlgorithm();
    }

    public void LoadAlgorithm(string path)
    {
        xmlPath = path; //$"{workFolderPath}/Graphs/{interactiveObjectName}.graphml"

        if (!string.IsNullOrEmpty(xmlPath))
        {
            if (xmlPath.Replace(" ", "") != "")
            {
                if (File.Exists(ProjectSettings.GlobalizePath(xmlPath)))
                {
                    hsmLogic = CyberiadaLogic.Load(xmlPath);
                    injector?.TryApply(hsmLogic.RootaState);
                    _= InitHsm();
                    _logger = new HSMLogger(this);
                    hsmLogic.SubscribeLogger(_logger);
                }
                else
                {
                    HSMLogger.Print(this, $"Ошибка: файл алгоритма не найден по пути {xmlPath}");
                }
            }
        }
    }

    public void StartAlgorithm()
    {
        if (hsmLogic != null)
        {
            //if (gml.currentState != null)
            //{
            hsmLogic.Start();
            //return;
            //}
        }

        //GD.PrintErr("GML cannot be started, it is not loaded");       
    }

    public void StopAlgorithm()
    {
        if (hsmLogic != null)
        {
            hsmLogic.Stop();
        }

        detector?.StopScanning();
        detector2?.StopScanning();
        audio?.Stop();
        move?.StopMoving();
        timer?.StopTimer();

        counter1?.ResetValue();
        counter2?.ResetValue();
    }

    public Node3D GetCurrentTargetObject()
    {
        return detector.previousDetectedObject as Node3D;
    }

    public Node3D GetCurrentTargetObject2()
    {
        return detector2.previousDetectedObject as Node3D;
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

    private T LinkComponent<T>(string nodeName, PackedScene prefab) where T : Node
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

        T newNode = prefab.Instantiate<T>();
        if (newNode == null) return null;

        newNode.Name = nodeName;

        parentNode.CallDeferred("add_child", newNode);

        return newNode;
    }

    public async void AsyncReloadAlgorithm()
    {
        await ToSignal(GetTree().CreateTimer(1), "timeout");

        ReloadAlgorithm();
    }

    public void SetAudiosPathes(List<string> audioPathes)
    {
        audio.SetAudiosPathes(audioPathes);
    }
}
