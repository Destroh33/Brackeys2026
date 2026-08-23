using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody RB;

    void Reset()
    {
        TryGetComponent(out RB);
    }

    virtual public void OnHit(EntityHealth entityHealth)
    {
        entityHealth.Kill();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EntityHealth entityHealth))
        {
            OnHit(entityHealth);
        }
        Destroy(gameObject);
    }
}
