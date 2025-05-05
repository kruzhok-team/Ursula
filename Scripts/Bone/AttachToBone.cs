using Godot;
using System;

public partial class AttachToBone : Node3D
{
    [Export]
    Node3D Model;

    [Export]
	string BoneName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
