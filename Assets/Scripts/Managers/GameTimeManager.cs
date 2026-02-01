using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    public float ElapsedTime { get; private set; }

    public event Action<float> OnTimeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        //  Solo avanza si no está pausado
        if (Time.timeScale == 0f)
            return;

        ElapsedTime += Time.deltaTime;

        // Avisar a la UI
        OnTimeChanged?.Invoke(ElapsedTime);
    }

    public void ResetTime()
    {
        ElapsedTime = 0f;
        OnTimeChanged?.Invoke(ElapsedTime);
    }
}
