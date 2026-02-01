using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    void OnEnable()
    {
        ScoreManager.Instance.OnScoreChanged += UpdateUI;
        UpdateUI(ScoreManager.Instance.Score);
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateUI;
    }

    void UpdateUI(int score)
    {
        scoreText.text = ("Puntaje: " + score.ToString());
    }
}
