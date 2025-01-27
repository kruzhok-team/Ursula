using Godot;
using System;

public partial class ObjectsCatalog : Node
{
    [Export] Control panelLoadObject;

    public void OnOpenPanelLoadObject()
    {
        panelLoadObject.Visible = true;
    }

    public void OnClosePanelLoadObject()
    {
        panelLoadObject.Visible = false;
    }
}
