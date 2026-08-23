using UnityEngine;

public class GunSway : MonoBehaviour
{
    public PlayerMovement player;
    public Transform cameraTransform;

    public float inertiaSensitivity = 0.05f;
    public float inertiaSpringStrength = 250f;
    public float inertiaSpringDamping = 150f;

    public float bobFrequency = 1f;
    public float bobAmountX = 0.018f;
    public float bobAmountY = 0.012f;
    public float bobSmoothing = 8f;

    public float maxTiltDegrees = 8f;
    public float tiltSmoothing = 6f;

    public float idleFrequency = 1.1f;
    public float idleAmount = 0.006f;
    public float idleBlendSpeed = 2f;

    public float jumpKickImpulse = 0.3f;
    public float landingKickImpulse = 0.3f;
    public float landingPitchImpulse = 2f;
    public float kickSpringStrength = 180f;
    public float kickDamping = 18f;

    private Vector3 baseLocalPos;
    private Quaternion baseLocalRot;
    private Vector3 prevCamEuler;
    private Vector2 inertiaOffset;
    private Vector2 inertiaVel;
    private float bobTimer;
    private Vector3 bobOffset;
    private float tiltZ;
    private float idleWeight;
    private float prevYVelocity;
    private float kickPos;
    private float kickVel;
    private float pitchPos;
    private float pitchVel;

    private void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;
        prevCamEuler = cameraTransform.eulerAngles;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        Vector3 velocity = player.Velocity;

        float yawDelta = Mathf.DeltaAngle(prevCamEuler.y, cameraTransform.eulerAngles.y);
        float pitchDelta = Mathf.DeltaAngle(prevCamEuler.x, cameraTransform.eulerAngles.x);
        prevCamEuler = cameraTransform.eulerAngles;

        inertiaVel += new Vector2(-yawDelta, pitchDelta) * inertiaSensitivity;
        float inertiaOmega = Mathf.Sqrt(Mathf.Max(inertiaSpringStrength, 0.001f));
        float inertiaZeta = inertiaSpringDamping / (2f * inertiaOmega);
        SpringUtil.Damp(ref inertiaOffset.x, ref inertiaVel.x, inertiaOmega, inertiaZeta, dt);
        SpringUtil.Damp(ref inertiaOffset.y, ref inertiaVel.y, inertiaOmega, inertiaZeta, dt);

        float horizSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        bool moving = horizSpeed > 0.3f && player.IsGrounded;
        idleWeight = Mathf.Lerp(idleWeight, moving ? 0f : 1f, 1f - Mathf.Exp(-idleBlendSpeed * dt));

        if (moving) bobTimer += horizSpeed * bobFrequency * dt;
        Vector3 targetBob = moving
            ? new Vector3(Mathf.Sin(bobTimer) * bobAmountX, Mathf.Sin(bobTimer * 2f) * bobAmountY, 0f)
            : Vector3.zero;
        bobOffset = Vector3.Lerp(bobOffset, targetBob, 1f - Mathf.Exp(-bobSmoothing * dt));

        float lateralVel = Vector3.Dot(velocity, cameraTransform.right) / Mathf.Max(player.maxSpeed, 0.001f);
        float targetTilt = -Mathf.Clamp(lateralVel, -1f, 1f) * maxTiltDegrees;
        tiltZ = Mathf.Lerp(tiltZ, targetTilt, 1f - Mathf.Exp(-tiltSmoothing * dt));

        float yVel = velocity.y;
        float yVelDelta = yVel - prevYVelocity;
        if (yVelDelta > 3f)
        {
            if (prevYVelocity < -1f)
            {
                float intensity = Mathf.Min(Mathf.Abs(prevYVelocity), 20f);
                kickVel -= intensity * landingKickImpulse;
                pitchVel -= intensity * landingPitchImpulse;
            }
            else
            {
                kickVel -= jumpKickImpulse;
            }
        }
        prevYVelocity = yVel;

        float kickOmega = Mathf.Sqrt(Mathf.Max(kickSpringStrength, 0.001f));
        float kickZeta = kickDamping / (2f * kickOmega);
        SpringUtil.Damp(ref kickPos, ref kickVel, kickOmega, kickZeta, dt);
        SpringUtil.Damp(ref pitchPos, ref pitchVel, kickOmega, kickZeta, dt);

        float idleX = Mathf.Sin(Time.time * idleFrequency) * idleAmount * idleWeight;
        float idleY = Mathf.Sin(Time.time * idleFrequency * 0.7f) * idleAmount * idleWeight;

        transform.localPosition = baseLocalPos + new Vector3(
            inertiaOffset.x + bobOffset.x + idleX,
            inertiaOffset.y + bobOffset.y + idleY + kickPos,
            0f);
        transform.localRotation = baseLocalRot * Quaternion.Euler(pitchPos, 0f, tiltZ);
    }
}
