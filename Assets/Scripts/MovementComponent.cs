using UnityEngine;
using ObjectPool;

// [RequireComponent(typeof(Rigidbody))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _objectDist = -60f;
    [SerializeField] private float _despawnDist = -110f;
    
    // private Rigidbody _rb;
    private const string GroundTag = "Ground";
    private bool _canSpawnGround = true;
    
    void Start()
    {
        // _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.position -= transform.forward * (_speed * Time.deltaTime);

        if (transform.position.z <= _objectDist && CompareTag(GroundTag) && _canSpawnGround)
        {
            ObjectSpawner.Instance.SpawnGround();
            _canSpawnGround = false;
        }

        if (transform.position.z <= _despawnDist)
        {
            _canSpawnGround = true;
            gameObject.SetActive(false);
        }
    }
}
