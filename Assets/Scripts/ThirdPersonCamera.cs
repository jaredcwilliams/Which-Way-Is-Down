using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 1f;
    public float mouseSensitivity = 0.2f;

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

        // Hold right mouse button to orbit around the player.
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseMovement = Mouse.current.delta.ReadValue();
            yaw += mouseMovement.x * mouseSensitivity;
            pitch -= mouseMovement.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -20f, 75f);
        }

        Vector3 focusPoint = target.position + Vector3.up * height;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        transform.position = focusPoint + rotation * new Vector3(0f, 0f, -distance);
        transform.LookAt(focusPoint);
    }
}