using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source (2D)")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.65f;

    [Header("Fade Defaults")]
    [SerializeField] private float defaultMusicFadeIn = 0.8f;
    [SerializeField] private float defaultMusicFadeOut = 0.4f;

    private Coroutine musicFadeRoutine;

    private void Awake()
    {
        var root = transform.root.gameObject;

        // Si ya existe un AudioManager, destruir el root nuevo completo
        if (Instance != null && Instance != this)
        {
            Destroy(root);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(root);

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
        // Arranque según escena actual
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void AutoWireSourceIfNeeded()
    {
        if (musicSource != null) return;
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

    public void PlayMusic(AudioClip clip, float fadeInSeconds = 0f)
    {
        if (!clip || !musicSource) return;

        if (musicSource.isPlaying && musicSource.clip == clip)
            return;

        musicSource.clip = clip;

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

    public void FadeOutMusic(float fadeOutSeconds = -1f)
    {
        if (!musicSource || !musicSource.isPlaying) return;

        if (fadeOutSeconds < 0f) fadeOutSeconds = defaultMusicFadeOut;
        FadeMusicTo(0f, fadeOutSeconds, stopAfter: true);
    }

    public void SetMusicVolume(float volume01)
    {
        musicVolume = Mathf.Clamp01(volume01);
        if (musicSource) musicSource.volume = musicVolume;
    }

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
