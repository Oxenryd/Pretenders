using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    private EasyTimer _activeTimer;

    public bool Active { get; private set; } = false;
    public float ShoveMultiplier
    { get; set; } = 1f;
    public float MoveSpeedMultiplier
    { get; set; } = 1f;
    public float TugPowerMultiplier
    { get;set; } = 1f;
    public float JumpPowerMultiplier
    { get;set; } = 1f;
    public float ShoveResistanceMultiplier
    { get;set; } = 1f;
    public int ExtraDoubleJumps
    { get; set; } = 0;
    public float StrugglePowerMultiplier
    { get; set; } = 1f;
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
        _activeTimer = new EasyTimer(5f, false, true);
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
        var noEffect = new Effect();
        return noEffect;
    }
}
