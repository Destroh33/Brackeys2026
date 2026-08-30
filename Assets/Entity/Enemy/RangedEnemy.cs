using UnityEngine;

public class RangedEnemy : Enemy
{
    enum State { Idle, Aim, Recover }

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform muzzle;
    [SerializeField] EnemyGun gun;

    [SerializeField] float attackRange = 20f;
    [SerializeField] float aimDuration = 0.9f;
    [SerializeField] float recoverDuration = 0.8f;
    [SerializeField] float spreadDegrees = 1.5f;
    [SerializeField] float bulletSpeed = 16f;

    State state = State.Idle;
    Collider[] ownColliders;

    protected override void Awake()
    {
        base.Awake();
        ownColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        StopMoving();
    }

    protected override void Think()
    {
        switch (state)
        {
            case State.Idle: TickIdle(); break;
            case State.Aim: TickAim(); break;
            case State.Recover: TickRecover(); break;
        }
    }

    void SetState(State next)
    {
        if (next == State.Aim && state == State.Idle)
        {
            AudioManager.PlayEventOn(SfxEvent.EnemyAlert, transform);
            AudioManager.PlayEventOn(SfxEvent.EnemyAim, transform);
        }

        state = next;
        EnterState();
    }

    bool CanAttack()
    {
        return CanSense() && DistanceToTarget <= attackRange && HasLineOfSight();
    }

    void TickIdle()
    {
        if (CanAttack()) SetState(State.Aim);
    }

    void TickAim()
    {
        if (!CanAttack())
        {
            SetState(State.Idle);
            return;
        }

        FaceTarget();

        if (StateElapsed < aimDuration) return;

        Fire();
        SetState(State.Recover);
    }

    void TickRecover()
    {
        FaceTarget();

        if (StateElapsed >= recoverDuration) SetState(State.Idle);
    }

    void Fire()
    {
        if (bulletPrefab == null) return;

        Vector3 origin = muzzle != null ? muzzle.position : EyePoint;
        Vector3 toTarget = (TargetPoint - origin).normalized;
        Vector3 direction = Quaternion.LookRotation(toTarget) * Util.GetRandomDirectionInCone(spreadDegrees * Mathf.Deg2Rad);

        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(direction));
        IgnoreOwnColliders(bullet);
        bullet.AddComponent<BulletWhoosh>().Configure(0.3f, 1f);

        if (bulletSpeed > 0f && bullet.TryGetComponent(out DefaultBulletMovement movement))
        {
            movement.SetSpeed(bulletSpeed);
        }

        if (gun) gun.Fire();
        AudioManager.PlayEventAt(SfxEvent.EnemyFire, origin);
    }

    void IgnoreOwnColliders(GameObject bullet)
    {
        foreach (Collider bulletCollider in bullet.GetComponentsInChildren<Collider>())
        {
            foreach (Collider ownCollider in ownColliders)
            {
                Physics.IgnoreCollision(bulletCollider, ownCollider);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
