using Godot;
using System;
using System.IO;

public partial class ItemLibraryObjectData : Control
{
    [Export]
    Label labelNameItem;

    [Export]
    Button buttonRemove;

    public bool showRemoveButton = true;

    string path;
    public Action<string> removeEvent = null;

    public override void _Ready()
    {
        buttonRemove.Visible = showRemoveButton;

        buttonRemove.ButtonDown += OnItemRemove;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        buttonRemove.ButtonDown -= OnItemRemove;
    }

    public void Invalidate(string path)
    {
        this.path = path;
        labelNameItem.Text = Path.GetFileName(path);
    }

    private void OnItemRemove()
    {
        removeEvent?.Invoke(path);
        this.Free();
    }
}
