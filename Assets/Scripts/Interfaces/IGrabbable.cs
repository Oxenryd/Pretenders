using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
    public bool IsGrabbed { get; set; }
    public GrabbablePosition GrabbablePosition { get; }
    public Vector3 GrabPointOffset { get; }
    public GameObject GameObject { get; }
    public void Grab(ICharacterMovement grabber);
    public void Drop();
}