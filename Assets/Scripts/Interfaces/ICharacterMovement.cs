using UnityEngine;
using UnityEngine.InputSystem;

public interface ICharacterMovement
{
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


    public void TryMove(InputAction.CallbackContext context);
    public void TryJump(InputAction.CallbackContext context);
    public void TryMoveAi(Vector2 direction);
    public void TryJumpAi();
    public void TryShove(Vector3 direction, float power);
    /// <summary>
    /// Bump is a short shove.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="power"></param>
    public void TryBump(Vector3 direction, float power);
    public void Halt();

}
