using UnityEngine;

public class DeathDebris : MonoBehaviour
{
    [SerializeField] float gravityScale = 2.4f;
    [SerializeField] bool levelHorizon;
    [SerializeField] float spinDamping = 2.2f;
    [SerializeField] float settleSpeed = 1.4f;
    [SerializeField] float settleThreshold = 1.2f;

    Rigidbody body;
    float settled;

    public void Configure(float gravity, bool horizon)
    {
        gravityScale = gravity;
        levelHorizon = horizon;
    }

    void Awake()
    {
        TryGetComponent(out body);
    }

    void FixedUpdate()
    {
        if (!body) return;

        body.AddForce(Physics.gravity * (gravityScale - 1f), ForceMode.Acceleration);
        body.angularVelocity *= Mathf.Exp(-spinDamping * Time.fixedDeltaTime);

        if (!levelHorizon) return;

        bool resting = body.linearVelocity.magnitude < settleThreshold;
        settled = resting ? settled + Time.fixedDeltaTime : 0f;
        if (settled < 0.3f) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.ProjectOnPlane(transform.up, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f) return;

        Quaternion upright = Quaternion.LookRotation(forward.normalized, Vector3.up);
        body.MoveRotation(Quaternion.Slerp(body.rotation, upright, 1f - Mathf.Exp(-settleSpeed * Time.fixedDeltaTime)));
    }
}
