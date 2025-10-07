using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
public class PassthroughFader : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the OVR Passthrough Layer to control.")]
    [SerializeField] private OVRPassthroughLayer passthroughLayer;

    [Header("Fade Settings")]
    [Tooltip("Duration of the fade animation (seconds).")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Minimum opacity when fully faded out (0 = black).")]
    [Range(0f, 1f)]
    [SerializeField] private float minOpacity = 0.15f;   // lowered from 0.4 → 0.15 for deeper fade

    [Tooltip("Maximum opacity when fully visible (1 = normal passthrough).")]
    [Range(0f, 1f)]
    [SerializeField] private float maxOpacity = 1.0f;

    private Tween fadeTween;

    private void Awake()
    {
        // Auto-assign passthrough if not set
        if (!passthroughLayer)
            passthroughLayer = FindFirstObjectByType<OVRPassthroughLayer>();
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
    }

    /// <summary>Fades the passthrough to full visibility (max opacity).</summary>
    public void FadePassthroughIn()
    {
        StartFade(maxOpacity);
    }

    /// <summary>Fades the passthrough to darker view (min opacity).</summary>
    public void FadePassthroughOut()
    {
        StartFade(minOpacity);
    }

    /// <summary>Starts a DOTween fade toward a target opacity.</summary>
    private void StartFade(float targetOpacity)
    {
        if (!passthroughLayer)
        {
            Debug.LogWarning("[PassthroughFaderDOTween] No OVRPassthroughLayer assigned.");
            return;
        }

        fadeTween?.Kill(); // Stop any existing tween

        fadeTween = DOTween.To(
            () => passthroughLayer.textureOpacity,
            x => passthroughLayer.textureOpacity = x,
            targetOpacity,
            fadeDuration
        ).SetEase(Ease.InOutSine);
    }
}
