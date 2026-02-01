using System;
using UnityEngine;
using Random = System.Random;

namespace ObjectPool
{
    public class ObjectSpawner : MonoBehaviour
    {
        public static ObjectSpawner Instance;
        
        [SerializeField] private float groundLength = 3f;

        [Header("Track Length Settings")]
        [SerializeField] private float targetTrackLength = 100f; 
        [SerializeField] private float lengthMargin = 10f;
        
        [Header("Obstacle Settings")]
        [SerializeField, Range(0f, 100f)] private float obstacleSpawnChance = 30f;
        [SerializeField, Range(0f, 100f)] private float tripleObstacleChance = 20f;
        [SerializeField] private int minEmptySpaces = 2;
        
        private Transform _lastSpawnedGround;
        private int _currentEmptySpaces = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            _lastSpawnedGround = transform;
        }

        private void Start()
        {
            InitializeTrack();
        }

        private void Update()
        {
            if (_lastSpawnedGround.position.z < targetTrackLength + lengthMargin)
                SpawnGround();
        }

        private void InitializeTrack()
        {
            while (_lastSpawnedGround.position.z < targetTrackLength)
            {
                SpawnGround();
            }
        }

        public void SpawnGround()
        {
            var position = _lastSpawnedGround.position + new Vector3(0, 0, groundLength);
            var lastGround = ObjectPool.Instance.SpawnFromPool(PoolType.Ground, position, Quaternion.identity);
            
            if (lastGround != null)
                _lastSpawnedGround = lastGround.transform;

            if (_currentEmptySpaces < minEmptySpaces)
            {
                _currentEmptySpaces++;
                return;
            }
            
            if (TrySpawnObstacle(position))
                _currentEmptySpaces = 0;
        }

        private bool TrySpawnObstacle(Vector3 groundPos)
        {
            if (UnityEngine.Random.Range(0f, 100f) > obstacleSpawnChance)
                return false;
            
            bool isTriple = UnityEngine.Random.Range(0f, 100f) <= tripleObstacleChance;

            if (isTriple)
                SpawnTripleObstacle(groundPos);
            else
                SpawnSingleObstacle(groundPos);

            return true;
        }

        private void SpawnTripleObstacle(Vector3 groundPos)
        {
            var position = new Vector3(0, 0, groundPos.z); 
            ObjectPool.Instance.SpawnFromPool(PoolType.FullLineObstacle, position, Quaternion.identity);
        }

        private void SpawnSingleObstacle(Vector3 groundPos)
        {
            int[] xPos = { -3, 0, 3 };
            var randomX = UnityEngine.Random.Range(xPos[0], xPos[2]);
            var position = new Vector3(randomX, 0, groundPos.z);
            ObjectPool.Instance.SpawnFromPool(PoolType.OneLineObstacle, position, Quaternion.identity);
        }
    }
}
