using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BlockShatter : MonoBehaviour
{
    [SerializeField] EntityHealth health;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] BlockRig rig;
    [SerializeField] Transform rigRoot;
    [SerializeField] GameObject burstEffect;

    [SerializeField] float explosionForce = 240.0f;
    [SerializeField] float explosionRadius = 2.6f;
    [SerializeField] float upwardModifier = 0.6f;
    [SerializeField] float knockbackSpeed = 2.2f;
    [SerializeField] float torqueImpulse = 0.9f;
    [SerializeField] float pieceMass = 0.5f;
    [SerializeField] float pieceDrag = 0.05f;
    [SerializeField] float pieceAngularDrag = 0.4f;
    [SerializeField] float pieceLifetime = 7.0f;
    [SerializeField] float pieceFadeDuration = 1.2f;
    [SerializeField] int pieceLayer = 2;
    [SerializeField] LayerMask pieceIgnoreLayers = 1 << 8;
    [SerializeField] Vector3 blastOffset = new(0.0f, 0.55f, 0.0f);

    readonly List<Transform> pieces = new();

    void Reset()
    {
        TryGetComponent(out health);
        TryGetComponent(out agent);
        TryGetComponent(out rig);
    }

    void OnEnable()
    {
        if (health) health.DeathEvent.AddListener(Shatter);
    }

    void OnDisable()
    {
        if (health) health.DeathEvent.RemoveListener(Shatter);
    }

    void Shatter()
    {
        if (!rigRoot) return;

        if (rig) rig.enabled = false;

        Vector3 inherited = Vector3.zero;
        if (agent)
        {
            inherited = agent.velocity;
            agent.enabled = false;
        }

        pieces.Clear();
        foreach (var renderer in rigRoot.GetComponentsInChildren<MeshRenderer>())
        {
            pieces.Add(renderer.transform);
        }

        Vector3 blastOrigin = transform.TransformPoint(blastOffset);
        Vector3 knockback = KnockbackDirection() * knockbackSpeed;

        foreach (var piece in pieces)
        {
            piece.SetParent(null, true);
            Launch(piece.gameObject, blastOrigin, inherited + knockback);
        }

        if (burstEffect)
        {
            Instantiate(burstEffect, blastOrigin, Quaternion.LookRotation(Vector3.up));
        }
    }

    void Launch(GameObject piece, Vector3 blastOrigin, Vector3 velocity)
    {
        piece.layer = pieceLayer;

        var collider = piece.AddComponent<BoxCollider>();
        collider.excludeLayers = pieceIgnoreLayers;

        var body = piece.AddComponent<Rigidbody>();
        body.mass = pieceMass;
        body.linearDamping = pieceDrag;
        body.angularDamping = pieceAngularDrag;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.excludeLayers = pieceIgnoreLayers;
        body.linearVelocity = velocity;
        body.AddExplosionForce(explosionForce, blastOrigin, explosionRadius, upwardModifier);
        body.AddTorque(Random.insideUnitSphere * torqueImpulse, ForceMode.Impulse);

        piece.AddComponent<BlockPiece>().Configure(pieceLifetime, pieceFadeDuration);
    }

    Vector3 KnockbackDirection()
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (!player) return Vector3.up;

        Vector3 away = transform.position - player.transform.position;
        away.y = 0.0f;
        if (away.sqrMagnitude < 0.0001f) return Vector3.up;

        return (away.normalized + Vector3.up * 0.35f).normalized;
    }
}
