using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{

    private Vector3 _origin = new Vector3(0, 9, 0);
    private Vector3 _direction;
    private float _radius = 10f;
    private float _speed = 1f;
    private float _spawnSpeed = 2f;
    private float _timeSinceLastFoodSpawn = 0f;

    public bool Running = false;

    [SerializeField] private Banana _bananaPrefab;
    [SerializeField] private Watermelon _waterMelonPrefab;
    [SerializeField] private HotDog _hotDogPrefab;
    [SerializeField] private Hamburger _hamburgerPrefab;
    [SerializeField] private GameObject _foodContainer;

    private Food[] _foodArray = new Food[100];
    private int _spawnIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _foodArray.Length; i++)
        {
            Food foodItem = Random.Range(0, 10) < 5 ? Instantiate(_bananaPrefab) : Random.Range(0, 10) < 5 ? Instantiate(_hotDogPrefab) : Random.Range(0, 10) < 7 ? Instantiate(_hamburgerPrefab) : Instantiate(_waterMelonPrefab);
            foodItem.Hide();
            _foodArray[i] = foodItem;
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
        if (!Running) return;

        MoveFoodManager();

        _timeSinceLastFoodSpawn += GameManager.Instance.DeltaTime;


        if (_timeSinceLastFoodSpawn > _spawnSpeed)
        {
            SpawnFood();
            _spawnSpeed = Random.Range(1f, 2f);
        }

    }

    void MoveFoodManager()
    {
        transform.position += _direction * _speed * Time.deltaTime;
        float distance = Vector3.Distance(_origin, transform.position);

        if (distance > _radius)
        {
            _direction = CalculateNewDirection(_direction);
        }
    }

    Vector3 CalculateNewDirection(Vector3 direction)
    {

        Vector3 oppositeDirection = -direction;
        oppositeDirection += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

        return oppositeDirection.normalized;
    }

    void SpawnFood()
    {
        var idx = _spawnIndex % _foodArray.Length;
 
        _foodArray[idx].Show(transform.position);
        _foodArray[idx].CanBeGrabbed = true;
        _spawnIndex++;

        _timeSinceLastFoodSpawn = 0;
    }

}
