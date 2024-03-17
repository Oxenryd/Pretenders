using Assets.Scripts.Collectibles;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class BrawlPowerUpManager : MonoBehaviour
    {
        public BrawlerPowerUp WeightGain;
        public BrawlerPowerUp SpeedUp;
        public BrawlerPowerUp UltraShove;
        public BrawlerPowerUp MegaJump;

        private BrawlerPowerUp _currentPowerUp;
        public BrawlerPowerType CurrentPowerUpInRotation { get; set; }
        private float _respawnTime { get; set; } = 20f;
        private EasyTimer _timeToRespawn;
        private EasyTimer _timeActive;
        
        

        public void Start()
        {
            _timeToRespawn = new EasyTimer(_respawnTime);
            GameManager.Instance.EarlyUpdate += _timeToRespawn.TickSubscription;  
            _timeToRespawn.Reset();
            _timeActive = new EasyTimer(WeightGain.LifeTime);
            GameManager.Instance.EarlyUpdate += _timeActive.TickSubscription;
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

        }

        void SpawnPowerUp()
        {
            RandomisePowerUp();

            BrawlerPowerUp _powerUp = GetPowerUpType(CurrentPowerUpInRotation);
            if (_powerUp != null)
            {
                _currentPowerUp = Instantiate(_powerUp, transform.position, Quaternion.identity);
                _timeActive.Reset();
            }
        }

        void DespawnPowerUp()
        {
            Destroy(_currentPowerUp.gameObject);
            _respawnTime = 20f + UnityEngine.Random.Range(0, 15);
            _timeToRespawn.Reset();
        }

        void RandomisePowerUp()
        {
            CurrentPowerUpInRotation = (BrawlerPowerType)Enum.ToObject(typeof(BrawlerPowerType), UnityEngine.Random.Range(0, 4));
        }

        private BrawlerPowerUp GetPowerUpType(BrawlerPowerType brawlerPowerType)
        {
            switch (brawlerPowerType)
            {
                case BrawlerPowerType.WeightGain:
                    return WeightGain;
                case BrawlerPowerType.SpeedUp:
                    return SpeedUp;
                case BrawlerPowerType.UltraShove:
                    return UltraShove;
                case BrawlerPowerType.MegaJump:
                    return MegaJump;
                default: 
                    return null;
            }
        }
    }
}
