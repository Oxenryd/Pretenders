using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Collectibles
{
    public class BrawlerPowerUp : MonoBehaviour, IPowerUp
    {
        [SerializeField] HeroMovement[] Player;
        public BrawlerPowerType Type { get; set; }
        public float LifeTime { get; set; } = 10f;
        public float ActivateTime { get; set; } = 5f;
        public bool Collected { get; set; } = false;
        public bool IsCollectable { get; set; } = false;
        public bool Expired { get; set; } = false;
        private EasyTimer _timeToActivate;

        void Start()
        {
            _timeToActivate = new EasyTimer(ActivateTime);
            gameObject.SetActive(false);            
        }

        void Update()
        {
            if (_timeToActivate.Done)
            {
                OnActivation();
            }
            
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
            _timeToActivate.Reset();
        }
        public void OnActivation()
        {
            IsCollectable = true;
        }
        public void OnPickup()
        {
            Collected = true;
            IsCollectable = false;
            ApplyEffect();
        }
        public void ApplyEffect()
        {
            switch (Type) 
            {
                case BrawlerPowerType.WeightGain:
                    {

                    }
                    break;
                case BrawlerPowerType.SpeedUp:
                    {
                        
                    }
                    break;
                case BrawlerPowerType.UltraShove:
                    {

                    }
                    break;
                case BrawlerPowerType.MegaJump:
                    {
                        
                    }
                    break;
            }
        }
        public void OnExpire()
        {
            Collected = false;
            gameObject.SetActive(false);
        }
        private void OnCollisionEnter(Collision collision)
        {
            OnPickup();
            OnExpire();
        }


        /*---------------------------------------The Loop ---------------------------------------*/
        //this probably aint true no more tbh
        //CanSpawn is true --> Manager can spawn item
        //Manager spawns item --> OnSpawn called
        //Once activation timer done--> OnActivation called
        //If player collects --> OnPickup called
        //Once lifetime over and not collected --> OnExpire called
        //Once lifetime over and collected --> OnEffectEnd called (OnExpire called)
        //Respawn timer starts --> Loop restarts

    }
}

