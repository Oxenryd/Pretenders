using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private EasyTimer lifeDuration;

    /// <summary>
    /// This class handles the duration of the particle system effects of the bomb explosions
    /// It will destroy the particle effects after three seconds
    /// </summary>
    void Start()
    {
        lifeDuration = new EasyTimer(3);
    }
    void Update()
    {
        if(lifeDuration.Done)
        {
            Destroy(gameObject);
        }
    }
}
