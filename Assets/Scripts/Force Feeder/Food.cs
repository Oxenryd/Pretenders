using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour, IGrabbable
{

    public bool IsGrabbed { get; set; }
    public GrabbablePosition GrabbablePosition { get; }
    public Vector3 GrabPointOffset { get; }
    public GameObject GameObject { get; }

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
}
