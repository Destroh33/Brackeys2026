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
        if (collision.gameObject.TryGetComponent(out EntityHealth entityHealth))
        {
            OnHit(entityHealth);
        }
        Destroy(gameObject);
    }
}
