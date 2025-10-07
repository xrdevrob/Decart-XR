using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using DG.Tweening;
using Tween = DG.Tweening.Tween; // for smooth rotation and scaling

/// <summary>
/// Rotates and scales the assigned video UI panel so it always faces
/// the user's camera and appears constant in their view.
/// </summary>
public class PanelBillboardController : MonoBehaviour
{
    [Header("UI Target")]
    [Tooltip("The specific UI GameObject (e.g. Video UI Canvas) to rotate/scale.")]
    [SerializeField] private GameObject videoUI;

    [Tooltip("Optional CanvasGroup for fade control (e.g., show/hide UI).")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Scaling Settings")]
    [Tooltip("Base scale factor — tweak until panel appears desired size in frustum.")]
    [SerializeField] private float scaleFactor = 1.4f;

    [Tooltip("Minimum allowed scale multiplier.")]
    [SerializeField] private float minScale = 0.4f;

    [Tooltip("Maximum allowed scale multiplier.")]
    [SerializeField] private float maxScale = 5f;

    [Tooltip("Smooth scaling duration when resizing after drag.")]
    [SerializeField] private float scaleDuration = 0.3f;

    [Header("Rotation Settings")]
    [Tooltip("If true, panel only rotates around Y axis (keeps upright).")]
    [SerializeField] private bool yAxisOnly;

    [Tooltip("If true, flips rotation direction (for back-facing canvases).")]
    [SerializeField] private bool invertFacing = true;

    [Tooltip("How fast the rotation interpolates toward the target direction.")]
    [SerializeField] private float rotationSmoothTime = 0.15f;

    [Header("Fade Settings")]
    [Tooltip("Fade duration for showing/hiding controls.")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Camera _targetCamera;
    private bool _isDragging;
    private Tween _fadeTween;
    private Tween _scaleTween;

    private void Start()
    {
        _targetCamera = Camera.main;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (!_targetCamera || !videoUI)
            return;

        if (!_isDragging)
            return; // do nothing when not dragging

        // --- SMOOTH ROTATION (active while dragging) ---
        Vector3 directionToCamera = invertFacing
            ? videoUI.transform.position - _targetCamera.transform.position
            : _targetCamera.transform.position - videoUI.transform.position;

        if (yAxisOnly)
            directionToCamera.y = 0f;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera.normalized, Vector3.up);
            // Smoothly rotate using Slerp instead of instant snapping
            videoUI.transform.rotation = Quaternion.Slerp(
                videoUI.transform.rotation,
                targetRotation,
                Time.deltaTime / rotationSmoothTime
            );
        }
        // no scaling during drag
    }

    /// <summary>
    /// Call this when the user starts dragging the panel.
    /// </summary>
    public void StartDragging()
    {
        _isDragging = true;
        _scaleTween?.Kill(); // cancel any active scale animation
    }

    /// <summary>
    /// Call this when the user stops dragging the panel.
    /// Smoothly scales the panel to match the current camera distance.
    /// </summary>
    public void StopDragging()
    {
        _isDragging = false;

        if (!_targetCamera || !videoUI)
            return;

        // --- SMOOTH RESIZE ONCE ---
        float distance = Vector3.Distance(_targetCamera.transform.position, videoUI.transform.position);
        float targetScale = distance * scaleFactor;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        _scaleTween?.Kill();
        _scaleTween = videoUI.transform.DOScale(Vector3.one * targetScale, scaleDuration)
            .SetEase(Ease.InOutSine);
    }

    // ---------------------- UI Fading ----------------------

    public void ShowControls()
    {
        if (!canvasGroup)
            return;

        _fadeTween?.Kill();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        _fadeTween = canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);
    }

    public void HideControls()
    {
        if (!canvasGroup)
            return;

        _fadeTween?.Kill();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _fadeTween = canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine);
    }
}
