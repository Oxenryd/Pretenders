using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [SerializeField] Collider _collider;
    private ICharacterMovement _grabber;
    private Rigidbody _rBody;
    private EasyTimer _grabbedTimer;
    private List<int> _beingGrabbedBy = new List<int>();
    private Tug _tugOWar = null;


    public GrabbablePosition GrabbablePosition
    { get { return GrabbablePosition.InFrontTwoHands; } }

    public float TimeToGrab
    { get; set; } = 0.3f;

    public void Hide()
    {
        Hidden = true;
        _rBody.isKinematic = true;
        _collider.enabled = false;
        gameObject.SetActive(false);
    }

    public void Show(Vector3 position)
    {
        transform.position = position;
        Show();
    }
    public void Show()
    {
        Hidden = false;
        gameObject.SetActive(true);
        _collider.enabled = true;
        _rBody.isKinematic = false;
        gameObject.SetActive(true);
    }

    public bool Hidden { get; set; } = false;

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

    public void Awake()
    {
        _collider = gameObject.GetComponent<Collider>();
        _rBody = _collider.attachedRigidbody;
        _grabbedTimer = new EasyTimer(TimeToGrab);      
    }

    public void Update()
    {
        if (Hidden)
            return;

        if (IsGrabbed)
        {
            transform.position = _grabber.GameObject.transform.position + (_grabber.CurrentDirection + new Vector3(0, GrabPointOffset.y, 0) * GrabPointOffset.z) ;
        }
    }
}

