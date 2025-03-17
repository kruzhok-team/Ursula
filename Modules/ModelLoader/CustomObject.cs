using Godot;
using System;
using System.Collections.Generic;

public partial class CustomObject : Node
{
	public static CustomObject current;

	public string objPath = "";
	public Node3D modelInstance;

	[Export] public Node3D Indicator;

	static List<CustomObject> customObjects = new List<CustomObject>();

	public static void SetVisibleIndicators(bool indVisible)
	{
		if (customObjects == null) 
			return;

		customObjects.RemoveAll(p => p == null);

		foreach (var co in customObjects)
		{
			if (co != null && IsInstanceValid(co) && co.Indicator != null)
				co.Indicator.Visible = indVisible;
		}
	}

	public void InitModel()
	{
		//await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		if (modelInstance != null)
			modelInstance.QueueFree();

		if (string.IsNullOrEmpty(objPath))
		{
			ModelLoader.OpenObj((path, instance) =>
			{
				if (instance != null)
				{
					objPath = path;
					modelInstance = instance;
					current = this;

					this.AddChild(modelInstance);
				}
				else
				{
					if (customObjects.Contains(this))
						customObjects.Remove(this);

					VoxLib.mapManager.DeleteItem(GetParent());
				}
			});
		}
		else
		{
			modelInstance = ModelLoader.LoadModelByPath(objPath);
			this.AddChild(modelInstance);
		}
	}

	public void LoadNewModel()
	{
		objPath = "";
		InitModel();
	}

	public override void _Ready()
	{
		if (!customObjects.Contains(this))
			customObjects.Add(this);
		//InitModel();
	}

	public override void _Input(InputEvent @event)
	{
		if (current != this) return;

		if (Input.IsKeyPressed(Key.Plus) && @event.IsPressed())
		{
			CalculateScale(0.1f);
		}
		else if (Input.IsKeyPressed(Key.Minus) && @event.IsPressed())
		{
			CalculateScale(-0.1f);
		}
	}

	private void CalculateScale(float step)
	{

	}
}
