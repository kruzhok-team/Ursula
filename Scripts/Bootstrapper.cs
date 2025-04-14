using Godot;
using System;
using VoxLibExample;

public partial class Bootstrapper : Node
{
    [Export]
    public MapManager MapManager;

    public override void _Ready()
    {
        GD.Print("Инициализация бутстреппера...");

        try
        {
            // Загрузка ассетов
            LoadAssets();

            // Инициализация менеджеров
            InitializeManagers();

            // Загрузка сцен
            LoadScenes();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Ошибка при инициализации: {ex.Message}");
            HandleInitializationError();
        }
    }

    private void InitializeManagers()
    {
        GD.Print("Инициализация менеджеров...");

        // Пример инициализации менеджеров
        //AddChild(new AssetManager());
        //AddChild(new AudioManager());
        //AddChild(new UIManager());

        GD.Print("Менеджеры инициализированы.");
    }

    private void LoadAssets()
    {
        GD.Print("Загрузка ассетов...");

        // Загрузка ассетов
        string path = ProjectSettings.GlobalizePath("res://addons/Ursula/Assets/MapAssets.tres");
        var mapAssets = ResourceLoader.Load<MapAssets>(path);
        VoxLib.mapAssets = mapAssets;

        GD.Print("Ассеты загружены.");
    }

    private void LoadScenes()
    {
        GD.Print("Загрузка сцен...");

        // Загрузка сцены главного меню
        string path = ProjectSettings.GlobalizePath("res://addons/Ursula/Ursula.tscn");
        var mainMenuScene = ResourceLoader.Load<PackedScene>(path);
        if (mainMenuScene != null)
        {
            var mainMenu = mainMenuScene.Instantiate();
            AddChild(mainMenu);

            GD.Print("Главная сцена загружена.");
        }
        else
        {
            GD.PrintErr("Сцена главного меню не найдена.");
        }

        // Загрузка сцены окружения
        path = ProjectSettings.GlobalizePath("res://addons/Ursula/Environment.tscn");
        var environmentScene = ResourceLoader.Load<PackedScene>(path);
        if (environmentScene != null)
        {
            var scene = environmentScene.Instantiate();
            AddChild(scene);

            GD.Print("Сцена окружения загружена.");
        }
        else
        {
            GD.PrintErr("Сцена окружения не найдена.");
        }

        path = ProjectSettings.GlobalizePath("res://addons/Ursula/StartupMenu.tscn");
        var startupMenu = ResourceLoader.Load<PackedScene>(path);
        if (startupMenu != null)
        {
            var scene = startupMenu.Instantiate();
            AddChild(scene);

            GD.Print("Сцена начала проекта загружена.");
        }
        else
        {
            GD.PrintErr("Сцена начала проекта не найдена.");
        }

        path = ProjectSettings.GlobalizePath("res://addons/Ursula/GameObjectCollectionAssetManager.tscn");
        var assetManager = ResourceLoader.Load<PackedScene>(path);
        if (assetManager != null)
        {
            var scene = assetManager.Instantiate();
            AddChild(scene);

            GD.Print("Сцена менеджера коллекции загружена.");
        }
        else
        {
            GD.PrintErr("Сцена менеджера коллекции не найдена.");
        }

    }


    private void HandleInitializationError()
    {
        GD.PrintErr("Произошла ошибка при инициализации. Переход к экрану ошибки.");

        // Загрузка экрана ошибки
        var errorScene = ResourceLoader.Load<PackedScene>("res://addons/Ursula/ErrorScreen.tscn");
        if (errorScene != null)
        {
            var errorScreen = errorScene.Instantiate();
            AddChild(errorScreen);
        }
    }
}
