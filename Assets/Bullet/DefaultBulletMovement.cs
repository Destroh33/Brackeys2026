using UnityEngine;

public class DefaultBulletMovement : MonoBehaviour
{
    [SerializeField] Bullet bullet;
    [SerializeField] float speed = 30.0f;

    void Reset()
    {
        TryGetComponent(out bullet);
    }

    void Start()
    {
        bullet.RB.linearVelocity = speed * transform.forward;
    }
}
