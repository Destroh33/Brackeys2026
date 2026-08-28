using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] float lifetime = 3.0f;
    [SerializeField] TrailRenderer[] detachTrails;

    void Start()
    {
        Invoke(nameof(Detach), Mathf.Max(0.0f, lifetime));
    }

    void Detach()
    {
        if (detachTrails == null)
        {
            Destroy(gameObject);
            return;
        }

        foreach (var trail in detachTrails)
        {
            if (!trail) continue;
            trail.transform.SetParent(null, true);
            trail.emitting = false;
            Destroy(trail.gameObject, trail.time + 0.1f);
        }
        Destroy(gameObject);
    }
}
