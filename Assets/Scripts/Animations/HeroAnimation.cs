using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroAnimation : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private Hero _player;
    private bool _isMoving = false;
    private bool _isJumping = false;
    private bool _isFalling = false;
    private bool _moveFallCheck = false;
    private bool _isMovingAfterFall = false;
    private bool _isGrounded;
    private string _isJumpingBoolName = "IsJumping";
    private string _isMovingBoolName = "IsMoving";
    private string _isFallingBoolName = "IsFalling";
    private string _isMoivngAfterFallBoolName = "IsMovingAfterFall";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        _animator.SetBool(_isMovingBoolName, _isMoving);
        _animator.SetBool(_isJumpingBoolName, _isJumping);
        _animator.SetBool(_isFallingBoolName, _isFalling);
        movingAfterFallCheck();
        _animator.SetBool(_isMoivngAfterFallBoolName, _isMovingAfterFall);

    }
    public void SetMoving(bool isMoving)
    {
        _isMoving = isMoving;
    }
    public void SetJumping(bool isJumping)
    {
        _isJumping = isJumping;
    }
    public void SetFalling(bool isFalling)
    {
        _isFalling = isFalling;
        if (_isFalling )
        {
            _moveFallCheck = true;
        }
    }
    public void SetGrounded(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }
    private void movingAfterFallCheck()
    {
        if (_isMoving && _moveFallCheck && _isGrounded)
        {
            _isMovingAfterFall = true;
            _moveFallCheck = false;
        }
        else
        {
            _isMovingAfterFall = false;
        }
    }
}
