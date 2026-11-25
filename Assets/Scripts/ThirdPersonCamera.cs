using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 1f;
    public float mouseSensitivity = 0.2f;

    [Header("Wall avoidance")]
    public float cameraRadius = 0.3f;
    public float wallPadding = 0.15f;

    private float yaw;
    private float pitch = 15f;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseMovement =
                Mouse.current.delta.ReadValue();

            yaw += mouseMovement.x * mouseSensitivity;
            pitch -= mouseMovement.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -20f, 75f);
        }

        Vector3 focusPoint =
            target.position + Vector3.up * height;

        Quaternion rotation =
            Quaternion.Euler(pitch, yaw, 0f);

        Vector3 backwards =
            rotation * Vector3.back;

        float allowedDistance = distance;

        // The room's colliders move during rotation. Update their
        // physics positions before checking for a wall.
        Physics.SyncTransforms();

        RaycastHit[] hits = Physics.SphereCastAll(
            focusPoint,
            cameraRadius,
            backwards,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            // The player's own collider must not block the camera.
            if (hit.collider.transform == target ||
                hit.collider.transform.IsChildOf(target))
            {
                continue;
            }

            allowedDistance = Mathf.Min(
                allowedDistance,
                Mathf.Max(
                    0.5f,
                    hit.distance - wallPadding
                )
            );
        }

        transform.position =
            focusPoint + backwards * allowedDistance;

        transform.LookAt(focusPoint);
    }
}