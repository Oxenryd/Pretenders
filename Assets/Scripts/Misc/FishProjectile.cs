using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishProjectile : ProjectileBase
{
    [SerializeField] private GameObject[] _projectileBases = new GameObject[4];

    public FishTypesEnum FishType
    { get; private set; }

    public GameObject InitFish(FishTypesEnum fishType)
    {
        FishType = fishType;
        var fishInt = (int)fishType;
        var index = fishInt > 3 ? 3 : fishInt < 0 ? 0 : fishInt;

        // TODO: Set different stats for differents fish types


        return Instantiate(_projectileBases[index], this.transform);
    }
}
