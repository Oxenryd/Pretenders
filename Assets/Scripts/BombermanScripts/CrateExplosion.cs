using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateExplosion : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Collider _collider;
    [SerializeField] private ParticleSystem _explosion;

    public void Explode()
    {
        _renderer.enabled = false;
        _collider.enabled = false;
        _explosion.Play();
    }

}
