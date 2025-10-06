using UnityEngine;
using DG.Tweening;
using SimpleWebRTC;
using Unity.WebRTC;

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

        loadingScreenGroup.alpha = 1f;
        videoScreenGroup.alpha = 0f;
        videoScreenGroup.gameObject.SetActive(true);
        loadingScreenGroup.gameObject.SetActive(true);

        StartLoadingPulse();

        // Subscribe to ICE state changes
        webRtcConnection.OnIceConnectionStateChanged += HandleIceStateChanged;
    }

    private void OnDestroy()
    {
        webRtcConnection.OnIceConnectionStateChanged -= HandleIceStateChanged;
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

    private void HandleIceStateChanged(RTCIceConnectionState state)
    {
        if (_hasConnected)
            return;

        if (state == RTCIceConnectionState.Connected)
        {
            _hasConnected = true;
            OnConnected();
        }
    }

    private void OnConnected()
    {
        Debug.Log("[VideoUIController] ICE Connected — fading out loading screen.");

        _pulseTween?.Kill();
        DOTween.Sequence()
            .Append(loadingScreenGroup.DOFade(0f, fadeDuration))
            .Join(videoScreenGroup.DOFade(1f, fadeDuration))
            .OnComplete(() =>
            {
                loadingScreenGroup.gameObject.SetActive(false);
                videoScreenGroup.alpha = 1f;
            });
    }
}
