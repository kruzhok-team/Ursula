using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class ControlGamesProject : Control
{
    public static ControlGamesProject instance;

    [Export]
    public PackedScene importGameItemPrefab;

    [Export]
    public HBoxContainer container;

    List<ControlGameItem> gameItems;

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

        Visible = true;
    }

    private async void _Instantiate()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        Instantiate();
    }

    public void Instantiate()
    {
        VoxLib.RemoveAllChildren(container);
        gameItems = new List<ControlGameItem>();

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
            gameItems.Add(gameItem);
        }

        CheckInstallGames();

    }

    public async void CheckInstallGames()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
#if !TOOLS
        if (gameItems.Count == 1) RunGame(gameItems[0].pathProjectDir);
#endif
    }

    string pathProjectDir;
    public void RunGame(string gameName)
    {
        pathProjectDir = gameName;
        VideoPlayer.instance.PlayVideo(pathProjectDir, OnVideoPreviewFinished);
    }

    private async void OnVideoPreviewFinished()
    {
        Texture2D texture = VoxLib.mapManager.GetGameImage(pathProjectDir);
        LoadingUI.instance.ShowLoading(texture);

        string importName = Path.GetFileNameWithoutExtension(pathProjectDir) + ".txt";
        string[] pathMaps = Directory.GetFiles(pathProjectDir, "*.txt");
        string pathMap = pathMaps[0];

        VoxLib.mapManager.LoadMapFromFile(pathMap);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        VoxLib.hud.HideAllControls();
        ControlPopupMenu.instance._HideAllMenu();

        VoxLib.mapManager.PlayGame();

        LoadingUI.instance.HideLoading();
    }

    private void GameItem_selectedEventHandler(string gameName)
    {
        RunGame(gameName);
    }


}
