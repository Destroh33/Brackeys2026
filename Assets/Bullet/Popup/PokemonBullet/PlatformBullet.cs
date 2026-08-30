using System.Collections.Generic;
using UnityEngine;

public class PlatformBullet : Bullet
{
    [SerializeField] List<GameObject> platformPrefabs = new();
    [SerializeField] GameObject impactEffect;
    [SerializeField] TrailRenderer trail;
    [SerializeField] LayerMask groundMask = 1;
    [SerializeField] float speed = 30.0f;
    [SerializeField] float lifetime = 5.0f;
    [SerializeField] float castDistance = 6.0f;
    [SerializeField] float castRadius = 0.0f;
    [SerializeField] float spawnSpacing = 2.0f;
    [SerializeField] float surfaceOffset = 0.05f;
    [SerializeField] float platformLifetime = 6.0f;
    [SerializeField] bool alignToSurface = true;

    float despawnTime;
    Vector3 lastSpawnPosition;
    int nextPrefab;
    bool despawned;

    protected override void Reset()
    {
        base.Reset();
        TryGetComponent(out trail);
    }

    void Start()
    {
        despawnTime = Time.time + lifetime;
        RB.linearVelocity = transform.forward * speed;

        lastSpawnPosition = RB.position;
        SpawnPlatform();
    }

    void FixedUpdate()
    {
        if (Time.time > despawnTime)
        {
            Despawn(RB.position, Vector3.up);
            return;
        }

        if ((RB.position - lastSpawnPosition).sqrMagnitude < spawnSpacing * spawnSpacing) return;

        lastSpawnPosition = RB.position;
        SpawnPlatform();
    }

    void SpawnPlatform()
    {
        if (platformPrefabs.Count == 0) return;

        var prefab = platformPrefabs[nextPrefab];
        nextPrefab = (nextPrefab + 1) % platformPrefabs.Count;
        if (!prefab) return;

        Vector3 origin = transform.position;
        Vector3 down = -transform.up;

        Vector3 position;
        Vector3 up;
        if (Cast(origin, down, out RaycastHit hit))
        {
            position = hit.point + hit.normal * surfaceOffset;
            up = alignToSurface ? hit.normal : -down;
        }
        else
        {
            position = origin + down * castDistance;
            up = -down;
        }

        var platform = Instantiate(prefab, position, PlatformRotation(up));
        if (platformLifetime > 0.0f) SelfDestruct.After(platform, platformLifetime);
    }

    bool Cast(Vector3 origin, Vector3 direction, out RaycastHit hit)
    {
        if (castRadius > 0.0f)
        {
            return Physics.SphereCast(origin, castRadius, direction, out hit, castDistance, groundMask, QueryTriggerInteraction.Ignore);
        }
        return Physics.Raycast(origin, direction, out hit, castDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    Quaternion PlatformRotation(Vector3 up)
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, up);
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.ProjectOnPlane(transform.up, up);
        if (forward.sqrMagnitude < 0.0001f) return Quaternion.identity;

        return Quaternion.LookRotation(forward.normalized, up);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsBullet(collision)) return;

        var health = collision.gameObject.GetComponentInParent<EntityHealth>();
        if (health) OnHit(health);

        var contact = collision.GetContact(0);
        Despawn(contact.point + contact.normal * 0.1f, contact.normal);
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
        Gizmos.color = new Color(0.3f, 1.0f, 0.5f, 0.8f);
        Gizmos.DrawLine(transform.position, transform.position - transform.up * castDistance);
    }
}
