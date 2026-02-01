using System.Collections;
using UnityEngine;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            Debug.LogError("SceneFader: Missing CanvasGroup.");
            enabled = false;
        }
    }

    public IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        canvasGroup.alpha = from;

        // Bloquea input durante la transición
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, a);
            yield return null;
        }

        canvasGroup.alpha = to;

        // Si quedó transparente, liberá input
        bool transparent = Mathf.Approximately(to, 0f);
        canvasGroup.blocksRaycasts = !transparent;
        canvasGroup.interactable = false;
    }
}
