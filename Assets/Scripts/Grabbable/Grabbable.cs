using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Grabbable : MonoBehaviour
{

    [SerializeField] protected PickupMeter _meter;
    [SerializeField] protected PickupAlert _alert;
    [SerializeField] protected Vector3 _grabbablePointOffset = new Vector3(0, 1f, 1f);
    [SerializeField] protected Vector3[] _handsOffsets = { new Vector3(), new Vector3() };
    private Collider _collider;
    private HeroMovement _grabber;
    private Vector3 _lastVelocity;
    private IRecievable _attachedTo;
    private Rigidbody _rBody;
    private EasyTimer _grabbedTimer;
    private EasyTimer _colliderTimer;
    private bool _pendingColliderEnable = false;
    private Dictionary<HeroMovement, bool> _potentialGrabbersGrabbing = new Dictionary<HeroMovement, bool>();
    [SerializeField] private Tug _tugOWar = null;
    [SerializeField] private GrabbablePosition _grabPosition = GrabbablePosition.InFrontTwoHands;

    private LayerMask _defaultLayerMask;
    private int _grabberLayer = 0;
    public bool GrabInProgress { get; set; } = false;

    public int GrabberLayer
    { get { return _grabberLayer; } }
    public bool ColliderEnabledWhileGrabbed
    { get; set; } = false;

    public float AttachPointDistance
    { get; set; } = 0f;

    public bool IsAttached
    { get; set; }

    public GrabbablePosition GrabbablePosition
    { get { return _grabPosition; } set { _grabPosition = value; } }

    public float TimeToGrab
    { get; set; } = GlobalValues.CHAR_GRAB_RADIUS_DEFAULT_TIMETOGRAB;

    public void Hide()
    {
        Hidden = true;
        _rBody.isKinematic = true;
        _collider.enabled = false;
        gameObject.SetActive(false);
    }

    public virtual void Attach(IRecievable attachedTo)
    {
        IsGrabbed = true;
        IsAttached = true;
        _attachedTo = attachedTo;
        _rBody.isKinematic = true;
        _collider.enabled = false;
    }
    public virtual void Detach()
    {
        IsAttached = false;
        IsGrabbed = false;
        _attachedTo = null;
        Vector3 randomDir = UnityEngine.Random.insideUnitSphere.normalized;
        _rBody.isKinematic = false;
        _pendingColliderEnable = true;
        _colliderTimer.Reset();
        _rBody.AddForce(randomDir * 9f, ForceMode.Impulse);
    }

    public HeroMovement Grabber
    { get { return _grabber; } }

    public void Show(Vector3 position)
    {
        transform.position = position;
        Show();
    }
    public void Show()
    {
        Hidden = false;
        gameObject.SetActive(true);
        //_collider.enabled = true;
        _pendingColliderEnable = false;
        _collider.enabled = true;
        _colliderTimer.Reset();
        _rBody.isKinematic = false;
        gameObject.SetActive(true);
    }

    public bool Hidden { get; set; } = false;

    public Vector3 GrabPointOffset
    { get { return _grabbablePointOffset; } }

    public GameObject GameObject
    { get { return this.GameObject; } }

    public bool IsGrabbed
    { get; set; }


    public void AbortGrab()
    {
        IsGrabbed = false;
        _rBody.isKinematic = false;
        _rBody.velocity = _lastVelocity;
        GrabInProgress = false;
        _meter.Abort();
        _grabber = null;
    }

    public void SignalCanNotGrab(HeroMovement potentialGrabber)
    {
        if (IsGrabbed) return;
        _alert.Deactivate();
        _potentialGrabbersGrabbing.Remove(potentialGrabber);
    }

    public void SignalCanGrab(HeroMovement potentialGrabber)
    {
        if (IsGrabbed) return;

        if (!_potentialGrabbersGrabbing.ContainsKey(potentialGrabber))
            _potentialGrabbersGrabbing.Add(potentialGrabber, false);

        if (_alert.Mode == AlertMode.Inactive || _alert.Mode == AlertMode.DeAnimating)
        {
            var hero = potentialGrabber.GameObject.GetComponent<Hero>();
            _alert.Activate(transform, hero.PrimaryColor);
        }
    }

    public bool TryGrab(HeroMovement grabber)
    {
        if (!IsGrabbed)
        {
            if (!_potentialGrabbersGrabbing.ContainsKey(grabber))
                _potentialGrabbersGrabbing.Add(grabber, true);
            else
                _potentialGrabbersGrabbing[grabber] = true;

            // TODO!! If another grabber enter the grabbing -> Start a tug!!!

            _alert.Hide();
            GrabInProgress = true;
            _lastVelocity = _rBody.velocity;
            _rBody.isKinematic = true;
            _grabber = grabber;
            _meter.Activate(_grabber.GameObject.transform.position + new Vector3(0, 2.3f, 0));
            return true;
        }
        return false;
    }

    public void Grab(HeroMovement grabber)
    {
        _potentialGrabbersGrabbing.Clear();
        IsGrabbed = true;
        
        if (ColliderEnabledWhileGrabbed)
        {
            _grabberLayer = grabber.GameObject.layer;
            var thisCollider = GetComponent<Collider>();
            thisCollider.excludeLayers = LayerUtil.Include(GlobalValues.GROUND_LAYER, grabber.GameObject.layer);          
        } else
            _collider.enabled = false;

        _grabber.Grab(this);

        _alert.Deactivate();
        StraightenUp();
    }
    public void Drop()
    {
        IsGrabbed = false;
        _grabber.Drop(this);
        if (!IsAttached)
        {
            if (ColliderEnabledWhileGrabbed)
            {
                _collider.enabled = false;
                _grabberLayer = -1;
                var thisCollider = GetComponent<Collider>();
                thisCollider.excludeLayers = 0;
            }         

            _pendingColliderEnable = true;
            _colliderTimer.Reset();
            _rBody.isKinematic = false;
            _rBody.AddForce(GlobalValues.CHAR_GRAB_DROPFORCE * (_grabber.CurrentDirection + Vector3.up).normalized, ForceMode.Impulse);
            _grabber = null;
        }
    }

    void Awake()
    {
        _collider = gameObject.GetComponent<Collider>();
        _rBody = _collider.attachedRigidbody;
        _grabbedTimer = new EasyTimer(TimeToGrab);
        _colliderTimer = new EasyTimer(GlobalValues.GRABBABLE_COLLIDER_TIMEOUT_DEFAULTTIME);
        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _meter = Instantiate(_meter, container.transform);
        _meter.gameObject.SetActive(false);
        _alert = Instantiate(_alert, container.transform);
        _alert.gameObject.SetActive(false);

    }
    protected void Start()
    {
        _meter.PickupComplete += OnPickupComplete;
        _meter.PickupAborted += OnPickupAborted;
    }

    private void OnPickupAborted(object sender, EventArgs e)
    {
        AbortGrab();
    }

    private void OnPickupComplete(object sender, EventArgs e)
    {
        Grab(_grabber);
    }

    protected void Update()
    {
        if (Hidden)
            return;

        if (_potentialGrabbersGrabbing.Count == 0)
            _alert.Deactivate();


        if (_pendingColliderEnable && !IsGrabbed && _colliderTimer.Done)
        {
            _collider.enabled = true;
            _pendingColliderEnable = false;
        }

        if (IsGrabbed && !IsAttached)
        {
            Quaternion rotation;
            var diff = Math.Abs(_grabber.FaceDirection.z + 1f);
            if (diff >= 0.01f)
                rotation = Quaternion.FromToRotation(Vector3.forward, _grabber.FaceDirection);
            else
                rotation = Quaternion.Euler(0, -180, 0);
            transform.rotation = rotation;
            transform.position = _grabber.GameObject.transform.position + (_grabber.FaceDirection * GrabPointOffset.z + new Vector3(0, GrabPointOffset.y, 0));         
        }
    }

    /// <summary>
    /// Default behaviour is to just Drop() which also makes the Grabber to be set to drop.
    /// </summary>
    /// <param name="response"></param>
    public virtual void ProcessTransferResponse(int response) { if (response == 0) Drop(); }
    public virtual object[] GetTransferables() { return new GameObject[] { this.gameObject }; }

    public virtual void Trigger() { }

    protected virtual void StraightenUp()
    {
        transform.rotation = Quaternion.identity;
    }
}