using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody RB;

    virtual protected void Reset()
    {
        TryGetComponent(out RB);
    }

    virtual public void OnHit(EntityHealth entityHealth)
    {
        entityHealth.Kill();
    }

    virtual protected void OnCollisionEnter(Collision collision)
    {
        var entityHealth = collision.gameObject.GetComponentInParent<EntityHealth>();
        if (entityHealth)
        {
            OnHit(entityHealth);
        }
        Destroy(gameObject);
    }
}
