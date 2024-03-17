using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{

    private Vector3 _origin = new Vector3(0, 6, 0);
    private Vector3 _direction;
    private float _radius = 5f;
    private float _speed = 1f;
    private float _spawnSpeed = 2f;
    private float _timeSinceLastFoodSpawn = 0f;

    [SerializeField] private Banana _bananaPrefab;
    [SerializeField] private GameObject _foodContainer;

    private Food[] _foodArray = new Food[10];


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _foodArray.Length; i++)
        {
            //Food foodItem = Instantiate();

        }
        // Generate a random angle in radians
        float angle = Random.Range(0f, 2f * Mathf.PI); // Range is 0 to 2π (360 degrees)

        // Convert the angle to a direction vector in the XZ plane
        float x = Mathf.Cos(angle);
        float z = Mathf.Sin(angle);

        _direction = new Vector3(x, 0f, z);
    }

    // Update is called once per frame
    void Update()
    {

        MoveFoodManager();

        _timeSinceLastFoodSpawn += GameManager.Instance.DeltaTime;


        //if (_timeSinceLastFoodSpawn > _spawnSpeed)
        //{
        //    SpawnFood();
        //}

    }

    void MoveFoodManager()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        float distance = Vector3.Distance(_origin, transform.position);

        if (distance > _radius && MovingAwayFromOrigin(_direction))
        {
            _direction = CalculateNewDirection(_direction);
        }
    }

    Vector3 CalculateNewDirection(Vector3 direction)
    {

        Vector3 oppositeDirection = -direction;
        oppositeDirection += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));

        return oppositeDirection.normalized;
    }

    bool MovingAwayFromOrigin(Vector3 direction)
    {
        Vector3 currentPosition = transform.position;
        Vector3 displacement = currentPosition - _origin;
        float dotProduct = Vector3.Dot(displacement.normalized, direction.normalized);

        return false ? dotProduct < 0 : true;
    }

    void SpawnFood()
    {
        //GameObject newFood = Instantiate(_foodPrefab, transform.position, transform.rotation);

        // Set the _foodContainer as the parent of the newly spawned food
        //if (_foodContainer != null)
        //{
        //    newFood.transform.parent = _foodContainer.transform;
        //}
        //else
        //{
        //    Debug.LogError("Food Container is not assigned!");
        //}

        _timeSinceLastFoodSpawn = 0;
        _spawnSpeed = Random.Range(2, 4);
        Debug.Log(_spawnSpeed);
    }

}
