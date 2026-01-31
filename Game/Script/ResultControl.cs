using Godot;
using System;

public partial class ResultControl : Panel
{
    [Export]
    Button restartButton;

    [Export]
    Button backToMainMenuButton;


    [Export]
    Label resultLabel;
    [Export]
    TextureRect resultTextureRect;

    [Export]
    Texture2D winTexture;

    [Export]
    Texture2D loseTexture;

    public override void _Ready()
    {
        base._Ready();

        CheckRes();

        restartButton.Pressed += () =>
		{
			GD.Print($"{nameof(Game)}:重新开始");
			GetTree().ReloadCurrentScene();
		};

		backToMainMenuButton.Pressed += () =>
		{
			GD.Print($"{nameof(Game)}:返回主菜单");
			GetTree().ChangeSceneToFile("res://Scene/Title.tscn");
		};
    }

    void CheckRes()
    {
        if (backToMainMenuButton == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Back to main menu button is null, set Back to main menu button for ResultControl.backToMainMenuButton in the editor.");
        }
        if (restartButton == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Restart button is null, set Restart button for ResultControl.restartButton in the editor.");
        }
        if (resultLabel == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Result label is null, set Result label for ResultControl.resultLabel in the editor.");
        }
        if (winTexture == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Win texture is null, set Win texture for ResultControl.winTexture in the editor.");
        }
        if (loseTexture == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Lose texture is null, set Lose texture for ResultControl.loseTexture in the editor.");
        }
        if (resultTextureRect == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Result texture rect is null, set Result texture rect for ResultControl.resultTextureRect in the editor.");
        }
    }

    public void ShowResultUI(bool win)
    {
        Visible = true;
        if (win)
        {
            resultLabel.Text = "You Win!";
            resultTextureRect.Texture = winTexture;
        }
        else
        {
            resultLabel.Text = "You Lose!";
            resultTextureRect.Texture = loseTexture;
        }
        GD.Print($"{nameof(ResultControl)}: Show result UI. {(win ? "Win" : "Lose")}");
    }
}
