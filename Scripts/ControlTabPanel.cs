using Godot;
using System;

public partial class ControlTabPanel : Panel
{
    [Export]
    public ContexPopupMenu[] panels;

    private int current = 0;

    public override void _EnterTree()
    {
        HideAll();
        OpenPanel(current);
    }

    void HideAll()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            //panels[i].Visible = false;
            panels[i].Hide();
        }
    }

    public void OpenPanel(int arg)
    {
        current = arg;
        HideAll();
        if (panels != null && current < panels.Length) panels[current].Show();
    }
}
