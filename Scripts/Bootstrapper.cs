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
        var mapAssets = ResourceLoader.Load<MapAssets>("res://addons/Ursula/Assets/MapAssets.tres");
        VoxLib.mapAssets = mapAssets;

        GD.Print("Ассеты загружены.");
    }

    private void LoadScenes()
    {
        GD.Print("Загрузка сцен...");

        // Загрузка сцены главного меню
        var mainMenuScene = ResourceLoader.Load<PackedScene>("res://addons/Ursula/Ursula.tscn");
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
        var environmentScene = ResourceLoader.Load<PackedScene>("res://addons/Ursula/Environment.tscn");
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

        var startupMenu = ResourceLoader.Load<PackedScene>("res://addons/Ursula/StartupMenu.tscn");
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

        var assetManager = ResourceLoader.Load<PackedScene>("res://addons/Ursula/GameObjectCollectionAssetManager.tscn");
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
