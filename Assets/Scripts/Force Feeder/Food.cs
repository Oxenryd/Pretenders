using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{

    public bool IsGrabbed { get; set; }
    public GrabbablePosition GrabbablePosition { get; }
    public Vector3 GrabPointOffset { get; }
    public GameObject GameObject { get; }
    public bool BeingGrabbed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Grab(GameObject grabber)
    {

    }

    public void Grab(ICharacterMovement grabber)
    {
        throw new System.NotImplementedException();
    }

    public void Drop()
    {
        throw new System.NotImplementedException();
    }
}
