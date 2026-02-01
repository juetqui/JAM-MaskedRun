using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.6f;

    [Header("Fade")]
    [SerializeField] private float fadeInSeconds = 0.8f;
    [SerializeField] private float fadeOutSeconds = 0.4f;

    Coroutine fadeRoutine;

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
        if (!musicSource) musicSource = GetComponent<AudioSource>();
        if (!musicSource)
        {
            Debug.LogError("MusicManager: Missing AudioSource. Add one to the same GameObject.");
            enabled = false;
            return;
        }

        // Asegura settings 2D + loop
        musicSource.spatialBlend = 0f;
        musicSource.loop = true;

        // Arranque con fade-in si está marcado PlayOnAwake
        if (musicSource.playOnAwake && musicSource.clip != null)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            FadeTo(defaultVolume, fadeInSeconds);
        }
        else
        {
            musicSource.volume = defaultVolume;
        }
    }

    /// <summary>
    /// Reproduce un clip (si es distinto) y opcionalmente hace fade-in.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        if (!clip)
        {
            Debug.LogWarning("MusicManager: PlayMusic called with null clip.");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.volume = fade ? 0f : defaultVolume;
        musicSource.Play();

        if (fade)
            FadeTo(defaultVolume, fadeInSeconds);
    }

    public void SetVolume(float v)
    {
        defaultVolume = Mathf.Clamp01(v);
        musicSource.volume = defaultVolume;
    }

    public void StopMusic(bool fade = true)
    {
        if (!musicSource.isPlaying) return;

        if (fade)
            FadeTo(0f, fadeOutSeconds, stopAfter: true);
        else
            musicSource.Stop();
    }

    private void FadeTo(float target, float seconds, bool stopAfter = false)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(target, seconds, stopAfter));
    }

    private IEnumerator FadeRoutine(float target, float seconds, bool stopAfter)
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
            t += Time.unscaledDeltaTime; // no depende del timescale
            float a = Mathf.Clamp01(t / seconds);
            musicSource.volume = Mathf.Lerp(start, target, a);
            yield return null;
        }

        musicSource.volume = target;
        if (stopAfter && Mathf.Approximately(target, 0f)) musicSource.Stop();
    }
}
