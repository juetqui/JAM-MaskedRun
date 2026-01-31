using System;
using UnityEngine;

namespace ObjectPool
{
    public class ObjectSpawner : MonoBehaviour
    {
        public static ObjectSpawner Instance;

        [SerializeField] private float groundSpawnDistance = 50f;
        
        private bool _spawningObject = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void SpawnGround()
        {
            var position = new Vector3(0, 0, groundSpawnDistance);
            
            ObjectPool.Instance.SpawnFromPool(PoolType.Ground, position, Quaternion.identity);
        }
    }
}
