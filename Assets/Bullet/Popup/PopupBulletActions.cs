using UnityEngine;

/// <summary>
/// Hit context for a spawned popup, exposed as methods a UnityEvent can target in the inspector.
/// </summary>
public class PopupBulletActions : MonoBehaviour
{
    [SerializeField] LayerMask targetMask = 1 << 7;

    readonly Collider[] overlaps = new Collider[64];

    public Vector3 Point { get; private set; }
    public Vector3 Normal { get; private set; } = Vector3.up;
    public EntityHealth Target { get; private set; }

    public void SetContext(Vector3 point, Vector3 normal, EntityHealth target)
    {
        Point = point;
        Normal = normal.sqrMagnitude < 0.0001f ? Vector3.up : normal.normalized;
        Target = target;
    }

    public void KillTarget()
    {
        if (Target && !Target.Dead) Target.Kill();
    }

    public void KillInRadius(float radius)
    {
        int count = Physics.OverlapSphereNonAlloc(Point, radius, overlaps, targetMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; ++i)
        {
            var health = overlaps[i].GetComponentInParent<EntityHealth>();
            if (health && !health.Dead) health.Kill();
        }
    }

    public void SpawnAtHit(GameObject prefab)
    {
        if (!prefab) return;
        Instantiate(prefab, Point, Quaternion.LookRotation(Normal));
    }
}
