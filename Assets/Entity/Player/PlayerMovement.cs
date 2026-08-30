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

    public float maxStepHeight = 0.55f;
    public float minStepHeight = 0.04f;
    public float stepProbeDistance = 0.22f;
    public float stepSkin = 0.03f;
    public float stepCoyoteTime = 0.2f;
    [Range(0f, 1f)] public float walkableNormal = 0.7f;
    public LayerMask groundMask = ~0;

    public float stepStride = 2.1f;
    public float minLandSpeed = 3.6f;
    public float hardLandSpeed = 9f;

    public Transform lookTransform;

    public bool IsGrounded { get; private set; }
    public Vector3 Velocity => rb.linearVelocity;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private InputSystem_Actions actions;
    private Vector2 moveInput;
    private float timeSinceGrounded;
    private float timeSinceJumpPressed = Mathf.Infinity;
    private float stepDistance;
    private bool wasGrounded;
    private float peakFallSpeed;

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
        TickFootsteps(dt);
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
        AudioManager.PlayEventOn(SfxEvent.Jump, transform);
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
        if (!IsGrounded && timeSinceGrounded > stepCoyoteTime) return;

        Vector3 wish = WishDirection();
        if (wish == Vector3.zero) return;

        if (!IsBlocked(wish)) return;

        float rise = FindStep(wish);
        if (rise <= minStepHeight) return;

        Vector3 target = rb.position + Vector3.up * (rise + stepSkin);
        if (!Fits(target)) return;

        rb.position = target;
        transform.position = target;

        Vector3 velocity = rb.linearVelocity;
        if (velocity.y < 0f) velocity.y = 0f;
        rb.linearVelocity = velocity;
    }

    private Vector3 WishDirection()
    {
        Vector3 forward = lookTransform != null ? lookTransform.forward : transform.forward;
        Vector3 right = lookTransform != null ? lookTransform.right : transform.right;

        Vector3 wish = forward * moveInput.y + right * moveInput.x;
        wish.y = 0f;

        if (wish.sqrMagnitude > 0.0001f) return wish.normalized;

        Vector3 velocity = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        return velocity.sqrMagnitude > 0.04f ? velocity.normalized : Vector3.zero;
    }

    private bool IsBlocked(Vector3 wish)
    {
        Vector3 foot = FootSphere(rb.position);
        float radius = capsule.radius * 0.95f;

        if (!Physics.SphereCast(foot, radius, wish, out RaycastHit hit, stepProbeDistance, StepMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return Vector3.Dot(hit.normal, Vector3.up) < walkableNormal;
    }

    private float FindStep(Vector3 wish)
    {
        const int samples = 3;

        Vector3 side = Vector3.Cross(Vector3.up, wish);
        Vector3 foot = FootSphere(rb.position);
        float footY = FootHeight();
        float best = 0f;

        for (int lane = -1; lane <= 1; ++lane)
        {
            Vector3 lateral = side * (lane * capsule.radius * 0.55f);

            for (int sample = 0; sample < samples; ++sample)
            {
                float forward = capsule.radius + Mathf.Lerp(0.04f, stepProbeDistance, sample / (samples - 1f));
                Vector3 origin = foot + lateral + wish * forward;
                origin.y = footY + maxStepHeight + stepSkin;

                if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxStepHeight + stepSkin, StepMask, QueryTriggerInteraction.Ignore)) continue;
                if (Vector3.Dot(hit.normal, Vector3.up) < walkableNormal) continue;

                float rise = hit.point.y - footY;
                if (rise > minStepHeight) best = Mathf.Max(best, rise);
                break;
            }
        }

        return Mathf.Min(best, maxStepHeight);
    }

    private float FootHeight()
    {
        return rb.position.y + transform.TransformVector(capsule.center).y - capsule.height * 0.5f;
    }

    private bool Fits(Vector3 bodyPosition)
    {
        Vector3 center = bodyPosition + transform.TransformVector(capsule.center);
        float half = Mathf.Max(0f, capsule.height * 0.5f - capsule.radius);

        return !Physics.CheckCapsule(
            center + Vector3.down * half,
            center + Vector3.up * half,
            capsule.radius * 0.95f,
            StepMask,
            QueryTriggerInteraction.Ignore);
    }

    private Vector3 FootSphere(Vector3 bodyPosition)
    {
        Vector3 center = bodyPosition + transform.TransformVector(capsule.center);
        return center + Vector3.down * (capsule.height * 0.5f - capsule.radius);
    }

    private int StepMask => groundMask & ~(1 << gameObject.layer);

    private void TickFootsteps(float dt)
    {
        if (!IsGrounded) peakFallSpeed = Mathf.Max(peakFallSpeed, -rb.linearVelocity.y);

        if (IsGrounded && !wasGrounded)
        {
            if (peakFallSpeed >= minLandSpeed)
            {
                float weight = Mathf.InverseLerp(minLandSpeed, hardLandSpeed * 1.6f, peakFallSpeed);
                var landing = peakFallSpeed > hardLandSpeed ? SfxEvent.LandHard : SfxEvent.LandSoft;
                AudioManager.PlayEventOn(landing, transform, Mathf.Lerp(0.3f, 1f, weight));
            }

            peakFallSpeed = 0f;
            stepDistance = 0f;
        }
        wasGrounded = IsGrounded;

        if (!IsGrounded) return;

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flat.magnitude < 0.6f)
        {
            stepDistance = 0f;
            return;
        }

        stepDistance += flat.magnitude * dt;
        if (stepDistance < stepStride) return;

        stepDistance = 0f;
        AudioManager.PlayEventOn(OnMetal() ? SfxEvent.StepMetal : SfxEvent.StepConcrete, transform);
    }

    private bool OnMetal()
    {
        Vector3 origin = transform.TransformPoint(capsule.center);
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, capsule.height, groundMask, QueryTriggerInteraction.Ignore)) return false;
        if (!hit.collider.TryGetComponent(out Renderer surface) || !surface.sharedMaterial) return false;

        string material = surface.sharedMaterial.name;
        return material.Contains("Grate") || material.Contains("Metal");
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
