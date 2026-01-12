using Godot;
using System;

public partial class InteractiveObjectInitialization : Node
{
	private static Random random;

	private Random Random
	{
		get
		{
			if (random is null)
				random = new Random(42);
			return random;
		}
	}

	public MoveScript moveScript
	{
		get
		{
			var moveScript = GetParent() as MoveScript;
			return moveScript;
		}
	}

	public object SetPosPoint(float x, float z)
	{
		moveScript.SetPosition(x, z);

		return null;
	}

	public object SetPosLine(float x1, float z1, float x2, float z2)
	{
		float a = Random.NextSingle();
		float x = Mathf.Lerp(x1, x2, a);
		float z = Mathf.Lerp(z1, z2, a);

		return SetPosPoint(x, z);
	}

	public object SetPosArea(float x1, float z1, float x2, float z2)
	{
		float x_a = Random.NextSingle();
		float z_a = Random.NextSingle();

		float x = Mathf.Lerp(x1, x2, x_a);
		float z = Mathf.Lerp(z1, z2, z_a);

		return SetPosPoint(x, z);
	}

	public object SetRotationLookAt(float x, float z)
	{
		moveScript.SetRotationLookAt(x, z);

		return null;
	}
}
