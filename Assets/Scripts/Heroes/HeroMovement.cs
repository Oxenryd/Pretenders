using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroMovement : MonoBehaviour, ICharacterMovement
{
    [SerializeField] private float _quickTurnFactor = 0.2f;
    [SerializeField] private float _jumpBufferTime = 0.13f;

    [SerializeField] private Rigidbody _body;
    [SerializeField] private Material _mat;
    [SerializeField] public PlayerInput Input;
    [SerializeField] public HeroType HeroType = HeroType.Basic;
    [SerializeField] private float _accelerationTime = 0.9f;
    [SerializeField] private float _retardTime = 0.9f;
    [SerializeField] private float _turnTime = 0.9f;
    [SerializeField] private float _moveSpeed = 2.5f;
    [SerializeField] private float _jumpPower = 5f;
    [ColorUsage(false)] public Color PrimaryColor;
    [ColorUsage(false)] public Color SecondaryColor;

    private EasyTimer _accelTimer;
    private EasyTimer _turnTimer;
    private EasyTimer _haltTimer;
    private EasyTimer _jumpBufferTimer;
    private float _stopSpeed = 0f;
    private bool _jumpButtonIsDown = false; // (instead of polling device with external calls)
    private bool _didJumpDecel = false;

    public int Index { get; set; }
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
    public Vector2 CurrentDirection { get; set; } = Vector2.zero;
    public Vector2 TargetDirection { get; set; } = Vector2.zero;
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



    // Start is called before the first frame update
    void Start()
    {
        // Set color to prefab instance picked color.
        var thisRenderer = GetComponentInChildren<MeshRenderer>();
        thisRenderer.sharedMaterial = new Material(_mat);
        thisRenderer.sharedMaterial.color = PrimaryColor;

        // Set the accelTimer, turnTimer and let them subscribe to
        // GameManagers' 'EarlyUpdate' for automatic ticking.
        _accelTimer = new EasyTimer(_accelerationTime);
        _turnTimer = new EasyTimer(AccelerationTime, true);
        _haltTimer = new EasyTimer(_retardTime);
        _jumpBufferTimer = new EasyTimer(_jumpBufferTime);
        GameManager.Instance.EarlyFixedUpdate += _accelTimer.TickSubscription;
        GameManager.Instance.EarlyFixedUpdate += _turnTimer.TickSubscription;
        GameManager.Instance.EarlyFixedUpdate += _haltTimer.TickSubscription;
        GameManager.Instance.EarlyFixedUpdate += _jumpBufferTimer.TickSubscription;
    }

    // Fixed Update: because we decided on physics based movement
    // all manipulation of velocities should be done here, where
    // the physics engine is syncing.
    void FixedUpdate()
    {
        // Cached external calls
        var fixedDeltaTime = Time.fixedDeltaTime;

        // Are we trying to move??
        // T's for t in lerps.
        float turnT = _turnTimer.Ratio;
        float accelT = _accelTimer.Ratio;
        float haltT = _haltTimer.Ratio;
        if (TryingToMove)
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
            { _body.velocity = new Vector3(_body.velocity.x, MaxJumpPower, _body.velocity.z); }
        if (TryingToJump)
        {
            if (CanMove && IsGrounded && !InJumpBuffer)
            {
                _doJump();
                TryingToJump = false;
            } else if (!InJumpBuffer)
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
        if (!_jumpButtonIsDown && IsJumping && !_didJumpDecel)
        {
            JumpDecel(); 
            _didJumpDecel = true; // Because calling this in update it is needed to lock this until grounded.
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

        // Actually set velocity
        _body.velocity = new Vector3(CurrentDirection.x * CurrentSpeed, _body.velocity.y, CurrentDirection.y * CurrentSpeed);
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
            _startMovingFromStandStill(context.ReadValue<Vector2>());
        } else if (CanMove && context.performed)
        {
            _resumeMoving(context.ReadValue<Vector2>());
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
            _startMovingFromStandStill(direction);
        }
        else if (CanMove && TryingToMove)
        {
            _resumeMoving(direction);
        }
        else
            Halt();
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

    // Collisions
    public void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.layer == GameManager.Instance.GroundLayer)
        {
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
        return Mathf.Round(Mathf.Atan2(TargetDirection.y, TargetDirection.x)) ==
            Mathf.Round(Mathf.Atan2(CurrentDirection.y, CurrentDirection.x));
    }
    private void _resumeMoving(Vector2 direction)
    {
        TargetDirection = direction;
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
    private void _startMovingFromStandStill(Vector2 direction)
    {
        TargetDirection = direction;
        TargetSpeed = direction.magnitude * MaxMoveSpeed;
        _accelTimer.Reset();
        _turnTimer.Reset();
        TryingToMove = true;
    }
}
