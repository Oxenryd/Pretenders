using UnityEngine;

namespace Assets.Scripts
{
    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private GameType CurrentGameType;
        private Effect powerUpEffect;
        public float ActivateTime { get; set; } = 5f;
        public bool Collected { get; set; } = false;
        public bool IsCollectable { get; set; } = false;
        private EasyTimer _timeToActivate;
        public enum PowerUpTypes { WeightGain, SpeedUp, UltraShove, MegaJump }
        public PowerUpTypes _powerUpType;
        private Effect weightGainEffect = new Effect();
        private Effect speedUpEffect = new Effect() { MoveSpeedMultiplier = 2f };
        private Effect ultraShoveEffect = new Effect() { ShoveMultiplier = 2f };
        private Effect megaJumpEffect = new Effect() { JumpPowerMultiplier = 2f };
        public Vector3 GetPosition()
        {
            return transform.position;
        }
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        public Collider GetCollider()
        {
            return transform.GetComponent<Collider>();
        }
        void Start()
        {
            _timeToActivate = new EasyTimer(ActivateTime);
            gameObject.SetActive(false);
        }
        private void Update()
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
            switch (_powerUpType)
            {
                case PowerUpTypes.WeightGain:
                    powerUpEffect = new Effect();
                    break;
                case PowerUpTypes.SpeedUp:
                    powerUpEffect = new Effect() { MoveSpeedMultiplier = 2f };
                    break;
                case PowerUpTypes.UltraShove:
                    powerUpEffect = new Effect() { ShoveMultiplier = 2f };
                    break;
                case PowerUpTypes.MegaJump:
                    powerUpEffect = new Effect() { JumpPowerMultiplier = 2f };
                    break;
                default:
                    break;
            }
            powerUpEffect.Activate();
        }
        public void OnExpire()
        {
            Collected = false;
            gameObject.SetActive(false);
        }
        public void OnTriggerEnter(Collider other)
        {
            var hero = other.gameObject.GetComponent<HeroMovement>();
            if (hero == null)
            {
                return;
            }
            if (IsCollectable)
            {
                OnPickup();
                hero.Effect = powerUpEffect;
            }
            else
            {
                return;
            }
        }

    }
}
