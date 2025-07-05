using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The player transform to follow
    public Vector3 offset; // Offset position from the player
    public float smoothSpeed = 0.125f; // Smooth speed for camera movement

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}

