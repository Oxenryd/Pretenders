using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    /// <summary>
    /// This class handles the movement of the background objects present in the platform brawler minigame. 
    /// The script is placed on a parent gameobject that has every other background object as a child, 
    /// so that all the objects move simultaneously making the game map appear to be moving.
    /// </summary>
    public class BackgroundMovement : MonoBehaviour
    {
        private EasyTimer _timeToMove;
        float _moveTime;
        float _acceleration;
        Vector3 _targetPosition;
        Vector3 _originalPosition;
        bool moveRight; //Checks if the background shopuld move to the right
        float _distance = 50;
        void Start()
        {
            _moveTime = UnityEngine.Random.Range(20, 30); //Randomly generates when the background should start moving
            _timeToMove = new EasyTimer(_moveTime);
            _originalPosition= gameObject.transform.position; //Define originalposition of the parent gameobject
            
            moveRight = true;
        }
        /// <summary>
        /// Update method which lerps the parent gameobject from its original position to a defined targetposition
        /// with some degree of acceleration, after a random amount of time between 20 and 30 seconds has passed.
        /// </summary>
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
