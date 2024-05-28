using UnityEngine;

namespace Assets.Scripts.PlatformBrawler
{
    /// <summary>
    /// Class which updates the position of the game map of the platform brawler minigame.
    /// Is used to make sure that the game's powerups only spawn in reachable positions, 
    /// taking into account that the map moves during the game.
    /// </summary>
    public class BrawlerLevelBounds : MonoBehaviour
    {
        [SerializeField] private Bounds _bounds;
        public GameObject BrawlerLevel;
        public BoxCollider _spawnArea;
        public Vector3 TopSpawnPosition { get; private set; }
        public Vector3 BottomSpawnPosition { get; private set; }
        public Vector3 LeftSpawnPosition { get; private set; }
        public Vector3 RightSpawnPosition { get; private set; }
        public Vector3 FrontSpawnPosition { get; private set; }
        public Vector3 BackSpawnPosition { get; private set; }

        void Start()
        {
            _spawnArea = BrawlerLevel.GetComponent<BoxCollider>();
            _bounds = _spawnArea.bounds;
        }

        private void Update()
        {
            UpdateBounds();
        }
        /// <summary>
        /// Method to update the bounds based on the position of the BrawlerLevel gameobject's box collider,
        /// which is used to define the area that powerups will spawn.
        /// </summary>
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
