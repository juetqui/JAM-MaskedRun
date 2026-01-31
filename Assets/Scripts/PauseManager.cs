using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayerInputActions inputActions;
    public bool IsPaused { get; private set; }

    void Awake()
    {
        inputActions = new PlayerInputActions();
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Resume(); // asegura estado correcto
    }
    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Pause.performed += OnPause;
    }

    void OnDisable()
    {
        inputActions.Player.Pause.performed -= OnPause;
        inputActions.Disable();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Si tu juego no usa mouse-look, podés dejarlo unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Play()
    {
        SceneManager.LoadScene("julian");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    //void OnDestroy()
    //{
    //    Time.timeScale = 1f;
    //}
}
