using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishProjectile : ProjectileBase
{
    [SerializeField] private GameObject[] _projectileBases = new GameObject[4];

    public bool Active { get; private set; } = false;
    public FishTypesEnum FishType
    { get; private set; }

    public void SetLExcludeLayerMask(params int[] layers)
    {
        _body.excludeLayers = LayerUtil.Include(layers);
    }

    public GameObject InitFish(FishTypesEnum fishType)
    {
        FishType = fishType;
        var fishInt = (int)fishType;
        var index = fishInt > 3 ? 3 : fishInt < 0 ? 0 : fishInt;

        switch (FishType)
        {
            case FishTypesEnum.Small:
            case FishTypesEnum.Medium:
            case FishTypesEnum.Large:
                AffectedByGravity = false;
                _speed = 50f;
                break;
            case FishTypesEnum.Shark:
                _speed = 30f;
                AffectedByGravity = true;
                break;
        }

        // TODO: Set different stats for differents fish types
        return Instantiate(_projectileBases[index], this.transform);
    }
}
