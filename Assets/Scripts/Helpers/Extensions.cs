using UnityEngine;

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