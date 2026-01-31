using Godot;
using System;

public partial class ResultControl : Panel
{
    [Export]
    Button okButton;
    [Export]
    Label resultLabel;
    [Export]
    TextureRect resultTextureRect;

    [Export]
    Texture2D winTexture;

    [Export]
    Texture2D loseTexture;

    public Button OkButton
    {
        get { return okButton; }
    }

    public override void _Ready()
    {
        base._Ready();

        CheckRes();
    }

    void CheckRes()
    {
        if (okButton == null)
        {
            GD.PrintErr($"{nameof(ResultControl)}: Ok button is null, set Ok button for ResultControl.okButton in the editor.");
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
