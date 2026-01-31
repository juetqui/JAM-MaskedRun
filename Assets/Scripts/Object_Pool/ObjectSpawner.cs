using UnityEngine;
using Random = System.Random;

namespace ObjectPool
{
    public class ObjectSpawner : MonoBehaviour
    {
        public static ObjectSpawner Instance;

        [SerializeField] private float groundSpawnDistance = 50f;
        
        [Header("Obstacle Settings")]
        [SerializeField, Range(0f, 100f)] private float obstacleSpawnChance = 30f;
        [SerializeField, Range(0f, 100f)] private float tripleObstacleChance = 20f;
        [SerializeField] private int minEmptySpaces = 2;
        
        // private bool _spawningObject = false;
        private int _currentEmptySpaces = 0;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void SpawnGround(Vector3 prevPos)
        {
            var position = new Vector3(prevPos.x, 0, groundSpawnDistance);
            ObjectPool.Instance.SpawnFromPool(PoolType.Ground, position, Quaternion.identity);

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
            var position = new Vector3(groundPos.x, 0, groundPos.z);
            Debug.Log(position.x);
            ObjectPool.Instance.SpawnFromPool(PoolType.OneLineObstacle, position, Quaternion.identity);
        }
    }
}
