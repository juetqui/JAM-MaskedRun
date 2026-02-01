using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Selectable))]
public class UIButtonFX : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Target")]
    [SerializeField] private RectTransform target; // normalmente el mismo botón
    [SerializeField] private bool affectChildrenToo = false; // si querés escalar todo el subtree (normalmente true no hace falta)

    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressedScale = 0.98f;
    [SerializeField] private float scaleDuration = 0.10f;
    [SerializeField] private AnimationCurve scaleCurve = null;

    [Header("Optional Color (Graphic)")]
    [SerializeField] private Graphic graphic; // Image/TMP/etc. (opcional)
    [SerializeField] private bool animateColor = false;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private Color pressedColor = Color.white;
    [SerializeField] private float colorDuration = 0.08f;

    [Header("SFX (Anti-spam)")]
    [SerializeField] private AudioSource sfxSource;     // recomendado: un AudioSource 2D (puede ser el del AudioManager)
    [SerializeField] private AudioClip hoverSfx;
    [SerializeField] private AudioClip clickSfx;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1.0f;

    [Tooltip("Mínimo tiempo entre SFX del mismo tipo. Evita 'rayado'.")]
    [SerializeField] private float hoverCooldown = 0.08f;
    [SerializeField] private float clickCooldown = 0.05f;

    [Tooltip("Si está activado, no vuelve a reproducir el mismo clip si ya está sonando (útil si usás sfxSource.clip + Play). " +
             "Con PlayOneShot no es estrictamente necesario, pero ayuda en setups raros.")]
    [SerializeField] private bool preventIfSourceBusy = false;

    private Vector3 baseScale;
    private Coroutine scaleRoutine;
    private Coroutine colorRoutine;

    private bool isHovered;
    private bool isPressed;
    private bool isSelected;

    private float lastHoverSfxTime = -999f;
    private float lastClickSfxTime = -999f;

    private Selectable selectable;

    private void Reset()
    {
        target = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();

        // Curva default agradable si no hay
        if (scaleCurve == null || scaleCurve.length == 0)
            scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        if (!target) target = GetComponent<RectTransform>();

        baseScale = target.localScale;

        // Defaults de colores si se habilita animateColor
        if (!graphic) graphic = GetComponent<Graphic>();
        if (graphic)
        {
            normalColor = graphic.color;
            if (hoverColor == Color.white) hoverColor = normalColor;
            if (pressedColor == Color.white) pressedColor = normalColor;
        }

        if (scaleCurve == null || scaleCurve.length == 0)
            scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    private void OnEnable()
    {
        // Estado visual consistente al habilitar
        ApplyVisualState(immediate: true);
    }

    // -------------------------
    // Pointer / Selection Events
    // -------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        isHovered = true;
        ApplyVisualState(immediate: false);
        TryPlayHoverSfx();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false; // por seguridad
        ApplyVisualState(immediate: false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        isPressed = true;
        ApplyVisualState(immediate: false);
        TryPlayClickSfx(); // opcional: click al down
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        ApplyVisualState(immediate: false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Para navegación con teclado/gamepad
        if (!IsInteractable()) return;

        isSelected = true;
        ApplyVisualState(immediate: false);
        // Opcional: sonido al seleccionar
        TryPlayHoverSfx();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        ApplyVisualState(immediate: false);
    }

    // -------------------------
    // Visual State
    // -------------------------

    private void ApplyVisualState(bool immediate)
    {
        Vector3 targetScale = GetDesiredScale();
        Color targetColor = GetDesiredColor();

        if (immediate)
        {
            target.localScale = targetScale;
            if (animateColor && graphic) graphic.color = targetColor;
            return;
        }

        StartScaleTween(targetScale);
        if (animateColor && graphic) StartColorTween(targetColor);
    }

    private Vector3 GetDesiredScale()
    {
        // Prioridad: pressed > hover/selected > normal
        if (isPressed)
            return baseScale * pressedScale;

        if (isHovered || isSelected)
            return baseScale * hoverScale;

        return baseScale;
    }

    private Color GetDesiredColor()
    {
        if (!graphic) return Color.white;

        if (isPressed) return pressedColor;
        if (isHovered || isSelected) return hoverColor;
        return normalColor;
    }

    private void StartScaleTween(Vector3 to)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTween(target.localScale, to, scaleDuration));

        // Si querés escalar “todo”, en UI normalmente basta con target (padre).
        // affectChildrenToo no hace falta porque escalar el RectTransform escala a sus hijos visualmente.
    }

    private IEnumerator ScaleTween(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            target.localScale = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            float eased = scaleCurve != null ? scaleCurve.Evaluate(a) : a;
            target.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        target.localScale = to;
    }

    private void StartColorTween(Color to)
    {
        if (colorRoutine != null) StopCoroutine(colorRoutine);
        colorRoutine = StartCoroutine(ColorTween(graphic.color, to, colorDuration));
    }

    private IEnumerator ColorTween(Color from, Color to, float duration)
    {
        if (duration <= 0f)
        {
            graphic.color = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            graphic.color = Color.Lerp(from, to, a);
            yield return null;
        }

        graphic.color = to;
    }

    // -------------------------
    // SFX (Anti-spam)
    // -------------------------

    private void TryPlayHoverSfx()
    {
        if (!hoverSfx || !sfxSource) return;
        if (!IsInteractable()) return;

        float now = Time.unscaledTime;
        if (now - lastHoverSfxTime < hoverCooldown) return;
        if (preventIfSourceBusy && sfxSource.isPlaying) return;

        sfxSource.PlayOneShot(hoverSfx, sfxVolume);
        lastHoverSfxTime = now;
    }

    private void TryPlayClickSfx()
    {
        if (!clickSfx || !sfxSource) return;
        if (!IsInteractable()) return;

        float now = Time.unscaledTime;
        if (now - lastClickSfxTime < clickCooldown) return;
        if (preventIfSourceBusy && sfxSource.isPlaying) return;

        sfxSource.PlayOneShot(clickSfx, sfxVolume);
        lastClickSfxTime = now;
    }

    private bool IsInteractable()
    {
        // Respeta Button/Selectable interactable y CanvasGroup blocks
        if (selectable && !selectable.IsInteractable()) return false;

        // Si hay CanvasGroup arriba que bloquea raycasts, no debería llegar el evento igual,
        // pero esto suma una capa de seguridad.
        return true;
    }
}
