using UnityEngine;

public class BangPopBullet : MonoBehaviour
{
    [SerializeField] Transform popRoot;
    [SerializeField] Rigidbody rb;
    [SerializeField] ParticleSystem puff;
    [SerializeField] float popDuration = 0.22f;
    [SerializeField] AnimationCurve popCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField] float wobbleAmplitude = 9.0f;
    [SerializeField] float wobbleFrequency = 7.0f;
    [SerializeField] float wobbleDamping = 2.2f;
    [SerializeField] float launchSpeed = 6.5f;
    [SerializeField] float launchArc = 0.3f;
    [SerializeField] float launchSpeedJitter = 1.0f;
    [SerializeField] float spinTorque = 0.25f;
    [SerializeField] float lifetime = 4.0f;
    [SerializeField] float shrinkDuration = 0.4f;

    Vector3 baseScale = Vector3.one;
    Quaternion baseRotation = Quaternion.identity;
    float startTime;

    void Reset()
    {
        TryGetComponent(out rb);
    }

    void Start()
    {
        startTime = Time.time;

        if (popRoot)
        {
            baseScale = popRoot.localScale;
            baseRotation = popRoot.localRotation;
            popRoot.localScale = Vector3.zero;
        }
        if (puff) puff.Play();
        AudioManager.PlayEventAt(SfxEvent.BangPop, transform.position);

        Launch();
    }

    void Launch()
    {
        if (!rb) return;

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 direction = (transform.forward + Vector3.up * launchArc).normalized;
        rb.linearVelocity = direction * (launchSpeed + Random.Range(-launchSpeedJitter, launchSpeedJitter));
        rb.AddTorque(Random.insideUnitSphere * spinTorque, ForceMode.Impulse);
    }

    void Update()
    {
        float elapsed = Time.time - startTime;

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (!popRoot) return;

        float shrinkStart = lifetime - shrinkDuration;
        float scale;
        if (elapsed < popDuration)
        {
            scale = popCurve.Evaluate(elapsed / popDuration);
        }
        else if (elapsed > shrinkStart)
        {
            scale = 1.0f - Mathf.SmoothStep(0.0f, 1.0f, (elapsed - shrinkStart) / shrinkDuration);
        }
        else
        {
            scale = 1.0f;
        }
        popRoot.localScale = baseScale * scale;

        float wobble = Mathf.Sin(elapsed * wobbleFrequency * Mathf.PI * 2.0f) * wobbleAmplitude * Mathf.Exp(-elapsed * wobbleDamping);
        popRoot.localRotation = baseRotation * Quaternion.Euler(0.0f, 0.0f, wobble);
    }
}
