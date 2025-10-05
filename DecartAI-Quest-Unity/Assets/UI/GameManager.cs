using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using SimpleWebRTC;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [Tooltip("Canvas displaying local webcam feed for WebRTC.")]
    [SerializeField] private GameObject WebRTCWebcamCanvas;

    [Tooltip("Camera used for recording or capturing effects (optional).")]
    [SerializeField] private Camera recordingCamera;

    [Header("Networking")]
    [Tooltip("WebRTC connection that handles model selection and video streaming.")]
    [SerializeField] private WebRTCConnection webRtcConnection;

    [SerializeField] private RawImage streamDisplay;
    
    private bool _didStart;
    private int _currentSoundIndex;
    private bool _animatingIntroEffect;
    private bool _videoTransmissionPending;
    private Tween _passthroughTween;
    private Tween _forceFieldSoundTween;

    private void Start()
    {
        if (WebRTCWebcamCanvas != null)
            WebRTCWebcamCanvas.SetActive(true);
        
        SelectModelAndStart(useLucy: false);
        
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 2f;

        if (GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp)
            urp.renderScale = 2f;
    }

    private void OnDestroy()
    {
        _passthroughTween?.Kill();
        _forceFieldSoundTween?.Kill();
    }

    private void SelectModelAndStart(bool useLucy)
    {
        if (!webRtcConnection)
        {
            Debug.LogError("WebRTCConnection reference is missing! Cannot start experience.");
            return;
        }

        webRtcConnection.SetModelChoice(useLucy);
        webRtcConnection.Connect();
        Debug.Log($"Selected model: {webRtcConnection.GetSelectedModelName()}");

        StartExperience();
    }

    private void StartExperience()
    {
        _didStart = true;

        if (_videoTransmissionPending)
        {
            DOVirtual.DelayedCall(3f, OnVideoTransmissionReceived);
        }
    }

    public void OnVideoTransmissionReceived()
    {
        if (!_didStart)
        {
            _videoTransmissionPending = true;
            return;
        }

        _videoTransmissionPending = false;
    }
}
