using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maxSpeed = 7f;
    public float groundAccel = 90f;
    public float airAccel = 20f;
    public float groundFriction = 60f;
    public float airFriction = 2f;

    public float jumpSpeed = 6.5f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;
    public float groundCheckDistance = 0.15f;

    public float maxStepHeight = 0.35f;
    public LayerMask groundMask = ~0;

    public Transform lookTransform;

    public bool IsGrounded { get; private set; }
    public Vector3 Velocity => rb.linearVelocity;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private InputSystem_Actions actions;
    private Vector2 moveInput;
    private float timeSinceGrounded;
    private float timeSinceJumpPressed = Mathf.Infinity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
        rb.useGravity = true;

        actions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        actions.Player.Enable();
    }

    private void OnDisable()
    {
        actions.Player.Disable();
    }

    private void OnDestroy()
    {
        actions.Dispose();
    }

    private void Update()
    {
        moveInput = actions.Player.Move.ReadValue<Vector2>();

        if (actions.Player.Jump.WasPressedThisFrame())
            timeSinceJumpPressed = 0f;
        else
            timeSinceJumpPressed += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        IsGrounded = CheckGrounded();
        timeSinceGrounded = IsGrounded ? 0f : timeSinceGrounded + dt;

        TryJump();
        ApplyMovement(dt);
        ApplyStepUp();
        ApplyFriction(dt);
    }

    private bool CheckGrounded()
    {
        if (rb.linearVelocity.y > 0.1f) return false;

        float radius = capsule.radius * 0.9f;
        Vector3 origin = transform.TransformPoint(capsule.center) + Vector3.down * (capsule.height * 0.5f - capsule.radius);
        return Physics.SphereCast(origin, radius, Vector3.down, out _, groundCheckDistance + capsule.radius * 0.1f, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void TryJump()
    {
        bool canJump = IsGrounded || timeSinceGrounded < coyoteTime;
        if (!canJump || timeSinceJumpPressed > jumpBufferTime) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpSpeed, rb.linearVelocity.z);
        timeSinceJumpPressed = Mathf.Infinity;
        timeSinceGrounded = coyoteTime;
        IsGrounded = false;
    }

    private void ApplyMovement(float dt)
    {
        if (moveInput == Vector2.zero) return;

        Vector3 forward = lookTransform != null ? lookTransform.forward : transform.forward;
        Vector3 right = lookTransform != null ? lookTransform.right : transform.right;

        Vector3 wishDir = forward * moveInput.y + right * moveInput.x;
        wishDir.y = 0f;
        wishDir.Normalize();

        float accel = IsGrounded ? groundAccel : airAccel;
        Vector3 add = wishDir * accel * dt;

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flat.magnitude >= maxSpeed && Vector3.Dot(add, flat) > 0f)
            add -= Vector3.Project(add, flat);

        Vector3 result = flat + add;
        if (result.magnitude > maxSpeed && result.magnitude > flat.magnitude)
            result = result.normalized * Mathf.Max(maxSpeed, flat.magnitude);

        rb.linearVelocity = new Vector3(result.x, rb.linearVelocity.y, result.z);
    }

    private void ApplyStepUp()
    {
        if (!IsGrounded || moveInput == Vector2.zero) return;

        Vector3 moveDir = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (moveDir.sqrMagnitude < 0.01f) return;
        moveDir.Normalize();

        Vector3 bottom = transform.TransformPoint(capsule.center) + Vector3.down * (capsule.height * 0.5f - 0.02f);
        Vector3 lowOrigin = bottom + Vector3.up * 0.05f;
        Vector3 highOrigin = bottom + Vector3.up * maxStepHeight;
        float probe = capsule.radius + 0.15f;

        bool blockedLow = Physics.Raycast(lowOrigin, moveDir, probe, groundMask, QueryTriggerInteraction.Ignore);
        bool blockedHigh = Physics.Raycast(highOrigin, moveDir, probe, groundMask, QueryTriggerInteraction.Ignore);
        if (!blockedLow || blockedHigh) return;

        Vector3 topOrigin = highOrigin + moveDir * probe;
        if (!Physics.Raycast(topOrigin, Vector3.down, out RaycastHit stepHit, maxStepHeight, groundMask, QueryTriggerInteraction.Ignore)) return;
        if (Vector3.Dot(stepHit.normal, Vector3.up) < 0.7f) return;

        rb.MovePosition(rb.position + Vector3.up * (maxStepHeight - stepHit.distance));
    }

    private void ApplyFriction(float dt)
    {
        if (moveInput != Vector2.zero) return;

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flat.magnitude < 0.05f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        float friction = IsGrounded ? groundFriction : airFriction;
        Vector3 slowed = flat - flat.normalized * friction * dt;
        if (Vector3.Dot(slowed, flat) < 0f) slowed = Vector3.zero;

        rb.linearVelocity = new Vector3(slowed.x, rb.linearVelocity.y, slowed.z);
    }
}
