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
        bool moveRight;
        float _distance = 50;
        void Start()
        {
            _moveTime = UnityEngine.Random.Range(20, 30);
            _timeToMove = new EasyTimer(_moveTime);
            _originalPosition= gameObject.transform.position;
            
            moveRight = true;
        }
        void Update()
        {
            if (moveRight)
            {
                _targetPosition 
                    = new Vector3(_originalPosition.x + _distance, _originalPosition.y, _originalPosition.z);
            }
            else
            {
                _targetPosition 
                    = new Vector3(_originalPosition.x - _distance, _originalPosition.y, _originalPosition.z);
            }
            if (_timeToMove.Done)
            {
                _acceleration += 0.00001f;
                gameObject.transform.position =
                    Vector3.Lerp(gameObject.transform.position, _targetPosition, Time.deltaTime * _acceleration);
                if (gameObject.transform.position == _targetPosition )
                {
                    _timeToMove.Reset();
                }
            }             
        }
    }
}
