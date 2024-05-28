using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Extensive class in charge of all character movement behaviours.
/// </summary>
public class HeroMovement : MonoBehaviour, IJumpHit
{
    [SerializeField] private bool _acceptInput = true;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Vector3 _gridOffset = Vector3.zero;
    [SerializeField] private ControlSchemeType _controlScheme = ControlSchemeType.TopDown;
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
    [SerializeField] private Grid _grid;
    [SerializeField] private Collider _bodyCollider;
    [SerializeField] private Collider _headCollider;
    [SerializeField] private LayerMask _bombLayer;
    [SerializeField] private float _airBrakeFactor = GlobalValues.JUMPDIRECTION_SLOWDOWN_MULTIPLIER;

    // EVENTS
    public event EventHandler<Grabbable> GrabbedGrabbable;
    public event EventHandler DroppedGrabbable;
    public event EventHandler StoppedGrabInProgress;
    public event EventHandler Triggered;
    public event EventHandler PressedTriggerButton;
    public event EventHandler PressedJumpButton;
    public event EventHandler PressedPushButton;
    public event EventHandler PressedGrabButton;
    public event EventHandler PressedEscapeButton;
    public void OnGrabGrabbable(Grabbable grabbable)
    { GrabbedGrabbable?.Invoke(this, grabbable); }
    public void OnDropGrabbable()
    { DroppedGrabbable?.Invoke(this, EventArgs.Empty); }
    protected void OnStoppedGrabInProgress()
    { StoppedGrabInProgress?.Invoke(this, EventArgs.Empty); }
    protected void OnTriggered()
    { Triggered?.Invoke(this, EventArgs.Empty); }
    protected void OnPressedTriggerButton()
    { PressedTriggerButton?.Invoke(this, EventArgs.Empty); }
    protected void OnPressedJumpButton()
    { PressedJumpButton?.Invoke(this, EventArgs.Empty); }
    protected void OnPressedPushButton()
    { PressedPushButton?.Invoke(this, EventArgs.Empty); }
    protected void OnPressedGrabButton()
    { PressedGrabButton?.Invoke(this, EventArgs.Empty); }
    protected void OnPressedEscapeButton()
    { PressedEscapeButton?.Invoke(this, EventArgs.Empty); }


    private Feet _feet;
    private EasyTimer _accelTimer;
    private EasyTimer _turnTimer;
    private EasyTimer _haltTimer;
    private EasyTimer _jumpBufferTimer;
    private EasyTimer _shoveStunTimer;
    private EasyTimer _bumpTimer;
    private EasyTimer _grabTimout;
    private EasyTimer _dragCooldown;
    private EasyTimer _shoveOffenderColDisableTimer;
    private EasyTimer _stunTimer;
    private EasyTimer _pushTimer;
    private EasyTimer _pushFailTimer;
    private EasyTimer _pushedTimer;
    private EasyTimer _pushRecoverTimer;
    private float _stopSpeed = 0f;
    private bool _jumpButtonIsDown = false;
    private bool _grabButtonIsDown = false;
    private bool _pushButtonIsDown = false;
    private bool _triggerButtonDown = false;
    private bool _didJumpDecel = false;
    private bool _startShoving = false;
    private bool _startBump = false;
    private bool _doDrop = false;
    private bool _canChangeQuadDirection = true;
    private bool _colShoveDisabled = false;
    private bool _tryingToGrab = false;
    private Vector3 _targetGridCenter;
    private float _distanceToGridTarget = 0f;
    private Vector3 _pendingQuadDirection = Vector3.zero;
    private Vector3 _shoveVector = Vector3.zero;
    private Vector3 _bumpVector = Vector3.zero;
    private Vector3 _gndNormal = new Vector3(0, 1, 0);
    private Vector3 _gndDampVelocity = Vector3.zero;
    private Vector3 _jumpDirection = Vector3.zero;
    private Vector3 _targetForceRotation = Vector3.zero;
    private int _dJumpsLeft = 1;
    private int _maxDJumps = 1;
    private Vector3 _gndTargetNormalVel = Vector3.zero;
    private bool _tryingToDrop = false;
    private bool _doneFirstLoop = false;
    private bool _triedToTrigger = false;
    private bool _tryingToPush = false;
    private bool _signalingGrab = false;
    private float _shovePower = GlobalValues.SHOVE_DEFAULT_SHOVEPOWER;
    private bool _isForceRotation = false;

    private bool _turningWinner = false;
    private GridOccupation _gridOccupation;
    private Vector2 _targetTile = Vector2.zero;
    private bool _pendingStop = false;
    private Hero _hero;

    public bool IsInPushRecovery { get; set; } = false;
    public Hero Hero
    {  get { return _hero; } }
    public bool CanGrabBombs { get; set; } = true;
    public bool HasWon { get; set; } = false;
    public int BombRangePlus { get; set; } = 0;
    public int MaxBombs { get; set; } = 1;
    public float ZOffset { get; set; } = 31f;
    public Vector2 StickInputVector
    { get; private set; } = Vector2.zero;
    public bool IsAlive { get; set; } = true;
    public bool CanThrowBombs
    { get; set; } = false;
    public bool JumpButtonDown
    { get { return _jumpButtonIsDown; } }
    public bool TriggerButtonDown
    { get { return _triggerButtonDown; } }
    public bool PushButtonDown
    { get { return _pushButtonIsDown; } }
    public bool GrabButtonDown
    { get { return _grabButtonIsDown; } }
    public bool AcceptInput
    { get { return _acceptInput; } set { _acceptInput = value; } }
    public bool IsPushFailed
    { get; set; } = false;
    public bool IsPushing
    { get; set; } = false;
    public float EffectDurationMultiplier
    { get; set; } = 1f;
    public Effect Effect { get; set; }
    public Vector3 GridCenterOffset
    { get { return _gridOffset; } set { _gridOffset = value; } }
    public Transform LeftHand
    { get { return _leftHand; } }
    public Transform RightHand
    { get { return _rightHand; } }
    public float TugPower
    { get; set; } = GlobalValues.TUG_DEFAULT_TUGPOWER;
    public sbyte TuggerIndex
    { get; set; } = 0;
    public bool IsTugging
    { get; set; } = false;
    public float ShovePower
    { get { return _shovePower; } set { _shovePower = value; } }
    public Vector3 Velocity
    { get { return _body.velocity; } }
    public DragStruggle DragStruggle
    { get { return _struggle; } set { _struggle = value; } }
    public HeroMovement Dragger { get; set; }
    public bool CanBeDragged { get; set; } = true;
    public bool CanTrigger { get; set; } = true;
    public Grabbable CurrentGrab { get; set; } = null;
    public GameObject GameObject
    { get; private set; }
    public bool TryingToTrigger
    { get { return _triedToTrigger; } set { _triedToTrigger = value; } }
    public bool TryingToGrab
    { get { return _tryingToGrab; } set { _tryingToGrab= value; } }
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
    public ControlSchemeType CurrentControlScheme
    { get { return _controlScheme; } set { _controlScheme = value; } }
    public float JumpVelocity { get; set; } = 0f;
    public bool IsGrounded { get; set; } = false;
    public bool IsJumping { get; set; } = false;
    public bool IsFalling { get; set; } = false;
    public bool IsMoving { get; set; } = false;
    public bool InJumpBuffer { get; set; } = false;
    public bool IsStunned { get; set; } = false;
    public bool IsShoved { get; set; } = false;
    public bool IsPushed { get; set; } = false;
    public bool IsBumped { get; set; } = false;
    public bool IsGrabbing { get; set; } = false;
    public bool IsGrabInProgress { get; set; } = false;
    public bool IsDraggingOther { get; set; } = false;
    public bool IsDraggedByOther { get; set; } = false;
    public bool CanBeStunned { get; set; } = true;
    public Rigidbody RigidBody { get { return _body; } }
    public Vector3 GroundPosition
    { get { return new Vector3(transform.position.x, 0, transform.position.z); } }


    // ------------------------------------------------------------------------------------- METHODS
    /// <summary>
    /// Signal a forced rotation change that will get applied next frame.
    /// </summary>
    /// <param name="rotation"></param>
    public void ForceRotation(Vector3 rotation)
    {
        _targetForceRotation = rotation;
        _isForceRotation = true;
    }
    /// <summary>
    /// Put this Hero in its "winning" state, appliying its animation and also turns it towards the camera (in most circumstances).<br></br>
    /// Provide the bool whether a "Halt()" command should be issued.
    /// </summary>
    /// <param name="halt"></param>
    public void SetWinner(bool halt)
    {
        if (halt)
            Halt();
        _body.velocity = Vector3.zero;
        ForceRotation(new Vector3(0f, 180f, 0f));
        _turningWinner = true;
        if (IsGrabbing)
        {
            Drop(CurrentGrab);
        }

        HasWon = true;
        //AcceptInput = false;
    }
    /// <summary>
    /// The actual Push this Hero recieves. How far this is bumbed depends on the state of the offender.
    /// </summary>
    /// <param name="offender"></param>
    public void Push(HeroMovement offender)
    {
        TryBump(offender.FaceDirection, GlobalValues.CHAR_PUSH_POWER);
        IsPushed = true;
        IsStunned = true;
        _pushedTimer.Reset();
    }

    public void OnHeadHit(HeroMovement offender)
    {
        if (_controlScheme == ControlSchemeType.Platform)
        {
            _feet.ForceDown();
        }
    }

    /// <summary>
    /// This is called on this offender of a head-Bonk, shoving the victim and temporarly setting state of this offender to avoid glitches.
    /// </summary>
    /// <param name="victim"></param>
    public void OnHitOthersHead(HeroMovement victim)
    {
        // Send the shove to the victim and temporarily disable this collider
        // to avoid the victim to get "stuck" during first part of shove.
        _bodyCollider.enabled = false;
        _colShoveDisabled = true;
        _shoveOffenderColDisableTimer.Reset();
        victim.TryShove(FaceDirection, this);
    }
    /// <summary>
    /// Called to release this Hero from a drag struggle.
    /// </summary>
    public void ReleaseFromDrag()
    {
        RigidBody.isKinematic = false;
        CanBeDragged = false;
        Dragger = null;
        IsDraggedByOther = false;
        _dragCooldown.Reset();
    }
    /// <summary>
    /// Try to bump this Hero in the direction and with the power.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="power"></param>
    public void TryBump(Vector3 direction, float power)
    {
        CurrentSpeed = 0;
        Halt();
        _bumpVector = direction * power;
        _startBump = true;
    }
    /// <summary>
    /// Try to shove this Hero in the direction. Provided offender's stats are compared to this Hero's stats to calculate the power of the shove.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="offender"></param>
    /// <param name="divider"></param>
    public void TryShove(Vector3 direction, HeroMovement offender) { TryShove(direction, offender, 1f); }
    /// <summary>
    /// Try to shove this Hero in the direction. Provided offender's stats are compared to this Hero's stats to calculate the power of the shove, divided by the divider.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="offender"></param>
    /// <param name="divider"></param>
    public void TryShove(Vector3 direction, HeroMovement offender, float divider)
    {
        var power = (offender.Effect.CurrentEffects().ShoveMultiplier - Effect.CurrentEffects().ShoveMultiplier + 1) * offender.ShovePower / divider;
        if (!IsShoved)
        {
            _startShoving = true;

            if (IsGrabbing)
            {
                CurrentGrab.KnockOff();
            }

            Vector3 forceDir = Vector3.zero;
            switch (CurrentControlScheme) // TODO: Add different models for force calc in differnt control modes.
            {
                case ControlSchemeType.Platform:
                case ControlSchemeType.TopDown:
                    forceDir = new Vector3(direction.x * power, GlobalValues.SHOVE_HEIGHT_BUMP_TOPDOWN, direction.z * power);
                    break;
            }

            _shoveVector = forceDir;
        }
    }
    /// <summary>
    /// A stronger type of 'Shove'. Works in the same way and shoves this Hero in the direction and power.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="power"></param>
    public void TryBlast(Vector3 direction, float power)
    {
        //var power = GlobalValues.SHOVE_DEFAULT_SHOVEPOWER * 0.5f;
        if (!IsShoved)
        {
            _startShoving = true;

            if (IsGrabbing)
            {
                CurrentGrab.KnockOff();
            }

            Vector3 forceDir = Vector3.zero;
            switch (CurrentControlScheme) // TODO: Add different models for force calc in differnt control modes.
            {
                case ControlSchemeType.Platform:
                case ControlSchemeType.TopDown:
                    forceDir = new Vector3(direction.x * power, GlobalValues.SHOVE_HEIGHT_BUMP_TOPDOWN, direction.z * power);
                    break;
            }
            var test = new Action(() => { });
            _shoveVector = forceDir;
        }
    }
    /// <summary>
    /// Init a halt command to stop this character and behaviour depends on ControlScheme set.
    /// </summary>
    public void Halt()
    {
        TryingToMove = false;
        if (!IsAlive)
        {
            return;
        }
        
        _stopSpeed = CurrentSpeed;
        _accelTimer.Reset();
        _turnTimer.Reset();
        _haltTimer.Reset();
    }
    /// <summary>
    /// The acutal command to set this character in the Grabbing state.
    /// </summary>
    /// <param name="grabbable"></param>
    public void Grab(Grabbable grabbable)
    {
        CurrentGrab = grabbable;
        IsGrabbing = true;
        _tryingToGrab = false;
        IsGrabInProgress = false;
    }
    /// <summary>
    /// Init a drop request that will be performed next frame.
    /// </summary>
    /// <param name="grabbable"></param>
    public void Drop(Grabbable grabbable)
    {
        _doDrop = true;
    }

    /// <summary>
    /// Sets this Hero up for a tug and provides info about what index this hero has in the struggle, important for whether the value in struggle should be incrmented or decremented by this Hero.
    /// </summary>
    /// <param name="tugIndex"></param>
    public void SetTug(sbyte tugIndex)
    {
        CurrentSpeed = CurrentSpeed * 0.15f;
        Halt();
        _tryingToDrop = false;
        _tryingToGrab = false;
        IsGrabInProgress = false;
        IsGrabbing = true;
        CanBeDragged = true;
        TuggerIndex = tugIndex;
        IsTugging = true;
    }
    /// <summary>
    /// Reset this Hero to a non-tugging state. Provide bool whether this hero won or not.
    /// </summary>
    /// <param name="winner"></param>
    public void ReleaseFromTug(bool winner)
    {
        IsTugging = false;
        if (winner)
            IsGrabbing = true;
        else
            IsGrabbing = false;
    }

    /// <summary>
    /// Stuns this Hero for time in seconds passed in.
    /// </summary>
    /// <param name="time"></param>
    public void Stun(float time)
    {
        if (CanBeStunned && !Effect.CurrentEffects().StunImmune)
        {
            CanMove = false;
            _stunTimer.Time = time;
            _stunTimer.Reset();
            IsStunned = true;
            _stopSpeed = CurrentSpeed;
        }
    }

    /// <summary>
    /// Start a dragStruggle between dragger and dragged.
    /// </summary>
    /// <param name="dragger"></param>
    /// <param name="dragged"></param>
    public void StartDragStruggle(HeroMovement dragger, HeroMovement dragged)
    {
        if (dragged.IsDraggingOther)
            dragged.DragStruggle.Abort();

        if (dragged.IsGrabbing)
        {
            dragged.CurrentGrab.Drop();
            dragged.IsGrabbing = false;
        }

        _struggle = GameManager.Instance.SceneManager.NextDragStruggle();
        dragged.DragStruggle = _struggle;
        _struggle.Activate(dragger, dragged);

        IsDraggingOther = true;
        CanBeDragged = true;

        dragged.Dragger = this;
        dragged.IsDraggedByOther = true;
        dragged.CanBeDragged = false;
        dragged.RigidBody.isKinematic = true;
    }

    // Handle Input Events
    // ------------------------------------------------------------------------------------- INPUTS START HERE
    // All the following methods are callbacks from inputManager and the InputSystem. They are called Try... because 
    // most of them sets a flag that something is being input from players and are to be handled in the next frame,
    // instead of forcefully setting character state.
    public void TryEscape(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnPressedEscapeButton();
        }
    }
    public void TryJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnPressedJumpButton();
            _jumpButtonIsDown = true;
            GameManager.Instance.InputManager.InvokeHeroPressedButton(this);

            if (!AcceptInput) return;
            TryingToJump = true;
        }
        else if (context.canceled)
        {
            JumpDecel();
            _jumpButtonIsDown = false;
        }
    }

    public void TryPush(InputAction.CallbackContext context)
    {
        if (_controlScheme == ControlSchemeType.BomberMan) return;

        if (context.started)
        {
            OnPressedPushButton();
            _pushButtonIsDown = true;
            GameManager.Instance.InputManager.InvokeHeroPressedButton(this);

            if (!AcceptInput) return;

            if (!CanMove || IsStunned || IsGrabbing || IsDraggingOther || IsTugging || IsDraggedByOther || IsJumping || IsFalling || IsPushing)
                return;

            _tryingToPush = true;

        }
        else if (context.canceled)
        {
            _pushButtonIsDown = false;
        }
    }
    public void TryGrab(InputAction.CallbackContext context)
    {
        // This is horrible...
        // A lot of functionality for one button.
        // Totally worth it =)
        if (context.started)
        {
            OnPressedGrabButton();
            _grabButtonIsDown = true;
            GameManager.Instance.InputManager.InvokeHeroPressedButton(this);
            if (!AcceptInput) return;
            if (!IsDraggingOther && !IsDraggedByOther && !IsTugging)
            {
                if (!IsGrabbing && !IsGrabInProgress && !IsTugging)
                {
                    _tryingToGrab = true;
                }
                else if (!IsTugging)
                {
                    _tryingToDrop = true;
                }

            }
            else if (!IsTugging)
            {
                if (IsDraggingOther)
                    _struggle.Decrease(GlobalValues.CHAR_DRAG_DRAGGER_DECREASE * Effect.CurrentEffects().StrugglePowerMultiplier);
                else
                    _struggle.Increase(GlobalValues.CHAR_DRAG_DRAGGED_INCREASE * Effect.CurrentEffects().StrugglePowerMultiplier);
            }
            else // IsTugging
            {
                CurrentGrab.TugPull(this);
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
            OnPressedTriggerButton();
            _triggerButtonDown = true;
            GameManager.Instance.InputManager.InvokeHeroPressedButton(this);
            if (!AcceptInput) return;
            _triedToTrigger = true;
        }
        else if (context.canceled)
        {
            _triggerButtonDown = false;
        }
    }

    public void TryTriggerAi()
    {
        if (!AcceptInput) return;
        throw new NotImplementedException();
    }

    public void TryGrabAi()
    {
        if (!AcceptInput) return;
        throw new System.NotImplementedException();
    }


    public void TryJumpAi()
    {
        if (!AcceptInput) return;
        TryingToJump = true;
    }

    /// <summary>
    /// TryMove is elaborate and handles different game modes differently.
    /// </summary>
    /// <param name="context"></param>
    public void TryMove(InputAction.CallbackContext context)
    {
        var inputDir = context.ReadValue<Vector2>();
        StickInputVector = inputDir;

        if (inputDir.magnitude < GlobalValues.INPUT_DEADZONE)
        {
            Halt();
            return;
        }

        if (!AcceptInput) return;

        if (CanMove && context.started)
        {
            Vector3 actualDir = _controlScheme == ControlSchemeType.BomberMan
                ? TransformHelpers.QuadDirQuantize(new Vector3(inputDir.x, 0, inputDir.y))
                : new Vector3(inputDir.x, 0, inputDir.y);

            if (_controlScheme == ControlSchemeType.BomberMan)
            {
                TryingToMove = true;
                TargetDirection = actualDir;
                if (FaceDirection != TargetDirection && CurrentSpeed != 0f)
                {
                    _pendingStop = true;
                }

            } else
                _startMovingFromStandStill(actualDir);
        }
        else if (CanMove && context.performed)
        {
            Vector3 actualDir = _controlScheme == ControlSchemeType.BomberMan
                ? TransformHelpers.QuadDirQuantize(new Vector3(inputDir.x, 0, inputDir.y))
                : new Vector3(inputDir.x, 0, inputDir.y);
            
            if (_controlScheme == ControlSchemeType.BomberMan)
            {
                TryingToMove = true;
                TargetDirection = actualDir;
                if (FaceDirection != TargetDirection && CurrentSpeed != 0f)
                {
                    _pendingStop = true;
                }
            } else
                _resumeMoving(actualDir);
        }
        else if (context.canceled)
        {
            Halt();
        }
    }

    /// <summary>
    /// Used by the dummy AI system to move Heroes around.
    /// </summary>
    /// <param name="direction"></param>
    public void TryMoveAi(Vector2 direction)
    {
        if (!AcceptInput) return;
        if (CanMove && !TryingToMove)
        {
            Vector3 inputDir = _controlScheme == ControlSchemeType.BomberMan
                ? TransformHelpers.QuadDirQuantize(new Vector3(direction.x, 0, direction.y))
                : new Vector3(direction.x, 0, direction.y);

            _startMovingFromStandStill(inputDir);
        }
        else if (CanMove && TryingToMove)
        {
            Vector3 inputDir = _controlScheme == ControlSchemeType.BomberMan
                ? TransformHelpers.QuadDirQuantize(new Vector3(direction.x, 0, direction.y))
                : new Vector3(direction.x, 0, direction.y);

            _resumeMoving(inputDir);
        }
        else
            Halt();
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


    // --------------------------------------------------------------------------------------------------------------- Start()

    // Start is called before the first frame update
    void Awake()
    {
        // Set the accelTimer, turnTimer and let them subscribe to
        // GameManagers' 'EarlyUpdate' for automatic ticking.
        this.GameObject = gameObject;
        _hero = GetComponent<Hero>();
        _feet = GetComponent<Feet>();
        _accelTimer = new EasyTimer(_accelerationTime, false, true);
        _turnTimer = new EasyTimer(_turnTime, true, true);
        _haltTimer = new EasyTimer(_retardTime, false, true);
        _jumpBufferTimer = new EasyTimer(_jumpBufferTime, false, true);
        _shoveStunTimer = new EasyTimer(_shoveStunDuration, false, true);
        _bumpTimer = new EasyTimer(GlobalValues.CHAR_BUMPDURATION, false, true);
        _grabTimout = new EasyTimer(GlobalValues.CHAR_GRAB_TIMEOUT, false, true);
        _dragCooldown = new EasyTimer(GlobalValues.CHAR_DRAG_DRAGGED_COOLDOWN, false, true);
        _dragCooldown.SetOff();
        _shoveOffenderColDisableTimer = new EasyTimer(GlobalValues.SHOVE_OFFENDCOL_DIS_DUR, false, true);
        _stunTimer = new EasyTimer(0, false, true);
        _pushTimer = new EasyTimer(GlobalValues.CHAR_PUSH_CHALLENGE_TIME, false, true);
        _pushFailTimer = new EasyTimer(GlobalValues.CHAR_PUSH_FAILED_STUN_TIME, false, true);
        _pushedTimer = new EasyTimer(GlobalValues.CHAR_PUSH_PUSHED_TIME, false, true);
        _pushRecoverTimer = new EasyTimer(GlobalValues.CHAR_PUSH_RECOVER_TIME, false, true);

        if (_controlScheme == ControlSchemeType.BomberMan)
        {
            _gridOccupation = _grid.GetComponent<GridOccupation>();
        }
        
        if (_controlScheme == ControlSchemeType.TopDown)
            TryMoveAi(Vector2.right);
        Effect = Effect.DefaultEffect();
    }

    // -------------------------------------------------------------------------------------------------------------- FixedUpdate()

    // Fixed Update: because we decided on physics based movement
    // all manipulation of velocities should be done here, where
    // the physics engine is syncing.
    void FixedUpdate()
    {
        if (_controlScheme == ControlSchemeType.PricePall)
            return;

        if(!IsAlive)
        {
            if (transform.position.y > GlobalValues.CHAR_DEAD_Y_LIMIT)
            {
                _body.velocity = Vector3.zero;
                _body.position = GlobalValues.CHAR_OFFLIMIT_POSITION;
            }
            return;
        }


        if (_isForceRotation)
        {
            var dir = Quaternion.Euler(_targetForceRotation) * Vector3.forward;
            transform.rotation = Quaternion.Euler(_targetForceRotation);
            FaceDirection = dir.normalized;
            TargetDirection = FaceDirection;
            CurrentDirection = FaceDirection;
            _isForceRotation = false;
            return;
        }


        // Cached external calls
        var fixedDeltaTime = Time.fixedDeltaTime;

        // ---------------------------------------------------------    SHOVING
        // Check if the disabled collider timer has
        // run out and reenable the body collider
        if (_colShoveDisabled && _shoveOffenderColDisableTimer.Done)
        {
            _bodyCollider.enabled = true;
            _colShoveDisabled = false;
        }

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
        if (_shoveStunTimer.Done && IsShoved)
        {
            TargetSpeed = _body.velocity.magnitude;
            IsShoved = false;
            IsStunned = false;   // Remember that more terms might be needed to check in the future if "IsStunned" can be set to false
        }
        // doing some manual drag if shoved
        if (IsShoved)
        {
            switch (CurrentControlScheme)
            {
                case ControlSchemeType.TopDown:
                    _body.velocity = new Vector3(_body.velocity.x * GlobalValues.SHOVE_DAMPING_MULTIPLIER, _body.velocity.y, _body.velocity.z * GlobalValues.SHOVE_DAMPING_MULTIPLIER);
                    break;

                case ControlSchemeType.Platform:
                    _body.velocity = new Vector3(_body.velocity.x * GlobalValues.SHOVE_DAMPING_MULTIPLIER, _body.velocity.y, 0);
                    break;

            }
        }

        // ---------------------------------------------------------    PUSHING
        if (IsPushed && _pushedTimer.Done)
        {
            IsPushed = false;
            IsStunned = false;
            _pushRecoverTimer.Reset();
            IsInPushRecovery = true;
        }

        if (IsInPushRecovery && _pushRecoverTimer.Done)
        {
            IsInPushRecovery = false;
        }

        if (_tryingToPush)
        {
            IsPushing = true;
            _pushTimer.Reset();
            _tryingToPush = false;

        }
        if (IsPushing && _pushTimer.Done)
        {
            IsPushing = false;
            if (!Effect.CurrentEffects().StunImmune)
            {

                IsPushFailed = true;
                Stun(GlobalValues.CHAR_PUSH_FAILED_STUN_TIME);
                _haltTimer.Reset();
                _pushFailTimer.Reset();
            }
        }
        if (IsPushFailed && _pushFailTimer.Done)
        {
            IsPushFailed = false;
            IsStunned = false;
        }



        // ---------------------------------------------------------    DRAGGING
        //Being dragged?
        if (IsDraggedByOther)
            CanMove = false;

        // Drag
        if (!_dragCooldown.Done)
            CanBeDragged = false;
        else
            CanBeDragged = true;


        // ---------------------------------------------------------    GRABBING / DRAGGING / TRANSFERRING
        if (IsBumped || IsShoved)
        {
            _tryingToGrab = false;
            _tryingToDrop = false;
        }
        if (_grabTimout.Done && !IsTugging)
        {
            grabDragStuffs();
        }
        _tryingToGrab = false;


        // ---------------------------------------------------------    TRIGGERING
        // Triggered?
        CanTrigger = true; // TODO: set conditions for triggering

        if (CanMove && CanTrigger && _triedToTrigger)
        {
            if (_controlScheme == ControlSchemeType.BomberMan)
                OnTriggered();

            _triedToTrigger = false;
            if (IsGrabbing && CurrentGrab.TriggerEnter())
            {
                OnTriggered();
            }

        }

        // ---------------------------------------------------------    BUMPING
        // Bumped?
        if (_startBump)
        {
            TryingToMove = false;
            _bumpTimer.Reset();
            _body.velocity = Vector3.zero;
            _body.AddForce(_bumpVector * Effect.CurrentEffects().BumbMultiplier, ForceMode.Impulse);
            CurrentSpeed = _body.velocity.magnitude;
            _startBump = false;
            IsBumped = true;
        }
        if (_bumpTimer.Done && IsBumped)
        {
            TargetSpeed = _body.velocity.magnitude;
            IsBumped = false;
        }

        // ---------------------------------------------------------    STUNNING
        // Stun
        if (IsStunned && _stunTimer.Done && !IsPushed)
        {
            IsStunned = false;
            CanMove = true;
        }

        // Stunned? Then you can certainly not move.
        if (IsStunned)
        {
            CanMove = false;
        }
        else
        {
            CanMove = true;
        }

        // ---------------------------------------------------------    JUMPING
        // Resolve jumping
        // Do a buffer to check if player reaches ground a moment later and
        // perform the jump then. Always check if jumpbutton is down though,
        // so even the jumpbuffer can jumpDecel() if it's not.
        void _doJump()
        {
            if (!IsGrounded)
            {
                IsDoubleJumping = true;
                _dJumpsLeft--;
            }
            if (!IsTugging)
                _body.velocity = (_gndNormal) * MaxJumpPower * Effect.CurrentEffects().JumpPowerMultiplier + new Vector3(_body.velocity.x, 0, _body.velocity.z);
            else
                _body.velocity = (_gndNormal) * MaxJumpPower / 4 * Effect.CurrentEffects().JumpPowerMultiplier + new Vector3(_body.velocity.x, 0, _body.velocity.z);
        }

        if (TryingToJump)
        {
            if (CanMove && !InJumpBuffer && (IsGrounded || _dJumpsLeft > 0))
            {
                _doJump();
                TryingToJump = false;
            }
            else if (CanMove && !InJumpBuffer)
            {
                _jumpBufferTimer.Reset();
                InJumpBuffer = true;
            }
            else if (_jumpBufferTimer.Done && InJumpBuffer)
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
        if (!IsGrounded)
        {
            _jumpDirection = TargetDirection;
            if (Mathf.Round(_body.velocity.y) > 0f)
            {
                IsJumping = true;
                IsFalling = false;
            }
            else if (Mathf.Round(_body.velocity.y) < 0f)
            {
                IsJumping = false;
                IsFalling = true;
            }
        }

        // ---------------------------------------------------------------------------------    MOVING
        // Are we trying to move??
        // T's for t in lerps.
        float turnT = _turnTimer.Ratio;
        float accelT = _accelTimer.Ratio;
        float haltT = _haltTimer.Ratio;
        if (TryingToMove && !IsBumped && !IsShoved && !IsPushing)
        {

            // Trying a "glassy" feeling of movement, with some time for acceleration and turning.
            if (_controlScheme != ControlSchemeType.BomberMan)
            {
                CurrentDirection = Vector3.Lerp(CurrentDirection, TargetDirection, turnT);
                CurrentSpeed = Mathf.Clamp(Mathf.Lerp(CurrentSpeed, MaxMoveSpeed * Effect.CurrentEffects().MoveSpeedMultiplier, accelT), 0f, TargetSpeed);
            } else // Below == Bomberman
            {
                if (IsAlive && !_pendingStop)
                {
                    if (_canChangeQuadDirection)
                    {
                        FaceDirection = TargetDirection;
                        CurrentDirection = FaceDirection;
                    }

                    if ( _validMovement() )
                    {
                        _targetTile = _gridOccupation.SetOccupied(_hero);
                        CurrentSpeed = MaxMoveSpeed * Effect.CurrentEffects().MoveSpeedMultiplier;
                    }
                }
            }
        } else if (!TryingToMove)
        {
            if  (_controlScheme == ControlSchemeType.Platform)
            {
                CurrentSpeed = 0;
            } else if (_controlScheme == ControlSchemeType.TopDown)
            {
                // Take some time to slow down.
                CurrentSpeed = Mathf.Lerp(_stopSpeed, 0f, haltT);
            }
        }

        // Moving?
        Vector2 planeVelocity = new Vector2(_body.velocity.x, _body.velocity.z);
        if (planeVelocity.sqrMagnitude > float.Epsilon) // Using sqr to save on sqrroots.
            IsMoving = true;
        else
            IsMoving = false;

        // Actually set velocity, if nothing is preventing it to change, like shoves etc.
        // Taking current controlscheme into consideration for axes.
        // Also storing last face direction
        if (!IsShoved && !IsBumped && !IsPushing)
        {
            if (IsGrabInProgress)
                CurrentSpeed = CurrentSpeed * 0.15f;
            else if (IsDraggingOther)
                CurrentSpeed = CurrentSpeed * GlobalValues.CHAR_DRAG_SPEED_MULTIPLIER;

            if (!IsGrounded && _controlScheme != ControlSchemeType.Platform)
            {
                var dot = Vector3.Dot(FaceDirection, _jumpDirection);
                if (dot < GlobalValues.JUMPDIRECTION_SLOWDOWN_DOT)
                {
                    CurrentSpeed = CurrentSpeed * _airBrakeFactor;
                }
            }

            if (IsPushFailed)
            {
                CurrentSpeed = Mathf.Lerp(CurrentSpeed, 0f, haltT);
            }

            Vector3 velocity = Vector3.zero;
            switch (CurrentControlScheme) // ------------------------------------------------------------ MOVING STATE
            {
                case ControlSchemeType.BomberMan:
                    if (CurrentSpeed != 0f)
                    {
                        velocity = FaceDirection.normalized * CurrentSpeed;
                        var targetPos = _gridOccupation.TileCenter(_targetTile);
                        _distanceToGridTarget = Vector3.Distance(targetPos, GroundPosition);
                        if (TransformHelpers.PassedGridTarget(this, targetPos))
                        {
                            if (_pendingStop || !TryingToMove)
                            {
                                CurrentSpeed = 0f;
                                velocity = Vector3.zero;
                                _canChangeQuadDirection = true;
                                _pendingStop = false;
                                FaceDirection = TargetDirection;
                            } else if (TryingToMove)
                            {
                                FaceDirection = TargetDirection;
                                CurrentDirection = FaceDirection;
                                if (_validMovement())
                                {
                                    _targetTile = _gridOccupation.SetOccupied(_hero);
                                    CurrentSpeed = MaxMoveSpeed * Effect.CurrentEffects().MoveSpeedMultiplier;
                                    velocity = FaceDirection.normalized * CurrentSpeed;
                                    _canChangeQuadDirection = false;
                                } else
                                {
                                    _targetTile = _gridOccupation.SetOccupiedForced(Hero.Index, transform.position);
                                    transform.position = _gridOccupation.TileCenter(_targetTile);
                                }
                            }
                        } else
                            _canChangeQuadDirection = false;
                    }
                    if (Hero.Index == 0)
                        Debug.Log($"TARGET: {_targetTile} - IN TILE: {_gridOccupation.XyFromVector3(GroundPosition)}");
                    break;

                case ControlSchemeType.TopDown:
                    if (IsDraggedByOther)
                    {
                        transform.position = Dragger.transform.position + Dragger.FaceDirection * GlobalValues.CHAR_DRAG_HOLD_DISTANCE;
                        FaceDirection = Dragger.FaceDirection;
                    } else
                    {
                        float grabSpeedFactor = IsGrabbing ? 1f - CurrentGrab.SpeedPenalty() : 1f;
                        velocity = new Vector3(CurrentDirection.x * CurrentSpeed * grabSpeedFactor, _body.velocity.y, CurrentDirection.z * CurrentSpeed * grabSpeedFactor);
                    }
                    break;

                case ControlSchemeType.Platform:

                    if (IsDraggedByOther)
                    {
                        transform.position = Dragger.transform.position + Dragger.FaceDirection * GlobalValues.CHAR_DRAG_HOLD_DISTANCE;
                        FaceDirection = Dragger.FaceDirection;
                    } else
                    {
                        velocity = new Vector3(CurrentDirection.x * CurrentSpeed, _body.velocity.y, 0);
                    }                      
                    break;

            }

            if (!IsDraggedByOther)
            {
                if (float.IsNaN(velocity.x))
                    velocity = new Vector3(0, velocity.y, velocity.z);
                if (float.IsNaN(velocity.z))
                    velocity = new Vector3(velocity.x, velocity.y, 0);

                _body.velocity = velocity;
            }
                  
            if (_controlScheme != ControlSchemeType.BomberMan)
            {
                var dirVelocity = new Vector3(velocity.x, 0, velocity.z).normalized;
                if (dirVelocity.sqrMagnitude > 0.005f && !IsShoved && !IsDraggedByOther && !IsFalling && !IsJumping)
                    FaceDirection = new Vector3(_body.velocity.x, 0f, _body.velocity.z).normalized;
            }

        }

        if (IsPushing)
        {
            _body.velocity = FaceDirection * GlobalValues.CHAR_PUSH_SPEED;
        }

        if (!IsGrounded)
        {
            _gndNormal = Vector3.SmoothDamp(_gndNormal, Vector3.up, ref _gndDampVelocity, 0.5f);
        }



        // ---------------------------------------------------------    ROTATING
        // Turn poco a poco to upright
        transform.up = Vector3.SmoothDamp(transform.up, _gndNormal, ref _gndTargetNormalVel, 0.08f);


        // Rotate
        transform.rotation = TransformHelpers.FixNegativeZRotation(Vector3.forward, FaceDirection);

        // ---------------------------------------------------------    LERP Z POSITION
        if (CurrentControlScheme == ControlSchemeType.Platform)
        {
            _body.position = new Vector3(_body.position.x, _body.position.y,
                Mathf.Lerp(_body.position.z, ZOffset, 1));
        }
        // ---------------------------------------------------------    END OF FIRST LOOP
        if (!_doneFirstLoop)
        {
            Halt();
            _doneFirstLoop = true;
        }
        // ---------------------------------------------------------    WINNER ?
        if (_turningWinner)
        {
            Halt();
            _acceptInput = false;
            _turningWinner = false;
        }
    }

    private void grabDragStuffs()
    {
        if (IsBumped || IsShoved || IsStunned) return;

        // Can grab?
        object foundObject;
        var hitSomething = checkGrabDragAvailable(out foundObject);
        if (hitSomething && !IsGrabbing)
        {
            var foundGrab = (foundObject as Grabbable);
            if (foundGrab != null && !IsDraggedByOther)
            {
                CurrentGrab = foundGrab;
                if ((CurrentGrab.IsGrabbed && CurrentGrab.CanBeTuggedWhileGrabbed) || CurrentGrab.GrabInProgress)
                {
                    if (Vector3.Dot(FaceDirection, CurrentGrab.Grabber.FaceDirection) < GlobalValues.TUG_DIRECTION_DOT_LIMIT)
                        foundGrab.PickupAlert.Ping(this, foundGrab.transform, true);
                }
                else if (!CurrentGrab.IsGrabbed && CurrentGrab.CanBeGrabbed)
                {
                    if ( (_controlScheme == ControlSchemeType.BomberMan && CanGrabBombs) || _controlScheme != ControlSchemeType.BomberMan)
                        foundGrab.PickupAlert.Ping(this, foundGrab.transform, false);
                }
            }
        }
        else if (IsGrabInProgress && (foundObject as Grabbable) != CurrentGrab)
        {
            CurrentGrab.AbortGrabInProgress();
            CurrentGrab = null;
            IsGrabInProgress = false;
            OnStoppedGrabInProgress();
        }
        else if (IsGrabbing && (foundObject as IRecievable) != null)
        {
            var recievable = (foundObject as IRecievable);
            recievable.TransferAlert.Ping(this, recievable.transform);
            if (_tryingToDrop)
            {
                _tryingToDrop = false;
                var resultFromRecievable = recievable.Transfer(CurrentGrab.GetTransferables());
                var dropCurrentGrab = CurrentGrab.ProcessTransferResponse(resultFromRecievable);

                if (dropCurrentGrab)
                {
                    ActualDrop();
                }
            }
        }

        // Trying to grab?
        if (CanMove && _tryingToGrab && hitSomething)
        {
            if ((_controlScheme == ControlSchemeType.BomberMan && CanGrabBombs) || _controlScheme != ControlSchemeType.BomberMan)
                doGrabbingDragging(foundObject);
            Halt();
        }

        if (_tryingToDrop)
        {
            OnDropGrabbable();
            _tryingToDrop = false;
            if (IsGrabbing)
            {
                if (CurrentGrab.Drop())
                    ActualDrop();
            }
        }
    }

    public void ActualDrop()
    {
        _doDrop = false;
        _grabTimout.Reset();
        CurrentGrab = null;
        IsGrabbing = false;
    }

    private bool checkGrabDragAvailable(out object foundObject)
    {
        var xyz = new Vector3(transform.position.x, transform.position.y + GlobalValues.CHAR_GRAB_CYLINDER_COLLIDER_Y_OFFSET, transform.position.z);


        var colliders = Physics.OverlapCapsule(
            xyz + FaceDirection * GlobalValues.CHAR_GRAB_CHECK_DISTANCE,
            xyz + Vector3.up + FaceDirection * GlobalValues.CHAR_GRAB_CHECK_DISTANCE,
            GlobalValues.CHAR_GRAB_RADIUS,
            LayerUtil.Exclude(GlobalValues.GROUND_LAYER));

        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == _headCollider || colliders[i] == _bodyCollider) continue;

                if (IsGrabbing)
                {
                    var recievable = colliders[i].gameObject.GetComponent<IRecievable>();
                    if (recievable != null)
                    {
                        foundObject = recievable;
                        return true;
                    }
                    foundObject = null;
                    return false;
                }

                var grabbable = colliders[i].gameObject.GetComponent<Grabbable>();
                if (grabbable != null)
                {
                    foundObject = grabbable;
                    return true;
                }
                else
                {
                    // Check again if the parent gameObject has a grabbable.
                    var grabbableInParent = colliders[i].gameObject.GetComponentInParent<Grabbable>();
                    if (grabbableInParent != null)
                    {
                        foundObject = grabbableInParent;
                        return true;
                    }

                    var draggable = colliders[i].gameObject.transform.GetComponentInParent<HeroMovement>();
                    if (draggable != null)
                    {
                        foundObject = draggable;
                        var distance = Vector3.Distance(_body.position, draggable.RigidBody.position);
                        if (IsPushing && distance <= GlobalValues.CHAR_PUSH_MIN_DISTANCE)
                        {
                            if (!draggable.IsPushed && !draggable.IsInPushRecovery)
                            {
                                IsPushing = false;
                                draggable.Push(this);
                            }
                        }
                        return true;
                    }

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
            if (grabbable.IsGrabbed && grabbable.CanBeTuggedWhileGrabbed)
            {
                if (Vector3.Dot(FaceDirection, grabbable.Grabber.FaceDirection) < GlobalValues.TUG_DIRECTION_DOT_LIMIT)
                {
                    grabbable.StartTug(this, grabbable.Grabber);
                    return true;
                }
            }
            else
            {
                if (grabbable.CanBeGrabbed && grabbable.TryGrab(this))
                {
                    _body.velocity = new Vector3(0, _body.velocity.y, 0);
                    IsGrabInProgress = true;
                    OnGrabGrabbable(grabbable);
                    return true;
                }
            }
        }
        else
        {
            var draggable = hitObject as HeroMovement;
            if (draggable == null)
                return false;

            var dot = Vector3.Dot(FaceDirection, draggable.FaceDirection);
            if (dot > GlobalValues.CHAR_DRAG_DOT_MIN || draggable.IsStunned)
            {
                if (draggable.CanBeDragged)
                {
                    StartDragStruggle(this, draggable);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Used by the Feet class to inform this Hero of being grounded.
    /// </summary>
    /// <param name="normal"></param>
    public void SignalGrounded(Vector3 normal)
    {
        _gndNormal = normal;
        _dJumpsLeft = _maxDJumps + Effect.CurrentEffects().ExtraDoubleJumps;
        IsDoubleJumping = false;
        IsGrounded = true;
        IsFalling = false;
        IsJumping = false;
        _didJumpDecel = false;
    }
    /// <summary>
    /// Set this Hero's Grounded state as "Not grounded".
    /// </summary>
    public void SignalNotGrounded()
    {
        IsGrounded = false;
    }



    // Privates

    // Check valid move direction in bomberman
    private bool _validMovement()
    {
        RaycastHit wallHit;
        LayerMask walls = LayerUtil.Include(GlobalValues.BLOCKS_LAYER);
        Physics.Raycast(transform.position + new Vector3(0, .5f, 0), TargetDirection, out wallHit, _grid.cellSize.x * 0.67f, walls);
        if (wallHit.collider || _gridOccupation.CheckOccupied(_hero))
        {
            Halt();
            return false;
        }

        return true;
    }
    private bool _movingInSameDirection()
    {
        return Mathf.Round(Mathf.Atan2(TargetDirection.z, TargetDirection.x)) ==
            Mathf.Round(Mathf.Atan2(CurrentDirection.z, CurrentDirection.x));
    }
    private void _resumeMoving(Vector3 direction)
    {
        if (IsTugging || IsPushing || IsPushFailed) return;
        TargetDirection = direction;

        if (IsGrabInProgress && CurrentGrab != null)
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
        if (IsTugging || IsPushing || IsPushFailed) return;
        TargetDirection = direction;
        TargetSpeed = direction.magnitude * MaxMoveSpeed;
        _accelTimer.Reset();
        _turnTimer.Reset();
        TryingToMove = true;
    }


    void OnCollisionEnter(Collision collision)
    {
        var bomb = collision.gameObject.GetComponent<Bomb>();
        if (bomb != null)
        {
            bomb.SetCanExplode(true);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        var xyz = new Vector3(transform.position.x, transform.position.y + GlobalValues.CHAR_GRAB_CYLINDER_COLLIDER_Y_OFFSET, transform.position.z);
        Gizmos.DrawWireSphere(xyz + FaceDirection * GlobalValues.CHAR_GRAB_CHECK_DISTANCE, GlobalValues.CHAR_GRAB_RADIUS);
        Gizmos.DrawWireSphere(xyz + Vector3.up + FaceDirection * GlobalValues.CHAR_GRAB_CHECK_DISTANCE, GlobalValues.CHAR_GRAB_RADIUS);
    }
}