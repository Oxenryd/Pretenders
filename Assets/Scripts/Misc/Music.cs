using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A music handling class that can fade in/out and crossfade between songs.
/// </summary>
public class Music : MonoBehaviour
{
    public static readonly int SILENCE = 0;
    public static readonly int OPENINGTHEME = 1;
    public static readonly int LOBBY = 2;
    public static readonly float DEFAULT_OVERHANG_TIME = 1.5f;


    [SerializeField] private AudioSource _channel0;
    [SerializeField] private AudioSource _channel1;
    
    private PtSong[] _songs;

    private EasyTimer _crossFadeTimer;
    private EasyTimer _overhangTimer;
    private EasyTimer _fadeOutTimer;
    private EasyTimer _fadeInTimer;

    private int _pendingSongindex = -1;
    private int _currentSongindex = -1;
    private AudioSource _mixingChannel;
    private AudioSource _mainOut;   
    private bool _crossfading = false;
    private bool _overhanging = false;
    private bool _fadeOut = false;
    private bool _fadeingIn = false;

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
        _fadeOutTimer = new EasyTimer(1f);
        _fadeInTimer = new EasyTimer(1f);
        _mainOut = _channel0;
        _mixingChannel = _channel1;
        _songs = new PtSong[]
        {
            new PtSong( (AudioClip)Resources.Load("Sounds/BGM/silence", typeof(AudioClip)), 0, 9f, false, false),
            new PtSong( (AudioClip)Resources.Load("Sounds/BGM/pretendersTheme", typeof(AudioClip)), 1.98f, 71.520f, true, true),
            new PtSong( (AudioClip)Resources.Load("Sounds/BGM/lobby", typeof(AudioClip)), 2.105f, 31.553f, false, true)
        };
        _currentSongindex = -1;
    }
    /// <summary>
    /// Fade out current playing song, taking 'fadeTime' in seconds time.
    /// </summary>
    /// <param name="fadeTime"></param>
    public void Fadeout(float fadeTime)
    {
        Crossfade(SILENCE, 0f, fadeTime);
    }

    /// <summary>
    /// Crossfades between currently playing song and passed in songIndex. Pass toTime to the starting time of next song and fadeTime for how long time the fade should take (in seconds).
    /// </summary>
    /// <param name="toSongIndex"></param>
    /// <param name="toTime"></param>
    /// <param name="fadeTime"></param>
    public void Crossfade(int toSongIndex, float toTime, float fadeTime)
    {
        if (_mainOut.clip == null)
        {
            _mainOut.clip = _songs[SILENCE].Clip;
            _currentSongindex = SILENCE;
        }
        _crossfading = true;
        _crossFadeTimer.Time = fadeTime;
        _crossFadeTimer.Reset();
        _mixingChannel.clip = _songs[toSongIndex].Clip;
        _mixingChannel.time = toTime;
        _mixingChannel.volume = 0;
        _mixingChannel.Play();
        _pendingSongindex = toSongIndex;
    }

    /// <summary>
    /// Just cut currently playing song and play song at index.
    /// </summary>
    /// <param name="toSongIndex"></param>
    public void PlayNow(int toSongIndex)
    { PlayNow(toSongIndex, _songs[toSongIndex].Beginning, 0f); }
    /// <summary>
    /// Stop current song and start playing passed in songindex at specific startAt time, take fadeInTime to complete the fade in.
    /// </summary>
    /// <param name="songIndex"></param>
    /// <param name="startAt"></param>
    /// <param name="fadeInTime"></param>
    public void PlayNow(int songIndex, float startAt, float fadeInTime)
    {
        _currentSongindex = songIndex;
        _stopAndReset();
        var song = _songs[songIndex];
        _mainOut.clip = song.Clip;
        _mainOut.time = startAt;
        if (fadeInTime == 0f)
            _mainOut.volume = Volume;
        else
        {
            _fadeingIn = true;
            _fadeInTimer.Time = fadeInTime;
            _fadeInTimer.Reset();
            _mainOut.volume = 0;
        }
        _mainOut.Play();
    }

    /// <summary>
    /// Forcefully stop music.
    /// </summary>
    public void StopNow()
    {
        _stopAndReset();
        _currentSongindex = -1;
    }

    private void Update()
    {
        if (_currentSongindex < 0) return;

        if (_fadeingIn)
        {
            _mainOut.volume = (Volume * _fadeInTimer.Ratio);
            if (_fadeInTimer.Done)
                _fadeingIn = false;
        }

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
                _currentSongindex = _pendingSongindex;
                _crossfading = false;
                _mainOut.Stop();
                swapChannels();
            }
        }
        
    }

    private void swapChannels()
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
