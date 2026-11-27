using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    public Transform cameraTransform;
    public Animator animator;
    public AnimationClip jumpingClip;
    public AnimationClip impactClip;
    public AnimationClip gettingUpClip;
    [Tooltip("Small gap between the lowest animated body point and the contact surface.")]
    public float recoveryGroundClearance = 0f;

    private CharacterController controller;
    private float verticalSpeed;
    private float stateEndsAt;
    private bool gravitySwitchPending;
    private float airborneSince;
    private float fallStartHeight;
    private int landingFrames;
    private Vector3 visualRestPosition;
    private SkinnedMeshRenderer[] recoveryRenderers;
    private Mesh[] recoveryMeshes;
    private readonly List<Vector3> bakedVertices = new List<Vector3>();
    private readonly RaycastHit[] groundHits = new RaycastHit[16];
    private bool alignRecovery;
    private Phase phase;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private const float TransitionSeconds = 0.08f;
    private enum Phase { Locomotion, Jumping, Falling, Impact, GettingUp }
    public bool CanSwitchGravity => phase != Phase.Impact && phase != Phase.GettingUp;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        if (animator == null || !animator.gameObject.activeInHierarchy)
            animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
        if (animator != null) visualRestPosition = animator.transform.localPosition;
        if (animator != null)
        {
            recoveryRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>();
            recoveryMeshes = new Mesh[recoveryRenderers.Length];
            for (int i = 0; i < recoveryMeshes.Length; i++)
                recoveryMeshes[i] = new Mesh();
        }
    }

    private void OnDestroy()
    {
        if (recoveryMeshes == null) return;
        foreach (Mesh mesh in recoveryMeshes) Destroy(mesh);
    }

    public void BeginGravitySwitch()
    {
        gravitySwitchPending = true;
        airborneSince = -1f;
        landingFrames = 0;
        verticalSpeed = 0f;
        phase = Phase.Locomotion;
        alignRecovery = false;
        SetSpeed(0f);
        Play("Locomotion");
    }

    private void Update()
    {
        if (!controller.enabled) return;
        if (phase == Phase.Impact && Time.time >= stateEndsAt)
        {
            phase = Phase.GettingUp;
            stateEndsAt = Time.time + Duration(gettingUpClip) + TransitionSeconds;
            Play("Getting Up");
        }
        else if (phase == Phase.GettingUp && Time.time >= stateEndsAt)
        {
            phase = Phase.Locomotion;
            Play("Locomotion");
        }

        bool recovering = phase == Phase.Impact || phase == Phase.GettingUp;
        Vector2 input = Vector2.zero;
        if (!recovering && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
        }
        input = Vector2.ClampMagnitude(input, 1f);
        SetSpeed(recovering ? 0f : input.magnitude);
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        Vector3 moveDirection = forward * input.y + right * input.x;
        if (!recovering && moveDirection.sqrMagnitude > 0.001f) transform.forward = moveDirection;

        bool groundedBefore = controller.isGrounded;
        if (groundedBefore && verticalSpeed < 0f) verticalSpeed = -2f;
        bool jumped = !recovering && !gravitySwitchPending && groundedBefore &&
            Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (jumped)
        {
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            phase = Phase.Jumping;
            stateEndsAt = Time.time + Duration(jumpingClip);
            Play("Jumping Up");
        }
        verticalSpeed += gravity * Time.deltaTime;
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = verticalSpeed;
        CollisionFlags collisions = controller.Move(velocity * Time.deltaTime);
        bool groundedNow = (collisions & CollisionFlags.Below) != 0 || controller.isGrounded;

        if (gravitySwitchPending && !groundedNow && phase != Phase.Falling)
        {
            phase = Phase.Falling;
            airborneSince = Time.time;
            fallStartHeight = transform.position.y;
            landingFrames = 0;
            Play("Falling");
        }
        // The switch did not cause a fall. A later walk or jump off this platform
        // is ordinary movement, not a delayed gravity-switch impact.
        if (gravitySwitchPending && groundedNow && phase == Phase.Locomotion)
            gravitySwitchPending = false;
        if (gravitySwitchPending && phase == Phase.Falling && groundedNow &&
            airborneSince >= 0f && Time.time - airborneSince >= 0.12f && verticalSpeed < 0f)
            landingFrames++;
        else
            landingFrames = 0;
        if (gravitySwitchPending && phase == Phase.Falling && landingFrames >= 2)
        {
            gravitySwitchPending = false;
            verticalSpeed = -2f;
            // A momentary loss of grounding after re-enabling the controller is
            // not an impact when the player stayed on the same platform.
            if (fallStartHeight - transform.position.y < 0.25f)
            {
                phase = Phase.Locomotion;
                Play("Locomotion");
            }
            else
            {
                phase = Phase.Impact;
                alignRecovery = true;
                stateEndsAt = Time.time + Duration(impactClip) + TransitionSeconds;
                SetSpeed(0f);
                Play("Falling Flat Impact");
            }
        }
        else if (phase == Phase.Jumping && groundedNow && Time.time >= stateEndsAt)
        {
            phase = Phase.Locomotion;
            Play("Locomotion");
        }
    }

    private static float Duration(AnimationClip clip) => clip != null ? clip.length : 0.5f;
    private void LateUpdate()
    {
        if (animator == null) return;
        if (alignRecovery && phase == Phase.Locomotion &&
            !animator.IsInTransition(0) &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash == LocomotionHash)
            alignRecovery = false;
        if (!alignRecovery)
        {
            animator.transform.localPosition = visualRestPosition;
            return;
        }
        if (recoveryRenderers == null) return;
        float lowest = float.PositiveInfinity;
        for (int i = 0; i < recoveryRenderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = recoveryRenderers[i];
            if (renderer == null || !renderer.enabled) continue;
            renderer.BakeMesh(recoveryMeshes[i]);
            bakedVertices.Clear();
            recoveryMeshes[i].GetVertices(bakedVertices);
            Matrix4x4 toWorld = renderer.localToWorldMatrix;
            foreach (Vector3 vertex in bakedVertices)
                lowest = Mathf.Min(lowest, toWorld.MultiplyPoint3x4(vertex).y);
        }
        if (float.IsPositiveInfinity(lowest)) return;
        float groundY = transform.position.y + controller.center.y - controller.height * 0.5f;
        int hitCount = Physics.RaycastNonAlloc(transform.position + Vector3.up * 0.25f,
            Vector3.down, groundHits, controller.height + 0.75f, ~0, QueryTriggerInteraction.Ignore);
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = groundHits[i];
            if (hit.collider == controller || hit.normal.y < 0.5f || hit.distance >= nearest) continue;
            nearest = hit.distance;
            groundY = hit.point.y;
        }
        Vector3 position = animator.transform.position;
        position.y += groundY + recoveryGroundClearance - lowest;
        animator.transform.position = position;
    }
    private void SetSpeed(float value) { if (animator != null) animator.SetFloat(SpeedHash, value); }
    private void Play(string state) { if (animator != null && animator.isActiveAndEnabled) animator.CrossFadeInFixedTime(state, TransitionSeconds); }
}
