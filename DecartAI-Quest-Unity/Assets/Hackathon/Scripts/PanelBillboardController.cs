using UnityEngine;
using DG.Tweening;

/// <summary>
/// Moves and rotates the panel based on the ray interactor while dragging,
/// preserving the initial offset so it never jumps when grabbed.
/// </summary>
public class PanelBillboardController : MonoBehaviour
{
    [Header("UI Target")]
    [SerializeField] private GameObject videoUI;

    [Header("Ray Setup")]
    [Tooltip("Transform of your ray interactor (e.g. RightHandRayInteractor or RightHandAnchor).")]
    [SerializeField] private Transform rayOrigin;

    [Tooltip("Default hover distance from the ray origin (used until offset is captured).")]
    [SerializeField] private float defaultRayDistance = 1.5f;

    [Tooltip("Smooth follow factor (0–1). Lower = smoother, higher = snappier.")]
    [SerializeField, Range(0.01f, 1f)] private float followSmooth = 0.2f;

    [Header("Rotation / Billboard")]
    [Tooltip("If true, keeps the panel upright (rotates only on Y).")]
    [SerializeField] private bool yAxisOnly = true;

    [Tooltip("Smooth rotation factor (0–1).")]
    [SerializeField, Range(0.01f, 1f)] private float rotationSmooth = 0.15f;

    [Tooltip("If true, flips facing direction (for back-facing panels).")]
    [SerializeField] private bool invertFacing = false;

    [Header("Scaling")]
    [SerializeField] private float scaleFactor = 1.4f;
    [SerializeField] private float minScale = 0.4f;
    [SerializeField] private float maxScale = 5f;
    [SerializeField] private float scaleDuration = 0.3f;

    [Header("Fade (optional)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    private Camera _cam;
    private bool _isDragging;
    private Tween _scaleTween, _fadeTween;
    private float _grabDistance;      // actual distance at grab start
    private Vector3 _grabOffsetWorld; // local offset from ray point to panel center

    private Transform PanelT => videoUI ? videoUI.transform : transform;

    private void Start()
    {
        _cam = Camera.main;
        if (canvasGroup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void LateUpdate()
    {
        if (!_isDragging || !rayOrigin || !PanelT || !_cam) return;

        // --- TRANSLATION ALONG RAY (preserving offset) ---
        Vector3 rayStart = rayOrigin.position;
        Vector3 rayDir = rayOrigin.forward;

        // Desired ray hit position (where the hand is pointing)
        Vector3 targetPos = rayStart + rayDir * _grabDistance + _grabOffsetWorld;

        // Smooth move
        PanelT.position = Vector3.Lerp(
            PanelT.position,
            targetPos,
            1f - Mathf.Pow(1f - followSmooth, Time.deltaTime * 60f)
        );

        // --- BILLBOARD ROTATION ---
        Vector3 toCam = invertFacing
            ? PanelT.position - _cam.transform.position
            : _cam.transform.position - PanelT.position;

        if (yAxisOnly) toCam.y = 0;
        if (toCam.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
            PanelT.rotation = Quaternion.Slerp(
                PanelT.rotation,
                targetRot,
                1f - Mathf.Pow(1f - rotationSmooth, Time.deltaTime * 60f)
            );
        }
    }

    public void StartDragging()
    {
        _isDragging = true;
        _scaleTween?.Kill();

        if (!rayOrigin || !PanelT) return;

        // Measure distance from ray origin to panel at grab start
        Vector3 panelPos = PanelT.position;
        Vector3 rayStart = rayOrigin.position;
        Vector3 rayDir = rayOrigin.forward;

        Vector3 rayToPanel = panelPos - rayStart;
        _grabDistance = Vector3.Dot(rayDir, rayToPanel); // signed distance along ray

        // If the ray doesn't reach the panel, fallback to default
        if (_grabDistance < 0.05f)
            _grabDistance = defaultRayDistance;

        // Compute offset (the difference between ray point and panel center)
        Vector3 hitPoint = rayStart + rayDir * _grabDistance;
        _grabOffsetWorld = panelPos - hitPoint;
    }

    public void StopDragging()
    {
        _isDragging = false;
        if (!_cam || !PanelT) return;

        float distance = Vector3.Distance(_cam.transform.position, PanelT.position);
        float targetScale = Mathf.Clamp(distance * scaleFactor, minScale, maxScale);

        _scaleTween?.Kill();
        _scaleTween = PanelT.DOScale(Vector3.one * targetScale, scaleDuration)
            .SetEase(Ease.InOutSine);
    }

    public void ShowControls()
    {
        if (!canvasGroup) return;
        _fadeTween?.Kill();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        _fadeTween = canvasGroup.DOFade(1, fadeDuration).SetEase(Ease.InOutSine);
    }

    public void HideControls()
    {
        if (!canvasGroup) return;
        _fadeTween?.Kill();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _fadeTween = canvasGroup.DOFade(0, fadeDuration).SetEase(Ease.InOutSine);
    }
}
