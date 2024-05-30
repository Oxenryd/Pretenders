using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateExplosion : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Collider _collider;
    [SerializeField] private ParticleSystem _explosion;

    /// <summary>
    /// This class handles the explosions of the individual crates.
    /// It disables the crate so it no longer blocks the player.
    /// It starts the explosion animation for the crate.
    /// </summary>
    public void Explode()
    {
        _renderer.enabled = false;
        _collider.enabled = false;
        _explosion.Play();
    }

}
