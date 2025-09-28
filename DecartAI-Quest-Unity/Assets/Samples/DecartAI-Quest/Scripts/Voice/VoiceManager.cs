// using UnityEngine;
// using UnityEngine.Events;
// using TMPro;
// using Oculus.Voice;
// using System.Reflection;
// using Meta.WitAi.CallbackHandlers;

// public class VoiceManager : MonoBehaviour
// {
//     [Header("COnfiguration")]
//     [SerializeField] private AppVoiceExperience appVoice;
//     [SerializeField] private WitResponseMatcher responseMatcher;
//     [SerializeField] private TMP_Text transcriptText;
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
    
//     [Header("Voice Events")]
//     [SerializeField] private UnityEvent wakeWordDetected;
//     [SerializeField] private UnityEvent<string> colpeteTranscrtion;

//     private bool _voiceCommandReady;

    
//     void Awake()
//     {
//         appVoice.VoiceEvents.OnRequestComplete.AddListener(ReactivateVoice);
//         appVoice.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
//         appVoice.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);


//         var eventField = typeof(WitResponseMatcher).GetField("OnMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
//         if (eventField != null && eventField.GetValue(responseMatcher) is MultiEvent OnMultiValueEvent) {
//             OnMultiValueEvent.AddListener(WakeWordDetected);
//         }

//         _voiceCommandReady = true;
//     }

//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
