using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    internal interface ICollectable
    {
        public bool Collected { get; set; }
        public bool IsCollectable { get; set; } 
        public GameObject gameObject { get; }
        
        public void OnPickup();
        public void Spawn();
        public void OnExpire();
        public void OnActivation();

    }
}
