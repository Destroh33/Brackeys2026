using UnityEngine;

public class ImFreeBulletMovement : MonoBehaviour
{
    [SerializeField] Bullet bullet;
    [SerializeField] LayerMask wallMask = 1;
    [SerializeField] float speed = 15.0f;
    [SerializeField] float freeDuration = 2.5f;
    [SerializeField] float lifetime = 7.0f;
    [SerializeField] float immuneDuration = 5.0f;
    [SerializeField] float bounceClearance = 0.25f;
    [SerializeField] float minSegmentDuration = 0.6f;
    [SerializeField] float maxSegmentDuration = 1.4f;
    [SerializeField] float minTurnSpeed = 220.0f;
    [SerializeField] float maxTurnSpeed = 420.0f;
    [SerializeField] float avoidTurnSpeed = 540.0f;
    [SerializeField] float avoidStrength = 1.5f;
    [SerializeField] float avoidSmoothing = 6.0f;
    [SerializeField] float probeDistance = 4.0f;
    [SerializeField] float probeRadius = 0.3f;
    [SerializeField] float whiskerAngle = 40.0f;

    float spawnFixedTime;
    float segmentEndFixedTime;
    Vector3 turnAxis;
    float turnSpeed;
    Vector3 smoothedAvoid;
    Vector3[] probeDirections;
    bool immune;

    void Reset()
    {
        TryGetComponent(out bullet);
    }

    void OnDestroy()
    {
        MusicManager.Pop(this);
    }

    void Start()
    {
        MusicManager.Push(this, Sfx.MusicFreedom);

        probeDirections = new Vector3[]
        {
            Vector3.forward,
            Quaternion.AngleAxis(whiskerAngle, Vector3.up) * Vector3.forward,
            Quaternion.AngleAxis(-whiskerAngle, Vector3.up) * Vector3.forward,
            Quaternion.AngleAxis(whiskerAngle, Vector3.right) * Vector3.forward,
            Quaternion.AngleAxis(-whiskerAngle, Vector3.right) * Vector3.forward,
        };

        spawnFixedTime = Time.fixedTime;
        bullet.RB.linearVelocity = transform.forward * speed;

        SetImmune(true);

        NextSegment();
    }

    void FixedUpdate()
    {
        if (Time.fixedTime > spawnFixedTime + lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (immune && Time.fixedTime > spawnFixedTime + immuneDuration) SetImmune(false);

        Quaternion rotation = bullet.RB.rotation;
        Vector3 desired = rotation * Vector3.forward;

        if (Time.fixedTime < spawnFixedTime + freeDuration)
        {
            if (Time.fixedTime > segmentEndFixedTime)
            {
                NextSegment();
            }
            desired = Quaternion.AngleAxis(turnSpeed * Time.fixedDeltaTime, rotation * turnAxis) * desired;
        }

        smoothedAvoid = Vector3.Lerp(smoothedAvoid, GetAvoidance(rotation), avoidSmoothing * Time.fixedDeltaTime);
        float urgency = Mathf.Clamp01(smoothedAvoid.magnitude);

        float turnRate = turnSpeed;
        if (urgency > 0.001f)
        {
            Vector3 escape = (desired + smoothedAvoid.normalized * avoidStrength).normalized;
            desired = Vector3.Slerp(desired, escape, urgency);
            turnRate = Mathf.Lerp(turnSpeed, avoidTurnSpeed, urgency);
        }

        Quaternion target = Quaternion.LookRotation(desired, rotation * Vector3.up);
        Quaternion next = Quaternion.RotateTowards(rotation, target, turnRate * Time.fixedDeltaTime);
        bullet.RB.MoveRotation(next);
        bullet.RB.linearVelocity = next * Vector3.forward * speed;
    }

    void SetImmune(bool value)
    {
        immune = value;
        bullet.Immune = value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!immune || Bullet.IsBullet(collision)) return;

        Vector3 normal = collision.GetContact(0).normal;
        Vector3 heading = bullet.RB.rotation * Vector3.forward;
        Vector3 bounced = Vector3.Reflect(heading, normal).normalized;

        bullet.RB.position = collision.GetContact(0).point + normal * bounceClearance;
        bullet.RB.MoveRotation(Quaternion.LookRotation(bounced, Vector3.up));
        bullet.RB.linearVelocity = bounced * speed;

        smoothedAvoid = normal * avoidStrength;
        NextSegment();
    }

    Vector3 GetAvoidance(Quaternion rotation)
    {
        Vector3 avoid = Vector3.zero;

        foreach (var localDirection in probeDirections)
        {
            Vector3 direction = rotation * localDirection;
            if (!Physics.SphereCast(bullet.RB.position, probeRadius, direction, out RaycastHit hit, probeDistance, wallMask, QueryTriggerInteraction.Ignore))
            {
                continue;
            }
            float closeness = 1.0f - hit.distance / probeDistance;
            avoid += hit.normal * closeness * closeness;
        }

        return avoid;
    }

    void NextSegment()
    {
        float angle = Random.value * 2.0f * Mathf.PI;
        turnAxis = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f);
        turnSpeed = Random.Range(minTurnSpeed, maxTurnSpeed);
        segmentEndFixedTime = Time.fixedTime + Random.Range(minSegmentDuration, maxSegmentDuration);
    }
}
