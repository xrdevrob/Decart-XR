using UnityEngine;

/// <summary>
/// Rotates and scales the assigned video UI panel so it always faces
/// the user's camera and appears constant in their view.
/// </summary>
public class PanelBillboardController : MonoBehaviour
{
    [Tooltip("The specific UI GameObject (e.g. Video UI Canvas) to rotate/scale.")]
    [SerializeField] private GameObject videoUI;

    [Tooltip("Base scale factor — tweak until panel appears desired size in frustum.")]
    [SerializeField] private float scaleFactor = 1.4f;

    [Tooltip("If true, panel only rotates around Y axis (keeps upright).")]
    [SerializeField] private bool yAxisOnly;

    [Tooltip("If true, flips rotation direction (for back-facing canvases).")]
    [SerializeField] private bool invertFacing;
    
    private Camera _targetCamera;

    private void Start()
    {
        _targetCamera = Camera.main;
    }

    private void Update()
    {
        if (!_targetCamera || !videoUI)
        {
            return;
        }

        Vector3 directionToCamera = invertFacing
            ? videoUI.transform.position - _targetCamera.transform.position
            : _targetCamera.transform.position - videoUI.transform.position;

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