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

    private float throwForce = 5;

    private bool canThrow = true;

    private Vector3 launchDirection;

    private int currentXplosion = 0;

    private int amountOfExplosions = 5;

    private bool startedThrowProcess = false;

    private bool threwBomb = false;

    //Fixa stackoverflow exception när man klickar på J
    //Vänta med explosion när man kastat
    //Nolla när man släppt bomb trajectory på ett ställe man inte får kasta

    private EasyTimer timer;
    private EasyTimer detonationTickTimer;
    private bool hasDetonated = false;
    private Dictionary<Vector3, bool> directions = new Dictionary<Vector3, bool>() { { Vector3.back, false }, { Vector3.forward, false }, { Vector3.left, false }, { Vector3.right, false } };
    public bool IsActive
    { get; set; } = false;



    void Awake()
    {
        //base.Awake();
        KinematicByDefault = true;
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
            canThrow = true;
            if (IsGrabbed)
                Drop();
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

            if (Grabber.TriggerButtonDown)
            {
                DrawTrajectory();
                startedThrowProcess = true;
                var newValue = throwForce + 10f * GameManager.Instance.DeltaTime;
                throwForce = Mathf.Clamp(newValue, 0, 27);
            }
            if (startedThrowProcess && !Grabber.TriggerButtonDown)
            {
                threwBomb = true;
                startedThrowProcess = false;
                if (canThrow)
                {
                    Rigidbody.isKinematic = false;
                    detonationTickTimer.Reset();
                    Drop();
                }
                else if (!canThrow)
                {
                    throwForce = 5;
                    lineRenderer.positionCount = 0;
                }
            }
        }
        if (HitTheFloor())
        {
            Rigidbody.isKinematic = true;
            gameObject.transform.position = new Vector3(Mathf.RoundToInt(gameObject.transform.position.x / 2) * 2, 0, Mathf.RoundToInt(gameObject.transform.position.z / 2) * 2);
        }
    }
    private bool HitTheFloor()
    {
        if (gameObject.transform.position.y <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void SpawnBomb(Vector3 charPosition)
    {
        hasDetonated = false;
        timer.Reset();
        currentXplosion = 0;
        IsActive = true;
        gameObject.SetActive(true);
        gameObject.transform.position = new Vector3(charPosition.x, 0, charPosition.z);
    }
    public override bool InjectDropAbort()
    {
        if (!canThrow)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public override void OnDropThrow()
    {
        if (threwBomb)
        {
            launchDirection = (Grabber.FaceDirection + Vector3.up).normalized;
            Rigidbody.AddForce(launchDirection * throwForce, ForceMode.Impulse);
            lineRenderer.positionCount = 0;
        }
        else
        {
            Rigidbody.isKinematic = false;
            base.OnDropThrow();
        }
        startedThrowProcess = false;
        threwBomb = false;
    }

    private void DrawTrajectory()
    {
        Material material = lineRenderer.material;
        Color currentColor = material.GetColor("_TintColor");

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

                currentColor.a = 0.2f;
                material.SetColor("_TintColor", currentColor);
                canThrow = false;
                return;
            }
            else
            {
                currentColor.a = 1;
                material.SetColor("_TintColor", currentColor);
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
