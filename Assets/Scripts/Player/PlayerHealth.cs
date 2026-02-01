using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invulnerabilitySeconds = 0.8f;

    [Header("Scenes")]
    [SerializeField] private string defeatSceneName = "Defeat";

    public int Lives { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<int, int> OnLivesChanged; // (current, max)
    public event Action OnDied;

    private float lastHitTime = -999f;

    void Awake()
    {
        Lives = maxLives;
        OnLivesChanged?.Invoke(Lives, maxLives);
    }

    public void TakeHit(int damage = 1)
    {
        if (IsDead) return;
        if (damage <= 0) return;

        // I-frames para no perder 3 vidas en un solo choque
        if (Time.time - lastHitTime < invulnerabilitySeconds) return;
        lastHitTime = Time.time;

        Lives = Mathf.Max(0, Lives - damage);
        OnLivesChanged?.Invoke(Lives, maxLives);

        Debug.Log(Lives);

        if (Lives <= 0)
            Die();
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDied?.Invoke();

        // Por seguridad: asegurar que no quede pausado
        Time.timeScale = 1f;

        SceneManager.LoadScene(defeatSceneName);
    }

    public void ResetLives()
    {
        IsDead = false;
        Lives = maxLives;
        lastHitTime = -999f;
        OnLivesChanged?.Invoke(Lives, maxLives);
    }

    void OnTriggerEnter(Collider other)
    {
        TakeHit(1);
    }
}
