using System;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    internal interface ICollectable
    {
        public bool Collected { get; set; }
        public bool CanSpawn {  get; set; }
        public bool IsActive { get; set; } 
        public GameObject gameObject { get; }
        public float RespawnTime { get; set; }
        public float SpawnTime { get; set; }

        



    }
}
