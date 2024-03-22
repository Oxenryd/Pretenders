using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private EasyTimer lifeDuration;
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
