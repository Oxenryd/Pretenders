using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HeadFeet : MonoBehaviour
{
    [SerializeField] private Collider _headBox;
    [SerializeField] private Collider _feetBox;
    [SerializeField] private Hero _hero;

    private Vector3 _lastPosition;
    private Vector3 _curDirection;
    private Rigidbody _heroBody; 

    public Collider HeadBox
        { get { return _headBox; } }

    public Collider FeetBox
        { get { return _feetBox; } }
    public Rigidbody OwnerBody
        { get { return _heroBody; } }

    public Vector3 BodyDirection
        { get { return _curDirection; } }

    // Start is called before the first frame update
    void Start()
    {
        _heroBody = _hero.gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        _curDirection = (_heroBody.transform.position - _lastPosition).normalized;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _lastPosition = _heroBody.transform.position;
    }
}
