using Godot;
using System;

public partial class ResultControl : Control
{
    [Export]
    TextureButton restartButton;

    [Export]
    TextureButton backToMainMenuButton;

    [Export]
    TextureRect winTexture;

    [Export]
    TextureRect failTexture;

    [Export]
    AudioStreamPlayer2D WinSFX;

    [Export]
    AudioStreamPlayer2D FaildSFX;

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

            AudioManager.Instance.PauseBGM();

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
    }

    public void ShowResultUI(bool win)
    {
        Visible = true;
        winTexture.Visible = win;
        failTexture.Visible = !win;
        if (win)
        {
            WinSFX.Play();
        }
        else
        {
            FaildSFX.Play();
        }
        //GD.Print($"{nameof(ResultControl)}: Show result UI. {(win ? "Win" : "Lose")}");
    }
}
