using Godot;


public partial class MessageBox : Control
{
    public static MessageBox instance;
    [Export] Label label;

    private double timer = 0f;

    public override void _Ready()
    {
        instance = this;
        CSharpBridgeRegistry.Process += CSProcess;
    }

    public void ShowMessage(string message)
    {
        if (label == null) return;

        label.Text = message;

        this.Visible = true;

        //HideMessage(5f);
        timer = 5f;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            if (this.Visible) HideMessage(0);
        }
    }

    public void CSProcess(double delta)
    {
        if (timer > 0 && this.Visible == true)
            timer -= delta;

        if (timer < 0 && this.Visible == true)
        {
            HideMessage();
        }
    }

    private async void HideMessage(float delay)
    {
        await ToSignal(GetTree().CreateTimer(delay), "timeout");
        HideMessage();
    }

    private void HideMessage()
    {
        this.Visible = false;
    }

    public override void _ExitTree()
    {
        CSharpBridgeRegistry.Process -= CSProcess;
    }
}
