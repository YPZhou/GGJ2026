using Godot;
using System;
using System.Collections.Generic;

public partial class ControlTips : Control
{
    [Export] Texture2D[] texture2Ds;

    [Export] TextureRect BtnW;
    [Export] TextureRect BtnA;
    [Export] TextureRect BtnS;
    [Export] TextureRect BtnD;


    public override void _Process(double delta)
    {
        BtnW.Texture = Input.IsKeyPressed(Key.W) ? texture2Ds[1] : texture2Ds[0];
        BtnA.Texture = Input.IsKeyPressed(Key.A) ? texture2Ds[3] : texture2Ds[2];
        BtnS.Texture = Input.IsKeyPressed(Key.S) ? texture2Ds[5] : texture2Ds[4];
        BtnD.Texture = Input.IsKeyPressed(Key.D) ? texture2Ds[7] : texture2Ds[6];
    }
}
