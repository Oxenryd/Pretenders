using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] Collider _collider;
    [SerializeField] private PickupMeter _meter;
    private ICharacterMovement _grabber;
    private Rigidbody _rBody;
    private EasyTimer _grabbedTimer;
    private List<int> _beingGrabbedBy = new List<int>();
    [SerializeField] private Tug _tugOWar = null;
    private GrabbablePosition _grabPosition = GrabbablePosition.InFrontTwoHands;
    public bool GrabInProgress { get; set; } = false;

    public GrabbablePosition GrabbablePosition
    { get { return _grabPosition; } set { _grabPosition = value; } }

    public float TimeToGrab
    { get; set; } = 0.3f;

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
        _collider.enabled = true;
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

    public bool BeingGrabbed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    public void AbortGrab()
    {
        GrabInProgress = false;
        _meter.Abort();
        _grabber = null;
    }

    public void TryGrab(ICharacterMovement grabber)
    {
        if (!IsGrabbed)
        {
            GrabInProgress = true;
            _grabber = grabber;
            _grabber.CurrentGrab = this;
            _grabber.IsGrabInProgress = true;
            _meter.Activate(_grabber.GameObject.transform.position + new Vector3(0, 1, 0) + _grabber.TargetDirection.normalized * 1f);
        }

    }

    public void Grab(ICharacterMovement grabber)
    {
        IsGrabbed = true;
        _grabber.Grab(this);
        _collider.attachedRigidbody.isKinematic = true;
        _collider.enabled = false;

    }
    public void Drop()
    {
        IsGrabbed = false;
        _grabber.Drop(this);
        _grabber = null;
        _collider.enabled = true;
        _collider.attachedRigidbody.isKinematic = false;
    }

    void Awake()
    {
        _collider = gameObject.GetComponent<Collider>();
        _rBody = _collider.attachedRigidbody;
        _grabbedTimer = new EasyTimer(TimeToGrab);
        var container = GameObject.FindWithTag(GlobalStrings.CONT_MISCCONTAINER);
        _meter = Instantiate(_meter, container.transform);
        _meter.gameObject.SetActive(false);

    }
    private void Start()
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

    public void Update()
    {
        if (Hidden)
            return;


        if (IsGrabbed)
        {
            transform.position = _grabber.GameObject.transform.position + (_grabber.CurrentDirection + new Vector3(0, GrabPointOffset.y, 0) * GrabPointOffset.z) ;
        }
    }
}

