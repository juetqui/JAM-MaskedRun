using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] private float pointsPerSecond = 10f;
    [SerializeField] private float multiplier = 1f;

    public int Score { get; private set; }
    public bool IsRunning { get; private set; } = true;

    public event Action<int> OnScoreChanged;

    private float accumulator;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!IsRunning) return;

        accumulator += pointsPerSecond * multiplier * Time.deltaTime;

        // Convertimos a enteros sin perder decimales acumulados
        int add = Mathf.FloorToInt(accumulator);
        if (add > 0)
        {
            accumulator -= add;
            AddScore(add);
        }
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        accumulator = 0f;
        OnScoreChanged?.Invoke(Score);
    }

    public void SetRunning(bool running) => IsRunning = running;

    public void SetMultiplier(float value) => multiplier = Mathf.Max(0f, value);
}
