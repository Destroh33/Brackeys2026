using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected EntityHealth Health;
    [SerializeField] protected Transform Eye;

    [SerializeField] protected float senseRadius = 25f;
    [SerializeField] protected float turnSpeed = 720f;
    [SerializeField] protected float targetAimHeight = 1f;
    [SerializeField] protected LayerMask sightBlockers = ~0;

    protected NavMeshAgent Agent { get; private set; }

    /// <summary>Seconds since the current state was entered.</summary>
    protected float StateElapsed { get; private set; }

    Player target;
    EntityHealth targetHealth;

    protected Player Target
    {
        get
        {
            if (target == null && GameManager.Instance != null)
            {
                target = GameManager.Instance.Player;
                if (target != null) target.TryGetComponent(out targetHealth);
            }
            return target;
        }
    }

    protected EntityHealth TargetHealth
    {
        get
        {
            _ = Target;
            return targetHealth;
        }
    }

    protected bool HasTarget => Target != null && (targetHealth == null || !targetHealth.Dead);
    protected bool Dead => Health != null && Health.Dead;

    protected Vector3 EyePoint => Eye != null ? Eye.position : transform.position + Vector3.up;
    protected Vector3 TargetPoint => Target.transform.position + Vector3.up * targetAimHeight;

    protected float DistanceToTarget => Vector3.Distance(transform.position, Target.transform.position);

    void Reset()
    {
        TryGetComponent(out Health);
    }

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
    }

    protected virtual void OnEnable()
    {
        if (Health != null) Health.DeathEvent.AddListener(OnDeath);
    }

    protected virtual void OnDisable()
    {
        if (Health != null) Health.DeathEvent.RemoveListener(OnDeath);
    }

    protected virtual void Update()
    {
        if (Dead) return;

        StateElapsed += Time.deltaTime;
        Think();
    }

    /// <summary>Per-frame behaviour, run only while alive.</summary>
    protected abstract void Think();

    protected void EnterState()
    {
        StateElapsed = 0f;
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    /// <summary>True when the player is alive and inside the sense radius.</summary>
    protected bool CanSense()
    {
        return HasTarget && DistanceToTarget <= senseRadius;
    }

    protected bool HasLineOfSight()
    {
        if (!HasTarget) return false;

        Vector3 origin = EyePoint;
        Vector3 offset = TargetPoint - origin;
        if (!Physics.Raycast(origin, offset.normalized, out RaycastHit hit, offset.magnitude, sightBlockers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return hit.transform.IsChildOf(Target.transform);
    }

    protected void MoveTo(Vector3 destination)
    {
        if (!Agent.isOnNavMesh) return;
        Agent.isStopped = false;
        Agent.SetDestination(destination);
    }

    protected void StopMoving()
    {
        if (!Agent.isOnNavMesh) return;
        Agent.isStopped = true;
        Agent.ResetPath();
    }

    protected void FaceTarget()
    {
        if (HasTarget) FaceDirection(Target.transform.position - transform.position);
    }

    protected void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(direction.normalized),
            turnSpeed * Time.deltaTime);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, senseRadius);
    }
}
