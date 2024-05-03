using System;
using UnityEngine;

public enum DefinedPowerupType
{
    None,
    WeightGain,
    Movespeed_33,
    MegaShove,
    Jump_50,
    BombPlus,
    BombRangePlus
}

public static class DefinedPowerUp
{
    public static GameObject GetPrefab(DefinedPowerupType type)
    {
        switch (type)
        {
            case DefinedPowerupType.WeightGain:
                return GameManager.Instance.PowerUpPrefabs[0];
            case DefinedPowerupType.Movespeed_33:
                return GameManager.Instance.PowerUpPrefabs[1];
            case DefinedPowerupType.MegaShove:
                return GameManager.Instance.PowerUpPrefabs[2];
            case DefinedPowerupType.Jump_50:
                return GameManager.Instance.PowerUpPrefabs[3];
            case DefinedPowerupType.BombPlus:
                return GameManager.Instance.PowerUpPrefabs[4];
            case DefinedPowerupType.BombRangePlus:
                return GameManager.Instance.PowerUpPrefabs[5];
            default: return null;
        }
    }
    public static Effect GetEffect(DefinedPowerupType type)
    {
        switch (type)
        {
            case DefinedPowerupType.WeightGain:
                return _effectList[0];
            case DefinedPowerupType.Movespeed_33:
                return _effectList[1];
            case DefinedPowerupType.MegaShove:
                return _effectList[2];
            case DefinedPowerupType.Jump_50:
                return _effectList[3];
            case DefinedPowerupType.BombPlus:
                return _effectList[4];
            case DefinedPowerupType.BombRangePlus:
                return _effectList[5];
            default: return Effect.DefaultEffect(); ;
        }      
    }

    private static Effect[] _effectList = new Effect[]
    {
        new Effect() {ShoveMultiplier = 1.25f},
        new Effect() {MoveSpeedMultiplier = 1.33f},
        new Effect() {ShoveMultiplier = 2f},
        new Effect() {JumpPowerMultiplier = 1.5f}, // 1.5
        new Effect() {ExtraBombs = 1 },
        new Effect() {ExtraBombRange = 1}
    };
}

