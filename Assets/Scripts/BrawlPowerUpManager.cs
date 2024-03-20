using Assets.Scripts.Collectibles;
using Assets.Scripts.Interfaces;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts
{
    public class BrawlPowerUpManager : MonoBehaviour
    {
        public BrawlerLevelBounds BrawlerLevelBounds;
        public GameObject[] powerUpObjects;
        private BrawlerPowerUp[] powerUps;
        private BrawlerPowerUp _currentPowerUp;
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
                _currentPowerUp.OnExpire();
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
                    powerUps[i].gameObject.SetActive(false); 
                }
            }
        }

        private void SpawnPowerUp()
        {
            RandomisePowerUp();
            BrawlerPowerType type = CurrentPowerUpInRotation;
           
                _currentPowerUp = powerUps[(int)type];

                Collider[] colliders = Physics.OverlapBox(_currentPowerUp.transform.position, 
                    _currentPowerUp.GetComponent<Collider>().bounds.extents);

                foreach (Collider collider in colliders)
                {
                if (collider.CompareTag(GlobalStrings.PLATFORM_TAG)
                || collider.CompareTag(GlobalStrings.CHARACTER_TAG))
                    {
                        Vector3 newRandomPosition = RandomiseSpawnPoint();
                        _currentPowerUp.transform.position = newRandomPosition;
                        return; 
                    }
                }
                _currentPowerUp.transform.position = RandomiseSpawnPoint();
                _currentPowerUp.gameObject.SetActive(true);
                _timeActive.Reset();                                   
        }

        private void DespawnPowerUp()
        {
            _currentPowerUp.gameObject.SetActive(false);
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
