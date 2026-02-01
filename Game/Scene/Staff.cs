using Godot;
using System;

public partial class Staff : Control
{
    [Export]
    Button btn_close;

    public override void _Ready()
    {
        btn_close.Pressed += () =>
        {
            Visible = false;
        };
    }
}
