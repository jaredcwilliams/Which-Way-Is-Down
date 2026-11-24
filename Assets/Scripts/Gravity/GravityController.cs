using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GravityController : MonoBehaviour
{
    [Header("Room Rotation")]
    public float rotationAmount = 90f;
    public float rotationDuration = 0.6f;

    private bool isRotating = false;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Temporary test key.
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            RotateRoom();
        }
    }

    public void RotateRoom()
    {
        if (isRotating)
        {
            return;
        }

        StartCoroutine(RotateRoomSmoothly());
    }

    private IEnumerator RotateRoomSmoothly()
    {
        isRotating = true;

        Quaternion startingRotation = transform.rotation;

        Quaternion targetRotation =
            startingRotation * Quaternion.Euler(
                0f,
                0f,
                rotationAmount
            );

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                elapsedTime / rotationDuration;

            progress = Mathf.SmoothStep(0f, 1f, progress);

            transform.rotation = Quaternion.Slerp(
                startingRotation,
                targetRotation,
                progress
            );

            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }
}