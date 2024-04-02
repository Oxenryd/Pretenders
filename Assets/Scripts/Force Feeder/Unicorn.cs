using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Unicorn : MonoBehaviour, IRecievable
{
    private float _speed = 1f;
    private float _lerpSpeed = 2f;
    private Vector3 _direction;
    private Vector3 _targetDirection;
    private enum UnicornState {idle, walking, turning, eating, pooping, puking, denying };
    private UnicornState _state = UnicornState.idle;
    private float _changeState;
    private float _passedTimeSinceLastStateChange = 0f;
    //private List<>
    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert { get { return _transferAlert; }}

    private float _rayCastLength = 3f;
    private int _wallLayerMask;
    private float _directionEqualityThreshold = 0.01f;
    public int _score; 
    // Start is called before the first frame update
    void Start()
    {
        _changeState = UnityEngine.Random.Range(2,6);
        _direction   = transform.forward;
        _targetDirection = _direction;
        _wallLayerMask = 1 << LayerMask.NameToLayer("WALLS");

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == GlobalValues.WALL_LAYER)
        {
            Vector3 collisionNormal = collision.contacts[0].normal;
            _targetDirection = Vector3.Reflect(_direction, collisionNormal);
            _passedTimeSinceLastStateChange = 0f;
        }
    }

    void FixedUpdate()
    {
        Vector3 rayOrigin = transform.position; // Starting from the object's position
        Vector3 rayDirection = transform.forward;
        RaycastHit hitInfo;

        // Define the radius of the sphere
        float sphereRadius = 2f; // Adjust the radius as needed

        // Perform the sphere cast
        if (Physics.SphereCast(rayOrigin, sphereRadius, rayDirection, out hitInfo, _rayCastLength, _wallLayerMask))
        {
            Vector3 hitNormal = hitInfo.normal;
            _targetDirection = Vector3.Reflect(_direction, hitNormal);
        }
        else
        {
            // Handle case when sphere cast doesn't hit anything
        }
    }


    // Update is called once per frame
    void Update()
    {

        if (_state == UnicornState.walking)
        {
            MoveUnicorn();
        }

       if (_state == UnicornState.walking || _state == UnicornState.idle)
        {
            _passedTimeSinceLastStateChange += GameManager.Instance.DeltaTime;
        }

       //Change state
        if (_passedTimeSinceLastStateChange > _changeState)
        {
            if (_state == UnicornState.idle)
            {
                _targetDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
                _state = UnicornState.walking;
            }
            else if (_state == UnicornState.walking)
            {
                _state = UnicornState.idle;
            }
            _changeState = UnityEngine.Random.Range(3, 6);
            _passedTimeSinceLastStateChange = 0f;
        }


        if (_targetDirection != _direction)
        {
            // Calculate the distance between _direction and _targetDirection
            float distance = Vector3.Distance(_direction, _targetDirection);

            // If the distance is within the threshold, assign _targetDirection to _direction
            if (distance < _directionEqualityThreshold)
            {
                _direction = _targetDirection;
            }
            else
            {
                // Otherwise, lerp towards the target direction
                _direction = Vector3.Lerp(_direction, _targetDirection, Time.deltaTime * _lerpSpeed);
            }
        }
    }


    void MoveUnicorn()
    {
        transform.position = transform.position + _direction.normalized * _speed * Time.deltaTime;

        if (_direction != Vector3.zero)
        {
            transform.rotation = TransformHelpers.FixNegativeZRotation(Vector3.forward, _direction);
        }
    }

    public int Transfer(object[] recievedObject)
    {
        var foodArray = (recievedObject as Food[]);


        if (foodArray == null)
        {
            return recievedObject.Length;
        }

        // Find an empty slot on the tray
        for (int i = 0; i < foodArray.Length; i++)
        {
            _score += foodArray[i].GetPoints();
            foodArray[i].Detach();
            foodArray[i].Hide();
        }
        return 0;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // Set the color of the wire sphere

        Vector3 rayOrigin = transform.position; // Starting from the object's position
        Vector3 rayDirection = transform.forward;

        // Define the radius of the sphere
        float sphereRadius = 2f; // Adjust the radius as needed

        // Draw the wire sphere representing the sphere cast
        Gizmos.DrawWireSphere(rayOrigin + rayDirection * _rayCastLength, sphereRadius);
    }
}
