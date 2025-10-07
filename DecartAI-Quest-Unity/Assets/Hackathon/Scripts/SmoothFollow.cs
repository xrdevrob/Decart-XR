using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [Tooltip("The target to follow.")]
    [SerializeField] private Transform target;

    [Tooltip("How quickly this object moves toward the target.")]
    [SerializeField] private float followSpeed = 5f;

    [Tooltip("If true, follows rotation as well.")]
    [SerializeField] private bool followRotation = true;

    private void LateUpdate()
    {
        if (!target)
        {
            return;
        }

        // Smooth position
        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * followSpeed
        );

        // Smooth rotation (optional)
        if (followRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                Time.deltaTime * followSpeed
            );
        }
    }
}