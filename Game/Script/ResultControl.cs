using Godot;
using System;

public partial class ResultControl : Panel
{
    [Export]
    Button okButton;

    public Button OkButton
    {
        get { return okButton; }
    }
}
