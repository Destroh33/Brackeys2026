using UnityEngine;

public class NukeBullet : Bullet
{
    [SerializeField] Transform spinner;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float speed = 26.0f;
    [SerializeField] float lifetime = 8.0f;
    [SerializeField] float spinSpeed = 240.0f;
    [SerializeField] float alignToVelocityLerp = 8.0f;

    float despawnTime;
    bool detonated;

    void Start()
    {
        RB.linearVelocity = transform.forward * speed;
        despawnTime = Time.time + lifetime;
    }

    void Update()
    {
        if (spinner) spinner.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
    }

    void FixedUpdate()
    {
        if (Time.time > despawnTime)
        {
            Detonate(RB.position, Vector3.up);
            return;
        }

        Vector3 velocity = RB.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f) return;

        Quaternion target = Quaternion.LookRotation(velocity.normalized);
        RB.MoveRotation(Quaternion.Slerp(RB.rotation, target, alignToVelocityLerp * Time.fixedDeltaTime));
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);
        Detonate(contact.point + contact.normal * 0.2f, contact.normal);
    }

    void Detonate(Vector3 position, Vector3 normal)
    {
        if (detonated) return;
        detonated = true;

        if (explosionPrefab)
        {
            Quaternion rotation = Quaternion.LookRotation(normal.sqrMagnitude < 0.0001f ? Vector3.up : normal);
            Instantiate(explosionPrefab, position, rotation);
        }
        Destroy(gameObject);
    }
}
