using Godot;
using System;

public partial class ContexPopupMenu : Button
{
    bool state = false;

    [Export] Panel PanelMenu;
    [Export] TextureRect arrow1;
    [Export] TextureRect arrow2;

    public void OnClick()
    {
        bool newState = !state;
        ControlPopupMenu.HideAllMenu();
        state = newState;
        SetState();
    }

    public void SetState()
    {
        if (PanelMenu != null) PanelMenu.Visible = state;
        arrow1.Visible = state;
        arrow2.Visible = !state;
    }

    public void Show()
    {
        state = true;
        SetState();
    }

    public void Hide()
    {
        state = false;
        SetState();
    }


}
