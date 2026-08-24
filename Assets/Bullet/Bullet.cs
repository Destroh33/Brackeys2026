using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody RB;

    [SerializeField] float lifetime = 5.0f;

    float expirationTime;

    void Reset()
    {
        TryGetComponent(out RB);
    }

    void Start()
    {
        expirationTime = Time.time + lifetime;
    }

    void Update()
    {
        if (Time.time > expirationTime) Destroy(gameObject);
    }

    virtual public void OnHit(EntityHealth entityHealth)
    {
        entityHealth.Kill();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out EntityHealth entityHealth))
        {
            OnHit(entityHealth);
        }
        Destroy(gameObject);
    }
}
