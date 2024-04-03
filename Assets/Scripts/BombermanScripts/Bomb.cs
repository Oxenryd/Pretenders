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

    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float timeBetweenPoints = 0.1f;

    [SerializeField]
    [Range(10, 100)]
    private int linePoints = 25;

    private float throwForce = 22;

    private bool canThrow = true;

    private Vector3 launchDirection;

    private int currentXplosion = 0;

    private int amountOfExplosions = 5;


    private EasyTimer timer;
    private EasyTimer detonationTickTimer;
    private bool hasDetonated = false;
    private Dictionary<Vector3, bool> directions = new Dictionary<Vector3, bool>() { { Vector3.back, false }, { Vector3.forward, false }, { Vector3.left, false }, { Vector3.right, false } };
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
            currentXplosion++;
            for (int i = 0; i < directions.Count; i++)
            {
                var directionKey = directions.Keys.ElementAt(i);
                if (!directions[directionKey])
                {
                    ExplosionCheckNearby(directionKey, currentXplosion);
                }
            }
            if (currentXplosion >= amountOfExplosions)
            {
                SetInactive();
            }
            detonationTickTimer.Reset();
        }
        if (IsGrabbed)
        {
            DrawTrajectory();
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
        if (canThrow)
        {
            Rigidbody.AddForce(launchDirection * throwForce, ForceMode.Impulse);
        }
        lineRenderer.positionCount = 0;
    }

    private void DrawTrajectory()
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        Vector3 startPosition = transform.position;
        Vector3 startVelocity = throwForce * (Grabber.FaceDirection + Vector3.up).normalized / Rigidbody.mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);
            lineRenderer.SetPosition(i, point);

            Vector3 lastPosition = lineRenderer.GetPosition(i - 1);
            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, levelMask))
            {
                canThrow = false;
                lineRenderer.SetPosition(i, hit.point);
                lineRenderer.positionCount = 1 + 1;
                return;
            }
            else
            {
                canThrow = true;
            }


            if (point.y <= 0f)
            {
                break;
            }
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
