using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiPopIn : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] float duration = 0.42f;
    [SerializeField] float riseFrom = 44f;
    [SerializeField] float scaleFrom = 0.86f;
    [SerializeField] float overshoot = 1.06f;
    [SerializeField] bool fade = true;

    RectTransform rect;
    CanvasGroup group;
    Vector2 restPosition;
    float elapsed;
    bool playing;

    void Awake()
    {
        rect = (RectTransform)transform;
        restPosition = rect.anchoredPosition;

        if (!fade) return;

        group = GetComponent<CanvasGroup>();
        if (!group) group = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        elapsed = 0f;
        playing = true;
        Apply(0f);
    }

    void Update()
    {
        if (!playing) return;

        elapsed += Time.unscaledDeltaTime;
        if (elapsed < delay) return;

        float t = Mathf.Clamp01((elapsed - delay) / Mathf.Max(0.0001f, duration));
        Apply(t);

        if (t >= 1f) playing = false;
    }

    void Apply(float t)
    {
        float eased = Ease(t);

        rect.anchoredPosition = restPosition + Vector2.up * Mathf.Lerp(-riseFrom, 0f, eased);
        rect.localScale = Vector3.one * Mathf.LerpUnclamped(scaleFrom, 1f, eased);

        if (group) group.alpha = Mathf.Clamp01(t * 2.2f);
    }

    float Ease(float t)
    {
        if (t >= 1f) return 1f;

        float back = 1f - Mathf.Pow(1f - t, 3f);
        float bump = Mathf.Sin(t * Mathf.PI) * (overshoot - 1f);
        return back + bump;
    }
}
