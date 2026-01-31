using Godot;
using System;

public partial class Hand : Sprite2D
{
    [Export]
    double handMoveSpeed = 280; // 手移动速度

    public override void _Process(double delta)
    {
        base._Process(delta);

        // 2. 获取输入方向
        Vector2 direction = Vector2.Zero;

        // 使用物理按键检测 (W/A/S/D)
        if (Input.IsKeyPressed(Key.W)) direction.Y -= 1;
        if (Input.IsKeyPressed(Key.S)) direction.Y += 1;
        if (Input.IsKeyPressed(Key.A)) direction.X -= 1;
        if (Input.IsKeyPressed(Key.D)) direction.X += 1;

        // 3. 移动逻辑
        if (direction != Vector2.Zero)
        {
            // 归一化向量，避免斜向移动比直线快
            direction = direction.Normalized();
            
            // 更新位置
            Position += (float)delta * (float)handMoveSpeed * direction;
        }
        
         var viewportRect = GetViewportRect();
         Position = Position.Clamp(viewportRect.Position, viewportRect.End);
    }


}   
