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
        
        [Header("Environment Settings")]
        [SerializeField] private float environmentTrackLength = 20f;
        [SerializeField] private float environmentLength = 15f;
        [SerializeField] private float environmentLeftOffset = 0f;
        [SerializeField] private float environmentRightOffset = 9f;
        
        private Transform _lastSpawnedGround;
        private Transform _lastSpawnedEnvLeft; 
        private Transform _lastSpawnedEnvRight;
        
        private int _currentEmptySpaces = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            _lastSpawnedGround = transform;
            _lastSpawnedEnvLeft = transform;
            _lastSpawnedEnvRight = transform;
        }

        private void Start()
        {
            InitializeTrack();
        }

        private void Update()
        {
            if (_lastSpawnedGround.position.z < targetTrackLength + lengthMargin)
                SpawnGround();
            
            if (_lastSpawnedEnvLeft.position.z < environmentTrackLength + lengthMargin)
                SpawnEnvironment(true);

            if (_lastSpawnedEnvRight.position.z < environmentTrackLength + lengthMargin)
                SpawnEnvironment(false);
        }

        private void InitializeTrack()
        {
            while (_lastSpawnedGround.position.z < targetTrackLength)
            {
                SpawnGround();
            }
            
            while (_lastSpawnedEnvLeft.position.z < environmentTrackLength)
            {
                SpawnEnvironment(true);
            }

            while (_lastSpawnedEnvRight.position.z < environmentTrackLength)
            {
                SpawnEnvironment(false);
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

        private void SpawnEnvironment(bool isLeft)
        {
            var lastEnv = isLeft ? _lastSpawnedEnvLeft : _lastSpawnedEnvRight;
            var type = isLeft ? PoolType.EnvironmentLeft : PoolType.EnvironmentRight;
            var xPos = isLeft ? environmentLeftOffset : environmentRightOffset;
            var position = new Vector3(xPos, 0, lastEnv.position.z + environmentLength);
        
            var envObj = ObjectPool.Instance.SpawnFromPool(type, position, Quaternion.identity);

            if (envObj == null) return;
            
            if (isLeft) _lastSpawnedEnvLeft = envObj.transform;
            else _lastSpawnedEnvRight = envObj.transform;
        }

        private bool TrySpawnObstacle(Vector3 groundPos)
        {
            if (UnityEngine.Random.Range(0f, 100f) > obstacleSpawnChance)
                return false;
            
            bool isTriple = UnityEngine.Random.Range(0f, 100f) <= tripleObstacleChance;

            if (isTriple)
                SpawnTripleObstacle(groundPos);
            else
                SpawnSingleObstacle(groundPos, PoolType.OneLineObstacle);

            return true;
        }

        private void SpawnTripleObstacle(Vector3 groundPos)
        {
            var position = new Vector3(3, 0, groundPos.z); 
            ObjectPool.Instance.SpawnFromPool(PoolType.FullLineObstacle, position, Quaternion.identity);
        }

        private void SpawnSingleObstacle(Vector3 groundPos, PoolType obstacleType)
        {
            int[] xPos = { 0, 3, 6 };
            var randomX = UnityEngine.Random.Range(xPos[0], xPos[2]);
            var position = new Vector3(randomX, 0, groundPos.z);
            ObjectPool.Instance.SpawnFromPool(obstacleType, position, Quaternion.identity);
        }
    }
}
