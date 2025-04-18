using Godot;
using System;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.Log.Model;
using Ursula.Settings.View;
using VoxLibExample;


public partial class VoxLib : Node, IInjectable
{
    public static VoxLib instance;

    public static MapManager mapManager;

    public static MapAssets mapAssets;

    public static HUD hud;

    public static LogScript log;

    public static GameManager GM;

    [Export]
    public ControlGamesProject CGP;

    public static TerrainManager terrainManager;
    public static WaterManager waterManager;

    public static string SETTINGPATH = "user://settings.cfg";

    [Inject]
    private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

    [Inject]
    private ISingletonProvider<LogModel> _LogModelProvider;

    Texture texture;

    void IInjectable.OnDependenciesInjected()
    {
    }

    public static float Sensitivity
    {
        get
        {
            return VoxLib.instance._Sensitivity;
        }
    }

    public override void _Ready()
	{
		base._Ready();
        instance ??= this;

        string[] gpuModel = OS.GetVideoAdapterDriverInfo();
		string drvInfo = "";
		for (int i = 0; i < gpuModel.Length; i++) drvInfo += gpuModel[i] + " ";

        try
        {
            texture = ResourceLoader.Load<Texture>("res://addons/Ursula/Av6.png");
            //ShowMessage($"Проверьте драйвера видео карты {drvInfo}. Ошибка загрузки тестовой текстуры.");
        }
        catch (Exception e)
        {
            GD.Print("Error loading test texture: " + e.Message);
			ShowMessage($"Проверьте или обновите драйвера видео карты {drvInfo}. Ошибка загрузки тестовой текстуры.");
        }
    }

	 public static void RemoveAllChildren(Node parent)
	 {
		if (parent == null) return;

		 foreach (Node child in parent.GetChildren())
		 {
			 child.QueueFree();
		 }
	 }
	
	public static void ShowMessage(string message)
	{
		MessageBox.instance?.ShowMessage(message);
	}

    public static void Log(string message)
    {
        VoxLib.instance?._Log(message);
    }

    public void _Log(string message)
    {
        if (TryGetLogModel(out var logModel))
            logModel.SetLogMessage(message);
    }

    public static void SetVisibleLog(bool value)
    {
        VoxLib.instance?._SetVisibleLog(value);
    }

    public void _SetVisibleLog(bool value)
    {
        if (TryGetLogModel(out var logModel))
            logModel.SetVisibleView(value);
    }

    private float _Sensitivity
    {
        get
        {
            if (!TryGetSettingsModel(out var settingsModel))
                return settingsModel._defaultSensitivity;
            return settingsModel.Sensitivity;
        }
    }

    private bool TryGetSettingsModel(out EnvironmentSettingsModel model, bool errorIfNotExist = false)
    {
        model = null;

        if (!(_settingsModelProvider?.TryGet(out model) ?? false))
        {
            if (errorIfNotExist)
                GD.PrintErr($"{typeof(VoxLib).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
        }
        return model != null;
    }

    private bool TryGetLogModel(out LogModel model, bool errorIfNotExist = false)
    {
        model = null;

        if (!(_LogModelProvider?.TryGet(out model) ?? false))
        {
            if (errorIfNotExist)
                GD.PrintErr($"{typeof(VoxLib).Name}: {typeof(LogModel).Name} is not instantiated!");
        }
        return model != null;
    }
}
