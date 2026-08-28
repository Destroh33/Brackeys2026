using UnityEngine;

public class WaterBlob : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform stretcher;
    [SerializeField] GameObject splashEffect;
    [SerializeField] TrailRenderer trail;
    [SerializeField] float lifetime = 4.0f;
    [SerializeField] float stretchPerSpeed = 0.035f;
    [SerializeField] float maxStretch = 3.5f;
    [SerializeField] float squashPerSpeed = 0.012f;

    Vector3 baseScale = Vector3.one;
    float despawnTime;

    void Reset()
    {
        TryGetComponent(out rb);
        TryGetComponent(out trail);
    }

    void Start()
    {
        if (stretcher) baseScale = stretcher.localScale;
        despawnTime = Time.time + lifetime;
    }

    void Update()
    {
        if (Time.time > despawnTime)
        {
            Despawn();
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f || !stretcher) return;

        float speed = velocity.magnitude;
        float stretch = Mathf.Min(maxStretch, 1.0f + speed * stretchPerSpeed);
        float squash = 1.0f / Mathf.Max(0.2f, 1.0f + speed * squashPerSpeed);

        stretcher.rotation = Quaternion.LookRotation(velocity / speed);
        stretcher.localScale = new Vector3(baseScale.x * squash, baseScale.y * squash, baseScale.z * stretch);
    }

    void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);
        if (splashEffect)
        {
            Instantiate(splashEffect, contact.point + contact.normal * 0.02f, Quaternion.LookRotation(contact.normal));
        }
        Despawn();
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
