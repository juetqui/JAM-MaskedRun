using UnityEngine;
using TMPro;

public class GameTimeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    void Start()
    {
        // Esperamos a que el manager exista
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTimeChanged += UpdateTime;
            UpdateTime(GameTimeManager.Instance.ElapsedTime);
        }
        else
        {
            Debug.LogError("GameTimeManager no está en la escena");
        }
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= UpdateTime;
    }

    void UpdateTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
