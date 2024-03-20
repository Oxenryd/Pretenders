namespace Assets.Scripts.Interfaces
{
    internal interface IPowerUp: ICollectable
    {
        public float LifeTime { get; set; }
        public float ActivateTime { get; set; }
        public bool Expired { get; set; }
        public void ApplyEffect(); 
    }
}
