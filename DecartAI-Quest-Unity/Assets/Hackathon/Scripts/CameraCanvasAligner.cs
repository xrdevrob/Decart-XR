using UnityEngine;
using PassthroughCameraSamples;

/// <summary>
/// Aligns a RawImage canvas with the passthrough camera projection.
/// Uses the same approach as the working CameraToWorld example,
/// plus a small adjustable size multiplier for fine-tuning coverage.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraCanvasAligner : MonoBehaviour
{
    [SerializeField] private RectTransform canvasTransform;
    [SerializeField] private PassthroughCameraEye eye = PassthroughCameraEye.Left;

    [SerializeField, Tooltip("Distance (in meters) to place the canvas from camera.")]
    private float canvasDistance = 1f;

    [SerializeField, Tooltip("Extra scale multiplier to slightly enlarge the canvas after alignment (e.g., 1.02–1.10).")]
    private float sizeMultiplier = 1.05f;

    private void Start()
    {
        if (!PassthroughCameraUtils.EnsureInitialized())
        {
            Debug.LogError("[CameraCanvasAligner] Passthrough Camera not initialized.");
            return;
        }

        AlignCanvas();
    }

    private void AlignCanvas()
    {
        // Get camera intrinsics and world pose
        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(eye);
        var camPose = PassthroughCameraUtils.GetCameraPoseInWorld(eye);

        Vector2Int resolution = intrinsics.Resolution;

        // Calculate FOV from screen edge rays
        var leftRay = PassthroughCameraUtils.ScreenPointToRayInCamera(eye, new Vector2Int(0, resolution.y / 2));
        var rightRay = PassthroughCameraUtils.ScreenPointToRayInCamera(eye, new Vector2Int(resolution.x, resolution.y / 2));

        float horizontalFovDegrees = Vector3.Angle(leftRay.direction, rightRay.direction);
        float horizontalFovRadians = horizontalFovDegrees * Mathf.Deg2Rad;

        // Calculate physical width at the desired distance
        float physicalWidth = 2f * canvasDistance * Mathf.Tan(horizontalFovRadians / 2f);

        // Scale the canvas to match this physical size, then apply extra multiplier
        float scale = (physicalWidth / canvasTransform.sizeDelta.x) * sizeMultiplier;
        canvasTransform.localScale = new Vector3(scale, scale, scale);

        // Position canvas at the specified distance in front of camera
        canvasTransform.position = camPose.position + camPose.rotation * Vector3.forward * canvasDistance;
        canvasTransform.rotation = camPose.rotation;

        Debug.Log($"[CameraCanvasAligner] {eye} eye aligned at {canvasDistance:F2}m, " +
                  $"FOV: {horizontalFovDegrees:F1}°, Scale: {scale:F3}, Multiplier: {sizeMultiplier:F3}");
    }
}
