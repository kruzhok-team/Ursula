using Godot;
using System;

public partial class GameManager : Node
{
    public static Action onPlayerInteractionObjectAction;

    public override void _Ready()
    {
        VoxLib.GM ??= this;
    }
}
