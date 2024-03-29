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
        private float _respawnTime = 2f;
        private float _activeTime  = 15f;
        private float _minRespawnBuffer = 0f;
        private float _maxRespawnBuffer = 2f;
        private EasyTimer _timeToRespawn;
        private EasyTimer _timeActive;

        // -15, 15   50,51   1, 14

        public void Start()
        {
            _timeToRespawn = new EasyTimer(_respawnTime);
            _timeActive = new EasyTimer(_activeTime);
            _timeToRespawn.Reset();
            LoadPowerUps();
        }

        public void Update()
        {
            if (_currentPowerUp == null && _timeToRespawn.Done)
            {
                SpawnPowerUp();
            }
            if (_currentPowerUp != null && _currentPowerUp.Collected)
            {
                DespawnPowerUp();
            }
            if (_currentPowerUp != null && _timeActive.Done)
            {
                DespawnPowerUp();
            }
            //if (_currentPowerUp != null)
            //{
            //    Debug.Log(_currentPowerUp.Collected);
            //}
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
            _currentPowerUp = powerUps[(int)type];
      
            _currentPowerUp.transform.position = RandomiseSpawnPoint();
            Collider powerUpCollider = _currentPowerUp.GetComponent<Collider>();

            while (true)

            {
                bool overlapping = false;
                foreach (Collider collider in PlatformColliders)
                {

                    if (powerUpCollider.bounds.Intersects(collider.bounds))                       
                    {
                        overlapping = true;
                    }                
                }
                if (!overlapping)
                {
                    break;
                }
                else
                {
                    _currentPowerUp.transform.position = RandomiseSpawnPoint();
                }
            }               
                _currentPowerUp.Spawn();
                _timeActive.Reset();
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
            CurrentPowerUpInRotation = (BrawlerPowerType)Enum.ToObject(typeof(BrawlerPowerType), UnityEngine.Random.Range(0, 400) / 100);
        }

    
    }
}
