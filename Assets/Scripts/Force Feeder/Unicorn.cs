using System.Collections.Generic;
using UnityEngine;

public class Unicorn : MonoBehaviour, IRecievable
{

    private float _speed = 2f;
    private float _rotationSpeed = 2f;
    private Vector3 _direction;
    private Vector3 _targetDirection;
    [SerializeField] private UnicornState _state = UnicornState.idle;
    [SerializeField] private int _index;
    public int Index { get { return _index; } }
    private float _changeStateTrigger;
    private float _passedTimeSinceLastStateChange = 0f;
    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert { get { return _transferAlert; } }
    private float _rayCastLength = 2f;
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private int _score = 0;
    public int Score { get { return _score; } }
    [SerializeField] private List<Food> _foodList = new List<Food>();
    private Food _foodToBeEaten = null;
    private float _eatingDistanceThreshold = 2f;

    private EasyTimer _eatTimer;
    private EasyTimer _collissionTimer;
    private RaycastHit _previousCollider = new RaycastHit();

    public event ScoreReachedEventHandler OnScoreReached;

    void Start()
    {
        _eatTimer = new EasyTimer(0.5f);

        _changeStateTrigger = UnityEngine.Random.Range(2, 6);
        _direction = transform.forward;
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
        Vector3 rayOrigin =  new Vector3(transform.position.x, transform.position.y- 2.0f,transform.position.z); // Starting from the object's position
        Vector3 rayDirection = _targetDirection - transform.position; // Direction towards the target
        RaycastHit hitInfo;

        // Perform the raycast
        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, _rayCastLength) && _state != UnicornState.eating)
        {
            if(hitInfo.collider != _previousCollider.collider)
            {
                _previousCollider = hitInfo;
                // Get the direction to the hit point
                Vector3 targetDirection = hitInfo.point - transform.position;

                // Calculate the reflection direction
                Vector3 reflectedDirection = Vector3.Reflect(rayDirection, hitInfo.normal);

                // Calculate the rotation towards the reflected direction
                Quaternion targetRotation = Quaternion.LookRotation(reflectedDirection);

                // Smoothly rotate towards the hit point
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            }
        }
    }

    void Update()
    {
        UpdateState();

        if (_state == UnicornState.walking)
        {
            Walk();
        }
        else if (_state == UnicornState.eating)
        {
            Eat();
        }

    }

    void Eat()
    {
        if (_foodToBeEaten == null && _foodList.Count > 0) _foodToBeEaten = FindClosestFood();
        else
        {
            // Rotate towards the food

            Vector3 directionToFood = (_foodToBeEaten.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToFood);
            Quaternion rotationY = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, rotationY.eulerAngles.y, 0f);

            // If close enough to the food, eat it
            float distanceToFood = Vector3.Distance(transform.position, _foodToBeEaten.transform.position);

            // Move towards the food
            if (distanceToFood > _eatingDistanceThreshold)
            {
                transform.position += transform.forward * _speed * Time.deltaTime;
            }
            else if (_eatTimer.Done)
            {
                EatFood();
            }


        }

    }

    private void EatFood()
    {
        _foodList.Remove(_foodToBeEaten);
        _foodToBeEaten.Hide();
        _foodToBeEaten = null;
    }

    public void Walk()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);

        float dotProduct = Vector3.Dot(_direction, _targetDirection);

        if (dotProduct > 0.95)
        {
            _direction = _targetDirection;

        }
        else
        {
            RotatateDirection();
        }

        transform.position = transform.position + _direction.normalized * _speed * Time.deltaTime;

        if (_direction != Vector3.zero)
        {
            transform.rotation = TransformHelpers.FixNegativeZRotation(Vector3.forward, _direction);
        }
    }

    private void RotatateDirection()
    {

        float rotationSpeed = Time.deltaTime * _rotationSpeed;

        Quaternion currentRotation = Quaternion.LookRotation(_direction);
        Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);

        // Slerp between the current rotation and the target rotation
        Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed);

        // Convert the new rotation back to a direction vector
        _direction = newRotation * Vector3.forward;

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
            foodArray[i].Detach(0.25f);
            _foodList.Add(foodArray[i]);

            foreach (var collider in foodArray[i].Colliders)
            {
                collider.excludeLayers = LayerUtil.Include(11, 12, 13, 14);
            }
        }

        if (_score >= GlobalValues.WINNING_POINTS_FORCE_FEEDER)
        {
            OnScoreReached?.Invoke();
        }

        return 0;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z);
        Vector3 rayDirection = transform.forward;

        // Draw the ray
        Gizmos.DrawRay(rayOrigin, rayDirection * _rayCastLength);
    }


    void UpdateState()
    {
        if (_state != UnicornState.eating && _foodList.Count > 0)
        {
            _state = UnicornState.eating;
        }
        else if (_state == UnicornState.eating && _foodList.Count == 0)
        {
            _state = UnicornState.idle;
        }
        else if (_state == UnicornState.walking || _state == UnicornState.idle)
        {
            _passedTimeSinceLastStateChange += GameManager.Instance.DeltaTime;

            // Changing gamestate if passedTimeSinceLastGameState is greater than changestate
            if (_passedTimeSinceLastStateChange > _changeStateTrigger)
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

                _changeStateTrigger = UnityEngine.Random.Range(3, 6);
                _passedTimeSinceLastStateChange = 0f;
            }
        }
    }

    public Food FindClosestFood()
    {
        float closestDistance = float.MaxValue;
        Food closestFood = null;
        if (_foodList.Count == 0) return null;

        _eatTimer.Reset();
        for (int i = 0; i < _foodList.Count; i++)
        {
            if (_foodList[i] != null)
            {
                float distance = Vector3.Distance(transform.position, _foodList[i].transform.position);
                if (distance < closestDistance)
                {
                    closestFood = _foodList[i];
                    closestDistance = distance;
                }
            }
        }

        _targetDirection = closestFood.transform.position - transform.position;

        return closestFood;
    }

    public delegate void ScoreReachedEventHandler();

}

