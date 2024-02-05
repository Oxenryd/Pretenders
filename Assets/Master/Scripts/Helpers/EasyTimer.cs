using System;

/// <summary>
/// Class for quick to setup and super simple to use Timers & Counters.
/// </summary>
public class EasyTimer
{
    private float _counter = 0;
    private float _time = 1f;
    private bool _countDown = false;
    private bool _onTime = false;
    private bool _pendingReset = false;


    public event EventHandler<float> Timed;
    /// <summary>
    /// Invoked when the timer reaches its set time.
    /// </summary>
    /// <param name="deltaTime"></param>
    protected void OnTimerDone(float deltaTime)
    {
        if (Timed == null) return;
        Timed.Invoke(this, deltaTime);
    }

    /// <summary>
    /// Returns a ratio between 0-1 how far into its current cycle this timer currently is at.
    /// </summary>
    public float Ratio
    {
        get
        {
            if (Done)
                return 1f;

            if (!_countDown)
                return _counter / _time;
            if (_counter >= _time && !_countDown)
                return 1f;

            if (_counter <= 0)
                return 1f;
            return 1 - (_counter / _time);
        }
    }

    /// <summary>
    /// Reset the timer to its starting state.
    /// </summary>
    public void Reset()
    {
        if (_countDown)
        {
            _counter = _time;
        } else
        {
            _counter = 0;
        }

        _onTime = false;
        _pendingReset = false;
    }
    public bool Done
    {
        get { return _onTime; }
    }
    public bool PendingReset
    { get { return _pendingReset; } }
    public bool Enabled
    { get; set; } = true;
    public float Counter
    { get { return _counter; } set { _counter = value; } }

    public float Time
    { get { return _time; } set { _time = value; } }

    public bool CountingDownwards
    { get { return _countDown; } set { _countDown = value; } }

    /// <summary>
    /// Class for quick to setup and super simple to use Timers & Counters.
    /// </summary>
    /// <param name="time"></param>
    public EasyTimer(float time) : this(time, false) { }
    /// <summary>
    /// Class for quick to setup and super simple to use Timers & Counters.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="countingDown"></param>
    public EasyTimer(float time, bool countingDown)
    {
        if (countingDown)
        {
            _countDown = true;
            _counter = _time;
        } else
        {
            _counter = 0;
        }
        _time = time;
    }

    /// <summary>
    /// Method to use with GameManagers EarlyUpdate Event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TickSubscription(object sender, float e)
    { Tick(e); }

    /// <summary>
    /// Force the "Done" state at next Tick().
    /// </summary>
    public void SetOff()
    {
        if (_countDown)
            _counter = 0;
        else
            _counter = _time;
    }

    /// <summary>
    /// Timer must be updated manually in unity for now.
    /// </summary>
    /// <param name="e"></param>
    public void Tick(float e)
    {
        if (!Enabled)
            return;

        if (_countDown && !_pendingReset)
        {
            _counter -= e;
            if (_counter <= 0)
            {
                if (!_pendingReset)
                    OnTimerDone(e);
                _pendingReset = true;
                _onTime = true;
                return;
            }
        } else if (!_pendingReset)
        {
            _counter += e;
            if (_counter >= _time)
            {
                if (!_pendingReset)
                    OnTimerDone(e);
                _pendingReset = true;
                _onTime = true;
                return;
            }
        }
    }
}

