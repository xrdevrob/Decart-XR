using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RenderHeads.Media.AVProVideo;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using Tween = DG.Tweening.Tween;

public class VideoPlayerControlsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MediaPlayer mediaPlayer;
    [SerializeField] private CanvasGroup playerControlsGroup;
    [SerializeField] private VideoUIController videoUIController;

    [Header("UI Elements (Toggles)")]
    [SerializeField] private Toggle playPauseToggle;
    [SerializeField] private Toggle skipBackToggle;
    [SerializeField] private Toggle skipForwardToggle;

    [Header("Sliders & Labels")]
    [SerializeField] private Slider timelineSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text currentTimeLabel;
    [SerializeField] private TMP_Text totalTimeLabel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float hideDelay = 2f;

    private Tween _fadeTween;
    private Coroutine _hideCoroutine;
    private bool _isHovered;
    private bool _hasStarted;
    private double _cachedDuration;

    // --- Scrubbing state ---
    private bool _isDragging;
    private bool _wasPlayingBeforeDrag;

    private void Start()
    {
        if (!mediaPlayer)
            mediaPlayer = FindFirstObjectByType<MediaPlayer>();

        playerControlsGroup.alpha = 0f;
        playerControlsGroup.interactable = false;
        playerControlsGroup.blocksRaycasts = false;

        HookUpUI();
        StartCoroutine(UpdateTimelineRoutine());
    }

    private void HookUpUI()
    {
        if (playPauseToggle)
            playPauseToggle.onValueChanged.AddListener(OnPlayPauseToggleChanged);

        if (skipBackToggle)
            skipBackToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn) SkipSeconds(-10);
                ResetMomentaryToggle(skipBackToggle);
            });

        if (skipForwardToggle)
            skipForwardToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn) SkipSeconds(10);
                ResetMomentaryToggle(skipForwardToggle);
            });

        if (volumeSlider)
            volumeSlider.onValueChanged.AddListener(SetVolume);

        if (timelineSlider)
        {
            timelineSlider.onValueChanged.AddListener(OnTimelineScrub);
        }
    }

    private void ResetMomentaryToggle(Toggle toggle)
    {
        DOVirtual.DelayedCall(0.15f, () => toggle.isOn = false);
    }

    public void ShowControls()
    {
        if (!videoUIController.serverConnected)
            return;

        _isHovered = true;
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        FadeControls(1f);
    }

    public void HideControlsWithDelay()
    {
        _isHovered = false;
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        if (!_isHovered)
            FadeControls(0f);
    }

    private void FadeControls(float targetAlpha)
    {
        _fadeTween?.Kill();
        playerControlsGroup.interactable = targetAlpha > 0.5f;
        playerControlsGroup.blocksRaycasts = targetAlpha > 0.5f;

        _fadeTween = playerControlsGroup.DOFade(targetAlpha, fadeDuration)
            .SetEase(Ease.InOutSine);
    }

    private void OnPlayPauseToggleChanged(bool isPlaying)
    {
        if (!mediaPlayer || mediaPlayer.Control == null)
            return;

        if (isPlaying)
        {
            if (!_hasStarted)
            {
                _hasStarted = true;
                FadeControls(1f);
            }
            mediaPlayer.Control.Play();
        }
        else
        {
            mediaPlayer.Control.Pause();
        }

        ForceTimelineRefresh();
    }

    private void SkipSeconds(float seconds)
    {
        if (mediaPlayer && mediaPlayer.Control != null)
        {
            double duration = GetDurationSafe();
            double newTime = Mathf.Clamp(
                (float)(mediaPlayer.Control.GetCurrentTime() + seconds),
                0f,
                (float)duration
            );
            mediaPlayer.Control.Seek(newTime);
            ForceTimelineRefresh();
        }
    }

    private void SetVolume(float value)
    {
        if (mediaPlayer?.Control != null)
            mediaPlayer.AudioVolume = value;
    }

    // --- Scrubbing ---
    private void OnTimelineScrub(float value)
    {
        if (!mediaPlayer || GetDurationSafe() <= 0)
            return;

        if (_isDragging)
        {
            // Update preview label while dragging
            double previewTime = GetDurationSafe() * value;
            if (currentTimeLabel)
                currentTimeLabel.text = FormatTime(previewTime);
        }
        else
        {
            // Final scrub seek when slider is released
            double targetTime = GetDurationSafe() * value;
            mediaPlayer.Control.Seek(targetTime);
            ForceTimelineRefresh();
        }
    }

    private void OnTimelineDragStart()
    {
        if (!mediaPlayer || mediaPlayer.Control == null)
            return;

        _isDragging = true;
        _wasPlayingBeforeDrag = mediaPlayer.Control.IsPlaying();
        if (_wasPlayingBeforeDrag)
            mediaPlayer.Control.Pause();
    }

    private void OnTimelineDragEnd()
    {
        if (!mediaPlayer || mediaPlayer.Control == null)
            return;

        _isDragging = false;
        double targetTime = GetDurationSafe() * timelineSlider.value;
        mediaPlayer.Control.Seek(targetTime);
        ForceTimelineRefresh();

        // Resume playback if it was playing before
        if (_wasPlayingBeforeDrag)
            mediaPlayer.Control.Play();
    }

    private IEnumerator UpdateTimelineRoutine()
    {
        while (true)
        {
            if (!_isDragging) // Skip updates while dragging
                UpdateTimelineUI();
            yield return null;
        }
    }

    private void UpdateTimelineUI()
    {
        if (!mediaPlayer || mediaPlayer.Control == null)
            return;

        double duration = GetDurationSafe();
        if (duration <= 0)
            return;

        double currentTime = mediaPlayer.Control.GetCurrentTime();
        float normalized = Mathf.Clamp01((float)(currentTime / duration));

        if (timelineSlider)
            timelineSlider.SetValueWithoutNotify(normalized);

        if (currentTimeLabel)
            currentTimeLabel.text = FormatTime(currentTime);
    }

    private double GetDurationSafe()
    {
        if (_cachedDuration <= 0)
        {
            if (mediaPlayer?.Info != null && mediaPlayer.Info.GetDuration() > 0)
            {
                _cachedDuration = mediaPlayer.Info.GetDuration();
            }
            else if (mediaPlayer?.Control != null)
            {
                var range = mediaPlayer.Control.GetSeekableTimes();
                if (range.Duration > 0)
                    _cachedDuration = range.Duration;
            }

            if (_cachedDuration > 0 && totalTimeLabel)
                totalTimeLabel.text = FormatTime(_cachedDuration);
        }

        return _cachedDuration;
    }

    private void ForceTimelineRefresh()
    {
        UpdateTimelineUI();
    }

    private string FormatTime(double seconds)
    {
        int totalSeconds = Mathf.FloorToInt((float)seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return $"{minutes:D2}:{secs:D2}";
    }
}


