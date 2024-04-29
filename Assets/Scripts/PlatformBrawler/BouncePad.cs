using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    internal class BouncePad : Grabbable
    {
        private Animator _animator;
        private Rigidbody _body;
        [SerializeField] private float _bouncePower;
        [SerializeField] private float _slowFall;       

        public void Start()
        {
            base.Start();
            ColliderEnabledWhileGrabbed = false;
            _animator = GetComponent<Animator>();
            _body = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {        
            GameObject _theBouncedOne = collision.gameObject;
            if(_theBouncedOne.layer == GrabberLayer && ColliderEnabledWhileGrabbed)
            {
                return;
            }
            Rigidbody _rigidbody = _theBouncedOne.GetComponent<Rigidbody>();
            
            if (_rigidbody == null)
            {
                return;
            }
                       
            _rigidbody.AddForce(Vector3.up * _bouncePower, ForceMode.Impulse);           
            _animator.Play("Bounce");
        }

    }
}
