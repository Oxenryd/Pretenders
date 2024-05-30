/// <summary>
/// Class for storing and activating effects, that can be provided through powerups, for example.
/// </summary>
public class Effect
{
    private static Effect _noEffect = null;

    private EasyTimer _activeTimer;

    public bool Expires { get; set; } = true;
    public bool Active { get; private set; } = false;
    public float BumbMultiplier
    { get; set; } = 1f;
    public float ShoveMultiplier
    { get; set; } = 1f;
    public float MoveSpeedMultiplier
    { get; set; } = 1f;
    public float TugPowerMultiplier
    { get; set; } = 1f;
    public float JumpPowerMultiplier
    { get; set; } = 1f;
    public int ExtraDoubleJumps
    { get; set; } = 0;
    public float StrugglePowerMultiplier
    { get; set; } = 1f;
    public bool StunImmune
    { get; set; } = false;
    public int ExtraBombs
    { get; set; } = 0;
    public int ExtraBombRange
    { get; set; } = 0;
    public float EffectDuration
    {
        get
        {
            return _activeTimer.Time;
        }
        set
        {
            _activeTimer = new EasyTimer(value);
        }
    }
    public float DurationMultiplier
    { get; set; } = 1f;

    public EasyTimer ActiveTimer
    { get { return _activeTimer; } }

    public Effect()
    {
        _activeTimer = new EasyTimer(GlobalValues.EFFECT_DEFAULT_DURATION, false, true);
        GameManager.Instance.EarlyFixedUpdate += OnFixedUpdate;
    }

    private void OnFixedUpdate(object sender, float e)
    {
        if (!Active) return;

        if (_activeTimer.Done)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        Active = false;
    }

    public void Activate()
    {
        Active = true;
        _activeTimer.Reset();
    }

    public Effect CurrentEffects()
    {
        if (!Active)
            return DefaultEffect();
        else
            return this;
    }

    public static Effect DefaultEffect()
    {
        if (_noEffect == null)
            _noEffect = new Effect();

        return _noEffect;
    }
}