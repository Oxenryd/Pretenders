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

    public bool ReadyToFire
    { get { return _cooldownTimer.Done; } }

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
        _cooldownTimer.Reset();
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

    private void OnCooldownOver(object sender, System.EventArgs e)
    {
        if (!IsGrabbed)
            Rigidbody.AddForce(Vector3.up * 3f);
    }

    void Start()
    {
        base.Start();
        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _clock = Instantiate(_clock, container.transform);
        _clock.gameObject.SetActive(false);

        _clock.CooldownDone += OnCooldownOver;
        _clock.PositionOffset = new Vector3(0f, 1.3f, 0);
        _clock.MaxBaseAlpha = 1f;
    }

    void Update()
    {
        base.Update();
        
        if (!_cooldownTimer.Done)
        {
            if (!_clock.Active)
                _clock.Activate(this.transform, _cooldown);
            
            _clock.Value = _cooldownTimer.Ratio;
        }

    }

    public override bool TriggerEnter()
    {
        if (_cooldownTimer.Done)
        {
            Debug.Log("PANG");
            _cooldownTimer.Reset();
            Grabber.TryBump(-Grabber.FaceDirection, GlobalValues.CHAR_BUMPFORCE * 0.66f);
            return true;
        }

        return false;

    }
}