using Assets.Scripts.Collectibles;
using Assets.Scripts.Interfaces;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    public class BrawlPowerUpManager : MonoBehaviour
    {
        public BrawlerLevelBounds BrawlerLevelBounds;
        public GameObject[] powerUpObjects;
        private BrawlerPowerUp[] powerUps;
        private BrawlerPowerUp _currentPowerUp;
        [SerializeField] Collider[] PlatformColliders;
        public BrawlerPowerType CurrentPowerUpInRotation { get; set; }
        private float _respawnTime { get; set; } = 20f;
        private float _minRespawnBuffer { get; set; } = 0f;
        private float _maxRespawnBuffer { get; set; } = 15f;
        private float _timeBeforeFirstSpawn = 10f;
        private EasyTimer _timeToRespawn;
        private EasyTimer _timeActive; 

        // -15, 15   50,51   1, 14
                     
        public void Start()
        {
            _timeToRespawn = new EasyTimer(_respawnTime);
            _timeToRespawn.Reset();
            _timeActive = new EasyTimer(_timeBeforeFirstSpawn);
            LoadPowerUps();
        }

        public void Update()
        {
            if (_currentPowerUp == null && _timeToRespawn.Done)
            {
                SpawnPowerUp();
            }
            if (_currentPowerUp != null && _timeActive.Done)
            {
                DespawnPowerUp();
            }
            if(_currentPowerUp.Collected)
            {
                DespawnPowerUp();
            }

        }
        private void LoadPowerUps()
        {
            powerUps = new BrawlerPowerUp[powerUpObjects.Length];

            for (int i = 0; i < powerUpObjects.Length; i++)
            {
                if (powerUpObjects[i] != null)
                {
                    powerUps[i] = powerUpObjects[i].GetComponent<BrawlerPowerUp>();
                }
            }
        }
        private void SpawnPowerUp()
        {
            RandomisePowerUp();
            BrawlerPowerType type = CurrentPowerUpInRotation;
            bool validSpawnPosition = false;
            _currentPowerUp = powerUps[(int)type];
            
            _currentPowerUp.transform.position = RandomiseSpawnPoint();
            
            while (!validSpawnPosition)
            {
                bool overlapping = false;
                foreach (Collider collider in PlatformColliders)
                {
                    if (collider.transform.position == _currentPowerUp.transform.position)
                    {
                        overlapping = true;
                        break;
                    }
                    if (!overlapping)
                    {
                        validSpawnPosition = true;
                    }
                    else
                    {
                        _currentPowerUp.transform.position = RandomiseSpawnPoint();
                    }
                }               
            }               
                _timeActive.Reset();
                _currentPowerUp.Spawn();
        }
        private void DespawnPowerUp()
        {
            _currentPowerUp.OnExpire();
            _currentPowerUp = null;
            _respawnTime = _respawnTime + UnityEngine.Random.Range(_minRespawnBuffer, _maxRespawnBuffer);
            _timeToRespawn.Reset();
        }
        private Vector3 RandomiseSpawnPoint()
        {
            Vector3 topSpawn = BrawlerLevelBounds.TopSpawnPosition;
            Vector3 bottomSpawn = BrawlerLevelBounds.BottomSpawnPosition;
            Vector3 leftSpawn = BrawlerLevelBounds.LeftSpawnPosition;
            Vector3 rightSpawn = BrawlerLevelBounds.RightSpawnPosition;
            Vector3 backSpawn = BrawlerLevelBounds.BackSpawnPosition;
            Vector3 frontSpawn = BrawlerLevelBounds.FrontSpawnPosition;
            Vector3 _randomPosition = new Vector3(UnityEngine.Random.Range(leftSpawn.x, rightSpawn.x),
                UnityEngine.Random.Range(bottomSpawn.y, topSpawn.y), UnityEngine.Random.Range(backSpawn.z, frontSpawn.z));
            return _randomPosition;
        }
        private void RandomisePowerUp()
        {
            CurrentPowerUpInRotation = (BrawlerPowerType)Enum.ToObject(typeof(BrawlerPowerType), UnityEngine.Random.Range(0, 400)/100);
        }
    
    }
}
