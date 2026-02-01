using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDamageOnHit : MonoBehaviour
{
    private PlayerHealth health;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    void OnTriggerEnter(Collider other)
    {
        health.TakeHit(1);
    }
}
