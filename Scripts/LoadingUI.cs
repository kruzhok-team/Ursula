using Godot;
using System;

public partial class LoadingUI : Control
{
    public static LoadingUI instance;
    Action cbLoading;

    [Export]
    Texture2D loadingTexture;

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

        Visible = false;
    }

    public void ShowLoading(Texture2D texture = null, Action cb = null)
    {
        cbLoading = cb;
        Visible = true;
        if (texture != null) ((TextureRect)GetNode("TextureRect")).Texture = texture;
        else ((TextureRect)GetNode("TextureRect")).Texture = loadingTexture;
    }

    public void HideLoading()
    {
        Visible = false;
        cbLoading?.Invoke();
        cbLoading = null;
    }
}
