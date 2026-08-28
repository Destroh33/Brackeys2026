using System.Collections.Generic;
using UnityEngine;

public class NukeExplosion : MonoBehaviour
{
    [SerializeField] LayerMask killMask = ~0;
    [SerializeField] float maxRadius = 18.0f;
    [SerializeField] float expandSpeed = 45.0f;
    [SerializeField] float sweepDuration = 1.2f;

    readonly HashSet<EntityHealth> killed = new();
    readonly Collider[] overlaps = new Collider[128];

    float startTime;

    void Start()
    {
        startTime = Time.time;
        Sweep();
    }

    void Update()
    {
        if (Time.time - startTime > sweepDuration)
        {
            enabled = false;
            return;
        }
        Sweep();
    }

    void Sweep()
    {
        float radius = Mathf.Min(maxRadius, (Time.time - startTime) * expandSpeed);
        if (radius <= 0.01f) return;

        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, overlaps, killMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; ++i)
        {
            var health = overlaps[i].GetComponentInParent<EntityHealth>();
            if (!health || health.Dead || !killed.Add(health)) continue;
            health.Kill();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.4f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
