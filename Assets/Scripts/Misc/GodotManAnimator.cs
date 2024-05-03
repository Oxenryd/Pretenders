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
    private int _pushingBool;
    private int _pushFailedBool;
    private int _stunnedBool;
    private int _pushedBool;
    private int _draggedBool;
    private int _draggingBool;
    private int _shovedBool;
    private int _gunBool;

    // Start is called before the first frame update
    void Start()
    {
        _gunBool = Animator.StringToHash("IsGunAnimation");
        _shovedBool = Animator.StringToHash("IsShoved");
        _draggedBool = Animator.StringToHash("IsDragged");
        _draggingBool = Animator.StringToHash("IsDragging");
        _groundedBool = Animator.StringToHash("IsGrounded");
        _movingBool = Animator.StringToHash("IsMoving");
        _jumpingBool = Animator.StringToHash("IsJumping");
        _fallingBool = Animator.StringToHash("IsFalling");
        _doubleJumpingBool = Animator.StringToHash("IsDoubleJumping");
        _moveSpeedFloat = Animator.StringToHash("CurrentSpeed");
        _pushingBool = Animator.StringToHash("IsPushing");
        _pushFailedBool = Animator.StringToHash("IsPushFailed");
        _stunnedBool = Animator.StringToHash("IsStunned");
        _pushedBool = Animator.StringToHash("IsPushed");
        _draggedBool = Animator.StringToHash("IsDragged");
        _draggingBool = Animator.StringToHash("IsDragging");
    }

    // Update is called once per frame
    void Update()
    {
        _anim.SetBool(_shovedBool, _hero.IsShoved);
        _anim.SetBool(_draggedBool, _hero.IsDraggedByOther);
        _anim.SetBool(_draggingBool, _hero.IsDraggingOther);
        _anim.SetBool(_pushedBool, _hero.IsPushed);
        _anim.SetBool(_stunnedBool, _hero.IsStunned);
        _anim.SetBool(_groundedBool, _hero.IsGrounded);
        _anim.SetBool(_movingBool, _hero.IsMoving);
        _anim.SetBool(_jumpingBool, _hero.IsJumping);
        _anim.SetBool(_fallingBool, _hero.IsFalling);
        _anim.SetBool(_doubleJumpingBool, _hero.IsDoubleJumping);
        _anim.SetBool(_pushingBool, _hero.IsPushing);
        _anim.SetBool(_pushFailedBool, _hero.IsPushFailed);
        _anim.SetFloat(_moveSpeedFloat, _hero.CurrentSpeed / _hero.MaxMoveSpeed);

        if (_hero.IsGrabbing)
        {
            if (_hero.CurrentGrab.GrabbablePosition == GrabbablePosition.InFrontTwoHands && !_hero.CurrentGrab.UseGunAnimation)
            {
                _anim.SetBool(_draggingBool, true);
                _anim.SetBool(_gunBool, false);
            } else if (_hero.CurrentGrab.GrabbablePosition != GrabbablePosition.InFrontTwoHands && !_hero.CurrentGrab.UseGunAnimation)
            {
                _anim.SetBool(_draggingBool, false);
                _anim.SetBool(_gunBool, false);
            } else if (_hero.CurrentGrab.GrabbablePosition != GrabbablePosition.InFrontTwoHands && _hero.CurrentGrab.UseGunAnimation)
            {
                _anim.SetBool(_draggingBool, false);
                _anim.SetBool(_gunBool, true);
            } else
            {
                _anim.SetBool(_draggingBool, false);
                _anim.SetBool(_gunBool, false);
            }
        } else
        {
            _anim.SetBool(_draggingBool, false);
            _anim.SetBool(_gunBool, false);
        }
    }
}
