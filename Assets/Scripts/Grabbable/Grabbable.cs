using System;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Scene = UnityEngine.SceneManagement.Scene;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

public class Grabbable : MonoBehaviour
{
    [SerializeField] protected Collider[] _colliders;
    [SerializeField] protected Rigidbody _rBody;
    [SerializeField] protected PickupMeter _meter;
    [SerializeField] protected PickupAlert _alert;
    [SerializeField] protected Vector3 _grabbablePointOffset = new Vector3(0, 1f, 1f);
    [SerializeField] protected Vector3[] _handsOffsets = { new Vector3(), new Vector3() };
    [SerializeField] protected Quaternion _grabbableRotationOffset = Quaternion.identity;
    [SerializeField] protected bool _canBeTuggedWhileGrabbed = false;
    private HeroMovement _grabber;
    private Vector3 _lastVelocity;
    private IRecievable _attachedTo;
    private EasyTimer _grabbedTimer;
    private EasyTimer _colliderTimer;
    private bool _pendingColliderEnable = false;
    [SerializeField] private Tug _tugOWar = null;
    [SerializeField] private GrabbablePosition _grabPosition = GrabbablePosition.InFrontTwoHands;

    public Tug Tug { get { return _tugOWar; }
    public bool KinematicByDefault
    { get; set; } = false;
    private int _grabberLayer = 0;
    public bool GrabInProgress { get; set; } = false;

    public bool DetachOnDrop { get; set; } = true;
    public int GrabberLayer
    { get { return _grabberLayer; } }
    public bool ColliderEnabledWhileGrabbed
    { get; set; } = false;

    public float AttachPointDistance
    { get; set; } = 0f;

    public bool IsAttached
    { get; set; }
    public Rigidbody Rigidbody
    { get { return _rBody; } }
    public GrabbablePosition GrabbablePosition
    { get { return _grabPosition; } set { _grabPosition = value; } }

    public float TimeToGrab
    { get; set; } = GlobalValues.CHAR_GRAB_RADIUS_DEFAULT_TIMETOGRAB;
    public PickupAlert PickupAlert
    { get { return _alert; } }
    /// <summary>
    /// Needs ColliderEnabledWhileGrabbed to be set to 'true' to work.
    /// </summary>
    public bool CanBeTuggedWhileGrabbed
    { get { return _canBeTuggedWhileGrabbed; } set { _canBeTuggedWhileGrabbed = value; } }

    public bool CanBeGrabbed
    { get; set; } = true;

    public void Hide()
    {
        Hidden = true;
        if (!KinematicByDefault)
            _rBody.isKinematic = true;
        foreach (var col in _colliders)
        {
            col.enabled = false;
        }
        gameObject.SetActive(false);
    }

    public void Attach(IRecievable attachedTo)
    {
        IsGrabbed = true;
        IsAttached = true;
        _attachedTo = attachedTo;
        if (!KinematicByDefault)
            _rBody.isKinematic = true;
        foreach (var col in _colliders)
        {
            col.enabled = false;
        }
    }

    public void Detach()
    { Detach(1f); }

    public void Detach(float powerMultiplier)
    {
        IsAttached = false;
        IsGrabbed = false;
        _attachedTo = null;
        Vector3 randomDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), 1f, UnityEngine.Random.Range(-1f, 1f)).normalized;
        if (!KinematicByDefault)
            _rBody.isKinematic = false;
        _pendingColliderEnable = true;
        _colliderTimer.Reset();
        _rBody.AddForce(randomDir * GlobalValues.GRABBABLE_DEFAULT_MAX_DETACH_POWER * UnityEngine.Random.Range(0.5f, 1f) * powerMultiplier, ForceMode.Impulse);
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
        _pendingColliderEnable = false;
        foreach (var col in _colliders)
        {
            col.enabled = true;
        }
        _colliderTimer.Reset();
        if (!KinematicByDefault)
            _rBody.isKinematic = false;
        gameObject.SetActive(true);
    }

    public Collider[] Colliders
    { get { return _colliders; } }

    public bool Hidden { get; set; } = false;

    public Vector3 GrabPointOffset
    { get { return _grabbablePointOffset; } }

    public GameObject GameObject
    { get { return this.GameObject; } }

    public bool IsGrabbed
    { get; set; }

    public void StartTug(HeroMovement hero1, HeroMovement hero2)
    {
        hero1.SetTug(-1);
        hero2.SetTug(1);
        Tug.Activate(hero1, hero2);
    }
    public void TugPull(HeroMovement hero)
    {
        Tug.Increase(hero.TuggerIndex * hero.TugPower * hero.Effect.CurrentEffects().TugPowerMultiplier);
    }

    public void AbortGrabInProgress()
    {
        IsGrabbed = false;
        if (!KinematicByDefault)
            _rBody.isKinematic = false;
        _rBody.velocity = _lastVelocity;
        GrabInProgress = false;
        _meter.Abort();
        _grabber = null;
    }

    public bool TryGrab(HeroMovement grabber)
    {
        if (!IsGrabbed && !GrabInProgress)
        {
            _alert.Hide();
            GrabInProgress = true;
            _lastVelocity = Vector3.ClampMagnitude(_rBody.velocity, GlobalValues.GRABBABLE_MAX_STORED_VELOCITY_MAGNITUDE);
            _rBody.velocity = Vector3.zero;
            if (!KinematicByDefault)
                _rBody.isKinematic = true;
            _grabber = grabber;
            _meter.Activate(_grabber.GameObject.transform.position + new Vector3(0, 2.3f, 0));
            return true;
        }
        else if (GrabInProgress)
        {
            _alert.Hide();
            _meter.Abort();
            StartTug(grabber, _grabber);
        }
        return false;
    }

    public virtual void Grab(HeroMovement grabber)
    {
        IsGrabbed = true;
        GrabInProgress = false;
        if (ColliderEnabledWhileGrabbed)
        {
            _grabberLayer = grabber.GameObject.layer;
            foreach (var col in _colliders)
            {
                col.enabled = true;
                col.excludeLayers = LayerUtil.Include(GlobalValues.GROUND_LAYER, grabber.GameObject.layer);
            }
        }
        else
            foreach (var col in _colliders)
            {
                col.enabled = false;
            }
        _grabber = grabber;
        _grabber.Grab(this);
        _alert.Deactivate();
        StraightenUp();
    }

    public virtual bool Drop()
    {
        if (InjectDropAbort()) return false;

        IsGrabbed = false;
        GrabInProgress = false;
        _grabber.ActualDrop();
        if (!IsAttached)
        {
            if (ColliderEnabledWhileGrabbed)
            {
                foreach (var col in _colliders)
                {
                    col.enabled = false;
                    col.excludeLayers = LayerUtil.Exclude(GlobalValues.GROUND_LAYER, _grabber.GameObject.layer);
                }
                _grabberLayer = -1;
            }
            _pendingColliderEnable = true;
            _colliderTimer.Reset();
            if (!KinematicByDefault)
            {
                _rBody.isKinematic = false;
                _rBody.velocity = Vector3.zero;
                _rBody.angularVelocity = Vector3.zero;
            }
            OnDropThrow();
            _grabber = null;
        }

        return true;
    }
    protected void Awake()
    {
        _grabbedTimer = new EasyTimer(TimeToGrab);
        _colliderTimer = new EasyTimer(GlobalValues.GRABBABLE_COLLIDER_TIMEOUT_DEFAULTTIME);
        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _meter = Instantiate(_meter, container.transform);
        _meter.gameObject.SetActive(false);
        _alert = Instantiate(_alert, container.transform);
        _alert.gameObject.SetActive(false);
        _tugOWar = Instantiate(_tugOWar, container.transform);
        _tugOWar.Grabbable = this;
    }

    protected void Start()
    {
        _meter.PickupComplete += OnPickupComplete;
    }

    private void OnPickupComplete(object sender, EventArgs e)
    {
        Grab(_grabber);
    }

    protected void Update()
    {
        if (Hidden)
            return;

        if (_pendingColliderEnable && !IsGrabbed && _colliderTimer.Done)
        {
            foreach (var col in _colliders)
            {
                col.enabled = true;
            }
            _pendingColliderEnable = false;
        }

        if (IsGrabbed && !IsAttached)
        {
            switch (_grabPosition)
            {
                case GrabbablePosition.AsBackpack:
                case GrabbablePosition.AboveHeadOneHand:
                case GrabbablePosition.InFrontOneHand:
                    transform.rotation = TransformHelpers.FixNegativeZRotation(Vector3.forward, _grabber.FaceDirection) * _grabbableRotationOffset;
                    transform.position = _grabber.LeftHand.position + _grabber.LeftHand.rotation * new Vector3(GrabPointOffset.x, GrabPointOffset.y, GrabPointOffset.z);
                    break;
                case GrabbablePosition.InFrontTwoHands:
                    transform.rotation = TransformHelpers.FixNegativeZRotation(Vector3.forward, _grabber.FaceDirection) * _grabbableRotationOffset;
                    transform.position = _grabber.GameObject.transform.position + (_grabber.FaceDirection * GrabPointOffset.z + new Vector3(0, GrabPointOffset.y, 0) + transform.rotation * new Vector3(GrabPointOffset.x, 0, 0));
                    break;
            }
        }
    }

    /// <summary>
    /// Default behaviour is to just Drop() which also makes the Grabber to be set to drop.
    /// Return whether or not to drop current grab after processing.
    /// </summary>
    /// <param name="response"></param>
    public virtual bool ProcessTransferResponse(int response)
    {
        if (response == 0)
        {
            _grabber.ActualDrop(); Drop();
            return true;
        }
        return false;
    }
    public virtual object[] GetTransferables() { return new GameObject[] { this.gameObject }; }

    public virtual bool TriggerEnter() { return false; }
    public virtual bool TriggerExit() { return false; }

    protected virtual void StraightenUp()
    {
        transform.rotation = Quaternion.identity;
    }
    public virtual void KnockOff() { Drop(); }
    public virtual bool InjectDropAbort() { return false; }
    public virtual void OnDropThrow()
    {
        Rigidbody.AddForce(_rBody.mass * GlobalValues.CHAR_GRAB_DROPFORCE * (_grabber.FaceDirection + Vector3.up).normalized, ForceMode.Impulse);
    }
}