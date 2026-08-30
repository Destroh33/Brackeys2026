using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody RB;

    [System.NonSerialized] public bool Immune;

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
        if (Immune || IsBullet(collision)) return;

        var entityHealth = collision.gameObject.GetComponentInParent<EntityHealth>();
        Vector3 point = collision.GetContact(0).point;

        if (entityHealth)
        {
            AudioManager.PlayEventAt(SfxEvent.HitFlesh, point);
            OnHit(entityHealth);
        }
        else
        {
            AudioManager.PlayEventAt(SfxEvent.HitConcrete, point);
        }

        Destroy(gameObject);
    }
}
