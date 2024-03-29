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
    
    private float throwForce = 12;
    private float throwVelocity;
   

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
            Debug.Log("Actual landing position: " + gameObject.transform.position);
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

    public override void OnDropThrow()
    {
        Vector3 launchDirection = (Grabber.FaceDirection + Vector3.up).normalized;
        DrawThrowLine(launchDirection, Grabber.transform.position);
        Rigidbody.AddForce(throwForce * launchDirection, ForceMode.Impulse);
    }

    private void DrawThrowLine(Vector3 launchDirection, Vector3 playerPos)
    {
        var points = SimulateThrowLine(launchDirection, playerPos);
        lineRenderer.positionCount = points.Count();

        for(int i = 0;i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }
    private List<Vector3> SimulateThrowLine(Vector3 launchDirection, Vector3 playerPos)
    {
        List<Vector3> linePoints = new List<Vector3>();
        float maxDuration = 5f;
        float timeCheckTick = 0.1f;
        int maxTicks = (int)(maxDuration/timeCheckTick);
        throwVelocity = throwForce / Rigidbody.mass * Time.deltaTime;

        for(int i = 0; i < maxTicks; i++)
        {
            //f(t) = (x0 + x*t, y0 + y*t - 9.81t²/2)
            Vector3 calculatedPos = playerPos + launchDirection * throwVelocity * i * timeCheckTick;
            calculatedPos.y += Physics.gravity.y / 2 * MathF.Pow(i * timeCheckTick, 2);

            linePoints.Add(calculatedPos);

            RaycastHit hit;
            if(Physics.Raycast(calculatedPos, Vector3.down, out hit, Mathf.Infinity, levelMask))
            {
                break;
            }
        }
        return linePoints;
    }

    //public override void OnDropThrow()
    //{
    //    Vector3 launchDirection = (Grabber.FaceDirection + Vector3.up).normalized;
    //    if (CheckIfCanThrow(throwForce))
    //    {
    //        Rigidbody.AddForce(throwForce * launchDirection, ForceMode.Impulse);
    //    }
    //    else
    //    {
    //        throwForce++;
    //        OnDropThrow();
    //    }

    //}

    //private bool CheckIfCanThrow(int force)
    //{
    //    Vector3 launchDirection = (Grabber.FaceDirection + Vector3.up).normalized;
    //    //landing pos är 2 för lite om man kastar i positiv z axel och 2 för mycket om man kastar i negativ z axel
    //    Vector3 landingPosition = transform.position + CalculateLandingPosition(launchDirection, force);

    //    RaycastHit hit;
    //    if (Physics.Raycast(landingPosition, Vector3.down, out hit, Mathf.Infinity, levelMask))
    //    {
    //        Debug.Log("Hit a collider: " + "actual landing position: " + gameObject.transform.position + " estimation: " + landingPosition + " an colliderPos: " + hit.transform.position);

    //        return false;
    //    }
    //    else
    //    {
    //        Debug.Log("Did not hit a collider pos: " + landingPosition);

    //        return true;
    //    }
    //}

    //private Vector3 CalculateLandingPosition(Vector3 launchDirection, int force)
    //{
    //    Vector3 initialVelocity = force * launchDirection;

    //    float timeOfFlight = (2 * initialVelocity.y) / Mathf.Abs(Physics.gravity.y);

    //    float horizontalDistance = initialVelocity.x * timeOfFlight;

    //    float lateralDistance = initialVelocity.z * timeOfFlight;

    //    Vector3 landingOffset = new Vector3(horizontalDistance, 0f, lateralDistance);

    //    return landingOffset;
    //}


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
