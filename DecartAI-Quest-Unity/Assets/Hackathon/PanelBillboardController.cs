using UnityEngine;

/// <summary>
/// Rotates and scales the assigned video UI panel so that it always faces
/// the user's camera and appears constant in size in their view.
/// </summary>
[ExecuteAlways]
public class PanelBillboardController : MonoBehaviour
{
    [Tooltip("The specific UI GameObject (e.g. Video UI Canvas) to rotate/scale.")]
    [SerializeField] private GameObject videoUI;

    [Tooltip("Base scale factor — tweak until panel appears the desired size in frustum.")]
    [SerializeField] private float scaleFactor = 0.5f;

    [Tooltip("If true, panel only rotates around Y axis (keeps upright).")]
    [SerializeField] private bool yAxisOnly;
    
    private Camera _targetCamera;

    private void Start()
    {
        if (_targetCamera == null)
        {
            _targetCamera = Camera.main;
            if (_targetCamera == null)
            {
                Debug.LogWarning("[PanelBillboardController] No camera found!");
            }
        }
    }

    private void Update()
    {
        if (!_targetCamera || !videoUI)
            return;

        // --- ROTATION ---
        Vector3 directionToCamera = _targetCamera.transform.position - videoUI.transform.position;

        if (yAxisOnly)
            directionToCamera.y = 0f;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera.normalized, Vector3.up);
            videoUI.transform.rotation = targetRotation;
        }

        // --- SCALING ---
        float distance = Vector3.Distance(_targetCamera.transform.position, videoUI.transform.position);
        videoUI.transform.localScale = Vector3.one * distance * scaleFactor;
    }
}