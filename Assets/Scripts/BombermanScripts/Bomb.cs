using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Bomb : MonoBehaviour
{

    //Flames

    [SerializeField]
    private GameObject explosion;

    [SerializeField]
    private LayerMask levelMask;

    [SerializeField]
    private float delayBeforeExplosion = 1;

    [SerializeField]
    private float delayAfterEachExplosion = 0.05f;

    private int currentXplosion = 0;

    private int amountOfExplosions = 5;


    private EasyTimer timer;
    private EasyTimer detonationTickTimer;
    private bool hasDetonated = false;
    private Dictionary<Vector3, bool> directions = new Dictionary<Vector3, bool>() { {Vector3.back, false }, { Vector3.forward, false }, { Vector3.left, false }, {Vector3.right, false } };
    public bool IsActive
    { get; set; } = false;

    void Awake()
    {
        timer = new EasyTimer(delayBeforeExplosion);
        detonationTickTimer = new EasyTimer(delayAfterEachExplosion);
    }
    public void SetInactive()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }
    public void PierresSpawn(Vector3 position)
    {
        
        IsActive = true;
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        timer.Reset();
    }

    void Update()
    {
        if (!IsActive) return;

        if (timer.Done && hasDetonated != true)
        {
             hasDetonated = true;
            detonationTickTimer.Reset();
        }
        if (hasDetonated && detonationTickTimer.Done)
        {
            currentXplosion++;
            for(int i  = 0; i < directions.Count; i++)
            {
                var directionKey = directions.Keys.ElementAt(i);
                if (!directions[directionKey])
                {
                    ExplosionCheckNearby(directionKey, currentXplosion);
                }
            }
            if ( currentXplosion >= amountOfExplosions)
            {
                SetInactive();
            }
            detonationTickTimer.Reset();
        }
        
    }
    public void SpawnBomb(Vector3 charPosition)
    {
        hasDetonated = false;
        timer.Reset();
        currentXplosion = 0;
        IsActive = true;
        gameObject.SetActive(true);
        gameObject.transform.position = charPosition;
    }

    private void ExplosionCheckNearby(Vector3 direction, int tick)
    {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, tick, levelMask);

            if (!hit.collider)
            {
               Instantiate(explosion, transform.position + (tick * direction), transform.rotation);
            }
            else
            {
                if (hit.collider.TryGetComponent<CrateExplosion>(out var crate))
                {
                    crate.Explode();
                    directions[direction] = true;
                }
                else
                {
                    directions[direction] = true;

                }
            }
    }


}
