using UnityEngine;
using ObjectPool;

// [RequireComponent(typeof(Rigidbody))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField] private PoolType _type;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _despawnDist = -110f;

    private void Update()
    {
        transform.position -= transform.forward * (_speed * Time.deltaTime);

        if (transform.position.z <= _despawnDist)
            gameObject.SetActive(false);
    }
}
