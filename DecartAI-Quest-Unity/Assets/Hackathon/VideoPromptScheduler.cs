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

    private void Update()
    {
        if (!mediaPlayer || !mediaPlayer.Control.IsPlaying())
        {
            return;
        }

        float currentTime = (float)mediaPlayer.Control.GetCurrentTime();
        foreach (var item in timedPrompts)
        {
            if (item.hasFired)
            {
                continue;
            }

            if (Mathf.Abs(currentTime - item.timestamp) <= triggerTolerance)
            {
                TriggerPrompt(item);
            }
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

    // Optional helper to reset fired flags (e.g., on replay)
    public void ResetPrompts()
    {
        foreach (var item in timedPrompts)
        {
            item.hasFired = false;
        }
    }
}
