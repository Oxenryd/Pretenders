using UnityEngine;
namespace Assets.Scripts.Interfaces
{
    internal interface IPowerUp: ICollectable
    {
        Vector3 GetPosition();
        void SetPosition(Vector3 position);
        Collider GetCollider();
        public float LifeTime { get; set; }
        public float ActivateTime { get; set; }
        public bool Expired { get; set; }
        public void ApplyEffect(); 
    }
}
