using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    public InputsPlayer inputActions;


    [Header("Pause")]
    [SerializeField] private bool useTimeScalePause = true;
    [SerializeField] private bool manageCursor = true;

    [Header("Audio")]
    [Tooltip("Si está activo, pausa TODO el audio del juego.")]
    [SerializeField] private bool pauseAllAudio = true;

    [Tooltip("Si está activo, además pausa/reanuda el AudioSource de música del AudioManager (si existe).")]
    [SerializeField] private bool alsoPauseAudioManagerMusic = false;

    [Header("Fade")]
    [SerializeField] private float fadeInSeconds = 0.12f;
    [SerializeField] private float fadeOutSeconds = 0.10f;

    [Header("Scene Flow (Build Index)")]
    [SerializeField] private int mainMenuBuildIndex = 0;

    public bool IsPaused { get; private set; }

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            Debug.LogError("PauseMenuController: falta CanvasGroup en PauseMenu.");
            enabled = false;
            return;
        }

        // Estado inicial
        ApplyCanvasState(alpha: 0f, interactable: false, blocksRaycasts: false);
        IsPaused = false;

        // Cursor inicial (opcional)
        if (manageCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Start()
    {
        inputActions = PlayerLaneMovement.Instance.inputActions;
    }

    private bool subscribed;
    private Coroutine bindRoutine;

    void OnEnable()
    {
        bindRoutine = StartCoroutine(BindInputsWhenReady());
    }

    void OnDisable()
    {
        if (bindRoutine != null) StopCoroutine(bindRoutine);
        UnbindInputs();
    }

    private IEnumerator BindInputsWhenReady()
    {
        // Esperar hasta que el Player exista y tenga inputActions
        while (PlayerLaneMovement.Instance == null || PlayerLaneMovement.Instance.inputActions == null)
            yield return null;

        inputActions = PlayerLaneMovement.Instance.inputActions;

        // Importante: NO llames Enable/Disable acá si el Player ya lo hace.
        inputActions.Player.Pause.performed += OnPause;
        subscribed = true;
    }

    private void UnbindInputs()
    {
        if (!subscribed) return;
        if (inputActions != null)
            inputActions.Player.Pause.performed -= OnPause;

        subscribed = false;
    }

    public void OnPause(InputAction.CallbackContext context) => SetPaused(!IsPaused);
    public void Resume() => SetPaused(false);
    public void Pause() => SetPaused(true);

    private void SetPaused(bool paused)
    {
        IsPaused = paused;

        // Gameplay pause
        if (useTimeScalePause)
            Time.timeScale = paused ? 0f : 1f;

        // Cursor
        if (manageCursor)
        {
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Audio global
        if (pauseAllAudio)
            AudioListener.pause = paused;

        // AudioManager music (opcional)
        if (alsoPauseAudioManagerMusic && AudioManager.Instance != null)
        {
            // Si tu AudioManager expone el source, mejor llamarlo por método.
            // Como no lo expone, podés agregar un método PauseMusic/ResumeMusic en AudioManager.
            // Acá hacemos el approach seguro: buscar AudioSource y pausar.
            var src = AudioManager.Instance.GetComponentInChildren<AudioSource>();
            if (src)
            {
                if (paused) src.Pause();
                else src.UnPause();
            }
        }

        // UI fade (unscaled)
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(paused ? FadeIn() : FadeOut());
    }

    private IEnumerator FadeIn()
    {
        ApplyCanvasState(alpha: canvasGroup.alpha, interactable: true, blocksRaycasts: true);
        yield return FadeTo(1f, fadeInSeconds);
        ApplyCanvasState(alpha: 1f, interactable: true, blocksRaycasts: true);
    }

    private IEnumerator FadeOut()
    {
        ApplyCanvasState(alpha: canvasGroup.alpha, interactable: false, blocksRaycasts: true);
        yield return FadeTo(0f, fadeOutSeconds);
        ApplyCanvasState(alpha: 0f, interactable: false, blocksRaycasts: false);
    }

    private IEnumerator FadeTo(float target, float seconds)
    {
        float start = canvasGroup.alpha;

        if (seconds <= 0f)
        {
            canvasGroup.alpha = target;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / seconds);
            canvasGroup.alpha = Mathf.Lerp(start, target, a);
            yield return null;
        }

        canvasGroup.alpha = target;
    }

    private void ApplyCanvasState(float alpha, bool interactable, bool blocksRaycasts)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = blocksRaycasts;
    }

    // -------------------------
    // Buttons
    // -------------------------

    public void RestartLevel()
    {
        HardUnpauseBeforeSceneLoad();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void GoToMainMenu()
    {
        HardUnpauseBeforeSceneLoad();
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    public void QuitGame()
    {
        HardUnpauseBeforeSceneLoad();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HardUnpauseBeforeSceneLoad()
    {
        IsPaused = false;

        if (useTimeScalePause) 
        { 
            Time.timeScale = 1f; 
        }
        if (pauseAllAudio) AudioListener.pause = false;

        if (manageCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        ApplyCanvasState(alpha: 0f, interactable: false, blocksRaycasts: false);
    }
}
