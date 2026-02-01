using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private float fadeOutSeconds = 0.6f;
    [SerializeField] private float waitSeconds = 0.6f;

    [Header("Music")]
    [SerializeField] private bool fadeOutMenuMusic = true;
    [SerializeField] private float musicFadeOutSeconds = 0.4f;

    private bool isLoading;

    private void Awake()
    {
        if (!sceneFader)
            sceneFader = FindAnyObjectByType<SceneFader>();
    }

    public void Run()
    {
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        // 1) Fade a negro
        if (sceneFader != null)
            yield return sceneFader.Fade(0f, 1f, fadeOutSeconds);

        // 2) Fade out de la música del menú (opcional)
        if (fadeOutMenuMusic && AudioManager.Instance != null)
            AudioManager.Instance.FadeOutMusic(musicFadeOutSeconds);

        // 3) Espera (si querés un pequeño "beat" antes de cargar)
        if (waitSeconds > 0f)
            yield return new WaitForSecondsRealtime(waitSeconds);

        // 4) Cargar siguiente escena por Build Index
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        int total = SceneManager.sceneCountInBuildSettings;
        if (next >= total)
        {
            Debug.LogError($"No hay escena siguiente en Build Settings. Current={current}, Total={total}");
            isLoading = false;
            yield break;
        }

        SceneManager.LoadScene(next);
        // La música de la nueva escena arranca sola en AudioManager.OnSceneLoaded()
    }
}
