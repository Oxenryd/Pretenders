using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    public GameObject[] _objectBuffer;
    [SerializeField] private float _boundMinX = -17.11f;
    [SerializeField] private float _boundMaxX = 18.23f;
    [SerializeField] private float _boundMinY = 31.11f;
    [SerializeField] private float _boundMaxY = 40.11f;
    [SerializeField] private float _boundMinZ = 31f;
    [SerializeField] private float _boundMaxZ = 31f;
    [SerializeField] private bool _useAutomaticBoundSpawn = true;
    [SerializeField] private int _bufferedPowerups = 50;
    [SerializeField] private DefinedPowerupType[] _brawlerPowerUps;
    [SerializeField] private int[] _randomLimitUpToHundred;
    [SerializeField] private float _timeBetweenSpawns = 10f;
    [SerializeField] private bool _autoApplyByDefaultInThisGame = false;

    private EasyTimer _spawnTimer;
    private int _currentIndex = -1;

    void Start()
    { 
        _objectBuffer = new GameObject[_bufferedPowerups];
        for (int i = 0; i < _objectBuffer.Length; i++)
        {
            var randomNumber = Random.Range(0, 101);
            for (int j = _brawlerPowerUps.Length - 1; j > -1; j--)
            {
                if (randomNumber >= _randomLimitUpToHundred[j])
                {
                    _objectBuffer[i] = Instantiate(DefinedPowerUp.GetPrefab(_brawlerPowerUps[j]));

                    var powerup = _objectBuffer[i].GetComponent<PowerUp>();
                    powerup.SetEffect(DefinedPowerUp.GetEffect(_brawlerPowerUps[j]));

                    if (_autoApplyByDefaultInThisGame)
                        powerup.AutoApplyEffect = true;

                    break;
                }
            }
        }
        _spawnTimer = new EasyTimer(_timeBetweenSpawns);
    }

    // Update is called once per frame
    void Update()
    {
        if (_spawnTimer.Done)
        {
            if (_useAutomaticBoundSpawn)
            {
                var pos = getRandomVectorInsideBounds();
                SpawnNext(pos);
            }
            _spawnTimer.Reset();
        }
    }

    public void SpawnNext(Vector3 position)
    {
        Debug.Log($"SPAWN! @ {position}");
        _currentIndex = (_currentIndex + 1) % _objectBuffer.Length;
        var powerup = _objectBuffer[_currentIndex].GetComponent<PowerUp>();
        powerup.Spawn(position);
    }

    private Vector3 getRandomVectorInsideBounds()
    {
        var randomPos = new Vector3(Random.Range(_boundMinX, _boundMaxX), Random.Range(_boundMinY, _boundMaxY), Random.Range(_boundMinZ, _boundMaxZ));
        return randomPos;
    }
}
