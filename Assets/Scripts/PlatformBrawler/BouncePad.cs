using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    /// <summary>
    /// A class which handles the behaviour of bouncepad objects in the platform brawler minigame.
    /// Inherits from the Grabbable class to allow the bouncepad to be picked up and moved by players.
    /// </summary>
    internal class BouncePad : Grabbable
    {
        private Animator _animator;
        private Rigidbody _body;
        [SerializeField] private float _bouncePower;
        [SerializeField] private float _slowFall;
        [SerializeField] private float _rayLength = 0.67f;

        private bool[] _playerBounced = new bool[] {false,false,false,false};
        private EasyTimer[] _bouncedTimers = new EasyTimer[4];

        void Start()
        {
            base.Start();
            ColliderEnabledWhileGrabbed = false; //Makes sure that players can not be bounced away while the boucepad is being carried
            _animator = GetComponent<Animator>();
            _body = GetComponent<Rigidbody>();
            for(int i = 0; i < 4; i++)
            {
                _bouncedTimers[i] = new EasyTimer(0.5f, false, true);
            }
        }
        /// <summary>
        /// Fixed update which uses a raycast to check if the player is landing on a platform making
        /// sure that they do not continue falling through.
        /// Resets the bounced state of the player once the bounce timer is finished.
        /// </summary>
        void FixedUpdate()
        {
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _rayLength))
            {
                if (hit.collider.gameObject.layer == GlobalValues.PLATFORM_LAYER)
                {
                    _body.isKinematic = true;
                }
            } else
                _body.isKinematic = false;

            for (int i = 0;i < 4;i++)
            { 
                if (_playerBounced[i] && _bouncedTimers[i].Done)
                {
                    _playerBounced[i] = false;
                }
            }
        }
        /// <summary>
        /// Method which handles what should happen when a player collides with a bouncepad. 
        /// Adds upward force to the player and plays the bouncepad animation.
        /// </summary>
        /// <param name="collision"></param>
        void OnCollisionEnter(Collision collision)
        {        
            GameObject theBouncedOne = collision.gameObject;
            if(theBouncedOne.layer == GrabberLayer && ColliderEnabledWhileGrabbed)
            {
                return;
            }
            Rigidbody rigidbody = theBouncedOne.GetComponent<Rigidbody>();
            
            if (rigidbody == null)
            {
                return;
            }

            var hero = theBouncedOne.GetComponent<Hero>();
            if (!_playerBounced[hero.Index])
            {
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
                rigidbody.AddForce(Vector3.up * _bouncePower, ForceMode.Impulse);
                _animator.Play("Bounce");
                _playerBounced[hero.Index] = true;
                _bouncedTimers[hero.Index].Reset();
            }

        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + -transform.up * _rayLength);
        }
    }
}
