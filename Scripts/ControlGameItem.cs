using Godot;
using System;
using System.IO;

public partial class ControlGameItem : Control
{
    [Export]
    public Label nameGame;

    [Export]
    public TextureRect textureGame;

    public string pathProjectDir { get; private set; }
    public string gameName { get; private set; }

    public event Action<string> selectedEvent;

    public void Invalidate(string pathProjectDir)
    {
        this.pathProjectDir = pathProjectDir;
        this.gameName = Path.GetFileNameWithoutExtension(pathProjectDir);

        nameGame.Text = gameName;

        string gameImagePath = null;
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".jpg"))
            gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".jpg";
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".JPG"))
            gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".JPG";
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".png"))
            gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".png";
        if (File.Exists(pathProjectDir + "/" + MapManager.GAMEIMAGE + ".PNG"))
            gameImagePath = pathProjectDir + "/" + MapManager.GAMEIMAGE + ".PNG";

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
        var handler = selectedEvent;
        selectedEvent?.Invoke(gameName);
    }
}
