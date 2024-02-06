using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface ICharacterMovement
{
    public ControlSchemeType CurrentControlScheme { get; set; }
    public int Index { get; set; }
    public bool AiControlled { get; set; }
    public bool CanMove { get; set; }
    public float MaxMoveSpeed { get; set; }
    public float MaxJumpPower { get; set; }
    public bool TryingToMove { get; set; }
    public bool TryingToJump { get; set; }
    public Vector2 CurrentDirection { get; set; }
    public Vector2 TargetDirection { get; set; }
    public float CurrentSpeed { get; set; }
    public float AccelerationTime { get; set; }
    public float JumpVelocity { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsJumping { get; set; }
    public bool IsFalling { get; set; }
    public bool IsMoving { get; set; }


    public void TryMove(InputAction.CallbackContext context);
    public void TryJump(InputAction.CallbackContext context);
}
