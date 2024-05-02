using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishGun : Grabbable
{
    [SerializeField] private FishProjectile _fishProjectilePrefab;
    [SerializeField] private CooldownClock _clock;
    [SerializeField] private Vector3 _projPoint = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _firePoint = new Vector3(0, 0, 0);
    [SerializeField] private FishProjectile[] _projectilePool = new FishProjectile[40];
    [SerializeField] private float _cooldown = 15f;

    private int _currentPoolIndex = 0;
    
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
        UseGunAnimation = true;
        _cooldownTimer = new EasyTimer(_cooldown);
        _cooldownTimer.Reset();
        for (int i = 0; i < _projectilePool.Length; i++)
        {
            _projectilePool[i] = Instantiate(_fishProjectilePrefab);
            var fishRandom = Random.Range(0, 1000);
            var fishType = FishTypesEnum.Small;
            if (fishRandom >= 900)
                fishType = FishTypesEnum.Shark;
            else if (fishRandom >= 700)
                fishType = FishTypesEnum.Large;
            else if (fishRandom >= 450)
                fishType = FishTypesEnum.Medium;

            _projectilePool[i].InitFish(fishType);
            _projectilePool[i].SetLExcludeLayerMask(this.gameObject.layer);
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

        _clock.Value = _cooldownTimer.Ratio;
    }

    public override bool TriggerEnter()
    {
        if (_cooldownTimer.Done)
        {
            _projectilePool[_currentPoolIndex].Launch(Grabber.GroundPosition + Grabber.transform.rotation * _firePoint, Grabber.FaceDirection);
            _currentPoolIndex++;
            _cooldownTimer.Reset();
            Grabber.TryBump(-Grabber.FaceDirection, GlobalValues.CHAR_BUMPFORCE * 0.66f);
            _clock.Activate(this.transform, _cooldown);
            return true;
        }

        return false;

    }
}