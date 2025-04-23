using Godot;
using System;
using System.IO;
using Ursula.GameProjects.Model;

public partial class ControlGameItemView : Control
{
    [Export]
    public Label NameGame;

    [Export]
    public Label SizeGame;

    [Export]
    public TextureRect TextureGame;

    [Export]
    public Button ButtonPlayGame;

    [Export]
    public Button ButtonTestMode;

    public string gameName { get; private set; }

    public event Action<string> clickItemEvent;

    GameProjectAssetInfo gameProject;

    public override void _Ready()
    {
        base._Ready();
        if (ButtonPlayGame != null) ButtonPlayGame.ButtonDown += ButtonPlayGame_ButtonDownEvent;
        if (ButtonTestMode != null) ButtonTestMode.ButtonDown += ButtonTestMode_ButtonDownEvent;
    }

    public async void Generate(GameProjectAssetInfo gameProject)
    {
        this.gameProject = gameProject;

        string nameGame = string.IsNullOrEmpty(gameProject.Template.Alias) ? gameProject.Name : gameProject.Template.Alias;
        this.gameName = nameGame;

        SizeGame.Text = gameProject.ProjectSizeMb;

        NameGame.Text = gameName;

        TextureGame.Texture = await gameProject.GetPreviewImage();

#if !TOOLS
        if (ButtonTestMode != null) ButtonTestMode.Visible = false;
#endif


    }

    public void OnPlayGame()
    {
        var handler = clickItemEvent;
        clickItemEvent?.Invoke(gameProject.Id);
    }

    private void OnPlayTest()
    {
        VoxLib.mapManager.playMode = PlayMode.testMode;
        var handler = clickItemEvent;
        clickItemEvent?.Invoke(gameProject.Id);
    }

    private void ButtonPlayGame_ButtonDownEvent()
    {
        OnPlayGame();
    }

    private void ButtonTestMode_ButtonDownEvent()
    {
        //VoxLib.mapManager.playMode = PlayMode.testMode;
        //gameProject?.PlayGame();
        OnPlayTest();
    }
}
