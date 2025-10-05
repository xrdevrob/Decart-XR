using UnityEngine;
using PassthroughCameraSamples;

/// <summary>
/// Aligns a RawImage canvas with the passthrough camera's left (or right) eye.
/// It positions the canvas far in front (e.g. 10 m) so it fills the background,
/// matching the camera’s field of view once at startup.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraCanvasAligner : MonoBehaviour
{
    [SerializeField, Tooltip("The RectTransform of the RawImage canvas to align.")]
    private RectTransform canvasTransform;

    [SerializeField, Tooltip("Which passthrough camera eye to align with (Left or Right).")]
    private PassthroughCameraEye eye = PassthroughCameraEye.Left;

    [SerializeField, Tooltip("Distance in meters from the camera. Use a large value (e.g., 10) for a background effect.")]
    private float distanceFromCamera = 10f;

    private void Start()
    {
        if (!PassthroughCameraUtils.EnsureInitialized())
        {
            Debug.LogError("[CameraCanvasAligner] Passthrough Camera not initialized.");
            return;
        }

        AlignCanvasToEye();
    }

    private void AlignCanvasToEye()
    {
        // Get passthrough camera pose in world space
        Pose cameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(eye);

        // Get intrinsics for FOV-based scaling
        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(eye);
        float fovX = Mathf.Atan(intrinsics.Resolution.x / (2f * intrinsics.FocalLength.x)) * 2f * Mathf.Rad2Deg;
        float fovY = Mathf.Atan(intrinsics.Resolution.y / (2f * intrinsics.FocalLength.y)) * 2f * Mathf.Rad2Deg;

        // Convert FOV to physical width/height at the target distance
        float widthAtDistance = 2f * distanceFromCamera * Mathf.Tan(fovX * Mathf.Deg2Rad / 2f);
        float heightAtDistance = 2f * distanceFromCamera * Mathf.Tan(fovY * Mathf.Deg2Rad / 2f);

        // Scale the canvas in world space to match the FOV area
        Vector2 canvasSize = canvasTransform.sizeDelta;
        float scaleX = widthAtDistance / canvasSize.x;
        float scaleY = heightAtDistance / canvasSize.y;
        canvasTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        // Align and rotate with camera
        canvasTransform.position = cameraPose.position + cameraPose.rotation * Vector3.forward * distanceFromCamera;
        canvasTransform.rotation = cameraPose.rotation;

        Debug.Log($"[CameraCanvasAligner] Canvas aligned with {eye} eye, distance {distanceFromCamera:F1}m, scale ({scaleX:F3}, {scaleY:F3}).");
    }
}
