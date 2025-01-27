using Godot;
using System;
using System.IO;

public partial class CustomItem : Node
{
    public string objPath = "";
    public Node3D modelInstance;

    public override void _Ready()
    {
        InitModel();
    }

    public async void InitModel()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        if (modelInstance != null)
            modelInstance.QueueFree();

        if (string.IsNullOrEmpty(objPath))
        {

        }
        else
        {
            modelInstance = ModelLoader.LoadModelByPath(objPath);
            if (modelInstance == null) return;

            this.AddChild(modelInstance);

            Node parent = GetParent();

            //ItemPropsScript ips = parent as ItemPropsScript;
            ItemPropsScript ips = (ItemPropsScript)this.GetParent().FindChild("ItemPropsScript", true, true);

            parent.Name = Path.GetFileNameWithoutExtension(objPath) + $"{ips.x}{ips.y}{ips.z}";
        }
    }

}
