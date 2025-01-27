using Godot;
using System;

public partial class ControlPopupMenu : Control
{
    public static ControlPopupMenu instance;

    [Export]
    public ContexPopupMenu[] contexPopupMenu { get; set; }

    public override void _Ready()
    {
        instance = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsKeyPressed(Key.Escape) && @event.IsPressed())
        {
            _HideAllMenu();
        }
    }

    public void _HideAllMenu()
    {
        for (int i = 0; i < contexPopupMenu.Length; i++)
        {
            contexPopupMenu[i].Hide();
        }
    }

    public static void HideAllMenu()
    {
        instance?._HideAllMenu();
    }
}
