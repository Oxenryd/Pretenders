using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public record PtSong
{
    private float _loopStart = 0f;

    public AudioClip Clip { get; set; }
    public bool Looping { get; set; } = true;
    public float LoopStart
    { get { return _loopStart; } set { _loopStart = value; } }
    public float Beginning
    {
        get
        {
            if (StartFromLoopStart)
                return _loopStart;
            else
                return 0f;
        }
    }
    public float LoopEnd { get; set; } = 0f; 
    public bool StartFromLoopStart { get; set; } = false;

    public PtSong(AudioClip clip, float loopStart, float loopEnd, bool startFromLoopStart, bool looping)
    {
        Clip = clip;
        LoopStart = loopStart;
        LoopEnd = loopEnd;
        StartFromLoopStart = startFromLoopStart;
        Looping = looping;
    }
}
