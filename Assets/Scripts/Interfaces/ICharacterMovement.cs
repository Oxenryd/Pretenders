using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface ICharacterMovement
{
    // Because of the way new InputSystem outputs it's input events
    // there are version of callbacks that differ between AI control and callbacks
    // from InputSystem for now because of ease of use. In the best of worlds there
    // would only be methods like: "PressedJump" and "Released Jump".
    // That will be for the future

    public GameObject GameObject { get; }
    public ControlSchemeType CurrentControlScheme { get; set; }
    public bool AiControlled { get; set; }
    public bool CanMove { get; set; }
    public float MaxMoveSpeed { get; set; }
    public float MaxJumpPower { get; set; }
    public bool TryingToMove { get; set; }
    public bool TryingToJump { get; set; }
    public Vector3 FaceDirection { get; set; }
    public Vector3 CurrentDirection { get; set; }
    public Vector3 TargetDirection { get; set; }
    public Vector3 GroundNormal { get; set; }
    public float CurrentSpeed { get; set; }
    public float AccelerationTime { get; set; }
    public float JumpVelocity { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsJumping { get; set; }
    public bool IsFalling { get; set; }
    public bool IsMoving { get; set; }
    public bool IsStunned { get; set; }
    public bool IsShoved { get; set; }
    public bool IsBumped { get; set; }
    public bool IsDoubleJumping { get; set; }
    public int NumberOfDoubleJumps { get; set; }
    public bool IsGrabbing { get; set; }
    public bool IsGrabInProgress { get; set; }
    public void Grab(Grabbable grabbable);
    public void Drop(Grabbable grabbable);
    public event EventHandler<Grabbable> GrabbedGrabbable;
    public event EventHandler DroppedGrabbable;
    public event EventHandler StoppedGrabInProgress;
    public Grabbable CurrentGrab { get; set; }

    public void TryGrab(InputAction.CallbackContext context);
    public void TryMove(InputAction.CallbackContext context);
    public void TryJump(InputAction.CallbackContext context);
    public void TryMoveAi(Vector2 direction);
    public void TryJumpAi();
    public void TryGrabAi();
    public void TryShove(Vector3 direction, float power);
    public void TryBump(Vector3 direction, float power);
    public void Halt();

}
