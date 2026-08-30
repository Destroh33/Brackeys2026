using System.Collections.Generic;
using UnityEngine;

public class FlareStrike : MonoBehaviour
{
    [SerializeField] TrailRenderer trail;
    [SerializeField] GameObject hitEffect;
    [SerializeField] LayerMask targetMask = (1 << 6) | (1 << 7);
    [SerializeField] float speed = 80.0f;
    [SerializeField] float sweepRadius = 0.35f;
    [SerializeField] float overshoot = 10.0f;
    [SerializeField] float fallbackTravel = 60.0f;

    readonly HashSet<EntityHealth> struck = new();

    Vector3 direction;
    float remaining;

    public void Launch(Vector3 target)
    {
        Vector3 delta = target - transform.position;
        remaining = delta.magnitude + overshoot;
        direction = delta.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Start()
    {
        if (remaining <= 0.0f)
        {
            direction = transform.forward;
            remaining = fallbackTravel;
        }
        AudioManager.PlayEventOn(SfxEvent.FlareStrike, transform);
    }

    void Update()
    {
        float step = Mathf.Min(speed * Time.deltaTime, remaining);
        remaining -= step;

        Sweep(transform.position, step);
        transform.position += direction * step;

        if (remaining <= 0.0f) Despawn();
    }

    // terrain is never queried, so the strike passes straight through it and only entities on the mask are hit
    void Sweep(Vector3 from, float distance)
    {
        if (distance <= 0.0001f) return;

        var hits = Physics.SphereCastAll(from, sweepRadius, direction, distance, targetMask, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            var health = hit.collider.GetComponentInParent<EntityHealth>();
            if (!health || health.Dead || !struck.Add(health)) continue;

            Vector3 point = hit.point == Vector3.zero ? hit.collider.bounds.center : hit.point;
            AudioManager.PlayEventAt(SfxEvent.HitFlesh, point);
            if (hitEffect) Instantiate(hitEffect, point, Quaternion.LookRotation(-direction));

            health.Kill();
        }
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
