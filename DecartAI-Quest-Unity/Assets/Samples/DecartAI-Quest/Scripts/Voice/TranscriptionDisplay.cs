using TMPro;
using UnityEngine;
using Oculus.Voice;
using QuestCameraKit.WebRTC;

public class VoiceIntentController : MonoBehaviour
{
    [Header("Voice Service")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;

    [Header("WebRTC")]
    [SerializeField] private WebRTCController webRTCController;

    [Header("UI")]
    [SerializeField] private TMP_Text fullTranscriptText;
    [SerializeField] private TMP_Text partialTranscriptText;

    private bool appVoiceActive;

    private void Awake()
    {
        // fullTranscriptText.text = partialTranscriptText.text = string.Empty;

        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener((transcription) => {
            webRTCController.QueueCustomPrompt(transcription);
            Debug.Log("Sent transcription to WebRTC: " + transcription);
            fullTranscriptText.text = transcription;
        });

        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener((transcription) => {
            partialTranscriptText.text = transcription;
        });

        // appVoiceExperience.VoiceEvents.OnRequestCreated.AddListener((request) => {
        //     appVoiceActive = true;
        //     Debug.Log("OnRequestCreated");
        // });

    //     appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(() => {
    //         appVoiceActive = false;
    //         Debug.Log("OnRequestCompleted");
    //     });
    }

    private void Update() {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
            appVoiceExperience.Activate();
        }
    }
}