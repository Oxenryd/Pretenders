using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroAnimation : MonoBehaviour
{
    private Animator _animator;
    private ICharacterMovement _characterMovement;
    private string _isJumpingBoolName = "IsJumping";
    private string _isMovingBoolName = "IsMoving";
    private string _isFallingBoolName = "IsFalling";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _characterMovement = transform.parent.GetComponent<HeroMovement>();
    }

    void Update()
    {
        _animator.SetBool(_isMovingBoolName, _characterMovement.IsMoving);
        _animator.SetBool(_isJumpingBoolName, _characterMovement.IsJumping);
        _animator.SetBool(_isFallingBoolName, _characterMovement.IsFalling);

        transform.eulerAngles = _characterMovement.CurrentDirection;
    }
}
