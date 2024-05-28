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
    private float _rayCastLength = 4f;
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private int _score = 0;
    public int Score { get { return _score; } }
    [SerializeField] private List<Food> _foodList = new List<Food>();
    private Food _foodToBeEaten = null;
    private float _eatingDistanceThreshold = 3f;

    private EasyTimer _eatTimer;
    private EasyTimer _eatTimerII;
    private EasyTimer _collissionTimer;
    private RaycastHit _previousCollider = new RaycastHit();

    public event ScoreReachedEventHandler OnScoreReached;

    void Start()
    {
        _eatTimer = new EasyTimer(0.5f);
        _eatTimerII = new EasyTimer(5f);

        _changeStateTrigger = UnityEngine.Random.Range(2, 6);
        _direction = transform.forward;
        _targetDirection = _direction;
        _wallLayerMask = 1 << LayerMask.NameToLayer("WALLS");

        var container = GameObject.FindWithTag(GlobalStrings.NAME_UIOVERLAY);
        _transferAlert = GameObject.Instantiate(_transferAlert, container.transform);
        _transferAlert.gameObject.SetActive(false);
    }


    void FixedUpdate()
    {

        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z);
        Vector3 rayDirection = transform.forward;
        RaycastHit hitInfo;
        rayDirection.Normalize();

        // Perform a raycast to check if a collission has occured.
        if (Physics.Raycast(rayOrigin, rayDirection, out hitInfo, _rayCastLength) && _state != UnicornState.eating)
        {
            if(hitInfo.collider != _previousCollider.collider)
            {
                _previousCollider = hitInfo;

                // Calculate the reflection direction
                _targetDirection = Vector3.Reflect(rayDirection, hitInfo.normal);

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

    /// <summary>
    /// Method for controlling a unicorn while in a state of eating.
    /// </summary>
    void Eat()
    {
        // Finds new food to be eaten 
        if (_foodToBeEaten == null && _foodList.Count > 0) _foodToBeEaten = FindClosestFood();
        else
        {
            //If food is present, the unicorn turns towards the food.
            Vector3 directionToFood = (_foodToBeEaten.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToFood);
            Quaternion rotationY = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, rotationY.eulerAngles.y, 0f);

            // Finds the distance to the food.
            float distanceToFood = Vector3.Distance(transform.position, _foodToBeEaten.transform.position);

            // Eat Timer two ensures that the food gets eaten even if the unicorn is not able to get to it for whatever reason
            if (_eatTimerII.Done)
            {
                EatFood();
            }
            //If the distance to the food is greater than the threshold, we move towards the food
            if (distanceToFood > _eatingDistanceThreshold)
            {
                transform.position += transform.forward * _speed * Time.deltaTime;
            }
            // Eat the food if the unicorn is close enough to it.
            else if (_eatTimer.Done)
            {
                EatFood();
            }
        }

    }

    /// <summary>
    /// Method for eating food that is given to the unicorn.
    /// </summary>
    private void EatFood()
    {
        // When eating the food, the item is removed from the foodlist and set to null which allows the unicorn to continue
        // Eating another piece of food. The food is then hidden which effectively returns it to the food spawner.
        _foodList.Remove(_foodToBeEaten);
        _foodToBeEaten.Hide();
        _foodToBeEaten = null;
    }

    /// <summary>
    /// Method for controlling the unicorns walk.
    /// </summary>
    public void Walk()
    {

        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f);

        float dotProduct = Vector3.Dot(_direction, _targetDirection);

        // Sets the direction to the target durectíon if dotproduct is greater than 0.97, otherwise you continue rotate the unicorn.
        if (dotProduct > 0.97)
        {
            _direction = _targetDirection;

        }
        else
        {
            RotatateDirection();
        }

        //Moves the unicorn in the given direction and also fixes the rotation.
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

        _eatTimer.Reset();
        _eatTimerII.Reset();
        _targetDirection = closestFood.transform.position - transform.position;

        return closestFood;
    }

    public delegate void ScoreReachedEventHandler();

}

