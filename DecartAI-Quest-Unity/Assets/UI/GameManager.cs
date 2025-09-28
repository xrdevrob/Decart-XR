using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public Button startButton;
    public PortalController portal;
    public Transform streamParent;
    public UIFader menuFader;
    public UIFader textFader;
    public OVRPassthroughLayer passthroughLayer;
    public GameObject WebRTCWebcamCanvas;
    public Camera recordingCamera;
    public GameObject effectCone;
    public AudioClip[] forceFieldSounds;
    public AudioSource audioSource;
    public Material introEffectMaterial;

    private bool didStart = false;
    private bool videoTransmissionPending = false;
    private bool playingForceFieldSounds = false;
    private int currentSoundIndex = 0;
    private bool animatingIntroEffect = false;
    
    private void Start()
    {
        startButton.onClick.AddListener(() => StartExperience());
        
        #if !UNITY_EDITOR
        WebRTCWebcamCanvas.gameObject.SetActive(true);
        RemoveSkybox();
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 2f;
        (GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset).renderScale = 2f;
        #endif
    }
    
    private void StartExperience()
    {
        didStart = true;
        menuFader.FadeOut();
        portal.Show();
        effectCone.gameObject.SetActive(true);

        if (passthroughLayer != null)
        {
            DOTween.To(
                () => passthroughLayer.textureOpacity,
                x => passthroughLayer.textureOpacity = x,
                0.25f, 1f);
        }

        if (videoTransmissionPending)
        {
            DOVirtual.DelayedCall(3f, OnVideoTransmissionReceived);
        }

        StartForceFieldSounds();
        StartIntroEffect();
    }

    public void OnVideoTransmissionReceived()
    {
        if (!didStart)
        {
            videoTransmissionPending = true;
            return;
        }

        videoTransmissionPending = false;
        StopForceFieldSounds();
        StopIntroEffect();

        streamParent.gameObject.SetActive(true);
        portal.Expand();
        effectCone.gameObject.SetActive(false);

        // Show text and apply portal material when not in wide FoV
        textFader.FadeIn(delay: 3f);

        RawImage[] rawImages = streamParent.GetComponentsInChildren<RawImage>();
        foreach (RawImage rawImage in rawImages)
        {
            rawImage.material = portal.portalMaterial;
        }
    }
    
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (!didStart) StartExperience();
            else ShowWave();
        }

        if (didStart && OVRInput.GetDown(OVRInput.Button.Two))
        {
            ShowWave();
        }
    }

    private void ShowWave()
    {
        effectCone.gameObject.SetActive(true);
        StartCoroutine(DisableEffectConeAfterDelay());
        audioSource.clip = forceFieldSounds[currentSoundIndex];
        audioSource.pitch = 0.75f;
        audioSource.Play();
        currentSoundIndex = (currentSoundIndex + 1) % forceFieldSounds.Length;

        if (introEffectMaterial != null)
        {
            StartIntroEffect();
        }
    }

    private IEnumerator DisableEffectConeAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        StopIntroEffect();
        effectCone.gameObject.SetActive(false);
    }
    
    private void StartForceFieldSounds()
    {
        if (forceFieldSounds == null || forceFieldSounds.Length == 0 || audioSource == null)
            return;

        playingForceFieldSounds = true;
        currentSoundIndex = 0;
        PlayNextForceFieldSound();
    }

    private void StopForceFieldSounds()
    {
        playingForceFieldSounds = false;
    }

    private void PlayNextForceFieldSound()
    {
        if (!playingForceFieldSounds || forceFieldSounds == null || forceFieldSounds.Length == 0)
            return;

        audioSource.clip = forceFieldSounds[currentSoundIndex];
        audioSource.pitch = 0.75f;
        audioSource.Play();

        float clipLength = 1.5f;
        DOVirtual.DelayedCall(clipLength, () =>
        {
            if (playingForceFieldSounds)
            {
                currentSoundIndex = (currentSoundIndex + 1) % forceFieldSounds.Length;
                PlayNextForceFieldSound();
            }
        });
    }

    private void StartIntroEffect()
    {
        if (introEffectMaterial == null)
            return;

        animatingIntroEffect = true;
        introEffectMaterial.SetFloat("_CustomTime", 0f);
        StartCoroutine(UpdateIntroEffectTime());
    }

    private void StopIntroEffect()
    {
        animatingIntroEffect = false;
    }

    private IEnumerator UpdateIntroEffectTime()
    {
        float startTime = Time.time;
        while (animatingIntroEffect)
        {
            float elapsedTime = Time.time - startTime;
            introEffectMaterial.SetFloat("_CustomTime", elapsedTime);
            yield return null;
        }
    }

    private void RemoveSkybox()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        DynamicGI.UpdateEnvironment();
    }
}
