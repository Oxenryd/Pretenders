using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBall : MonoBehaviour, IGrabbable
{
    public GrabbablePosition GrabbablePosition
    { get { return GrabbablePosition.InFrontTwoHands; } }

    public Vector3 GrabPointOffset
    { get; set; }

    public GameObject GameObject => throw new System.NotImplementedException();

}
