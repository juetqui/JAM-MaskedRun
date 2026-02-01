using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ObjectPool
{
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public PoolType type;
            public GameObject prefab;
            public int size;
        }

        public static ObjectPool Instance;
        
        public List<Pool> pools;
        public Dictionary<PoolType, Queue<GameObject>> PoolDictionary;

        public GameObject objectToSpawn;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            PoolDictionary = new Dictionary<PoolType, Queue<GameObject>>();

            foreach (var pool in pools)
            {
                var objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    var obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                
                PoolDictionary.Add(pool.type, objectPool);
            }
        }

        public GameObject SpawnFromPool(PoolType type, Vector3 position, Quaternion rotation)
        {
            if (!PoolDictionary.ContainsKey(type))
            {
                Debug.LogWarning("Type:" + type +" doesn't exist");
                return null;
            }

            objectToSpawn = PoolDictionary[type].Dequeue();
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);
            
            PoolDictionary[type].Enqueue(objectToSpawn);
            
            return objectToSpawn;
        }
    }

    public enum PoolType
    {
        Ground,
        OneLineObstacle,
        FullColumnObstacle,
        FullMovableColumnObstacle,
        FullLineObstacle,
        EnvironmentLeft,
        EnvironmentRight
    }
}
