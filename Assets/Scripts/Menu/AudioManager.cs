using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source (2D)")]
    [SerializeField] private AudioSource musicSource; // loop

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.65f;

    [Header("Fade Defaults")]
    [SerializeField] private float defaultMusicFadeIn = 0.8f;
    [SerializeField] private float defaultMusicFadeOut = 0.4f;

    private Coroutine musicFadeRoutine;

    // -------------------------
    // Lifecycle
    // -------------------------

    private void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AutoWireSourceIfNeeded();
        ConfigureSource();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Arranque según escena actual (por si no es index 0)
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // -------------------------
    // Setup Helpers
    // -------------------------

    private void AutoWireSourceIfNeeded()
    {
        if (musicSource != null) return;

        // Intenta tomar el primer AudioSource del mismo GO.
        musicSource = GetComponent<AudioSource>();
    }

    private void ConfigureSource()
    {
        if (!musicSource)
        {
            Debug.LogError("AudioManager: Necesitas asignar un AudioSource a musicSource.");
            enabled = false;
            return;
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f; // 2D
        musicSource.volume = musicVolume;
    }

    // -------------------------
    // Scene Hook
    // -------------------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Regla:
        // - buildIndex 0 => menú
        // - buildIndex >= 1 => juego
        if (scene.buildIndex == 0)
        {
            if (menuMusic != null)
                PlayMusic(menuMusic, defaultMusicFadeIn);
        }
        else
        {
            if (gameMusic != null)
                PlayMusic(gameMusic, defaultMusicFadeIn);
        }
    }

    // -------------------------
    // Public API - Music
    // -------------------------

    /// <summary>
    /// Reproduce música (si es distinta a la actual). Aplica fade-in opcional.
    /// </summary>
    public void PlayMusic(AudioClip clip, float fadeInSeconds = 0f)
    {
        if (!clip || !musicSource) return;

        // Si ya está sonando este clip, no hagas nada.
        if (musicSource.isPlaying && musicSource.clip == clip)
            return;

        // Cambia clip
        musicSource.clip = clip;

        // Arranca desde 0 para un fade limpio
        if (fadeInSeconds > 0f)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            FadeMusicTo(musicVolume, fadeInSeconds, stopAfter: false);
        }
        else
        {
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Fade out de la música actual y la detiene al final.
    /// </summary>
    public void FadeOutMusic(float fadeOutSeconds = -1f)
    {
        if (!musicSource || !musicSource.isPlaying) return;

        if (fadeOutSeconds < 0f) fadeOutSeconds = defaultMusicFadeOut;
        FadeMusicTo(0f, fadeOutSeconds, stopAfter: true);
    }

    /// <summary>
    /// Ajusta volumen objetivo de música (se aplica instantáneo al source).
    /// </summary>
    public void SetMusicVolume(float volume01)
    {
        musicVolume = Mathf.Clamp01(volume01);
        if (musicSource) musicSource.volume = musicVolume;
    }

    // -------------------------
    // Internals - Fades
    // -------------------------

    private void FadeMusicTo(float targetVolume, float seconds, bool stopAfter)
    {
        if (!musicSource) return;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeMusicRoutine(targetVolume, seconds, stopAfter));
    }

    private IEnumerator FadeMusicRoutine(float targetVolume, float seconds, bool stopAfter)
    {
        float startVolume = musicSource.volume;

        if (seconds <= 0f)
        {
            musicSource.volume = targetVolume;
            if (stopAfter && Mathf.Approximately(targetVolume, 0f))
                musicSource.Stop();
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / seconds);
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, a);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (stopAfter && Mathf.Approximately(targetVolume, 0f))
            musicSource.Stop();
    }
}
