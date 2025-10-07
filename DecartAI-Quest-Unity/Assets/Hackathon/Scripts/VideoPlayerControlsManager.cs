using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RenderHeads.Media.AVProVideo;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class VideoPlayerControlsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MediaPlayer mediaPlayer;
    [SerializeField] private CanvasGroup playerControlsGroup;
    [SerializeField] private VideoUIController videoUIController;

    [Header("UI Elements")]
    [SerializeField] private Toggle playPauseToggle;
    [SerializeField] private Toggle skipBackToggle;
    [SerializeField] private Toggle skipForwardToggle;
    [SerializeField] private Slider timelineSlider;
    [SerializeField] private Slider volumeSlider;

    [Header("Time Labels")]
    [SerializeField] private TMP_Text currentTimeLabel;  // Left
    [SerializeField] private TMP_Text totalTimeLabel;    // Right

    [Header("Icons")]
    [SerializeField] private Image playPauseImg;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float hideDelay = 2f;

    private bool _isDragging;
    private bool _wasPlayingBeforeDrag;
    private bool _isHovered;
    private Tween _fadeTween;
    private Coroutine _hideCoroutine;
    private double _cachedDuration;
    private bool _ignoreUIChange; // prevent recursive toggle triggers

    private void Start()
    {
        if (!mediaPlayer)
        {
            mediaPlayer = FindFirstObjectByType<MediaPlayer>();
        }

        playerControlsGroup.alpha = 0f;
        playerControlsGroup.interactable = false;
        playerControlsGroup.blocksRaycasts = false;

        HookUpUI();
        HookUpTimelineDragEvents();
        HookUpMediaEvents();

        StartCoroutine(UpdateTimelineRoutine());
    }

    private void OnDestroy()
    {
        UnhookMediaEvents();
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
            volumeSlider.onValueChanged.AddListener(v => { if (mediaPlayer) mediaPlayer.AudioVolume = v; });

        if (timelineSlider)
            timelineSlider.onValueChanged.AddListener(OnTimelineScrub);
    }

    private void HookUpTimelineDragEvents()
    {
        if (!timelineSlider) return;

        var trigger = timelineSlider.GetComponent<EventTrigger>() ?? timelineSlider.gameObject.AddComponent<EventTrigger>();

        AddEvent(trigger, EventTriggerType.PointerDown, _ => OnTimelineDragStart());
        AddEvent(trigger, EventTriggerType.BeginDrag, _ => OnTimelineDragStart());
        AddEvent(trigger, EventTriggerType.PointerUp, _ => OnTimelineDragEnd());
        AddEvent(trigger, EventTriggerType.EndDrag, _ => OnTimelineDragEnd());
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action<BaseEventData> cb)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(cb));
        trigger.triggers.Add(entry);
    }

    private void HookUpMediaEvents()
    {
        if (mediaPlayer)
            mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
    }

    private void UnhookMediaEvents()
    {
        if (mediaPlayer)
            mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
    }

    // ---------------------- Media Events ----------------------

    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType type, ErrorCode error)
    {
        switch (type)
        {
            case MediaPlayerEvent.EventType.MetaDataReady:
            case MediaPlayerEvent.EventType.ReadyToPlay:
            case MediaPlayerEvent.EventType.FirstFrameReady:
            case MediaPlayerEvent.EventType.Started:
            case MediaPlayerEvent.EventType.FinishedPlaying:
                _cachedDuration = 0;
                RefreshUIFromPlayer();

                // ✅ When first frame is ready, update total time label
                if (type == MediaPlayerEvent.EventType.FirstFrameReady && totalTimeLabel)
                {
                    double duration = GetDurationSafe();
                    totalTimeLabel.text = FormatTime(duration);
                }
                break;
        }
    }

    // ---------------------- UI Controls ----------------------

    private void OnPlayPauseToggleChanged(bool shouldPlay)
    {
        if (_ignoreUIChange || !mediaPlayer || mediaPlayer.Control == null)
            return;

        if (shouldPlay)
            mediaPlayer.Control.Play();
        else
            mediaPlayer.Control.Pause();

        UpdatePlayPauseIcon(shouldPlay);
    }

    private void UpdatePlayPauseIcon(bool isPlaying)
    {
        if (playPauseImg)
            playPauseImg.sprite = isPlaying ? pauseIcon : playIcon;
    }

    private void RefreshUIFromPlayer()
    {
        if (!mediaPlayer || mediaPlayer.Control == null)
            return;

        bool playing = mediaPlayer.Control.IsPlaying();

        _ignoreUIChange = true;
        if (playPauseToggle)
            playPauseToggle.isOn = playing;
        _ignoreUIChange = false;

        UpdatePlayPauseIcon(playing);
        UpdateTimelineUI();
    }

    private void SkipSeconds(float seconds)
    {
        if (mediaPlayer?.Control == null) return;

        double duration = GetDurationSafe();
        double current = mediaPlayer.Control.GetCurrentTime();
        double target = Mathf.Clamp((float)(current + seconds), 0f, (float)duration);
        mediaPlayer.Control.Seek(target);
    }

    private void ResetMomentaryToggle(Toggle t)
    {
        DOVirtual.DelayedCall(0.15f, () => t.isOn = false);
    }

    // ---------------------- Timeline ----------------------

    private void OnTimelineScrub(float value)
    {
        if (!mediaPlayer || GetDurationSafe() <= 0) return;

        if (_isDragging)
        {
            double previewTime = GetDurationSafe() * value;
            if (currentTimeLabel)
                currentTimeLabel.text = FormatTime(previewTime);
        }
        else
        {
            double targetTime = GetDurationSafe() * value;
            mediaPlayer.Control.Seek(targetTime);
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

        if (_wasPlayingBeforeDrag)
            mediaPlayer.Control.Play();

        RefreshUIFromPlayer();
    }

    // ---------------------- Update Loop ----------------------

    private IEnumerator UpdateTimelineRoutine()
    {
        while (true)
        {
            if (!_isDragging)
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

        double current = mediaPlayer.Control.GetCurrentTime();
        float normalized = Mathf.Clamp01((float)(current / duration));

        // update slider only if not scrubbing
        if (timelineSlider)
            timelineSlider.SetValueWithoutNotify(normalized);

        // left = current, right = total
        if (currentTimeLabel)
            currentTimeLabel.text = FormatTime(current);
    }

    private double GetDurationSafe()
    {
        if (mediaPlayer?.Info != null && mediaPlayer.Info.GetDuration() > 0)
            _cachedDuration = mediaPlayer.Info.GetDuration();
        else if (mediaPlayer?.Control != null)
        {
            var range = mediaPlayer.Control.GetSeekableTimes();
            if (range.Duration > 0)
                _cachedDuration = range.Duration;
        }

        return _cachedDuration;
    }

    private string FormatTime(double seconds)
    {
        if (double.IsNaN(seconds) || double.IsInfinity(seconds))
            return "--:--";

        int totalSeconds = Mathf.FloorToInt((float)seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return $"{minutes:D2}:{secs:D2}";
    }

    // ---------------------- Fade Controls ----------------------

    public void ShowControls()
    {
        if (videoUIController && !videoUIController.serverConnected)
            return;

        _isHovered = true;
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

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
}
