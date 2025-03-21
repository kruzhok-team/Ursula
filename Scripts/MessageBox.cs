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

        HideMessage(5f);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            if (this.Visible) HideMessage(0);
        }
    }

    public async void HideMessage(float delay)
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");
        this.Visible = false;
    }
}
