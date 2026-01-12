using bearloga.addons.Ursula.Modules.LogicInjector;
using Godot;
using System;

[Serializable]
public partial class ItemPropsScript : ItemBase
{
    public string AssetInfoId { get; set; }
    public string GameObjectSample { get; set; }
    public int GameObjectSampleHash { get; set; }

    public int id;
	public int type;

	public int state;

	public float x => (int)(GlobalTransform.Origin.X * 100) / 100f;
	public float y => (int)(GlobalTransform.Origin.Y * 100) / 100f;
    public float z => (int)(GlobalTransform.Origin.Z * 100) / 100f;

    public float positionY;

    public byte rotation;

    public InteractiveObject IO;

    public Injector LogicInjector;

    float _scale = 1;



    public float scale
	{
		get
		{
			return _scale;
		}
		set
		{
            if (value < 0)
				_scale = 0.1f;
			else
				_scale = value;
        }
	}

	public override void _Ready()
	{
        InteractiveObject IO = (InteractiveObject)this.GetParent().FindChild("InteractiveObject", true, true);
        if (IO != null) this.IO = IO;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

	public void DeleteItem()
	{
        //if (VoxLib.mapManager.gameItems != null && VoxLib.mapManager.gameItems.Contains(this))
        //{
        //    VoxLib.mapManager.gameItems.Remove(this);
        //    VoxLib.mapManager.ChangeWorldBytesItem(x, y, z, (byte)0, (byte)0);
        //    if (VoxLib.mapManager.voxTypes != null) VoxLib.mapManager.voxTypes[x, y, z] = 0;
        //    if (VoxLib.mapManager.voxData != null) VoxLib.mapManager.voxData[x, y, z] = 0;
        //    if (VoxLib.mapManager._voxGrid != null) VoxLib.mapManager._voxGrid.Set(x, y, z, 0);
        //}
    }

	public void Use()
	{
		//string name = GetParent().Name;
		//GD.Print($"Used ips={name}");

        var interactiveObject = GetNodeOrNull("InteractiveObject") as InteractiveObject;
        if (interactiveObject == null) interactiveObject = GetParent().GetNodeOrNull("InteractiveObject") as InteractiveObject;
        interactiveObject?.onThisInteraction?.Invoke();

        var baseAnimation = GetNodeOrNull("AnimationObject") as BaseAnimation;
        if (baseAnimation == null) baseAnimation = GetParent().GetNodeOrNull("AnimationObject") as BaseAnimation;
		baseAnimation?.UseAction.Invoke();
    }

}
