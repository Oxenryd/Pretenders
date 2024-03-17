using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBall : MonoBehaviour, IGrabbable
{
    [SerializeField] Collider _collider;
    private ICharacterMovement _grabber;
    public GrabbablePosition GrabbablePosition
    { get { return GrabbablePosition.InFrontTwoHands; } }

    public Vector3 GrabPointOffset
    { get { return new Vector3(0, 1f, 1f); } }

    public GameObject GameObject
    { get { return this.GameObject; } }

    public bool IsGrabbed
    { get; set; }

    public bool BeingGrabbed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    public void Grab(ICharacterMovement grabber)
    {
        IsGrabbed = true;
        _grabber = grabber;
        _grabber.Grab(this);
        _collider.attachedRigidbody.isKinematic = true;
        _collider.enabled = false;

    }
    public void Drop()
    {
        IsGrabbed = false;
        _grabber.Drop(this);
        _grabber = null;
        _collider.enabled = true;
        _collider.attachedRigidbody.isKinematic = false;
    }

    public void Update()
    {
        if (IsGrabbed)
        {

            transform.position = _grabber.GameObject.transform.position + (_grabber.CurrentDirection + new Vector3(0, GrabPointOffset.y, 0) * GrabPointOffset.z) ;
        }
    }
}

