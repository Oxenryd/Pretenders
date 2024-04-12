using System;
using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    public class BrawlerLevelBounds : MonoBehaviour
    {
        public GameObject BrawlerLevel;
        public BoxCollider _spawnArea;
        private Bounds _bounds;
        public Vector3 TopSpawnPosition { get; private set; }
        public Vector3 BottomSpawnPosition { get; private set; }
        public Vector3 LeftSpawnPosition { get; private set; }
        public Vector3 RightSpawnPosition { get; private set; }
        public Vector3 FrontSpawnPosition { get; private set; }
        public Vector3 BackSpawnPosition { get; private set; }

        private EasyTimer _timeToMove;
        private float _moveTime = 5f;
        private Vector3 _endPosition;
        private float _acceleration;

        void Start()
        {
            _timeToMove = new EasyTimer(_moveTime);
            _spawnArea = BrawlerLevel.GetComponent<BoxCollider>();
            _bounds = _spawnArea.bounds;
            _endPosition = new Vector3(gameObject.transform.position.x + 30, 
                gameObject.transform.position.y, gameObject.transform.position.z);
            _timeToMove.Reset();
        }

        private void Update()
        {
            UpdateBounds();
            //if (_timeToMove.Done && gameObject.transform.position != _endPosition)
            //{
            //    _acceleration += 0.001f;
            //    gameObject.transform.position =
            //        Vector3.Lerp(gameObject.transform.position, _endPosition, Time.deltaTime * _acceleration);
            //}
        }

        void UpdateBounds()
        {
            _bounds = _spawnArea.bounds;
            TopSpawnPosition = _bounds.center + new Vector3(0, _bounds.extents.y, 0);
            BottomSpawnPosition = _bounds.center - new Vector3(0, _bounds.extents.y, 0);
            LeftSpawnPosition = _bounds.center - new Vector3(_bounds.extents.x, 0, 0);
            RightSpawnPosition = _bounds.center + new Vector3(_bounds.extents.x, 0, 0);
            FrontSpawnPosition = _bounds.center + new Vector3(0, 0, _bounds.extents.z);
            BackSpawnPosition = _bounds.center - new Vector3(0, 0, _bounds.extents.z);
        }
    }
}
