using UnityEngine;

public class PiercingBullet : Bullet
{
    [SerializeField] Transform spinner;
    [SerializeField] TrailRenderer trail;
    [SerializeField] GameObject pierceEffect;
    [SerializeField] GameObject impactEffect;
    [SerializeField] LayerMask targetMask = 1 << 7;
    [SerializeField] float speed = 70.0f;
    [SerializeField] float lifetime = 4.0f;
    [SerializeField] float sweepRadius = 0.25f;
    [SerializeField] int maxPierces = 6;
    [SerializeField] float speedLossPerPierce = 0.06f;
    [SerializeField] float spinSpeed = 900.0f;

    float despawnTime;
    int pierces;
    Vector3 lastPosition;

    protected override void Reset()
    {
        base.Reset();
        TryGetComponent(out trail);
    }

    void Start()
    {
        RB.linearVelocity = transform.forward * speed;
        despawnTime = Time.time + lifetime;
        lastPosition = RB.position;
    }

    void Update()
    {
        if (spinner) spinner.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
    }

    void FixedUpdate()
    {
        if (Time.time > despawnTime)
        {
            Despawn();
            return;
        }

        Sweep();
        lastPosition = RB.position;
    }

    void Sweep()
    {
        Vector3 delta = RB.position - lastPosition;
        float distance = delta.magnitude;
        if (distance < 0.0001f) return;

        Vector3 direction = delta / distance;
        var hits = Physics.SphereCastAll(lastPosition, sweepRadius, direction, distance, targetMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            var health = hit.collider.GetComponentInParent<EntityHealth>();
            if (!health || health.Dead) continue;

            health.Kill();
            Spawn(pierceEffect, hit.point == Vector3.zero ? hit.collider.bounds.center : hit.point, -direction);
            RB.linearVelocity *= 1.0f - speedLossPerPierce;

            ++pierces;
            if (pierces >= maxPierces)
            {
                Despawn();
                return;
            }
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);
        Spawn(impactEffect, contact.point, contact.normal);
        Despawn();
    }

    void Spawn(GameObject prefab, Vector3 position, Vector3 normal)
    {
        if (!prefab) return;
        Instantiate(prefab, position, Quaternion.LookRotation(normal.sqrMagnitude < 0.0001f ? Vector3.up : normal));
    }

    void Despawn()
    {
        if (trail)
        {
            trail.transform.SetParent(null, true);
            trail.emitting = false;
            Destroy(trail.gameObject, trail.time + 0.1f);
        }
        Destroy(gameObject);
    }
}
