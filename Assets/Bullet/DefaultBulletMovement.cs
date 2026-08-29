using UnityEngine;

public class DefaultBulletMovement : MonoBehaviour
{
    [SerializeField] Bullet bullet;
    [SerializeField] float speed = 30.0f;
    [SerializeField] float lifetime = 5.0f;

    float startFixedTime;

    public void SetSpeed(float value)
    {
        speed = value;
        if (bullet && bullet.RB) bullet.RB.linearVelocity = speed * transform.forward;
    }

    void Reset()
    {
        TryGetComponent(out bullet);
    }

    void Start()
    {
        bullet.RB.linearVelocity = speed * transform.forward;
        startFixedTime = Time.fixedTime;
    }

    void FixedUpdate()
    {
        if (Time.fixedTime > startFixedTime + lifetime)
        {
            Destroy(gameObject);
        }
    }
}
