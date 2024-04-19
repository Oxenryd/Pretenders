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
        Vector3 _endPosition;
        void Start()
        {
            _timeToMove = new EasyTimer(_moveTime);
            _endPosition = new Vector3
                (gameObject.transform.position.x + 30, gameObject.transform.position.y, gameObject.transform.position.z);
        }
        void Update()
        {
            if (_timeToMove.Done && gameObject.transform.position != _endPosition)
            {
                _acceleration += 0.001f;
                gameObject.transform.position =
                    Vector3.Lerp(gameObject.transform.position, _endPosition, Time.deltaTime * _acceleration);
            }
        }
    }
}
