using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector2 offset = new Vector2(0f, 0f);

    [Header("Smooth Settings")]
    [Tooltip("Lower = snappier, Higher = more floaty")]
    public float smoothTime = 0.25f;

    [Header("X Limits")]
    public float minX;
    public float maxX;

    Vector3 velocity; // required by SmoothDamp

    float lockedY;

    void Start()
    {
        // Lock the Y position at start
        lockedY = transform.position.y;
       // transform.position = new Vector3(minX,3,-10);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Desired X position
        float desiredX = target.position.x + offset.x;

        // Clamp X within range
        desiredX = Mathf.Clamp(desiredX, minX, maxX);

        Vector3 desiredPosition = new Vector3(
            desiredX,
            lockedY,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }
}
