using Godot;
using System;

public partial class AudioManager : Control
{
    public static AudioManager Instance { get; private set; }

    [Export] AudioStreamPlayer2D BGM;

    private bool _init = false;

    public override void _Ready()
    {
        Instance = this;
    }

    public void PlayBGM(bool replay = false)
    {
        if (replay || !_init)
        {
            BGM?.Play();
            _init = true;
        }
        else
        {
            BGM.StreamPaused = false;
        }
    }

    public void PauseBGM()
    {
        if (BGM != null) 
        {
            BGM.StreamPaused = true;
        }
    }

    public void StopBGM() 
    {
        if (BGM != null)
        {
            BGM.Playing = false;
        }
    }

}
