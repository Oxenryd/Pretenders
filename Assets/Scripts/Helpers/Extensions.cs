using System;
using UnityEngine;
using static UnityEngine.UI.Image;

// This cs file contains multiple helpers and extension methods for various things.

public static class LayerUtil
{
    public static LayerMask Include(params int[] layerNumbers) 
    {
        LayerMask newMask = 0;
        return newMask.Include(layerNumbers); 
    }
    public static LayerMask Exclude(params int[] layerNumbers)
    {
        LayerMask newMask = ~0;
        return newMask.Exclude(layerNumbers);
    }

}

public static class LayerMaskExtensions
{
    public static LayerMask Include(this LayerMask original, params int[] layerNumbers)
    {
        foreach (var layer in layerNumbers)
        {
            original.value |= (1 << layer);
        }
        return original;
    }
    public static LayerMask Exclude(this LayerMask original, params int[] layerNumbers)
    {
        foreach (var layer in layerNumbers)
        {
            original.value &= ~(1 << layer);
        }
        return original;
    }
}

public static class TransformHelpers
{
    public static Quaternion FixNegativeZRotation(Vector3 from, Vector3 to)
    {
        Quaternion rotation;
        var diff = Math.Abs(to.z + 1f);
        if (diff >= 0.01f)
            rotation = Quaternion.FromToRotation(from, to);
        else
            rotation = Quaternion.Euler(0, -180, 0);
        return rotation;
    }

    public static Vector3 QuadDirQuantize(Vector3 inputDirection)
    {
        var outX = 0;
        var outZ = 0;
        if (inputDirection.z > GlobalValues.INPUT_DEADZONE) //0.7071f
            outZ = 1;   
        else if (inputDirection.z < -GlobalValues.INPUT_DEADZONE)
            outZ = -1;

        if (inputDirection.x > GlobalValues.INPUT_DEADZONE)
        {
            outX = 1;
            outZ = 0;
        }          
        else if (inputDirection.x < -GlobalValues.INPUT_DEADZONE)
        {
            outX = -1;
            outZ = 0;
        }

        return new Vector3(outX, 0, outZ);
    }

    public static void QuadDirVectorToPlayerInput(this PlayerInputEnum original, Vector3 inputDirection)
    {
        original &= ~PlayerInputEnum.DirectionUp;
        original &= ~PlayerInputEnum.DirectionLeft;
        original &= ~PlayerInputEnum.DirectionDown;
        original &= ~PlayerInputEnum.DirectionRight;

        if (inputDirection.z > 0.7071f)
            original |= PlayerInputEnum.DirectionUp;
        else if (inputDirection.z < -0.7071f)
            original |= PlayerInputEnum.DirectionDown;

        if (inputDirection.x > 0.7071f)
            original |= PlayerInputEnum.DirectionRight;

        else if (inputDirection.x < -0.7071f)
            original |= PlayerInputEnum.DirectionLeft;

    }


    public static bool PassedGridTarget(HeroMovement hero, Vector3 gridCenterTarget)
    {
        if (hero.FaceDirection.x > 0)
        {
            if (hero.GroundPosition.x >= gridCenterTarget.x) return true;
        } else if (hero.FaceDirection.x < 0)
            if (hero.GroundPosition.x <= gridCenterTarget.x) return true;

        if (hero.FaceDirection.z > 0)
        {
            if (hero.GroundPosition.z >= gridCenterTarget.z) return true;
        } else if (hero.FaceDirection.z < 0)
            if (hero.GroundPosition.z <= gridCenterTarget.z) return true;

        return false;
    }
}