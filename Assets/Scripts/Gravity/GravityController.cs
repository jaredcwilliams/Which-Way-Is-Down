using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GravityController : MonoBehaviour
{
    [Header("Room Rotation")]
    public float rotationAmount = 90f;
    public float rotationDuration = 0.6f;

    [Header("Player")]
    public Transform player;

    private bool isRotating;

    private void Update()
    {
        // Temporary test key, as in the existing prototype.
        if (Keyboard.current != null &&
            Keyboard.current.gKey.wasPressedThisFrame)
        {
            RotateRoom();
        }
    }

    public void RotateRoom()
    {
        if (!isRotating && player != null)
        {
            StartCoroutine(RotateRoomSmoothly());
        }
    }

    private IEnumerator RotateRoomSmoothly()
    {
        isRotating = true;

        CharacterController characterController =
            player.GetComponent<CharacterController>();
        PlayerController playerController =
            player.GetComponent<PlayerController>();

        bool movementWasEnabled =
            playerController != null && playerController.enabled;
        bool colliderWasEnabled =
            characterController != null && characterController.enabled;

        // Pause player movement while we carry them through the rotation.
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        Quaternion startingRotation = transform.rotation;
        Quaternion targetRotation =
            startingRotation * Quaternion.Euler(0f, 0f, rotationAmount);

        Vector3 startingPlayerPosition = player.position;
        Vector3 offsetFromPivot =
            startingPlayerPosition - transform.position;

        float elapsedTime = 0f;
        float duration = Mathf.Max(0.01f, rotationDuration);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            ApplyRotation(
                Quaternion.Slerp(
                    startingRotation,
                    targetRotation,
                    progress
                ),
                startingRotation,
                offsetFromPivot
            );

            yield return null;
        }

        ApplyRotation(
            targetRotation,
            startingRotation,
            offsetFromPivot
        );

        // Let normal movement and falling resume.
        if (characterController != null)
        {
            characterController.enabled = colliderWasEnabled;
        }

        if (playerController != null)
        {
            playerController.enabled = movementWasEnabled;
        }

        isRotating = false;
    }

    private void ApplyRotation(
        Quaternion roomRotation,
        Quaternion startingRotation,
        Vector3 startingPlayerOffset
    )
    {
        transform.rotation = roomRotation;

        Quaternion change =
            roomRotation * Quaternion.Inverse(startingRotation);

        player.position =
            transform.position + change * startingPlayerOffset;
    }
}