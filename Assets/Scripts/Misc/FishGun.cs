using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class FishGun : Grabbable
{
    [SerializeField] private FishProjectile _fishProjectilePrefab;
    [SerializeField] private CooldownClock _clock;
    [SerializeField] private Vector3 _projPoint = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _firePoint = new Vector3(0, 0, 0);
    [SerializeField] private FishProjectile[] _projectilePool = new FishProjectile[40];
    [SerializeField] private float _cooldown = 15f;

    [SerializeField] private LineRenderer _line;
    [SerializeField] private SpriteRenderer _cross;
    [SerializeField] private Vector3 _crossOffset;

    [SerializeField] private bool _allSharks = false;

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
            if (!_allSharks)
            {               
                var fishRandom = Random.Range(0, 1000);
                var fishType = FishTypesEnum.Small;
                if (fishRandom >= 750)
                    fishType = FishTypesEnum.Shark;
                else if (fishRandom >= 600)
                    fishType = FishTypesEnum.Large;
                else if (fishRandom >= 300)
                    fishType = FishTypesEnum.Medium;

                _projectilePool[i].InitFish(fishType);
            } else
            {
                _projectilePool[i].InitFish(FishTypesEnum.Shark);
            }

            _projectilePool[i].SetLExcludeLayerMask(this.gameObject.layer);
            _projectilePool[i].FishGun = this;
        }
    }

    public void ResetCooldown()
    {
        _cooldownTimer.SetOff();
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
        _clock.Activate(transform, _cooldown);
    }

    void Update()
    {
        base.Update();

        _clock.Value = _cooldownTimer.Ratio;

        if (!IsGrabbed)
        {
            _cross.enabled = false;
            _line.positionCount = 0;
            return;
        }

        if (_projectilePool[_currentPoolIndex].FishType != FishTypesEnum.Shark)
        {
            _cross.enabled = false;
            var point = Grabber.transform.position + Grabber.transform.rotation * _firePoint;
            if (Physics.Raycast(point, Grabber.FaceDirection, out RaycastHit hit, 100f))
            {
                if (hit.collider.gameObject.layer != GlobalValues.GROUND_LAYER && !hit.collider.isTrigger)
                {
                    _line.positionCount = 2;
                    _line.SetPosition(0, point);
                    _line.SetPosition(1, hit.point);
                } else
                {
                    _line.positionCount = 2;
                    _line.SetPosition(0, point);
                    _line.SetPosition(1, point + Grabber.FaceDirection * 100f);
                }
            } else
            {
                _line.positionCount = 2;
                _line.SetPosition(0, point);
                _line.SetPosition(1, point + Grabber.FaceDirection * 100f);
            }
        } else
        {
            _cross.enabled = true;
            _cross.transform.position = (transform.position + transform.rotation * _crossOffset);
        }


    }

    public override bool TriggerEnter()
    {
        if (_cooldownTimer.Done)
        {
            _projectilePool[_currentPoolIndex].Launch(Grabber.transform.position + Grabber.transform.rotation * _firePoint, Grabber.FaceDirection);
            _currentPoolIndex++;
            _cooldownTimer.Reset();
            Grabber.TryBump(-Grabber.FaceDirection, GlobalValues.CHAR_BUMPFORCE * 0.66f);
            _clock.Activate(this.transform, _cooldown);
            return true;
        }

        return false;

    }
}