using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Transition SFX")]
    [SerializeField] private AudioClip transitionBell;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1.0f;

    Coroutine musicFadeRoutine;

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

        // Auto-wire si no se asignó
        if (!musicSource || !sfxSource)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                if (!musicSource) musicSource = sources[0];
                if (!sfxSource) sfxSource = sources[1];
            }
        }

        if (!musicSource || !sfxSource)
        {
            Debug.LogError("AudioManager: Need 2 AudioSources (music + sfx).");
            enabled = false;
            return;
        }

        // Config base
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Al iniciar en menú
        PlayMusic(menuMusic, fadeInSeconds: 0.5f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Regla simple por BuildIndex:
        // index 0 = menú → menuMusic
        // index >=1 = juego → gameMusic
        if (scene.buildIndex == 0)
        {
            PlayMusic(menuMusic, fadeInSeconds: 0.5f);
        }
        else
        {
            PlayMusic(gameMusic, fadeInSeconds: 0.8f);
        }
    }

    public void PlayTransitionBell()
    {
        if (transitionBell)
            sfxSource.PlayOneShot(transitionBell, sfxVolume);
    }

    public void PlayMusic(AudioClip clip, float fadeInSeconds = 0f)
    {
        if (!clip) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);

        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.Play();

        if (fadeInSeconds <= 0f)
        {
            musicSource.volume = musicVolume;
        }
        else
        {
            musicFadeRoutine = StartCoroutine(FadeMusicTo(musicVolume, fadeInSeconds));
        }
    }

    public void FadeOutMusic(float fadeOutSeconds)
    {
        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(FadeMusicTo(0f, fadeOutSeconds, stopAfter: true));
    }

    private IEnumerator FadeMusicTo(float target, float seconds, bool stopAfter = false)
    {
        float start = musicSource.volume;
        if (seconds <= 0f)
        {
            musicSource.volume = target;
            if (stopAfter && Mathf.Approximately(target, 0f)) musicSource.Stop();
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / seconds);
            musicSource.volume = Mathf.Lerp(start, target, a);
            yield return null;
        }

        musicSource.volume = target;
        if (stopAfter && Mathf.Approximately(target, 0f)) musicSource.Stop();
    }
}
