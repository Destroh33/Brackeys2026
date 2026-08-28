using System.Collections.Generic;
using UnityEngine;

public class PoisonGasCloud : MonoBehaviour
{
    [SerializeField] ParticleSystem[] emitters;
    [SerializeField] LayerMask affectMask = ~0;
    [SerializeField] float maxRadius = 6.5f;
    [SerializeField] float growDuration = 1.2f;
    [SerializeField] float duration = 9.0f;
    [SerializeField] float dwellToKill = 1.4f;
    [SerializeField] float tickInterval = 0.15f;
    [SerializeField] float dwellRecoveryMultiplier = 0.5f;

    readonly Dictionary<EntityHealth, float> dwell = new();
    readonly Dictionary<EntityHealth, float> decaying = new();
    readonly Collider[] overlaps = new Collider[64];
    readonly List<EntityHealth> inside = new();
    readonly List<EntityHealth> stale = new();

    float startTime;
    float nextTickTime;

    void Start()
    {
        startTime = Time.time;
        nextTickTime = Time.time;
    }

    void Update()
    {
        float elapsed = Time.time - startTime;

        if (elapsed > duration)
        {
            foreach (var emitter in emitters ?? System.Array.Empty<ParticleSystem>())
            {
                if (emitter) emitter.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            enabled = false;
            return;
        }

        if (Time.time < nextTickTime) return;

        float delta = tickInterval + (Time.time - nextTickTime);
        nextTickTime = Time.time + tickInterval;
        Tick(elapsed, delta);
    }

    void Tick(float elapsed, float delta)
    {
        float radius = maxRadius * Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, growDuration));
        if (radius <= 0.01f) return;

        inside.Clear();
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, overlaps, affectMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; ++i)
        {
            var health = overlaps[i].GetComponentInParent<EntityHealth>();
            if (!health || health.Dead || inside.Contains(health)) continue;
            inside.Add(health);
        }

        stale.Clear();
        decaying.Clear();
        foreach (var pair in dwell)
        {
            if (!pair.Key || pair.Key.Dead)
            {
                stale.Add(pair.Key);
                continue;
            }
            if (inside.Contains(pair.Key)) continue;

            float recovered = pair.Value - delta * dwellRecoveryMultiplier;
            if (recovered <= 0.0f) stale.Add(pair.Key);
            else decaying.Add(pair.Key, recovered);
        }
        foreach (var key in stale) dwell.Remove(key);
        foreach (var pair in decaying) dwell[pair.Key] = pair.Value;

        foreach (var health in inside)
        {
            dwell.TryGetValue(health, out float accumulated);
            accumulated += delta;
            if (accumulated >= dwellToKill)
            {
                dwell.Remove(health);
                health.Kill();
                continue;
            }
            dwell[health] = accumulated;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 1.0f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
