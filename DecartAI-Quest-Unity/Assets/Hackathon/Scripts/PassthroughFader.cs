using UnityEngine;
using System.Collections;

public class PassthroughFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OVRPassthroughLayer passthroughLayer;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Minimum passthrough opacity when faded out.")]
    [Range(0f, 1f)]
    [SerializeField] private float minOpacity = 0.4f;

    [Tooltip("Maximum passthrough opacity when fully visible.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxOpacity = 1.0f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (!passthroughLayer)
            passthroughLayer = FindFirstObjectByType<OVRPassthroughLayer>();
    }

    /// <summary>Fades passthrough to the maximum opacity (fully visible).</summary>
    public void FadePassthroughIn()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(maxOpacity));
    }

    /// <summary>Fades passthrough to the minimum opacity (darker view).</summary>
    public void FadePassthroughOut()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(minOpacity));
    }

    private IEnumerator FadeRoutine(float target)
    {
        if (!passthroughLayer) yield break;

        float start = passthroughLayer.textureOpacity;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            passthroughLayer.textureOpacity = Mathf.Lerp(start, target, t);
            yield return null;
        }

        passthroughLayer.textureOpacity = target;
        fadeRoutine = null;
    }
}