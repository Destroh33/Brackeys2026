using UnityEngine;

public class BlockPiece : MonoBehaviour
{
    [SerializeField] float lifetime = 7.0f;
    [SerializeField] float fadeDuration = 1.2f;

    Vector3 baseScale;
    float startTime;

    public void Configure(float pieceLifetime, float pieceFadeDuration)
    {
        lifetime = pieceLifetime;
        fadeDuration = pieceFadeDuration;
    }

    void Start()
    {
        baseScale = transform.localScale;
        startTime = Time.time;
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        float fadeStart = lifetime - fadeDuration;
        if (elapsed < fadeStart) return;

        float fade = 1.0f - Mathf.SmoothStep(0.0f, 1.0f, (elapsed - fadeStart) / Mathf.Max(0.0001f, fadeDuration));
        transform.localScale = baseScale * fade;
    }
}
