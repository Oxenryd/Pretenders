using System.Collections.Generic;
using UnityEngine;

public class Unicorn : MonoBehaviour, IRecievable
{
    private float _speed = 1f;
    private float _rotationSpeed = 2f;
    private Vector3 _direction;
    private Vector3 _targetDirection;
    private enum UnicornState {idle, walking, turning, eating, pooping, puking, denying };
    [SerializeField] private UnicornState _state = UnicornState.idle;
    private float _changeStateTrigger;
    private float _passedTimeSinceLastStateChange = 0f;
    //private List<>
    [SerializeField] private TransferAlert _transferAlert;
    public TransferAlert TransferAlert { get { return _transferAlert; }}

    private float _rayCastLength = 3f;
    private int _wallLayerMask;
    private float _directionEqualityThreshold = 0.01f;
    [SerializeField] private int _score = 0;
    [SerializeField] private List<Food> _foodList = new List<Food>();
    private Food _foodToBeEaten = null;
    private float _passedTimeDirectionChange = 0;
    private float _eatingDistanceThreshold = 1f;
    // Start is called before the first frame update

    void Start()
    {
        _changeStateTrigger = UnityEngine.Random.Range(2,6);
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

    void Update()
    {
        UpdateState();

        //Actions performed during a state
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
        if (_foodToBeEaten == null)
        {
            _foodToBeEaten = FindClosestFood();
        }

        if (_foodToBeEaten != null)
        {
            // Rotate towards the food
            Vector3 directionToFood = (_foodToBeEaten.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToFood);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

            // Move towards the food
            transform.position += transform.forward * _speed * Time.deltaTime;

            // If close enough to the food, eat it
            //float distanceToFood = Vector3.Distance(transform.position, _foodToBeEaten.transform.position);
            //if (distanceToFood < _eatingDistanceThreshold)
            //{
            //    EatFood();
            //}
        }
    }

    private void EatFood()
    {
        _foodList.Remove(_foodToBeEaten);
        _foodToBeEaten = null;


    }

    public void Walk()
    {
        float dotProduct = Vector3.Dot(_direction, _targetDirection);

        if (dotProduct > 0.95)
        {
            _direction = _targetDirection;
            _passedTimeDirectionChange = 0f;
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
            foodArray[i].Detach();
            _foodList.Add(foodArray[i]);

            foreach (var collider in foodArray[i].Colliders)
            {
                collider.excludeLayers = LayerUtil.Include(11, 12, 13, 14);
            }

        }
        return 0;
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow; // Set the color of the wire sphere

    //    Vector3 rayOrigin = transform.position; // Starting from the object's position
    //    Vector3 rayDirection = transform.forward;

    //    // Define the radius of the sphere
    //    float sphereRadius = 2f; // Adjust the radius as needed

    //    // Draw the wire sphere representing the sphere cast
    //    Gizmos.DrawWireSphere(rayOrigin + rayDirection * _rayCastLength, sphereRadius);
    //}


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
        if( _foodList.Count == 0)  return null;

        for(int i = 0;i < _foodList.Count; i++)
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

}
