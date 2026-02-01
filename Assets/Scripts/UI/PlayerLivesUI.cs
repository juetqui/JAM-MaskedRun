using UnityEngine;
using UnityEngine.UI;

public class PlayerLivesUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image[] hearts; // tamaño 3

    void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged -= UpdateUI;
    }

    private void UpdateUI(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            // Activo si esa vida “existe”
            hearts[i].enabled = i < current;
        }
    }
}
