using UnityEngine;

public class ElectroBullet : Bullet
{
    [SerializeField] GameObject orbiterPrefab;
    [SerializeField] GameObject impactEffect;
    [SerializeField] TrailRenderer trail;
    [SerializeField] LayerMask targetMask = 1 << 7;
    [SerializeField] int orbiterCount = 3;
    [SerializeField] float orbitRadius = 0.9f;
    [SerializeField] float orbitSpeed = 540.0f;
    [SerializeField] float orbitWobble = 0.15f;
    [SerializeField] float orbitWobbleSpeed = 6.0f;
    [SerializeField] float speed = 34.0f;
    [SerializeField] float lifetime = 3.0f;
    [SerializeField] float damageRadius = 2.5f;

    readonly Collider[] overlaps = new Collider[64];

    Collider[] ownColliders;
    Transform orbitRoot;
    Transform[] orbiters;
    float startTime;
    float despawnTime;
    bool despawned;

    protected override void Reset()
    {
        base.Reset();
        TryGetComponent(out trail);
    }

    void Awake()
    {
        ownColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        startTime = Time.time;
        despawnTime = startTime + lifetime;
        RB.linearVelocity = transform.forward * speed;
        SpawnOrbiters();
    }

    void SpawnOrbiters()
    {
        if (!orbiterPrefab || orbiterCount <= 0) return;

        orbitRoot = new GameObject("Orbiters").transform;
        orbitRoot.SetParent(transform, false);

        orbiters = new Transform[orbiterCount];
        for (int i = 0; i < orbiterCount; ++i)
        {
            float angle = 360.0f / orbiterCount * i;
            var orbiter = Instantiate(orbiterPrefab, orbitRoot).transform;
            orbiter.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);
            orbiter.localPosition = orbiter.localRotation * Vector3.up * orbitRadius;
            orbiters[i] = orbiter;

            // Visual only: the bullet's own overlap sweep does the damage.
            foreach (var orbiterCollider in orbiter.GetComponentsInChildren<Collider>())
            {
                orbiterCollider.enabled = false;
            }
        }
    }

    void Update()
    {
        if (!orbitRoot) return;

        orbitRoot.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime, Space.Self);

        if (orbitWobble <= 0.0f) return;

        float elapsed = Time.time - startTime;
        for (int i = 0; i < orbiters.Length; ++i)
        {
            var orbiter = orbiters[i];
            if (!orbiter) continue;

            float phase = elapsed * orbitWobbleSpeed + Mathf.PI * 2.0f * i / orbiters.Length;
            float radius = orbitRadius * (1.0f + Mathf.Sin(phase) * orbitWobble);
            orbiter.localPosition = orbiter.localRotation * Vector3.up * radius;
        }
    }

    void FixedUpdate()
    {
        if (Time.time > despawnTime)
        {
            Despawn(RB.position, Vector3.up);
            return;
        }

        // Enemies never deflect the bullet, so drive the velocity rather than trusting physics.
        RB.linearVelocity = transform.forward * speed;
        Zap();
    }

    void Zap()
    {
        int count = Physics.OverlapSphereNonAlloc(RB.position, damageRadius, overlaps, targetMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; ++i)
        {
            var health = overlaps[i].GetComponentInParent<EntityHealth>();
            if (!health || health.Dead) continue;
            health.Kill();
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsBullet(collision)) return;

        if (PassesThrough(collision.collider))
        {
            IgnoreFrom(collision.collider);
            return;
        }

        var contact = collision.GetContact(0);
        Despawn(contact.point + contact.normal * 0.1f, contact.normal);
    }

    bool PassesThrough(Collider other)
    {
        if ((targetMask.value & (1 << other.gameObject.layer)) != 0) return true;
        return other.GetComponentInParent<EntityHealth>() != null;
    }

    void IgnoreFrom(Collider other)
    {
        if (ownColliders == null) return;

        foreach (var ownCollider in ownColliders)
        {
            if (ownCollider) Physics.IgnoreCollision(ownCollider, other, true);
        }
    }

    void Despawn(Vector3 position, Vector3 normal)
    {
        if (despawned) return;
        despawned = true;

        if (impactEffect)
        {
            Quaternion rotation = Quaternion.LookRotation(normal.sqrMagnitude < 0.0001f ? Vector3.up : normal);
            Instantiate(impactEffect, position, rotation);
        }
        if (trail)
        {
            trail.transform.SetParent(null, true);
            trail.emitting = false;
            Destroy(trail.gameObject, trail.time + 0.1f);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1.0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
