using Godot;
using System;
using System.IO;

public partial class ControlGameItem : Control
{
    [Export]
    public Label nameGame;

    [Export]
    public TextureRect textureGame;

    string pathProjectDir;

    public void Invalidate(string pathProjectDir)
    {
        this.pathProjectDir = pathProjectDir;

        string name = Path.GetFileNameWithoutExtension(pathProjectDir);

        nameGame.Text = name;

        string gameImagePath = null;
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".jpg")) gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".jpg";
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".JPG")) gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".JPG";
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".png")) gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".png";
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".PNG")) gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".PNG";

        if (gameImagePath != null)
        {
            Image img = new Image();
            var err = img.Load(gameImagePath);

            if (err != Error.Ok)
            {
                GD.Print("Failed to load image from path: " + gameImagePath);
            }
            else
            {
                ImageTexture texture = ImageTexture.CreateFromImage(img);
                textureGame.Texture = texture;
            }
        }
    }

    public void OnPlayGame()
    {
        PlayGame();
    }

    public async void PlayGame()
    {
        string importName = Path.GetFileNameWithoutExtension(pathProjectDir) + ".txt";
        string[] pathMaps = Directory.GetFiles(pathProjectDir, "*.txt");
        string pathMap = pathMaps[0]; // pathProjectDir + "/" + importName;
        VoxLib.mapManager.LoadMapFromFile(pathMap);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        VoxLib.mapManager.PlayGame();
        VoxLib.hud.OnCloseControlGameProject();
        VoxLib.hud.OnCloseControlProject();       
    }
}
