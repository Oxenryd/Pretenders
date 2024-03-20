using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] Collider _collider;
    [SerializeField] protected PickupMeter _meter;
    [SerializeField] protected PickupAlert _alert;
    private ICharacterMovement _grabber;
    private Vector3 _lastVelocity;
    private Rigidbody _rBody;
    private EasyTimer _grabbedTimer;
    private EasyTimer _colliderTimer;
    private bool _pendingColliderEnable = false;
    private Dictionary<ICharacterMovement, bool> _potentialGrabbersGrabbing = new Dictionary<ICharacterMovement, bool>();
    [SerializeField] private Tug _tugOWar = null;
    private GrabbablePosition _grabPosition = GrabbablePosition.InFrontTwoHands;
    public bool GrabInProgress { get; set; } = false;

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

    public ICharacterMovement Grabber
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
        _pendingColliderEnable = true;
        _colliderTimer.Reset();
        _rBody.isKinematic = false;
        gameObject.SetActive(true);
    }

    public bool Hidden { get; set; } = false;

    public Vector3 GrabPointOffset
    { get { return new Vector3(0, 1f, 1f); } }

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

    public void SignalCanNotGrab(ICharacterMovement potentialGrabber)
    {
        if (IsGrabbed) return;
        _alert.Deactivate();
        _potentialGrabbersGrabbing.Remove(potentialGrabber);
    }

    public void SignalCanGrab(ICharacterMovement potentialGrabber)
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

    public bool TryGrab(ICharacterMovement grabber)
    {
        if (!IsGrabbed)
        {
            if (!_potentialGrabbersGrabbing.ContainsKey(grabber))
                _potentialGrabbersGrabbing.Add(grabber, true);
            else
                _potentialGrabbersGrabbing[grabber] = true;

            // TODO!! If another grabber enter the grabbing -> Start a tug!!!

            _alert.Deactivate();
            GrabInProgress = true;
            _lastVelocity = _rBody.velocity;
            _rBody.isKinematic = true;
            _grabber = grabber;
            _meter.Activate(_grabber.GameObject.transform.position + new Vector3(0, 2, 0) + _grabber.TargetDirection.normalized * 1f);
            return true;
        }
        return false;
    }

    public void Grab(ICharacterMovement grabber)
    {
        _potentialGrabbersGrabbing.Clear();
        IsGrabbed = true;
        _grabber.Grab(this);      
        _collider.enabled = false;
        _alert.Deactivate();
    }
    public void Drop()
    {
        IsGrabbed = false;
        _grabber.Drop(this);
        _pendingColliderEnable = true;
        _colliderTimer.Reset();
        _rBody.isKinematic = false;
        _rBody.AddForce(GlobalValues.CHAR_GRAB_DROPFORCE * (_grabber.CurrentDirection + Vector3.up).normalized, ForceMode.Impulse);
        _grabber = null;
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

        if (_pendingColliderEnable)
        {
            _collider.enabled = true;
            _pendingColliderEnable = false;
        }

        if (IsGrabbed)
        {
            transform.position = _grabber.GameObject.transform.position + (_grabber.CurrentDirection + new Vector3(0, GrabPointOffset.y, 0) * GrabPointOffset.z) ;
        }
    }
}
