using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    public static readonly int OPENINGTHEME = 0;
    public static readonly float DEFAULT_OVERHANG_TIME = 1.5f;


    [SerializeField] private AudioSource _channel0;
    [SerializeField] private AudioSource _channel1;
    
    private PtSong[] _songs;

    private EasyTimer _crossFadeTimer;
    private EasyTimer _overhangTimer;

    private int _currentSongindex = -1;
    private AudioSource _mixingChannel;
    private AudioSource _mainOut;   
    private bool _crossfading = false;
    private bool _overhanging = false;

    public float Volume
    { get; set; } = 1f;

    public float LoopOverhangTime
    { get; set; } = DEFAULT_OVERHANG_TIME;
    public bool LoopWithOverHang
    { get; set; } = true;
    void Awake()
    {
        _crossFadeTimer = new EasyTimer(1f);
        _overhangTimer = new EasyTimer(LoopOverhangTime);
        _mainOut = _channel0;
        _mixingChannel = _channel1;
        _songs = new PtSong[]
        {
            new PtSong( (AudioClip)Resources.Load("Sounds/BGM/pretendersTheme", typeof(AudioClip)), 1.98f, 71.520f, false, true)
        };
        _currentSongindex = -1;
    }

    public void Crossfade(int toSongIndex, float toTime, float fadeTime)
    {
        _crossfading = true;
        _crossFadeTimer.Time = fadeTime;
        _crossFadeTimer.Reset();
        _mixingChannel.clip = _songs[toSongIndex].Clip;
        _mixingChannel.time = toTime;
        _mixingChannel.volume = 0;
    }

    public void PlayNow(int songIndex)
    {
        _currentSongindex = songIndex;
        _stopAndReset();
        var song = _songs[songIndex];
        _mainOut.clip = song.Clip;
        _mainOut.time = song.Beginning;
        _mainOut.volume = Volume;
        _mainOut.Play();
    }

    public void StopNow()
    {
        _stopAndReset();
        _currentSongindex = -1;
    }

    private void Update()
    {
        if (_currentSongindex < 0) return;


        if (!_crossfading && _songs[_currentSongindex].Looping)
        {
            if (_mainOut.time >= _songs[_currentSongindex].LoopEnd)
            {
                if (LoopWithOverHang && !_overhanging)
                {
                    _overhanging = true;
                    _overhangTimer.Reset();
                    _mixingChannel.clip = _mainOut.clip;
                    _mixingChannel.volume = Volume;
                    _mixingChannel.time = _songs[_currentSongindex].LoopStart;
                    _mixingChannel.Play();
                }
                else
                    _mainOut.time = _songs[_currentSongindex].LoopStart;
            }
        }

        if (_overhanging && !_crossfading)
        {
            _mainOut.volume = Volume - (_overhangTimer.Ratio * Volume);
            if (_overhangTimer.Done)
            {
                _overhanging = false;
                _mainOut.Stop();
                _mainOut.time = _songs[_currentSongindex].Beginning;
                swapChannels();
            }
        }

        if (_crossfading)
        {
            _mixingChannel.volume = Volume * _crossFadeTimer.Ratio;
            _mainOut.volume = Volume - (Volume * _crossFadeTimer.Ratio);
            if (_crossFadeTimer.Done)
            {
                _crossfading = false;
                swapChannels();
            }
        }
        
    }

    private unsafe void swapChannels()
    {
        var swap = _mainOut;
        _mainOut = _mixingChannel;
        _mixingChannel = swap;
    }


    private void _stopAndReset()
    {
        if (_mainOut.isPlaying)
            _mainOut.Stop();
        if (_mixingChannel.isPlaying)
            _mixingChannel.Stop();

        _mainOut = _channel0;
        _mixingChannel = _channel1;
    }
}
