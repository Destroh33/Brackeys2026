using UnityEngine;

public class PopupBullet : Bullet
{
    [SerializeField] Transform spinner;
    [SerializeField] GameObject popupPrefab;
    [SerializeField] float speed = 26.0f;
    [SerializeField] float lifetime = 5.0f;
    [SerializeField] float spinSpeed = 220.0f;
    [SerializeField] bool popOnLifetimeEnd = true;

    float despawnTime;
    bool popped;

    void Start()
    {
        RB.linearVelocity = transform.forward * speed;
        despawnTime = Time.time + lifetime;
    }

    void Update()
    {
        if (spinner) spinner.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);

        if (Time.time <= despawnTime) return;

        if (popOnLifetimeEnd) Pop(transform.position, -transform.forward, null);
        else Destroy(gameObject);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);
        Pop(
            contact.point + contact.normal * 0.1f,
            contact.normal,
            collision.gameObject.GetComponentInParent<EntityHealth>()
        );
    }

    void Pop(Vector3 point, Vector3 normal, EntityHealth target)
    {
        if (popped) return;
        popped = true;

        if (popupPrefab)
        {
            var popup = Instantiate(popupPrefab);
            if (popup.TryGetComponent(out PopupBulletActions actions))
            {
                actions.SetContext(point, normal, target);
            }
        }
        Destroy(gameObject);
    }
}
