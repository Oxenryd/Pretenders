using System;
using UnityEngine;

public enum DefinedPowerupType
{
    None,
    WeightGain,
    Movespeed_33,
    MegaShove,
    Jump_50
}

public class DefinedPowerUp
{
    public DefinedPowerupType Type;
    public GameObject preFab
    {  
        get
        {
            switch(Type)
            {
                case DefinedPowerupType.WeightGain:
                    return GameManager.Instance.PowerUpPrefabs[0];
                case DefinedPowerupType.Movespeed_33:
                    return GameManager.Instance.PowerUpPrefabs[1];
                case DefinedPowerupType.MegaShove:
                    return GameManager.Instance.PowerUpPrefabs[2];
                case DefinedPowerupType.Jump_50:
                    return GameManager.Instance.PowerUpPrefabs[3];
                default: return null;
            }
        }
    }
    public Effect Effect
    {
        get
        {
            switch (Type)
            {
                case DefinedPowerupType.WeightGain:
                    return _effectList[0];
                case DefinedPowerupType.Movespeed_33:
                    return _effectList[1];
                case DefinedPowerupType.MegaShove:
                    return _effectList[2];
                case DefinedPowerupType.Jump_50:
                    return _effectList[3];
                default: return Effect.DefaultEffect(); ;
            }
        }
    }

    public DefinedPowerUp(DefinedPowerupType type)
    { this.Type = type; }

    private static Effect[] _effectList = new Effect[]
    {
        new Effect() {ShoveMultiplier = 1.25f},
        new Effect() {MoveSpeedMultiplier = 1.33f},
        new Effect() {ShoveMultiplier = 2f},
        new Effect() {JumpPowerMultiplier = 1.5f}
    };
}

