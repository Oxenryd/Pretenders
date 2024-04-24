using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    public class BackgroundMovement : MonoBehaviour
    {
        private EasyTimer _timeToMove;
        float _moveTime;
        float _acceleration;
        Vector3 _targetPosition;
        Vector3 _originalPosition;
        Vector3 _currentPosition;
        float _distance = 50;
        void Start()
        {
            _moveTime = UnityEngine.Random.Range(20, 30);
            _timeToMove = new EasyTimer(_moveTime);
            _currentPosition = gameObject.transform.position;
            _originalPosition = _currentPosition;
            _targetPosition = new Vector3
                (gameObject.transform.position.x + _distance, gameObject.transform.position.y, gameObject.transform.position.z);
            Debug.Log(_currentPosition);
        }
        void Update()
        {
            _currentPosition = gameObject.transform.position;
            if (_timeToMove.Done && _currentPosition != _targetPosition)
            {
                _acceleration += 0.0001f;
                gameObject.transform.position =
                    Vector3.Lerp(gameObject.transform.position, _targetPosition, Time.deltaTime * _acceleration);
            }    
            if (_timeToMove.Done && _currentPosition == _targetPosition)
            {
                _acceleration += 0.0001f;
                gameObject.transform.position =
                    Vector3.Lerp(_currentPosition, _originalPosition, Time.deltaTime * _acceleration);
            }
        }
    }
}
