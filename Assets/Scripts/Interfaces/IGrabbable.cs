using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
    public GrabbablePosition GrabbablePosition { get; }
    public Vector3 GrabPointOffset { get; }
    public GameObject GameObject { get; }
    
}