using Godot;
using System;
using static Godot.PhysicsServer3D;


public partial class CreateItems : Node3D
{
	public static CreateItems instance;

	[Export] private Camera3D _camera;

	//[Export] private RayCast3D _raycast;

	public override void _Ready()
	{
		//instance ??= this;
		if (instance != null) instance.Free();
		instance = this;

		//_raycast = GetNode<RayCast3D>("RayCast3D");
		//_raycast.Enabled = true;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (instance == this) instance = null;
	}

	public override void _Input(InputEvent @event)
	{
		if (instance != this) return;

		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
		{
            if (Raycaster.Hit(_camera, eventMouseButton.Position, out Node collider, out Vector3 pos))
			{
				string name = collider.Name;
                if (name.Contains("Bound")) return;

				if (eventMouseButton.ButtonIndex == MouseButton.Left)
				{
					VoxLib.mapManager.tempScale = 1f;
                    VoxLib.mapManager.Building(collider, pos);
				}
				else if (eventMouseButton.ButtonIndex == MouseButton.Right)
					VoxLib.mapManager.DeleteItem(collider);
            }
        }
    }

	public override void _PhysicsProcess(double delta)
	{
		//PhysicsDirectSpaceState3D spaceState = GetViewport().GetWorld3D().DirectSpaceState;
		//World3D world = GetViewport().GetWorld3D();

	}


	private void PerformRaycast()
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();

		Vector3 from = _camera.ProjectRayOrigin(mousePosition);
		Vector3 to = from + _camera.ProjectRayNormal(mousePosition) * 1000;

		//PhysicsDirectSpaceState3D spaceState = GetViewport().GetWorld3D().DirectSpaceState;
		//var result = spaceState.IntersectRay(from, to);

		//if (result.Count > 0)
		//{
		//    Vector3 collisionPoint = (Vector3)result["position"];
		//    Node collider = (Node)result["collider"];
		//    GD.Print($"Hit object: {collider.Name} at position {collisionPoint}");
		//}
	}
}
