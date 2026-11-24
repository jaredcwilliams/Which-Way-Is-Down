using UnityEngine;
using UnityEngine.InputSystem;

public class GravitySwitch : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GravityController roomController;

    [Header("Interaction")]
    public float interactionRange = 2.5f;

    private void Update()
    {
        if (player == null || roomController == null)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionRange &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            roomController.RotateRoom();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}