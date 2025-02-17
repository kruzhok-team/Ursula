using Godot;
using System;
using VoxLibExample;


public partial class VoxLib : Node
{
	public static VoxLib instance;

	public static MapManager mapManager;

	public static MapAssets mapAssets;

    public static HUD hud;

	public static LogScript log;

    public static GameManager GM;

    [Export]
	public MapAssets _mapAssets;

    [Export]
    public ControlGamesProject CGP;

    public static CreateTerrain createTerrain;
	public static CreateWater createWater;

	public static string SETTINGPATH = "user://settings.cfg";

	Texture texture;

	public override void _Ready()
	{
		base._Ready();
        instance ??= this;
        mapAssets ??= _mapAssets;

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
	
	public static float Sensitivity()
	{
		return hud.sensitivity;
    }

	public static void ShowMessage(string message)
	{
		MessageBox.instance?.ShowMessage(message);
	}
}
