using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private float fadeOutSeconds = 0.6f;
    [SerializeField] private float waitAfterBellSeconds = 1.2f; // tiempo para que suene la campana

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
        // 1) Fade out visual
        if (sceneFader != null)
            yield return sceneFader.Fade(0f, 1f, fadeOutSeconds);

        // 2) Música del menú a 0 (opcional) + campana
        if (AudioManager.Instance != null)
        {
            if (fadeOutMenuMusic)
                AudioManager.Instance.FadeOutMusic(musicFadeOutSeconds);

            AudioManager.Instance.PlayTransitionBell();
        }

        // 3) Espera mientras suena la campana
        if (waitAfterBellSeconds > 0f)
            yield return new WaitForSecondsRealtime(waitAfterBellSeconds);

        // 4) Carga por Build Index (siguiente escena)
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
        // La música de la nueva escena arranca desde AudioManager.OnSceneLoaded
    }
}
