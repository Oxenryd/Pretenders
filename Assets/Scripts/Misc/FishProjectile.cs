using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores and sets up projectiles for the FishGun.
/// </summary>
public class FishProjectile : ProjectileBase
{
    [SerializeField] private GameObject[] _projectileBases = new GameObject[4];
    [SerializeField] private ParticleSystem[] _particles = new ParticleSystem[3];
    [SerializeField] private ParticleSystem _muzzle;
    private EasyTimer _growthTimer;
    private float _radius = GlobalValues.FISHGUN_SHARK_RADIUS;

    public bool Active { get; private set; } = false;
    public FishTypesEnum FishType
    { get; private set; }

    public void SetLExcludeLayerMask(params int[] layers)
    {
        _body.excludeLayers = LayerUtil.Include(layers);
    }

    /// <summary>
    /// Gets and sets up a projectile on its fish type.
    /// </summary>
    /// <param name="fishType"></param>
    /// <returns></returns>
    public GameObject InitFish(FishTypesEnum fishType)
    {
        _growthTimer = new EasyTimer(GlobalValues.FISHGUN_GROWTH_TIME);
        FishType = fishType;
        var fishInt = (int)fishType;
        var index = fishInt > 3 ? 3 : fishInt < 0 ? 0 : fishInt;

        switch (FishType)
        {
            case FishTypesEnum.Small:
            case FishTypesEnum.Medium:
            case FishTypesEnum.Large:
                AffectedByGravity = false;
                _speed = 48f;
                break;
            case FishTypesEnum.Shark:
                _speed = 15f;
                AffectedByGravity = true;
                break;
        }

        return Instantiate(_projectileBases[index], this.transform);
    }

    public override void Launch(Vector3 position, Vector3 direction)
    {
        Direction = direction;
        _body.transform.forward = new Vector3(direction.z, 0, -direction.x);
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
        if (FishType != FishTypesEnum.Shark)
        {        
            _body.AddForce(direction * _speed, ForceMode.Impulse);
            _growthTimer.Time = GlobalValues.FISHGUN_GROWTH_TIME;
        } else
        {
            _growthTimer.Time = GlobalValues.FISHGUN_GROWTH_TIME * 2f;
            _body.AddForce(new Vector3(direction.normalized.x, 1f, direction.normalized.z).normalized * _speed, ForceMode.Impulse);
            _body.detectCollisions = false;
        }
        _growthTimer.Reset();
        _muzzle.Stop();
        _muzzle.Play();
    }

    void Update()
    {
        if (!_despawning)
            transform.localScale = Vector3.one * _growthTimer.Ratio;

        if (FishType == FishTypesEnum.Shark && _growthTimer.Done)
            _body.detectCollisions = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_isLethal) return;

        if (FishType == FishTypesEnum.Shark)
        {
            foreach (var particle in _particles)
            {
                particle.Stop();
                particle.Play();
            }
            var hits = Physics.OverlapSphere(transform.position, _radius);
            foreach (var hit in hits)
            {
                var hero = hit.transform.GetComponentInParent<HeroMovement>();
                if (hero != null)
                {
                    var dir = hero.transform.position - transform.position;
                    hero.TryBlast(dir, GlobalValues.SHOVE_DEFAULT_SHOVEPOWER * 0.5f);
                    FishGun.ResetCooldown();
                }
            }
        } else
        {
            _body.useGravity = true;
            var hero = collision.transform.GetComponentInParent<HeroMovement>();
            if (hero != null)
            {
                hero.TryBlast(Direction, GlobalValues.SHOVE_DEFAULT_SHOVEPOWER * 0.18f);
                FishGun.ResetCooldown();
            }
        }

        _isLethal = false;
    }

    void OnDrawGizmos()
    {
        if (FishType == FishTypesEnum.Shark)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _radius);
        }
    }
}
