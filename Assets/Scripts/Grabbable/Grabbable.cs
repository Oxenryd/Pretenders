using System;
using UnityEngine;

/// <summary>
/// Base class for objects and items that should be able to be picked up and carried around.
/// Also has triggering properties and behaviours that can be set by inherited classes.
/// </summary>
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

    public Tug Tug { get { return _tugOWar; } }

    public bool KinematicByDefault
    { get; set; } = false;
    private int _grabberLayer = 0;
    public bool GrabInProgress { get; set; } = false;
    public bool UseGunAnimation { get; set; } = false;
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

    /// <summary>
    /// Make this object invisible and disable its collider.
    /// </summary>
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

    /// <summary>
    /// Attach this grabbable to another object implementing the IRecievable interface.
    /// </summary>
    /// <param name="attachedTo"></param>
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

    /// <summary>
    /// Releases this grabbable from an IRecievable.
    /// </summary>
    public void Detach()
    { Detach(1f); }
    /// <summary>
    /// Releases this grabbable from an IRecievable wth provided power for tossing the object away in a random direction.
    /// </summary>
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

    /// <summary>
    /// Makes this grabbable visible at a specific position.
    /// </summary>
    /// <param name="position"></param>
    public void Show(Vector3 position)
    {
        transform.position = position;
        Show();
    }
    /// <summary>
    /// Makes this grabbable visible and sets its collider to enabled.
    /// </summary>
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

    /// <summary>
    /// Starts a Tug-o-war between two heroes.
    /// </summary>
    /// <param name="hero1"></param>
    /// <param name="hero2"></param>
    public void StartTug(HeroMovement hero1, HeroMovement hero2)
    {
        hero1.SetTug(-1);
        hero2.SetTug(1);
        Tug.Activate(hero1, hero2);
    }
    /// <summary>
    /// This is called to increment/decrement the meter during a tug-o-war, based on the heroes provided/involved in the tug.
    /// </summary>
    /// <param name="hero"></param>
    public void TugPull(HeroMovement hero)
    {
        Tug.Increase(hero.TuggerIndex * hero.TugPower * hero.Effect.CurrentEffects().TugPowerMultiplier);
    }

    /// <summary>
    /// Aborts a progressing grab.
    /// </summary>
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

    /// <summary>
    /// Check for conditions and if able the provided grabber grabs this grabbable and returns whether a grab was possible or not.
    /// </summary>
    /// <param name="grabber"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Forcefully set this grabbable as Grabbed by the grabber.
    /// </summary>
    /// <param name="grabber"></param>
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

    /// <summary>
    /// Sets this grabbable to a non-grabbed state and its previous grabber in a non-grabbing state.<br></br>
    /// bool InjectDropAbort() can be overriden to create a custom behaviour for aborting a requested drop.
    /// </summary>
    /// <returns></returns>
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
                    transform.position = _grabber.RightHand.position + _grabber.RightHand.rotation * new Vector3(GrabPointOffset.x, GrabPointOffset.y, GrabPointOffset.z);
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
    /// <summary>
    /// If a grabbable also implements IRecievable, this can be overriden to present objects that can be transferred. <br></br>
    /// By default this return an array containing only this grabbable. Needs to be cast to correct type.
    /// </summary>
    /// <returns></returns>
    public virtual object[] GetTransferables() { return new GameObject[] { this.gameObject }; }

    /// <summary>
    /// Override to provide inherited class with triggered behaviour. Returns whether triggered or not.
    /// </summary>
    /// <returns></returns>
    public virtual bool TriggerEnter() { return false; }
    /// <summary>
    /// Override to provide inherited class with triggered behaviour. Returns whether exited trigger or not.
    /// </summary>
    /// <returns></returns>
    public virtual bool TriggerExit() { return false; }

    /// <summary>
    /// Called to just forced upward direction of object.
    /// </summary>
    protected virtual void StraightenUp()
    {
        transform.rotation = Quaternion.identity;
    }
    /// <summary>
    /// This can be overidden to make a seperate behaviour when this grabbable is being "Knocked off" rather than just a Dropped. <br></br>
    /// Default behaviour is same as a Drop();
    /// </summary>
    public virtual void KnockOff() { Drop(); }
    /// <summary>
    /// Override to make a conditional abort of a Drop-attempt.<br></br>
    /// Default behaviour is false, indicating to not abort a drop.
    /// </summary>
    /// <returns></returns>
    public virtual bool InjectDropAbort() { return false; }

    /// <summary>
    /// This can be overridden to apply a different move speed penalty than default while carrying this Grabbable.
    /// </summary>
    /// <returns></returns>
    public virtual float SpeedPenalty()
    {
        return GlobalValues.CHAR_GRAB_DEFAULT_SPEEDPENALTY;
    }
    /// <summary>
    /// Override to alter how this grabbable should behave when succesfully being dropped.
    /// </summary>
    public virtual void OnDropThrow()
    {
        Rigidbody.AddForce(_rBody.mass * GlobalValues.CHAR_GRAB_DROPFORCE * (_grabber.FaceDirection + Vector3.up).normalized, ForceMode.Impulse);
    }
}