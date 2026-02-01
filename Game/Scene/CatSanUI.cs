using Godot;
using System;

public partial class CatSanUI : Control
{
    [Export]
    TextureProgressBar progressBar;

    public override void _Ready()
    {
        base._Ready();
        if (progressBar is null)
        {
            GD.PrintErr($"{nameof(CatSanUI)}: progressBar is not set, set progressBar in the editor.");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void UpdateProgress(double progress)
    {
        progressBar.Value = progress;
    }

}
