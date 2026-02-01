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

    [Export]
    AudioStreamPlayer2D StarFX;
    [Export]
    AudioStreamPlayer2D CatFX;
    [Export]
    AudioStreamPlayer2D OpenDoor_SFX;

	[Export]
	Polygon2D startButtonPolygon;
	[Export]
	Texture2D startButtonTexture_Normal;
	[Export]
	Texture2D startButtonTexture_Hover;

    [Export]
    AnimationPlayer TitleAnimPlayer;
    [Export]
    TextureButton ExitBtn;

	[Export]
	TextureRect tutorial;

    [Export]
    Button btn_creditor;

    [Export]
    Control Staff;


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

		tutorial.Visible = false;
		startButtonPolygon.Texture = startButtonTexture_Normal;
        area2D.InputPickable = true;
        area2D.MouseEntered += () =>
        {
			tutorial.Visible = true;
			startButtonPolygon.Texture = startButtonTexture_Hover;
            MouseEntered = true;
            Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Arrow, new Vector2(16, 16));
        };
        area2D.MouseExited += () =>
        {
			tutorial.Visible = false;
			startButtonPolygon.Texture = startButtonTexture_Normal;
            MouseEntered = false;
            Input.SetCustomMouseCursor(null);
        };

        StarFX.Finished += () =>
        {
            TitleAnimPlayer.Play("start");
        };

        ExitBtn.Pressed += Exit_Game;

        OpenDoor_SFX.Finished += () =>
        {
            GetTree().Quit();
        };

        btn_creditor.Pressed += () =>
        {
            Staff.Visible = true;
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
			startButtonPolygon.Visible = false;
            Input.SetCustomMouseCursor(null);
            StarFX.Play();
        }
    }

    public void ChangScene()
    {
        GetTree().ChangeSceneToPacked(GameScene);
    }

    public void Exit_Game()
    {
        OpenDoor_SFX.Play();
    }

}
