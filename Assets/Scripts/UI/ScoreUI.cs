using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    void Start()
    {
        // Esperamos a que el manager exista
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateUI;
            UpdateUI(ScoreManager.Instance.Score);
        }
        else
        {
            Debug.LogError("GameTimeManager no está en la escena");
        }
    }

    void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateUI;
    }

    void UpdateUI(int score)
    {
        scoreText.text = ("Puntaje: " + score.ToString());
    }
}
