using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
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
            ColliderEnabledWhileGrabbed = false;
            _animator = GetComponent<Animator>();
            _body = GetComponent<Rigidbody>();
            for(int i = 0; i < 4; i++)
            {
                _bouncedTimers[i] = new EasyTimer(0.5f, false, true);
            }
        }

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
