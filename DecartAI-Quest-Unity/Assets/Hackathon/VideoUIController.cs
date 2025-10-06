using UnityEngine;
using DG.Tweening;
using SimpleWebRTC;

public class VideoUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WebRTCConnection webRtcConnection;
    [SerializeField] private CanvasGroup loadingScreenGroup;
    [SerializeField] private CanvasGroup videoScreenGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float pulseMinAlpha = 0.4f;
    [SerializeField] private float pulseCycleDuration = 1.5f;

    private Tween _pulseTween;
    private bool _hasConnected;

    private void Start()
    {
        if (!webRtcConnection)
            webRtcConnection = FindFirstObjectByType<WebRTCConnection>();

        if (!loadingScreenGroup || !videoScreenGroup)
        {
            Debug.LogError("[VideoUIController] Missing CanvasGroup references.");
            return;
        }

        // Initialize UI states
        loadingScreenGroup.alpha = 1f;
        videoScreenGroup.alpha = 0f;
        videoScreenGroup.gameObject.SetActive(true);
        loadingScreenGroup.gameObject.SetActive(true);

        StartLoadingPulse();
    }

    private void Update()
    {
        // Trigger fade once WebRTCConnectionActive = true (ICE Connected)
        if (!_hasConnected && webRtcConnection != null && webRtcConnection.WebRTCConnectionActive)
        {
            _hasConnected = true;
            OnConnected();
        }
    }

    private void OnDestroy()
    {
        _pulseTween?.Kill();
    }

    private void StartLoadingPulse()
    {
        _pulseTween = DOTween.Sequence()
            .Append(loadingScreenGroup.DOFade(pulseMinAlpha, pulseCycleDuration / 2f))
            .Append(loadingScreenGroup.DOFade(1f, pulseCycleDuration / 2f))
            .SetLoops(-1)
            .SetEase(Ease.InOutSine);
    }

    private void OnConnected()
    {
        Debug.Log("[VideoUIController] WebRTC ICE state: Connected — fading to video.");

        _pulseTween?.Kill();

        DOTween.Sequence()
            .Append(loadingScreenGroup.DOFade(0f, fadeDuration))
            .Join(videoScreenGroup.DOFade(1f, fadeDuration))
            .OnComplete(() =>
            {
                loadingScreenGroup.gameObject.SetActive(false);
                videoScreenGroup.alpha = 1f; // finalize
            });
    }
}
