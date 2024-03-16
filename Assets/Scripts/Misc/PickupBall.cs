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
    { get { return Vector3.forward * transform.localScale.z + new Vector3(0, 1.5f, 0); } }

    public GameObject GameObject
    { get { return this.GameObject; } }

    public bool IsGrabbed
    { get; set; }

    public void Grab(ICharacterMovement grabber)
    {
        IsGrabbed = true;
        _grabber = grabber;
        _collider.attachedRigidbody.isKinematic = true;
        _collider.enabled = false;
        
    }

    public void Update()
    {
        if (IsGrabbed)
        {
            transform.position = _grabber.GameObject.transform.position +  _grabber.CurrentDirection + GrabPointOffset;
        }
    }
}