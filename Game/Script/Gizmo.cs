using Godot;
using System;

public partial class Gizmo : Node2D
{
    Vector2[] points;
    
    public Vector2[] Points 
    { 
        get => points; 
        set 
        { 
            points = value; 
            QueueRedraw(); 
        } 
    }

    public override void _Ready()
    {
        base._Ready();
        ZIndex = 1000;
    }
    public override void _Draw()
    {
        base._Draw();

        if (points != null)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(points[i], points[i + 1], Colors.Red, 2);
            }
        }
    }

    public void SetPoints(Vector2[] points)
    {
        this.points = points;
    }

}
