using UnityEngine;

public class PoisonGasBullet : Bullet
{
    [SerializeField] Transform spinner;
    [SerializeField] GameObject cloudPrefab;
    [SerializeField] ParticleSystem hiss;
    [SerializeField] float speed = 24.0f;
    [SerializeField] float fuse = 1.6f;
    [SerializeField] Vector3 spinAxis = new Vector3(1.0f, 0.35f, 0.0f);
    [SerializeField] float spinSpeed = 540.0f;

    float burstTime;
    bool burst;

    void Start()
    {
        RB.linearVelocity = transform.forward * speed;
        burstTime = Time.time + fuse;
    }

    void Update()
    {
        if (spinner) spinner.Rotate(spinAxis.normalized, spinSpeed * Time.deltaTime, Space.Self);
        if (Time.time >= burstTime) Burst(transform.position);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsBullet(collision)) return;

        var contact = collision.GetContact(0);
        Burst(contact.point + contact.normal * 0.3f);
    }

    void Burst(Vector3 position)
    {
        if (burst) return;
        burst = true;

        if (cloudPrefab) Instantiate(cloudPrefab, position, Quaternion.identity);
        if (hiss)
        {
            hiss.transform.SetParent(null, true);
            hiss.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(hiss.gameObject, 2.0f);
        }
        Destroy(gameObject);
    }
}
