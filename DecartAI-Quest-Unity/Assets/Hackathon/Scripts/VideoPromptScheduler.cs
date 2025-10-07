using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RenderHeads.Media.AVProVideo;   // AVPro namespace

[Serializable]
public class TimedPrompt
{
    [Tooltip("Time in seconds when this prompt should trigger.")]
    public float timestamp;

    [Tooltip("Prompt text to send at this timestamp.")]
    public string prompt;

    [Tooltip("Optional UnityEvent to fire when this prompt is reached.")]
    public UnityEvent onTriggered;

    [HideInInspector] public bool hasFired = false;
}

public class VideoPromptScheduler : MonoBehaviour
{
    [Tooltip("Reference to the AVPro MediaPlayer that is playing the video.")]
    [SerializeField] private MediaPlayer mediaPlayer;

    [Tooltip("List of prompts to trigger at specific times during playback.")]
    [SerializeField] private List<TimedPrompt> timedPrompts = new();

    [Tooltip("WebRTC controller used to send prompts to the AI system.")]
    [SerializeField] private QuestCameraKit.WebRTC.WebRTCController webRtcController;

    [Tooltip("How close (in seconds) we can be to a timestamp for it to trigger.")]
    [SerializeField] private float triggerTolerance = 0.1f;

    private void Reset()
    {
        mediaPlayer = FindFirstObjectByType<MediaPlayer>();
        webRtcController = FindFirstObjectByType<QuestCameraKit.WebRTC.WebRTCController>();
    }

    private void OnEnable()
    {
        if (mediaPlayer)
            mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
    }

    private void OnDisable()
    {
        if (mediaPlayer)
            mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
    }

    private void Update()
    {
        if (!mediaPlayer || mediaPlayer.Control == null || !mediaPlayer.Control.IsPlaying())
            return;

        float currentTime = (float)mediaPlayer.Control.GetCurrentTime();
        foreach (var item in timedPrompts)
        {
            if (item.hasFired)
                continue;

            if (Mathf.Abs(currentTime - item.timestamp) <= triggerTolerance)
                TriggerPrompt(item);
        }
    }

    private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType type, ErrorCode error)
    {
        switch (type)
        {
            case MediaPlayerEvent.EventType.Started:
            case MediaPlayerEvent.EventType.Unpaused:
                // Resume prompt tracking
                break;
            case MediaPlayerEvent.EventType.Paused:
                // Video paused — optional: you could temporarily stop Update checks if needed
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                // Reset when the video finishes
                ResetPrompts();
                break;
            case MediaPlayerEvent.EventType.Closing:
            case MediaPlayerEvent.EventType.ReadyToPlay:
                ResetPrompts();
                break;
            case MediaPlayerEvent.EventType.MetaDataReady:
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                break;
            case MediaPlayerEvent.EventType.Error:
                break;
            case MediaPlayerEvent.EventType.SubtitleChange:
                break;
            case MediaPlayerEvent.EventType.Stalled:
                break;
            case MediaPlayerEvent.EventType.Unstalled:
                break;
            case MediaPlayerEvent.EventType.ResolutionChanged:
                break;
            case MediaPlayerEvent.EventType.StartedSeeking:
                break;
            case MediaPlayerEvent.EventType.FinishedSeeking:
                break;
            case MediaPlayerEvent.EventType.StartedBuffering:
                break;
            case MediaPlayerEvent.EventType.FinishedBuffering:
                break;
            case MediaPlayerEvent.EventType.PropertiesChanged:
                break;
            case MediaPlayerEvent.EventType.PlaylistItemChanged:
                break;
            case MediaPlayerEvent.EventType.PlaylistFinished:
                break;
            case MediaPlayerEvent.EventType.TextTracksChanged:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void TriggerPrompt(TimedPrompt item)
    {
        item.hasFired = true;

        Debug.Log($"[VideoPromptScheduler] Triggered prompt at {item.timestamp:F1}s: {item.prompt}");

        if (webRtcController && !string.IsNullOrEmpty(item.prompt))
        {
            webRtcController.QueueCustomPrompt(item.prompt);
        }

        item.onTriggered?.Invoke();
    }

    public void ResetPrompts()
    {
        foreach (var item in timedPrompts)
            item.hasFired = false;

        Debug.Log("[VideoPromptScheduler] Prompts reset.");
    }
}
