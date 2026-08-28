using UnityEngine;
using UnityEngine.AI;

public class BlockRig : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform hips;
    [SerializeField] Transform torso;
    [SerializeField] Transform head;
    [SerializeField] Transform armLeft;
    [SerializeField] Transform armRight;
    [SerializeField] Transform legLeft;
    [SerializeField] Transform legRight;

    [SerializeField] float stridePerMeter = 2.6f;
    [SerializeField] float walkSpeedReference = 2.0f;
    [SerializeField] float legSwing = 40.0f;
    [SerializeField] float armSwing = 26.0f;
    [SerializeField] float armLeftRestAngle;
    [SerializeField] float armRightRestAngle;
    [SerializeField, Range(0.0f, 1.0f)] float armLeftSwingScale = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)] float armRightSwingScale = 1.0f;
    [SerializeField] float heldArmDrift = 2.5f;
    [SerializeField] float armFlare = 7.0f;
    [SerializeField] float bobHeight = 0.05f;
    [SerializeField] float leanPerSpeed = 4.0f;
    [SerializeField] float maxLean = 11.0f;
    [SerializeField] float breathAmplitude = 0.014f;
    [SerializeField] float breathSpeed = 1.7f;
    [SerializeField] float swayAngle = 2.2f;
    [SerializeField] float swaySpeed = 0.8f;
    [SerializeField] float headTurnMax = 55.0f;
    [SerializeField] float smoothing = 12.0f;

    [System.NonSerialized] public float RightArmOverride;
    [System.NonSerialized] public Quaternion RightArmOverrideRotation = Quaternion.identity;

    Vector3 hipsRestPosition;
    float phase;
    float idlePhase;
    float speed;

    void Reset()
    {
        agent = GetComponentInParent<NavMeshAgent>();
    }

    void Start()
    {
        if (hips) hipsRestPosition = hips.localPosition;
        phase = Random.value * Mathf.PI * 2.0f;
        idlePhase = Random.value * Mathf.PI * 2.0f;
    }

    void LateUpdate()
    {
        float measured = agent ? agent.velocity.magnitude : 0.0f;
        speed = Mathf.Lerp(speed, measured, smoothing * Time.deltaTime);

        float walk = Mathf.Clamp01(speed / Mathf.Max(0.01f, walkSpeedReference));
        phase += speed * stridePerMeter * Time.deltaTime;
        idlePhase += Time.deltaTime;

        float swing = Mathf.Sin(phase);
        float breath = Mathf.Sin(idlePhase * breathSpeed) * breathAmplitude * (1.0f - walk);
        float sway = Mathf.Sin(idlePhase * swaySpeed) * swayAngle * (1.0f - walk);

        if (hips)
        {
            float bob = Mathf.Abs(Mathf.Cos(phase)) * bobHeight * walk;
            hips.localPosition = hipsRestPosition + Vector3.up * (bob + breath);
        }

        if (torso)
        {
            float lean = Mathf.Min(speed * leanPerSpeed, maxLean);
            torso.localRotation = Quaternion.Euler(lean, sway * 0.5f, -swing * 3.0f * walk + sway);
        }

        if (legLeft) legLeft.localRotation = Quaternion.Euler(swing * legSwing * walk, 0.0f, 0.0f);
        if (legRight) legRight.localRotation = Quaternion.Euler(-swing * legSwing * walk, 0.0f, 0.0f);

        if (armLeft) armLeft.localRotation = ArmRotation(armLeftRestAngle, -swing, armLeftSwingScale, walk, sway, -1.0f);
        if (armRight)
        {
            Quaternion walkPose = ArmRotation(armRightRestAngle, swing, armRightSwingScale, walk, sway, 1.0f);
            float overrideWeight = Mathf.Clamp01(RightArmOverride);
            armRight.localRotation = overrideWeight > 0.001f
                ? Quaternion.Slerp(walkPose, RightArmOverrideRotation, overrideWeight)
                : walkPose;
        }

        UpdateHead();
    }

    Quaternion ArmRotation(float restAngle, float swing, float swingScale, float walk, float sway, float side)
    {
        float held = 1.0f - swingScale;
        float pitch = restAngle + swing * armSwing * walk * swingScale + Mathf.Sin(idlePhase * breathSpeed) * heldArmDrift * held;
        return Quaternion.Euler(pitch, sway * held, armFlare * side * swingScale);
    }

    void UpdateHead()
    {
        if (!head) return;

        float yaw = 0.0f;
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (player)
        {
            Transform aim = player.Center ? player.Center : player.transform;
            Vector3 local = transform.InverseTransformPoint(aim.position);
            yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -headTurnMax, headTurnMax);
        }

        head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(0.0f, yaw, 0.0f), smoothing * Time.deltaTime);
    }
}
