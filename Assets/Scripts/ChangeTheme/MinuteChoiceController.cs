using System;
using UnityEngine;

public class MinuteChoiceController : MonoBehaviour
{
    public static MinuteChoiceController Instance;
    
    [Header("Timing")]
    [SerializeField] private float intervalSeconds = 60f; // cada 1 minuto

    [Header("UI")]
    [SerializeField] private GameObject choicePanel;

    // Evento X: 0 = A, 1 = B
    public Action<WorldType> OnChoiceSelected;

    private float nextTriggerTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (choicePanel != null)
            choicePanel.SetActive(false);

        // Primer trigger: al minuto 1
        nextTriggerTime = intervalSeconds;
    }

    void Start()
    {
        if (GameTimeManager.Instance == null)
        {
            Debug.LogError("GameTimeManager no est� en la escena.");
            return;
        }

        GameTimeManager.Instance.OnTimeChanged += HandleTimeChanged;
    }

    void OnDisable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= HandleTimeChanged;
    }

    private void HandleTimeChanged(float elapsed)
    {
        // Cada vez que se cumple el pr�ximo minuto
        if (elapsed >= nextTriggerTime)
        {
            nextTriggerTime += intervalSeconds; // agenda el siguiente minuto
            PauseAndShow();
        }
    }

    private void PauseAndShow()
    {
        Time.timeScale = 0f;

        if (choicePanel != null)
            choicePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideAndResume()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // Bot�n opci�n A
    public void ChooseA()
    {
        HideAndResume();
        OnChoiceSelected?.Invoke(WorldType.Forest);
    }

    // Bot�n opci�n B
    public void ChooseB()
    {
        HideAndResume();
        OnChoiceSelected?.Invoke(WorldType.Fire);
    }
}
