using UnityEngine;
using System.Collections;

namespace Assets.Scripts.PlatformBrawler
{
    internal class BouncePad : Grabbable
    {
        private Animator _animator;
        [SerializeField] private float _bouncePower;
       
        private void OnCollisionEnter(Collision collision)
        {
            _animator = GetComponent<Animator>();
            GameObject _theBouncedOne = collision.gameObject;

            Rigidbody _rigidbody = _theBouncedOne.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                return;
            }
            _rigidbody.AddForce(Vector3.up * _bouncePower);
            _animator.Play("Bounce");
        }

    }
}
