using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Bomb : Grabbable
{

    [SerializeField]
    private GameObject explosion;

    [SerializeField]
    private LayerMask levelMask;

    [SerializeField]
    private float delayBeforeExplosion = 10;

    [SerializeField]
    private float delayAfterEachExplosion = 0.05f;

    [SerializeField]
    private LineRenderer lineRenderer;
    
    private float throwForce = 22;
    private float throwVelocity;

    private Vector3 launchDirection;
   

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
        base.Awake();
        lineRenderer.startColor = Color.white;
        timer = new EasyTimer(delayBeforeExplosion);
        detonationTickTimer = new EasyTimer(delayAfterEachExplosion);
    }
    public void SetInactive()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }
    void Update()
    {
        base.Update();
        if (!IsActive) return;

        if (timer.Done && hasDetonated != true)
        {
             hasDetonated = true;
            detonationTickTimer.Reset();
        }
        if (hasDetonated && detonationTickTimer.Done)
        {
          //  Debug.Log("Actual landing position: " + gameObject.transform.position);
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
        if (IsGrabbed)
        {
            launchDirection = (Grabber.FaceDirection + Vector3.up).normalized;
            SimulateThrowLine();
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

    public override void OnDropThrow()
    {
        launchDirection = (Grabber.FaceDirection + Vector3.up).normalized;
        Rigidbody.AddForce(launchDirection * throwForce, ForceMode.Impulse);
        lineRenderer.positionCount = 0;
    }



    private void SimulateThrowLine()
    {
        lineRenderer.positionCount = 0;
        float maxDuration = 5f;
        float timeTick = 0.5f;
        throwVelocity = throwForce / Rigidbody.mass;

        for(float t = 0; t < maxDuration; t += timeTick)
        {
            Vector3 newPosition = transform.position + launchDirection * throwVelocity * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.positionCount++;

            if (newPosition.y <= 0f)
            {
                break;
            }
            Debug.Log(newPosition);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPosition);

            //RaycastHit hit;
            //if (Physics.Raycast(newPosition, Vector3.down, out hit, 2, levelMask))
            //{
            //    break;
            //}
        }
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
