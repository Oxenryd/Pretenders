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

    private BombermanManager bombermanManager;

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

    private GridOccupation _gridOccupation;

    [SerializeField]
    private GameObject crossDrawing;
    private SpriteRenderer spriteRenderer;

    private bool _canExplode = true;
    private int _bombIndex = 1;
    private EasyTimer timer;
    private EasyTimer detonationTickTimer;
    private bool hasDetonated = false;
    private Dictionary<Vector3, bool> directions = new Dictionary<Vector3, bool>() { { Vector3.back, false }, { Vector3.forward, false }, { Vector3.left, false }, { Vector3.right, false } };
    public bool IsActive
    { get; set; } = false;

    public Hero Hero { get; set; }


    /// <summary>
    /// This class handles bomb behavior. 
    /// It handles the explosions, the spawning, the drawing of the trajectory and the throwing of the bomb.
    /// </summary>



    private void Start()
    {
        base.Start();
        bombermanManager = GameObject.FindWithTag("BombManager").GetComponent<BombermanManager>();
        spriteRenderer = crossDrawing.GetComponent<SpriteRenderer>();
        _gridOccupation = GameObject.FindWithTag("BombermanGrid").GetComponent<GridOccupation>();
    }
    void Awake()
    {
        base.Awake();
        crossDrawing.SetActive(false);
        KinematicByDefault = true;
        lineRenderer.startColor = Color.white;
        timer = new EasyTimer(delayBeforeExplosion);
        detonationTickTimer = new EasyTimer(delayAfterEachExplosion);
    }
    public void SetCanExplode(bool canExplode)
    {
        _canExplode = canExplode;
    }

    public void SetInactive()
    {
        gameObject.SetActive(false);
        IsActive = false;
    }


    /// <summary>
    /// The update method handles mostly the timers of the explosions.
    /// When a player puts down bomb the timer determines how many seconds it will go before the bomb explodes.
    /// When the timer is done it starts another timer starts,
    /// which determines how long time it will go between each explosion in the cross explosion.
    /// </summary>
    void Update()
    {
        base.Update();
        if (!IsActive) return;

        if (timer.Done && !hasDetonated && _canExplode) //Pre-detonation
        {
            canThrow = true;
            if (IsGrabbed)
                Drop();
            hasDetonated = true;
            detonationTickTimer.Reset();
        }
        if (hasDetonated && detonationTickTimer.Done) // Has detonated
        {
            _gridOccupation.RemoveOccupiedByBomb(this, _bombIndex);
            explosionCheckOnTop();        
            currentXplosion++;
            for (int i = 0; i < directions.Count; i++)
            {
                var directionKey = directions.Keys.ElementAt(i);
                if (!directions[directionKey])
                {
                    explosionCheckNearby(directionKey, currentXplosion);
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
            _gridOccupation.RemoveOccupiedByBomb(this, _bombIndex);
            if (Grabber.TriggerButtonDown)
            {
                DrawTrajectory();
                startedThrowProcess = true;
                var newValue = throwForce + 10f * GameManager.Instance.DeltaTime;
                throwForce = Mathf.Clamp(newValue, 0, 27);
            }
            if (startedThrowProcess && !Grabber.TriggerButtonDown) // Threw bomb
            {
                threwBomb = true;
                _canExplode = false;
                crossDrawing.SetActive(false);
                startedThrowProcess = false;
                if (canThrow)
                {
                    Rigidbody.isKinematic = false;
                    detonationTickTimer.Reset();
                    Drop();
                }
                else if (!canThrow)
                {
                    _canExplode = true;
                    throwForce = 5;
                    lineRenderer.positionCount = 0;
                }
            }
        }
        else if (IsActive)
            _gridOccupation.SetOccupiedByBomb(this, _bombIndex);

        if (HitTheFloor())
        {
            _canExplode = true;
            Rigidbody.isKinematic = true;
            gameObject.transform.position = _gridOccupation.TileCenter(_gridOccupation.GetTileBomb(Hero, _bombIndex));//new Vector3(Mathf.RoundToInt(gameObject.transform.position.x / 2) * 2, 0, Mathf.RoundToInt(gameObject.transform.position.z / 2) * 2);
            _gridOccupation.SetOccupiedByBomb(this, _bombIndex);
        }
    }

    /// <summary>
    /// This method will stop the bomb from continuing.
    /// </summary>
    public void Detonate()
    {
        canThrow = true;
        if (IsGrabbed)
            Drop();
        hasDetonated = true;
        detonationTickTimer.Reset();
        _gridOccupation.RemoveOccupiedByBomb(this, _bombIndex);
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

    /// <summary>
    /// This method spawns the bomb.
    /// Since the bombs are recycled this method also resets variables so the explosions are set correctly.
    /// </summary>
    public void SpawnBomb(Vector3 charPosition, int bombIndex, int explosionLength)
    {
        amountOfExplosions = explosionLength;
        _bombIndex = bombIndex;
        if (_gridOccupation == null)
            _gridOccupation = GameObject.FindWithTag("BombermanGrid").GetComponent<GridOccupation>();

        hasDetonated = false;
        timer.Reset();
        currentXplosion = 0;
        IsActive = true;
        gameObject.SetActive(true);
        gameObject.transform.position = new Vector3(charPosition.x, 0, charPosition.z);

        for(int i = 0; i < directions.Count; i++)
        {
            var directionKey = directions.Keys.ElementAt(i);
            directions[directionKey] = false;
        }
        _gridOccupation.SetOccupiedByBomb(this, _bombIndex);
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

    /// <summary>
    /// This method adds behavior to the bomb throw.
    /// There's two ways a player can throw a bomb.
    /// Either when holding L which will draw the trajectory and the bomb will be thrown based on throw force.
    /// If the player doesn't hold in L and just click L the bomb will be thrown straight down without any force.
    /// </summary>
    public override void OnDropThrow()
    {
        if (threwBomb)
        {
            //transform.position += Grabber.FaceDirection * 0.5f + new Vector3(0, 0.5f, 0);
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

    /// <summary>
    /// This method draws the trajectory. 
    /// It uses physics equations to determine the gravitational effects and the motion of the bomb with velocity.
    /// To draw the trajectory it uses line renderer and also raycast to see if a a collider is blocking the bomb
    /// </summary>
    private void DrawTrajectory()
    {
        lineRenderer.transform.rotation = Quaternion.Euler(0,0,45);
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
            crossDrawing.transform.position = lastPosition;
            
            crossDrawing.SetActive(true);

            LayerMask mask = LayerUtil.Include(GlobalValues.BLOCKS_LAYER);
            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, mask))
            {
                spriteRenderer.color = new Color(1, 0, 0, 0.2f);
                lineRenderer.material.SetColor("_TintColor", spriteRenderer.color);
                canThrow = false;
                return;
            } else
            {
                spriteRenderer.color = new Color(0, 1, 0, 0.2f);
                lineRenderer.material.SetColor("_TintColor", spriteRenderer.color);
                canThrow = true;
            }


            if (point.y <= 0f)
            {
                break;
            }
        }

    }

    /// <summary>
    /// This method checks if the explosion of the bomb hits a player higher up than the bomb itself
    /// </summary>
    private void explosionCheckOnTop()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + new Vector3(0, -1f, 0), 0.5f, Vector3.up, out hit, 2f, levelMask))
        {
            var heroCollision = hit.collider.GetComponentInParent<Hero>();
            if (heroCollision != null)
            {
                _gridOccupation.RemoveHero(heroCollision);
                bombermanManager.PlayerDeath(heroCollision);
            }
        }
    }

    /// <summary>
    /// When a bomb explosion has started it uses this method to see if its hitting nearby colliders. 
    /// If the explosion reaches a wall or crate it will stop.
    /// If the explosion reaches a hero the hero will get killed but the explosion will keep on going
    /// </summary>
    private void explosionCheckNearby(Vector3 direction, int tick)
    {
        RaycastHit hit;
        Physics.Raycast(transform.position + new Vector3(0, .75f, 0), direction, out hit, tick, levelMask);

        if (hit.collider != null)
        {
            var bombCollision = hit.collider.GetComponent<Bomb>();
            if (bombCollision != null)
            {
                bombCollision.Detonate();
            }

            if (hit.collider.TryGetComponent<CrateExplosion>(out var crate))
            {
                crate.Explode();
                directions[direction] = true;
            }

            var heroCollision = hit.collider.GetComponentInParent<Hero>();
            if (heroCollision != null)
            {
                _gridOccupation.RemoveHero(heroCollision);
                bombermanManager.PlayerDeath(heroCollision);
            } else
            {
                directions[direction] = true;
            }
        } else
        {
            Instantiate(explosion, transform.position + (tick * direction), transform.rotation);
        }
    }


}
