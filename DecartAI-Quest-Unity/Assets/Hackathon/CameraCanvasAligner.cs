using UnityEngine;
using PassthroughCameraSamples;

/// <summary>
/// Precisely aligns a RawImage canvas so that it matches the passthrough camera’s
/// physical projection. Every pixel corresponds to what the left (or right) eye camera sees.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraCanvasAlignerProper : MonoBehaviour
{
    [SerializeField] private RectTransform canvasTransform;
    [SerializeField] private PassthroughCameraEye eye = PassthroughCameraEye.Left;
    [SerializeField, Tooltip("Distance in meters from the camera projection plane. 10m gives a 'background' feel.")]
    private float distanceFromCamera = 10f;

    private void Start()
    {
        if (!PassthroughCameraUtils.EnsureInitialized())
        {
            Debug.LogError("[CameraCanvasAlignerProper] Passthrough Camera not initialized.");
            return;
        }

        AlignToPassthroughCamera();
    }

    private void AlignToPassthroughCamera()
    {
        // 1. Get the passthrough camera intrinsics
        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(eye);
        var camPose = PassthroughCameraUtils.GetCameraPoseInWorld(eye);

        float fx = intrinsics.FocalLength.x;
        float fy = intrinsics.FocalLength.y;
        float cx = intrinsics.PrincipalPoint.x;
        float cy = intrinsics.PrincipalPoint.y;
        float width = intrinsics.Resolution.x;
        float height = intrinsics.Resolution.y;

        // 2. Compute half-angles in radians from intrinsics
        float left = -cx / fx;
        float right = (width - cx) / fx;
        float top = cy / fy;
        float bottom = -(height - cy) / fy;

        // 3. Get the 4 corner points at the target plane distance
        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(left,  top,  1); // upper left
        corners[1] = new Vector3(right, top,  1); // upper right
        corners[2] = new Vector3(right, bottom, 1); // lower right
        corners[3] = new Vector3(left,  bottom, 1); // lower left

        for (int i = 0; i < 4; i++)
            corners[i] *= distanceFromCamera; // scale to plane distance

        // 4. Calculate plane width/height from those corners
        float widthMeters = Vector3.Distance(corners[0], corners[1]);
        float heightMeters = Vector3.Distance(corners[0], corners[3]);

        // 5. Scale the canvas so its rect fits exactly that physical size
        Vector2 canvasSize = canvasTransform.sizeDelta;
        float scaleX = widthMeters / canvasSize.x;
        float scaleY = heightMeters / canvasSize.y;
        canvasTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        // 6. Position the canvas in front of the camera
        canvasTransform.position = camPose.position + camPose.rotation * Vector3.forward * distanceFromCamera;
        canvasTransform.rotation = camPose.rotation;

        Debug.Log($"[CameraCanvasAlignerProper] Canvas aligned using intrinsics: {widthMeters:F3}m × {heightMeters:F3}m plane at {distanceFromCamera:F1}m");
    }
}
