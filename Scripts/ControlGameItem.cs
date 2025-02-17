using Godot;
using System;
using System.IO;

public partial class ControlGameItem : Control
{
    [Export]
    public Label nameGame;

    [Export]
    public TextureRect textureGame;

    [Export]
    public Button buttonTestMode;

    public string pathProjectDir { get; private set; }
    public string gameName { get; private set; }

    public event Action<string> selectedEvent;


    public void Invalidate(string pathProjectDir)
    {
        this.pathProjectDir = pathProjectDir;
        this.gameName = Path.GetFileNameWithoutExtension(pathProjectDir);

        nameGame.Text = gameName;

        Texture2D texture = VoxLib.mapManager.GetGameImage(pathProjectDir);
        textureGame.Texture = texture;

#if !TOOLS
        buttonTestMode.Visible = false;
#endif


    }

    public void OnPlayGame()
    {
        //VoxLib.mapManager.playMode = PlayMode.playGameMode;
        var handler = selectedEvent;
        //selectedEvent?.Invoke(gameName);
        selectedEvent?.Invoke(pathProjectDir);
    }

    private void OnPlayTest()
    {
        VoxLib.mapManager.playMode = PlayMode.testMode;
        var handler = selectedEvent;
        selectedEvent?.Invoke(pathProjectDir);
    }
}
