using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Collectibles
{
    public class BrawlerPowerUp : MonoBehaviour, IPowerUp
    {
        [SerializeField] Material InitialMaterial;
        [SerializeField] Material FinalMaterial;
        private float duration = 1.0f;
        private Renderer rend;
        [SerializeField] HeroMovement[] Player;
        public BrawlerPowerType Type { get; set; }
        public float LifeTime { get; set; } = 10f;
        public float ActivateTime { get; set; } = 5f;
        public bool Collected { get; set; } = false;
        public bool IsCollectable { get; set; } = false;
        public bool Expired { get; set; } = false;
        private EasyTimer _timeToActivate;
        private Quaternion _initialRotation;
        private Vector3 _initialScale;

        void Start()
        {
            _timeToActivate = new EasyTimer(ActivateTime);
            gameObject.SetActive(false);
            rend = GetComponent<Renderer>();
            rend.material = InitialMaterial;
            _initialRotation = gameObject.transform.rotation;
            _initialScale = gameObject.transform.localScale;
        }

        void Update()
        {
            if (_timeToActivate.Done)
            {
                OnActivation();
            }
            else
            {
                float lerp = Mathf.PingPong(Time.time, duration) / duration;
                rend.material.Lerp(InitialMaterial, FinalMaterial, lerp);

                float rotationSpeed = 20f; 
                float rotationAngle = Time.deltaTime * rotationSpeed;
                gameObject.transform.Rotate(rotationAngle, 0, 0);
                float scaleFactor = Mathf.Lerp(0.5f, 1.5f, Mathf.PingPong(Time.time, duration) / duration);
                gameObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }

        public void Spawn()
        {
            gameObject.SetActive(true);
            _timeToActivate.Reset();
        }
        public void OnActivation()
        {
            rend.material = FinalMaterial;
            IsCollectable = true;
            gameObject.transform.rotation = _initialRotation;
            gameObject.transform.localScale = _initialScale;
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
            if (IsCollectable)
            {
                OnPickup();
            }
            else
            {
                return;
            }
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

