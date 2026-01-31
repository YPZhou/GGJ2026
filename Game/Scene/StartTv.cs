using Godot;
using System;

public partial class StartTv : Sprite2D
{
    public bool MouseEntered { get; private set; }
    [Export]
    Area2D area2D;
    [Export]
    Texture2D cursorTexture;

    [Export]
    PackedScene GameScene;

    public override void _Ready()
    {
        if (area2D == null)
        {
            GD.PrintErr("Area2D is null, set Area2D for TV.area2D in the editor.");
        }
        if (cursorTexture == null)
        {
            GD.PrintErr("Cursor texture is null, set Cursor texture for TV.cursorTexture in the editor.");
        }
        if (GameScene == null)
        {
            GD.PrintErr("GameScene is null, set GameScene for StartTv.GameScene in the editor.");
        }


        area2D.InputPickable = true;
        area2D.MouseEntered += () =>
        {
            MouseEntered = true;
            Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Arrow, new Vector2(16, 16));
        };
        area2D.MouseExited += () =>
        {
            MouseEntered = false;
            Input.SetCustomMouseCursor(null);
        };
    }


    public override void _Process(double delta)
    {
        TickStart();
    }

    void TickStart()
    {
        if (MouseEntered && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            GD.Print($"{nameof(Game)}:开始游戏");
            Input.SetCustomMouseCursor(null);
            GetTree().ChangeSceneToPacked(GameScene);
        }
    }

}
