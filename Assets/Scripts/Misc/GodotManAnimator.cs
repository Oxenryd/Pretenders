using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodotManAnimator : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private HeroMovement _hero;

    private int _movingBool;
    private int _groundedBool;
    private int _jumpingBool;
    private int _fallingBool;
    private int _doubleJumpingBool;
    private int _moveSpeedFloat;

    // Start is called before the first frame update
    void Start()
    {
        _groundedBool = Animator.StringToHash("IsGrounded");
        _movingBool = Animator.StringToHash("IsMoving");
        _jumpingBool = Animator.StringToHash("IsJumping");
        _fallingBool = Animator.StringToHash("IsFalling");
        _doubleJumpingBool = Animator.StringToHash("IsDoubleJumping");
        _moveSpeedFloat = Animator.StringToHash("CurrentSpeed");

    }

    // Update is called once per frame
    void Update()
    {
        _anim.SetBool(_groundedBool, _hero.IsGrounded);
        _anim.SetBool(_movingBool, _hero.IsMoving);
        _anim.SetBool(_jumpingBool, _hero.IsJumping);
        _anim.SetBool(_fallingBool, _hero.IsFalling);
        _anim.SetBool(_doubleJumpingBool, _hero.IsDoubleJumping);
        _anim.SetFloat(_moveSpeedFloat, _hero.CurrentSpeed / _hero.MaxMoveSpeed);
    }
}
