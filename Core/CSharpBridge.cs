using Godot;

public partial class CSharpBridge : Node
{
    public override void _Process(double delta)
    {
        CSharpBridgeRegistry.Process?.Invoke(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        CSharpBridgeRegistry.PhysicsProcess?.Invoke(delta);
    }
}

