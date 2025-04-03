using Godot;
using System;
using Ursula.GameObjects.Model;


public partial class StartupItemInfoView : Node
{
    [Export]
    private Label LabelNameAsset;

    [Export]
    private Button ButtonClickAsset;

    [Export]
    private TextureRect PreviewImageRect;

    [Export]
    private TextureRect LoadObjectImageRect;

    public Action<int> clickItemEvent = null;

    private int id;

    public override void _Ready()
    {
        ButtonClickAsset.ButtonDown += OnItemClickEvent;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ButtonClickAsset.ButtonDown -= OnItemClickEvent;
    }

    public void Invalidate(int id, string name, Texture2D texture)
    {
        this.id = id;
        LabelNameAsset.Text = name;
        PreviewImageRect.Texture = texture;
        LoadObjectImageRect.Visible = false;
    }

    private void OnItemClickEvent()
    {
        clickItemEvent?.Invoke(id);
    }
}
