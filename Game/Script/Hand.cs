using Godot;
using System;

public partial class Hand : Sprite2D
{
    [Export]
    double handMoveSpeed = 280; // 手移动速度

    [Export]
    Area2D interactArea;

    void CheckRes()
    {
        if (interactArea == null)
        {
            GD.PrintErr($"{nameof(Hand)}: Interact area is null, set Interact area for Hand.interactArea in the editor.");
        }
    }

    public override void _Ready()
    {
        base._Ready();

        interactArea.AreaEntered += OnAreaEntered;
        interactArea.AreaExited += OnAreaExited;

        CheckRes();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // 更新位置
        Position += (float)handMoveSpeed * (float)delta * ProcessInput(delta);

        var viewportRect = GetViewportRect();
        Position = Position.Clamp(viewportRect.Position, viewportRect.End);
    }

    static Vector2 ProcessInput(double delta)
    {
        Vector2 direction = Vector2.Zero;

        if (Input.IsKeyPressed(Key.W)) direction.Y -= 1;
        if (Input.IsKeyPressed(Key.S)) direction.Y += 1;
        if (Input.IsKeyPressed(Key.A)) direction.X -= 1;
        if (Input.IsKeyPressed(Key.D)) direction.X += 1;

        if (direction != Vector2.Zero)
        {
            direction = direction.Normalized();
        }

        return direction;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetParent()?.GetParent() is Cat cat)
        {
            GD.Print("Touching Cat Head!");
            cat.SetMasked(true);
        }
    }

    private void OnAreaExited(Area2D area)
    {
        if (area.GetParent()?.GetParent() is Cat cat)
        {
            GD.Print("Stop Touching Cat Head.");
            cat.SetMasked(false);
        }
    }

}
