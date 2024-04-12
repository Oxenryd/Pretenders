using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    [SerializeField] private Rigidbody _body;
    [SerializeField] private Vector3 _spawnPointOffset;
    [SerializeField] private float _timeToLive = 4f;
    [SerializeField] private float _despawnTime = 1f;
    [SerializeField] private float _speed = 10f;

    private EasyTimer _ttlTimer;
    private EasyTimer _despawnTimer;
    private bool _despawning = false;

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
    { get; set; } = true;
    public float ProjectileSpeed
    { get { return _speed; } set { _speed = value; } }


    public void Launch(Vector3 position, Vector3 direction)
    {
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
    }


    void Awake()
    {
        _ttlTimer = new EasyTimer(_timeToLive, false, true);
        _despawnTimer = new EasyTimer(_despawnTime, false, true);
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (_ttlTimer.Done)
        {
            _despawning = true;
            _despawnTimer.Reset();
        }
        if (_despawning)
        {
            Despawn(_despawnTimer.Ratio);
        }
        if (_despawnTimer.Done && _despawning)
        {
            _despawning = false;
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
}
