using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [SerializeField] private float _groundDecel = 0.1f;
    [SerializeField] protected Rigidbody _body;
    [SerializeField] private Vector3 _spawnPointOffset;
    [SerializeField] private float _timeToLive = 2.5f;
    [SerializeField] private float _despawnTime = 1.5f;
    [SerializeField] protected float _speed = 10f;

    private bool _affectedByGravity = false;

    protected EasyTimer _ttlTimer;
    protected EasyTimer _despawnTimer;
    protected bool _despawning = false;
    protected bool _launched = false;
    protected bool _isLethal = false;

    public FishGun FishGun { get; set; }

    public Vector3 Direction { get; protected set; } = Vector3.zero;

    public float TimeToLive
    { 
        get { return _timeToLive; }
        set
        { 
            _timeToLive = value;
            _ttlTimer = new EasyTimer(value);
        }
    }
    public float DespawnTime
    {
        get { return _despawnTime; }
        set
        {
            _despawnTime = value;
            _despawnTimer = new EasyTimer(value);
        }
    }
    public bool AffectedByGravity
    {
        get
        {
            return _affectedByGravity;
        }
        set
        {
            _affectedByGravity = value;
        }
    }
    public float ProjectileSpeed
    { get { return _speed; } set { _speed = value; } }


    public virtual void Launch(Vector3 position, Vector3 direction)
    {
        _isLethal = true;
        _launched = true;
        _despawning = false;
        gameObject.SetActive(true);
        _ttlTimer.Reset();
        if (AffectedByGravity)
            _body.useGravity = true;
        else
            _body.useGravity = false;
        _body.velocity = Vector3.zero;
        _body.transform.position = position;
        _body.AddForce(direction * _speed, ForceMode.Impulse);
        OnLaunch();
    }

    public virtual void OnLaunch() { }

    void Awake()
    {
        _ttlTimer = new EasyTimer(_timeToLive, false, true);
        _despawnTimer = new EasyTimer(_despawnTime, false, true);
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!_launched) return; 

        if (_ttlTimer.Done && !_despawning)
        {
            _despawning = true;
            _despawnTimer.Reset();
        }
        if (_despawning)
        {
            Despawn(1 - _despawnTimer.Ratio);
        }
        if (_despawnTimer.Done && _despawning)
        {
            _despawning = false;
            _launched = false;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Ratio is the ratio of total despawning time: When starting to despawn, it will be 0 and when despawning time is complete, it will be 1.
    /// </summary>
    /// <param name="ratio"></param>
    public virtual void Despawn(float ratio)
    {
        transform.localScale = Vector3.one * ratio;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _body.useGravity = true;
        _isLethal = false;
    }
}
