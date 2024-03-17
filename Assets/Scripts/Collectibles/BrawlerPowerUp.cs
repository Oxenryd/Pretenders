using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Collectibles
{
    public class BrawlerPowerUp : MonoBehaviour, IPowerUp
    {
        public float LifeTime { get; set; } = 10f;
        public float ActivateTime { get; set; } = 5f;
        public bool Collected { get; set; } = false;
        public bool IsCollectable { get; set; } = false;
        public bool Expired { get; set; } = false;
        private EasyTimer _timeToActivate;

        void Start()
        {
            _timeToActivate = new EasyTimer(ActivateTime);
            GameManager.Instance.EarlyUpdate += _timeToActivate.TickSubscription;
            OnSpawn();
        }

        void Update()
        {
            if (_timeToActivate.Done)
            {
                OnActivation();
            }
        }

        public void OnSpawn()
        {
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
            //apply effects to the player
        }
        public void OnExpire()
        {
            Expired = true;
        }
        public void OnEffectEnd()
        {
            OnExpire();
            //reset the changed values for the player
        }
        
        /*---------------------------------------The Loop ---------------------------------------*/ //this probably aint true no more tbh
        //CanSpawn is true --> Manager can spawn item
        //Manager spawns item --> OnSpawn called
        //Once activation timer done--> OnActivation called
        //If player collects --> OnPickup called
        //Once lifetime over and not collected --> OnExpire called
        //Once lifetime over and collected --> OnEffectEnd called (OnExpire called)
        //Respawn timer starts --> Loop restarts

    }
}
