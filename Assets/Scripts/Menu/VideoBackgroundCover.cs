using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
public class VideoBackgroundCover : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RectTransform targetRect; // normalmente el mismo rect del RawImage

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if (!targetRect) targetRect = (RectTransform)transform;

        if (!videoPlayer)
            videoPlayer = FindAnyObjectByType<VideoPlayer>();

        if (videoPlayer)
            videoPlayer.prepareCompleted += OnPrepared;
    }

    private void Start()
    {
        if (videoPlayer)
        {
            // Asegura que se prepare antes de calcular ratio
            if (!videoPlayer.isPrepared) videoPlayer.Prepare();
            else FitCover();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer)
            videoPlayer.prepareCompleted -= OnPrepared;
    }

    private void OnPrepared(VideoPlayer vp)
    {
        FitCover();
        vp.Play();
    }

    private void FitCover()
    {
        // Medidas del video
        float videoW = videoPlayer.texture != null ? videoPlayer.texture.width : (float)videoPlayer.width;
        float videoH = videoPlayer.texture != null ? videoPlayer.texture.height : (float)videoPlayer.height;

        if (videoW <= 0 || videoH <= 0) return;

        float videoAspect = videoW / videoH;

        // Medidas del rect UI
        float rectW = targetRect.rect.width;
        float rectH = targetRect.rect.height;
        if (rectW <= 0 || rectH <= 0) return;

        float rectAspect = rectW / rectH;

        // Cover: expandir el lado que falte
        // Ajustamos UVRect para recortar centrado
        Rect uv = new Rect(0, 0, 1, 1);

        if (rectAspect > videoAspect)
        {
            // pantalla más ancha -> recortar arriba/abajo
            float scale = videoAspect / rectAspect;
            uv.height = scale;
            uv.y = (1f - scale) * 0.5f;
        }
        else
        {
            // pantalla más alta -> recortar laterales
            float scale = rectAspect / videoAspect;
            uv.width = scale;
            uv.x = (1f - scale) * 0.5f;
        }

        rawImage.uvRect = uv;
    }
}
