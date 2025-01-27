using Godot;


public partial class MessageBox : Control
{
    public static MessageBox instance;
    [Export] Label label;

    public override void _Ready()
    {
        instance = this;
    }

    public void ShowMessage(string message)
    {
        if (label == null) return;

        label.Text = message;

        this.Visible = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            this.Visible = false;
        }
    }
}
