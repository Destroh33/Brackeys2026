using UnityEngine;

public class MeleeEnemy : Enemy
{
    enum State { Idle, Chase, WindUp, Lunge, Recover }

    [SerializeField] EnemyBat weapon;

    [SerializeField] float lungeRange = 3.5f;
    [SerializeField] float windUpDuration = 0.4f;
    [SerializeField] float lungeSpeed = 14f;
    [SerializeField] float lungeDuration = 0.25f;
    [SerializeField] float recoverDuration = 0.6f;
    [SerializeField] float hitRadius = 1.5f;
    [SerializeField] float stopDistance = 1.3f;

    State state = State.Idle;
    Vector3 lungeDirection;
    bool hitThisLunge;

    protected override void Think()
    {
        switch (state)
        {
            case State.Idle: TickIdle(); break;
            case State.Chase: TickChase(); break;
            case State.WindUp: TickWindUp(); break;
            case State.Lunge: TickLunge(); break;
            case State.Recover: TickRecover(); break;
        }
    }

    void SetState(State next)
    {
        state = next;
        EnterState();

        if (!weapon) return;

        switch (next)
        {
            case State.WindUp: weapon.WindUp(windUpDuration); break;
            case State.Lunge: weapon.Swing(lungeDuration); break;
            case State.Recover: weapon.Recover(recoverDuration); break;
        }
    }

    void TickIdle()
    {
        if (CanSense()) SetState(State.Chase);
    }

    void TickChase()
    {
        if (!CanSense())
        {
            StopMoving();
            SetState(State.Idle);
            return;
        }

        MoveTo(Target.transform.position);
        FaceTarget();

        if (DistanceToTarget <= lungeRange)
        {
            StopMoving();
            SetState(State.WindUp);
        }
    }

    void TickWindUp()
    {
        FaceTarget();

        if (StateElapsed < windUpDuration) return;

        lungeDirection = HasTarget ? Vector3.ProjectOnPlane(Target.transform.position - transform.position, Vector3.up).normalized : transform.forward;
        hitThisLunge = false;
        SetState(State.Lunge);
    }

    void TickLunge()
    {
        FaceDirection(lungeDirection);
        if (Agent.isOnNavMesh && (!HasTarget || DistanceToTarget > stopDistance))
        {
            Agent.Move(lungeDirection * lungeSpeed * Time.deltaTime);
        }

        TryHitTarget();

        if (StateElapsed >= lungeDuration) SetState(State.Recover);
    }

    void TickRecover()
    {
        FaceTarget();

        if (StateElapsed >= recoverDuration) SetState(State.Chase);
    }

    void TryHitTarget()
    {
        if (hitThisLunge || !HasTarget || DistanceToTarget > hitRadius) return;

        hitThisLunge = true;
        if (TargetHealth != null) TargetHealth.Kill();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lungeRange);
    }
}
