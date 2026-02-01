using Godot;
using System;
using Godot.Collections;

public partial class AudioManager : Control
{
    public enum EAudioSFX
    {
        Cat,
        TV,
    }

    public static AudioManager Instance { get; private set; }

    [Export] AudioStreamPlayer2D BGM;

    [Export] AudioStreamPlayer2D SFX;

    [Export] Dictionary<EAudioSFX, AudioStream> SFXs;

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

    public void PlaySFX(EAudioSFX esfx)
    {
        if (SFXs.TryGetValue(esfx, out AudioStream audioStream))
        {
            SFX.Stream = audioStream;
            SFX.Play();
        }
    }

}
