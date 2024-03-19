using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroMovement : MonoBehaviour, ICharacterMovement
{
    [SerializeField] private float _quickTurnFactor = 0.2f;
    [SerializeField] private float _jumpBufferTime = 0.13f;
    [SerializeField] private Rigidbody _body;
    [SerializeField] private PlayerInput _input;
    [SerializeField] private float _accelerationTime = 0.9f;
    [SerializeField] private float _retardTime = 0.9f;
    [SerializeField] private float _turnTime = 0.9f;
    [SerializeField] private float _moveSpeed = 2.5f;
    [SerializeField] private float _jumpPower = 5f;
    [SerializeField] private float _shoveStunDuration = 1f;
    [SerializeField] private DragStruggle _struggle;

    // EVENTS
    public event EventHandler<Grabbable> GrabbedGrabbable;
    public event EventHandler DroppedGrabbable;
    public event EventHandler StoppedGrabInProgress;
    public event EventHandler Triggered;
    public void OnGrabGrabbable(Grabbable grabbable)
    { GrabbedGrabbable?.Invoke(this, grabbable); }
    public void OnDropGrabbable()
    { DroppedGrabbable?.Invoke(this, EventArgs.Empty); }
    protected void OnStoppedGrabInProgress()
    { StoppedGrabInProgress?.Invoke(this, EventArgs.Empty); }
    protected void OnTriggered()
    { Triggered?.Invoke(this, EventArgs.Empty); }

    private EasyTimer _accelTimer;
    private EasyTimer _turnTimer;
    private EasyTimer _haltTimer;
    private EasyTimer _jumpBufferTimer;
    private EasyTimer _shoveStunTimer;
    private EasyTimer _bumpTimer;
    private EasyTimer _grabTimout;
    private float _stopSpeed = 0f;
    private bool _jumpButtonIsDown = false; // (instead of polling device with external calls)
    private bool _grabButtonIsDown = false;
    private bool _didJumpDecel = false;
    private bool _startShoving = false;
    private bool _startBump = false;
    private Vector3 _shoveVector = Vector3.zero;
    private Vector3 _bumpVector = Vector3.zero;
    private Vector3 _gndNormal = new Vector3(0, 1, 0);
    private Vector3 _gndDampVelocity = Vector3.zero;
    private int _dJumpsLeft = 1;
    private int _maxDJumps = 1;
    private Vector3 _gndTargetNormalVel = Vector3.zero;
    private bool _droppingGrab = false;
    private bool _triedToTrigger = false;
    public bool _triggerButtonDown = false;

    public bool CanBeDragged { get; set; } = true;
    public bool CanTrigger { get; set; } = true;
    public Grabbable CurrentGrab { get; set; } = null;
    public GameObject GameObject
    { get; private set; }
    public bool TryingToTrigger
    { get { return _triedToTrigger; } set { _triedToTrigger = value; } }
    public bool TryingToGrab
    { get; set; } = false;
    public bool IsDoubleJumping
        { get; set; } = false;
    public int NumberOfDoubleJumps
        { get { return _maxDJumps; } set { _maxDJumps = value; } }
    public bool CanMove { get; set; } = true;
    public float CurrentSpeed { get; set; } = 0f;
    public float MaxMoveSpeed
        { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float MaxJumpPower
        { get { return _jumpPower; } set { _jumpPower = value; } }
    public float TargetSpeed { get; set; } = 0f;
    public bool AiControlled { get; set; } = true;
    public bool TryingToMove { get; set; } = false;
    public bool TryingToJump { get; set; } = false;
    public Vector3 CurrentDirection { get; set; } = Vector2.zero;
    public Vector3 TargetDirection { get; set; } = Vector2.zero;
    public Vector3 FaceDirection { get; set; } = Vector2.zero;
    public Vector3 GroundNormal
        { get { return _gndNormal; } set { _gndNormal = value; } }
    public float AccelerationTime
        { get { return _accelerationTime; } set { _accelerationTime = value; } }
    public float RetardTime
        { get { return _retardTime; } set { _retardTime = value; } }
    public float TurnTime
        { get { return _turnTime; } set { _turnTime = value; } }
    public ControlSchemeType CurrentControlScheme { get; set; } = ControlSchemeType.TopDown;
    public float JumpVelocity { get; set; } = 0f;
    public bool IsGrounded { get; set; } = false;
    public bool IsJumping { get; set; } = false;
    public bool IsFalling { get; set; } = false;
    public bool IsMoving { get; set; } = false;
    public bool InJumpBuffer { get; set; } = false;
    public bool IsStunned { get; set; } = false;
    public bool IsShoved { get; set; } = false;
    public bool IsBumped { get; set; } = false;
    public bool IsGrabbing { get; set; } = false;
    public bool IsGrabInProgress { get; set; } = false;
    public bool IsDraggingOther { get; set; } = false;
    public bool IsDraggedByOther { get; set; } = false;

    public void StartTug(Tug tug)
    {

    }

    public void Grab(Grabbable grabbable)
    {
        IsGrabbing = true;
        IsGrabInProgress = false;
    }
    public void Drop(Grabbable grabbable)
    {
        _droppingGrab = true;
    }


    // Handle Input Events
    public void TryJump(InputAction.CallbackContext context)
    {
        if (context.started)
        { 
            TryingToJump = true;
            _jumpButtonIsDown = true;
        } else if (context.canceled)
        {
            JumpDecel();
            _jumpButtonIsDown = false;
        }
    }

    public void TryGrab(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!IsGrabbing)
            {
                TryingToGrab = true;
                _grabButtonIsDown = true;
            } else
            {
                _droppingGrab = true;
            }
        }
        else if (context.canceled)
        {
            _grabButtonIsDown = false;
        }
    }

    public void TryTrigger(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _triedToTrigger = true;
            _triggerButtonDown = true;
        }
        else if (context.canceled)
        {
            _triggerButtonDown = false;
        }
    }

    public void TryTriggerAi()
    {
        throw new NotImplementedException();
    }

    public void TryGrabAi()
    {
        throw new System.NotImplementedException();
    }


    public void TryJumpAi()
    {
        TryingToJump = true;
    }

    /// <summary>
    /// Events from device to "Move" action trigger these, and its different states.
    /// </summary>
    /// <param name="context"></param>
    public void TryMove(InputAction.CallbackContext context)
    {
        if (CanMove && context.started)
        {
            var inputDir = context.ReadValue<Vector2>();
            _startMovingFromStandStill(new Vector3(inputDir.x, 0, inputDir.y));
        } else if (CanMove && context.performed)
        {
            var inputDir = context.ReadValue<Vector2>();
            _resumeMoving(new Vector3(inputDir.x, 0, inputDir.y));
        } else if (context.canceled)
        {
            Halt();
        }
    }

    /// <summary>
    /// Used by the AiController due to a limitation in (new) InputSystem,
    /// where it is not possible as of now to Trigger an Action from code.
    /// </summary>
    /// <param name="direction"></param>
    public void TryMoveAi(Vector2 direction)
    {
        if (CanMove && !TryingToMove)
        {
            var inputDir = new Vector3(direction.x, 0, direction.y);
            _startMovingFromStandStill(inputDir);
        }
        else if (CanMove && TryingToMove)
        {
            var inputDir = new Vector3(direction.x, 0, direction.y);
            _resumeMoving(inputDir);
        }
        else
            Halt();
    }

    public void TryBump(Vector3 direction, float power)
    {
        _bumpVector = direction * power;
        _startBump = true;
    }

    public void TryShove(Vector3 direction, float power)
    {
        if (!IsShoved)
        {
            _startShoving = true;

            Vector3 forceDir = Vector3.zero;
            switch (CurrentControlScheme) // TODO: Add different models for force calc in differnt control modes.
            {              
                case ControlSchemeType.TopDown:
                    forceDir = new Vector3(direction.x * power, GlobalValues.SHOVE_HEIGHT_BUMP_TOPDOWN, direction.z * power);                   
                    break;
            }

            _shoveVector = forceDir;
        }
    }

    public void Halt()
    {
        TryingToMove = false;
        _stopSpeed = CurrentSpeed;
        _accelTimer.Reset();
        _turnTimer.Reset();
        _haltTimer.Reset();
    }

    /// <summary>
    /// Remove some factor of airvelocity when releasing jump button mid air.
    /// </summary>
    public void JumpDecel()
    {
        if (IsJumping)
        {
            _body.velocity = new Vector3(_body.velocity.x, _body.velocity.y / 2, _body.velocity.z);
        }
    }


    // --------------------------------------------------------------------------------------------------- Start()

    // Start is called before the first frame update
    void Start()
    {
        // Set the accelTimer, turnTimer and let them subscribe to
        // GameManagers' 'EarlyUpdate' for automatic ticking.
        this.GameObject = gameObject;
        _accelTimer = new EasyTimer(_accelerationTime, false, true);
        _turnTimer = new EasyTimer(_turnTime, true, true);
        _haltTimer = new EasyTimer(_retardTime, false, true);
        _jumpBufferTimer = new EasyTimer(_jumpBufferTime, false, true);
        _shoveStunTimer = new EasyTimer(_shoveStunDuration, false, true) ;
        _bumpTimer = new EasyTimer(GlobalValues.CHAR_BUMPDURATION);
        _grabTimout = new EasyTimer(GlobalValues.CHAR_GRAB_TIMEOUT);
    }

    // --------------------------------------------------------------------------------------------------------------------------------------------------- FixedUpdate()

    // Fixed Update: because we decided on physics based movement
    // all manipulation of velocities should be done here, where
    // the physics engine is syncing.
    void FixedUpdate()
    {
        // Cached external calls
        var fixedDeltaTime = Time.fixedDeltaTime;

        // Shoved?
        if (_startShoving)
        {         
            TryingToMove = false;
            IsShoved = true;
            IsStunned = true; // Shove might be considered a kind of stun
            _shoveStunTimer.Reset(); 
            _body.velocity = Vector3.zero;
            _body.AddForce(_shoveVector, ForceMode.Impulse);
            _startShoving = false;
        }
        if (_shoveStunTimer.Done)
        {
            IsShoved = false;
            IsStunned = false;   // Remember that more terms might be needed to check in the future if "IsStunned" can be set to false
        }
        // doing some manual drag if shoved
        if (IsShoved)
        {
            switch (CurrentControlScheme)
            {
                case ControlSchemeType.TopDown:
                    _body.velocity = _body.velocity - new Vector3(_body.velocity.x * _shoveStunTimer.Ratio, 0, _body.velocity.z * _shoveStunTimer.Ratio);
                    break;
            }
            
        }


        // Grab/Drag Stuff
        if (_grabTimout.Done)
        {
            grabDragStuffs();
        }


        // Triggered?
        CanTrigger = true; // TODO: set conditions for triggering

        if (CanMove && CanTrigger && _triedToTrigger)
        {
            _triedToTrigger = false;
            OnTriggered();
        }


        // Bumped?
        if (_startBump)
        {
            TryingToMove = false;
            _bumpTimer.Reset();
            _body.velocity = Vector3.zero;
            _body.AddForce(_bumpVector, ForceMode.Impulse);
            _startBump = false;
            IsBumped = true;
        }
        if (_bumpTimer.Done)
        {
            IsBumped = false;
        }


        // Stunned? Then you can certainly not move.
        if (IsStunned)
        {
            CanMove = false;
        } else
        {
            CanMove = true;
        }

        // Are we trying to move??
        // T's for t in lerps.
        float turnT = _turnTimer.Ratio;
        float accelT = _accelTimer.Ratio;
        float haltT = _haltTimer.Ratio;
        if (TryingToMove && !IsBumped)
        {
            // Trying a "glassy" feeling of movement, with some time for acceleration and turning.
            CurrentDirection = Vector3.Lerp(CurrentDirection, TargetDirection, turnT);
            CurrentSpeed = Mathf.Clamp(Mathf.Lerp(CurrentSpeed, MaxMoveSpeed, accelT), 0f, TargetSpeed);
        } else 
        {
            // Take some time to slow down.
            CurrentSpeed = Mathf.Lerp(_stopSpeed, 0f, haltT);
        }

        // Resolve jumping
        // Do a buffer to check if player reaches ground a moment later and
        // perform the jump then. Always check if jumpbutton is down though,
        // so even the jumpbuffer can jumpDecel() if it's not.
        void _doJump()
        {
            if (!IsGrounded)
                _dJumpsLeft--;
            _body.velocity = (_gndNormal) * MaxJumpPower + new Vector3(_body.velocity.x, 0, _body.velocity.z);
        }

        if (TryingToJump)
        {
            if (CanMove && !InJumpBuffer && (IsGrounded  || _dJumpsLeft > 0) )
            {
                _doJump();
                TryingToJump = false;
            } else if (CanMove && !InJumpBuffer)
            {
                _jumpBufferTimer.Reset();
                InJumpBuffer = true;
            } else if (_jumpBufferTimer.Done && InJumpBuffer)
            {
                if (CanMove && IsGrounded)
                {
                    _doJump();
                }
                InJumpBuffer = false;
                TryingToJump = false;
            }
        }
        if (CanMove && !_jumpButtonIsDown && IsJumping && !_didJumpDecel)
        {
            JumpDecel();
            _didJumpDecel = true; // Because calling this in update it is needed to lock this until grounded to avoid mulitle attempts of decel.
        }


        // Falling? Jumping?
        if (Mathf.Round(_body.velocity.y) > 0f)
        {
            IsJumping = true;
            IsFalling = false;
        } else if (Mathf.Round(_body.velocity.y) < 0f)
        {
            IsJumping = false;
            IsFalling = true;
        }

        

        // Moving?
        Vector2 planeVelocity = new Vector2(_body.velocity.x, _body.velocity.z);
        if (Mathf.Round(planeVelocity.sqrMagnitude) > 0f) // Using sqr to save on sqrroots.
            IsMoving = true;
        else
            IsMoving = false;

        // Actually set velocity, if nothing is preventing it to change, like shoves etc.
        // Taking current controlscheme into consideration for axes.
        // Also storing last face direction
        if (!IsShoved && !IsBumped)
        {
            switch (CurrentControlScheme)
            {
                case ControlSchemeType.TopDown:
                    if (IsGrabInProgress)
                        CurrentSpeed = CurrentSpeed * 0.15f;
                    _body.velocity = new Vector3(CurrentDirection.x * CurrentSpeed, _body.velocity.y, CurrentDirection.z * CurrentSpeed);
                    if (_body.velocity.sqrMagnitude > 0f && !IsShoved)
                        FaceDirection = new Vector3(_body.velocity.x, 0f, _body.velocity.z).normalized;
                    break;
            }
        }
        if (!IsGrounded)
        {
            _gndNormal = Vector3.SmoothDamp(_gndNormal, Vector3.up, ref _gndDampVelocity, 0.5f);
        }

        // Turn poco a poco to upright
        transform.up = Vector3.SmoothDamp(transform.up, _gndNormal, ref _gndTargetNormalVel, 0.08f);
    }

    private void grabDragStuffs()
    {
        // Can grab?
        RaycastHit hit;
        object foundObject;
        var hitSomething = checkGrabDragAvailable(out foundObject, out hit);
        if (hitSomething)
        {
            var foundGrab = (foundObject as Grabbable);
            if (foundGrab != null)
            {
                foundGrab.SignalCanGrab(this);
                CurrentGrab = foundGrab;
            }

        }
        else if (!IsGrabbing)
        {
            if (CurrentGrab != null)
                CurrentGrab.SignalCanNotGrab(this);
            //CurrentGrab = null;                                          // Very suspicious. Maybe this is the random null reference bug. Will check out /Pierre
        }
        else if (IsGrabInProgress && (foundObject as Grabbable) != CurrentGrab)
        {
            CurrentGrab.AbortGrab();
            CurrentGrab = null;
            IsGrabInProgress = false;
            OnStoppedGrabInProgress();
        }

        // Trying to grab?
        if (CanMove && TryingToGrab && hitSomething)
        {
            TryingToGrab = false;
            doGrabbingDragging(foundObject);   
            Halt();
        }
        if (_droppingGrab)
        {
            OnDropGrabbable();
            _droppingGrab = false;
            if (IsGrabbing)
            {
                CurrentGrab.Drop();
                _grabTimout.Reset();
                CurrentGrab = null;
                IsGrabbing = false;
            }
        }
    }

    private bool checkGrabDragAvailable(out object foundObject, out RaycastHit hit)
    {
        if (Physics.SphereCast(transform.position, GlobalValues.CHAR_GRAB_RADIUS, CurrentDirection.normalized, out hit, GlobalValues.CHAR_GRAB_CHECK_DISTANCE))
        {
            var grabbable = hit.collider.gameObject.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                foundObject = grabbable;
                return true;
            } else
            {
                var draggable = hit.collider.gameObject.transform.GetComponentInParent<ICharacterMovement>();
                if (draggable != null)
                {
                    foundObject = draggable;
                    return true;
                }
                    
            }
        }
        foundObject = null;
        return false;
    }

    private bool doGrabbingDragging(object hitObject)
    {
        var grabbable = hitObject as Grabbable;
        if (grabbable != null)
        {
            OnGrabGrabbable(grabbable);
            if (grabbable.TryGrab(this))
            {
                _body.velocity = new Vector3(0, _body.velocity.y, 0);
                IsGrabInProgress = true;
                return true;
            }
        } else
        {
            var draggable = hitObject as ICharacterMovement;
            if (draggable == null)
                return false;

            var dot = Vector3.Dot(CurrentDirection, draggable.CurrentDirection);
            if (dot > GlobalValues.CHAR_DRAG_DOT_MIN)
            {
                if (!draggable.IsDraggedByOther && draggable.CanBeDragged)
                {
                    
                    return true;
                }
            }
        }
        return false;
    }

    // Collisions
    public void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.layer == GameManager.Instance.GroundLayer)
        {
            _gndNormal = collision.collider.transform.up;
            _dJumpsLeft = _maxDJumps;
            IsGrounded = true;
            IsFalling = false;
            IsJumping = false;
            _didJumpDecel = false;
        }
            
    }
    public void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.layer == GameManager.Instance.GroundLayer)
        {
            IsGrounded = false;
        }        
    }


    // Privates
    private bool _movingInSameDirection()
    {
        return Mathf.Round(Mathf.Atan2(TargetDirection.z, TargetDirection.x)) ==
            Mathf.Round(Mathf.Atan2(CurrentDirection.z, CurrentDirection.x));
    }
    private void _resumeMoving(Vector3 direction)
    {
        TargetDirection = direction;

        if (IsGrabInProgress)
        {
            var dirToGrab = (CurrentGrab.gameObject.transform.position - _body.position).normalized;
            if (Vector3.Dot(dirToGrab, TargetDirection.normalized) > 0.49f)
            {
                return;
            }
        }

        if (direction.magnitude * MaxMoveSpeed >= CurrentSpeed)
            TargetSpeed = direction.magnitude * MaxMoveSpeed;
        else
            TargetSpeed = CurrentSpeed * 0.99f;

        if (!_movingInSameDirection())
            _turnTimer.Reset();

        // Allowing quick turns when moving slow
        if (TargetSpeed < _quickTurnFactor * MaxMoveSpeed)
            _turnTimer.SetOff();

        TryingToMove = true;
    }
    private void _startMovingFromStandStill(Vector3 direction)
    {
        TargetDirection = direction;
        TargetSpeed = direction.magnitude * MaxMoveSpeed;
        _accelTimer.Reset();
        _turnTimer.Reset();
        TryingToMove = true;
    }

    public void StartDragStruggle(ICharacterMovement dragger, ICharacterMovement dragged)
    {

    }

    void OnDrawGizmos()
    {
        Vector3 start = transform.position + new Vector3(0, 1, 0);
        Vector3 end = start + CurrentDirection.normalized * GlobalValues.CHAR_GRAB_CHECK_DISTANCE;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(start, GlobalValues.CHAR_GRAB_RADIUS);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(end, GlobalValues.CHAR_GRAB_RADIUS);
    }
}
