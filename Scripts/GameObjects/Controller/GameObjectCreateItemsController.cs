using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;


public partial class GameObjectCreateItemsController : Node3D, IInjectable
{
	[Export] private Camera3D _camera;

    [Inject]
    private ISingletonProvider<GameObjectCreateItemsModel> _GameObjectCreateItemsModelProvider;

    private GameObjectCreateItemsModel _gameObjectCreateItemsModel;

    void IInjectable.OnDependenciesInjected()
    {
    }

    public override void _Ready()
	{
		base._Ready();
        _ = SubscribeEvent();
    }

    private async GDTask SubscribeEvent()
    {
        _gameObjectCreateItemsModel = await _GameObjectCreateItemsModelProvider.GetAsync();
    }

    protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public override void _Input(InputEvent @event)
	{
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

                    _gameObjectCreateItemsModel.SetGameObjectCreateItem(collider, pos, 1f, 0);

                }
                else if (eventMouseButton.ButtonIndex == MouseButton.Right)
                {
                    VoxLib.mapManager.DeleteItem(collider);
                    _gameObjectCreateItemsModel.SetGameObjectDeleteItem(collider);
                }
            }
        }
    }


}
