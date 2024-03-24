using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishGun : Grabbable
{
    [SerializeField] private FishProjectile _fishProjectilePrefab;
    [SerializeField] private CooldownClock _clock;
    [SerializeField] private Vector3 _projPoint = new Vector3(0, 0, 0);
    private FishProjectile[] _projectilePool = new FishProjectile[40];

    private int _currentPoolIndex = 0;
    private float _cooldown = 15f;
    private EasyTimer _cooldownTimer;

    public float Cooldown
    {
        get { return _cooldown; }
        set
        { 
            _cooldown = value;
            _cooldownTimer = new EasyTimer(value);
        }
    }

    void Awake()
    {
        base.Awake();
        _cooldownTimer = new EasyTimer(_cooldown);

        for (int i = 0; i < _projectilePool.Length; i++)
        {
            _projectilePool[i] = Instantiate(_fishProjectilePrefab, transform);
            var fishRandom = Random.Range(0, 1000);
            var fishType = FishTypesEnum.Small;
            if (fishRandom >= 900)
                fishType = FishTypesEnum.Shark;
            else if (fishRandom >= 700)
                fishType = FishTypesEnum.Large;
            else if (fishRandom >= 450)
                fishType = FishTypesEnum.Medium;

            _projectilePool[i].InitFish(fishType);
        }
    }

    void Update()
    {
        base.Update();


    }
}