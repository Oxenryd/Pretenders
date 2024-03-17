using UnityEngine;

namespace Assets.Scripts
{
    internal class BouncePad : MonoBehaviour
    {
        [SerializeField] private float _bouncePower;
        private void OnCollisionEnter(Collision collision)
        {
            GameObject _theBouncedOne = collision.gameObject;
            Rigidbody _rigidbody = _theBouncedOne.GetComponent<Rigidbody>();
            _rigidbody.AddForce(Vector3.up * _bouncePower);
        }
    }
}
