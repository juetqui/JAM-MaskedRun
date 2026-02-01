using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseRoot; // el objeto PauseMenu (root)

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("Behavior")]
    [Tooltip("Si true, usa Time.timeScale = 0 para pausar gameplay.")]
    [SerializeField] private bool useTimeScalePause = true;

    [Tooltip("Si true, muestra el cursor al pausar y lo oculta al reanudar.")]
    [SerializeField] private bool manageCursor = true;

    [Header("Scene Flow (Build Index)")]
    [Tooltip("A qué build index volver para el menú principal. Por defecto 0.")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (!pauseRoot) pauseRoot = gameObject;

        // Asegura estado inicial
        SetPaused(false, instant: true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void Pause()
    {
        SetPaused(true);
    }

    private void SetPaused(bool paused, bool instant = false)
    {
        IsPaused = paused;

        if (pauseRoot)
            pauseRoot.SetActive(paused);

        if (useTimeScalePause)
            Time.timeScale = paused ? 0f : 1f;

        if (manageCursor)
        {
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Si querés: pausar audio global (opcional)
        // AudioListener.pause = paused;
    }

    // -------------------------
    // Button Actions
    // -------------------------

    public void RestartLevel()
    {
        // Reanuda antes de recargar para evitar que la escena quede con timescale 0.
        if (useTimeScalePause) Time.timeScale = 1f;

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void GoToMainMenu()
    {
        if (useTimeScalePause) Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    public void QuitGame()
    {
        if (useTimeScalePause) Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
