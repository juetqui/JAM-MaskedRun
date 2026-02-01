using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources (2D)")]
    [SerializeField] private AudioSource musicSource; // loop
    [SerializeField] private AudioSource sfxSource;   // one-shots

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Transition SFX")]
    [SerializeField] private AudioClip transitionBell;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1.0f;

    [Header("Fade Defaults")]
    [SerializeField] private float defaultMusicFadeIn = 0.8f;
    [SerializeField] private float defaultMusicFadeOut = 0.4f;

    [Header("SFX Anti-Spam")]
    [Tooltip("Evita que el mismo SFX se dispare demasiadas veces por segundo.")]
    [SerializeField] private float sfxCooldown = 0.06f;

    private Coroutine musicFadeRoutine;
    private float lastSfxTime = -999f;

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

        AutoWireSourcesIfNeeded();
        ConfigureSources();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Evita leaks de eventos si el objeto se destruye por alguna razón
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

    private void AutoWireSourcesIfNeeded()
    {
        // Si no asignaste en Inspector, intenta tomar 2 AudioSources del mismo GO.
        if (musicSource != null && sfxSource != null)
            return;

        var sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            if (!musicSource) musicSource = sources[0];
            if (!sfxSource) sfxSource = sources[1];
        }
    }

    private void ConfigureSources()
    {
        if (!musicSource || !sfxSource)
        {
            Debug.LogError("AudioManager: Necesitas 2 AudioSources asignados (musicSource y sfxSource).");
            enabled = false;
            return;
        }

        // Music source
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f; // 2D
        musicSource.volume = musicVolume;

        // SFX source
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D
        sfxSource.volume = sfxVolume;
    }

    // -------------------------
    // Scene Hook
    // -------------------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Regla actual:
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

    /// <summary>
    /// Ajusta volumen de SFX (para PlayOneShot).
    /// </summary>
    public void SetSfxVolume(float volume01)
    {
        sfxVolume = Mathf.Clamp01(volume01);
        if (sfxSource) sfxSource.volume = sfxVolume;
    }

    // -------------------------
    // Public API - SFX
    // -------------------------

    /// <summary>
    /// Reproduce la campana de transición (one-shot). Tiene cooldown anti-spam.
    /// </summary>
    public void PlayTransitionBell()
    {
        PlaySfx(transitionBell);
    }

    /// <summary>
    /// Reproduce cualquier SFX (one-shot). Tiene cooldown anti-spam.
    /// </summary>
    public void PlaySfx(AudioClip clip)
    {
        if (!clip || !sfxSource) return;

        float now = Time.unscaledTime;
        if (now - lastSfxTime < sfxCooldown) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
        lastSfxTime = now;
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
