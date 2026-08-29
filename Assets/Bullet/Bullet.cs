using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody RB;

    virtual protected void Reset()
    {
        TryGetComponent(out RB);
    }

    public static bool IsBullet(Collision collision)
    {
        return collision.gameObject.GetComponentInParent<Bullet>() != null;
    }

    virtual public void OnHit(EntityHealth entityHealth)
    {
        entityHealth.Kill();
    }

    virtual protected void OnCollisionEnter(Collision collision)
    {
        if (IsBullet(collision)) return;

        var entityHealth = collision.gameObject.GetComponentInParent<EntityHealth>();
        if (entityHealth)
        {
            OnHit(entityHealth);
        }
        Destroy(gameObject);
    }
}
