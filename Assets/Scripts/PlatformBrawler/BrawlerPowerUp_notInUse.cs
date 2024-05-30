﻿//using Assets.Scripts.Interfaces;
//using System;
//using UnityEngine;

//namespace Assets.Scripts.Collectibles
//{
//    public class BrawlerPowerUp : MonoBehaviour, IPowerUp
//    {
//        [SerializeField] Material InitialMaterial;
//        [SerializeField] Material FinalMaterial;
//        private Effect powerUpEffect;
//        private float duration = 1.0f;
//        private Renderer rend;
//        public float LifeTime { get; set; } = 10f;
//        public float ActivateTime { get; set; } = 5f;
//        public bool Collected { get; set; } = false;
//        public bool IsCollectable { get; set; } = false;
//        public bool Expired { get; set; } = false;
//        private EasyTimer _timeToActivate;
//        public Vector3 GetPosition()
//        {
//            return transform.position;
//        }
//        public void SetPosition(Vector3 position)
//        {
//            transform.position = position;
//        }
//        public Collider GetCollider()
//        {
//            return transform.GetComponent<Collider>();
//        }
//        void Start()
//        {
//            _timeToActivate = new EasyTimer(ActivateTime);
//            gameObject.SetActive(false);
//            rend = GetComponent<Renderer>();
//            rend.material = InitialMaterial;
//            powerUpEffect = new Effect();
//        }

//        void Update()
//        {
//            if (_timeToActivate.Done)
//            {
//                OnActivation();
//            }
//            else
//            {
//                float lerp = Mathf.PingPong(Time.time, duration) / duration;
//                rend.material.Lerp(InitialMaterial, FinalMaterial, lerp);
//            }
//            Rotation();
//        }

//        private void Rotation()
//        {
//            float rotationSpeed = 30f;
//            float rotationAngle = Time.deltaTime * rotationSpeed;
//            gameObject.transform.Rotate(rotationAngle, rotationAngle, rotationAngle);
//        }

//        public void Spawn()
//        {
//            gameObject.SetActive(true);
//            _timeToActivate.Reset();
//        }
//        public void OnActivation()
//        {
//            rend.material = FinalMaterial;
//            IsCollectable = true;
//        }
//        public void OnPickup()
//        {
//            Collected = true;
//            IsCollectable = false;
//            ApplyEffect();
//        }
//        public void ApplyEffect()
//        {
//            powerUpEffect.Activate();
//        }
//        public void OnExpire()
//        {
//            Collected = false;
//            gameObject.SetActive(false);
//        }
        
//        public void OnTriggerEnter(Collider other)
//        {
//            var hero = other.gameObject.GetComponent<HeroMovement>();
//            if (hero == null)
//            {
//                return;
//            }
//            if (IsCollectable)
//            {
//                OnPickup();
//                hero.Effect = powerUpEffect;
//            }
//            else
//            {
//                return;
//            }
//        }
//    }
//}
