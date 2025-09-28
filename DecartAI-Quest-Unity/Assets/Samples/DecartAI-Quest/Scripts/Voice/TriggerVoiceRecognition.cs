using UnityEngine;
using Oculus.Voice;

public class TriggerVoiceRecognition : MonoBehaviour
{
    public AppVoiceExperience appVoice;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            appVoice.Activate();
        }
    }
}
