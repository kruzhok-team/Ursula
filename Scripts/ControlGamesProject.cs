using Godot;
using System;
using System.IO;

public partial class ControlGamesProject : Control
{
    public static ControlGamesProject instance;

    [Export]
    public PackedScene importGameItemPrefab;

    [Export]
    public HBoxContainer container;

    public override void _Ready()
    {
        if (instance != null)
        {
            if (IsInstanceValid(instance))
                instance.Free();
            else
                instance = null;
        }

        instance = this;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        _Instantiate();
    }

    private async void _Instantiate()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        Instantiate();
    }

    public void Instantiate()
    {
        VoxLib.RemoveAllChildren(container);

        string pathImport = VoxLib.mapManager.IMPORTPROJECTPATH;

        if (!Directory.Exists(pathImport))
            Directory.CreateDirectory(pathImport);

        string[] subDirectories = Directory.GetDirectories(pathImport);

        for (int i = 0; i < subDirectories.Length; i++)
        {
            Node instance = importGameItemPrefab.Instantiate();

            ControlGameItem gameItem = instance as ControlGameItem;

            if (gameItem == null)
                continue;

            gameItem.selectedEvent += GameItem_selectedEventHandler;
            gameItem.Invalidate(subDirectories[i]);

            container.AddChild(instance);
        }
    }

    public async void RunGame(string gameName, bool hideAllMenus = false)
    {
        string pathProjectDir = gameName;
        string importName = Path.GetFileNameWithoutExtension(pathProjectDir) + ".txt";
        string[] pathMaps = Directory.GetFiles(pathProjectDir, "*.txt");
        string pathMap = pathMaps[0];
        VoxLib.mapManager.LoadMapFromFile(pathMap);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        VoxLib.mapManager.PlayGame();
        VoxLib.hud.OnCloseControlGameProject();
        VoxLib.hud.OnCloseControlProject();

        if (hideAllMenus)
            ControlPopupMenu.instance._HideAllMenu();
    }

    private void GameItem_selectedEventHandler(string gameName)
    {
        RunGame(gameName);
    }

}
