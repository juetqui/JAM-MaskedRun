using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayerInputActions inputActions;
    public bool IsPaused { get; private set; }
    private static PauseManager _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        inputActions = new PlayerInputActions();
        if (pausePanel != null) pausePanel.SetActive(false);
        Resume();
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        if (inputActions == null) return;

        inputActions.Enable();
        inputActions.Player.Pause.performed += OnPause;
    }

    void OnDisable()
    {
        if (inputActions == null) return;

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

        Debug.Log($"[PauseManager] Pause() en escena: {SceneManager.GetActiveScene().name} | instanceID: {GetInstanceID()} | pausePanel: {(pausePanel ? pausePanel.name : "NULL")}");

        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
        else
            Debug.LogWarning("[PauseManager] pausePanel es NULL, por eso no se muestra el menú");

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

    public void GoToMainMenu()
    {
        Resume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Play()
    {
        Resume();
        SceneManager.LoadScene("julian");
    }
}