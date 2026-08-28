using UnityEngine;

public class EnemyBat : MonoBehaviour
{
    enum Phase { Idle, WindUp, Swing, Recover }

    [SerializeField] BlockRig rig;
    [SerializeField] TrailRenderer swingTrail;
    [SerializeField] Vector3 readyAngles = new(-12.0f, 0.0f, 0.0f);
    [SerializeField] Vector3 windUpAngles = new(-155.0f, 0.0f, 0.0f);
    [SerializeField] Vector3 swingAngles = new(-40.0f, 0.0f, 0.0f);
    [SerializeField] AnimationCurve windUpEase = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField] AnimationCurve swingEase = new(new Keyframe(0.0f, 0.0f), new Keyframe(0.45f, 0.85f), new Keyframe(1.0f, 1.0f));
    [SerializeField] float blendSpeed = 16.0f;

    Phase phase = Phase.Idle;
    float phaseStart;
    float phaseDuration;
    float weight;

    void Awake()
    {
        if (swingTrail) swingTrail.emitting = false;
    }

    public void WindUp(float duration)
    {
        Begin(Phase.WindUp, duration);
    }

    public void Swing(float duration)
    {
        Begin(Phase.Swing, duration);
        if (swingTrail) swingTrail.emitting = true;
    }

    public void Recover(float duration)
    {
        Begin(Phase.Recover, duration);
        if (swingTrail) swingTrail.emitting = false;
    }

    void Begin(Phase next, float duration)
    {
        phase = next;
        phaseStart = Time.time;
        phaseDuration = Mathf.Max(0.0001f, duration);
    }

    void Update()
    {
        if (!rig) return;

        float life = Mathf.Clamp01((Time.time - phaseStart) / phaseDuration);
        Quaternion pose;
        float target;

        switch (phase)
        {
            case Phase.WindUp:
                pose = Quaternion.Slerp(Quaternion.Euler(readyAngles), Quaternion.Euler(windUpAngles), windUpEase.Evaluate(life));
                target = 1.0f;
                break;
            case Phase.Swing:
                pose = Quaternion.Slerp(Quaternion.Euler(windUpAngles), Quaternion.Euler(swingAngles), swingEase.Evaluate(life));
                target = 1.0f;
                break;
            case Phase.Recover:
                pose = Quaternion.Slerp(Quaternion.Euler(swingAngles), Quaternion.Euler(readyAngles), life);
                target = 1.0f - life;
                break;
            default:
                pose = Quaternion.Euler(readyAngles);
                target = 0.0f;
                break;
        }

        weight = Mathf.MoveTowards(weight, target, blendSpeed * Time.deltaTime);
        rig.RightArmOverride = weight;
        rig.RightArmOverrideRotation = pose;
    }
}
