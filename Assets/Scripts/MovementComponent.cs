using UnityEngine;
using ObjectPool;

// [RequireComponent(typeof(Rigidbody))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField] private PoolType _type;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _newSpawnDist = -60f;
    [SerializeField] private float _despawnDist = -110f;
    
    // private Rigidbody _rb;
    private bool _canSpawn = true;
    
    void Start()
    {
        // _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.position -= transform.forward * (_speed * Time.deltaTime);

        if (transform.position.z <= _newSpawnDist && _canSpawn)
        {
            ObjectSpawner.Instance.SpawnGround(transform.position);
            _canSpawn = false;
        }

        if (transform.position.z > _despawnDist) return;
        
        _canSpawn = true;
        gameObject.SetActive(false);
    }
}
