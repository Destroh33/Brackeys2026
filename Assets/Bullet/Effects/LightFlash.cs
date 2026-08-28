using UnityEngine;

public class LightFlash : MonoBehaviour
{
    [SerializeField] Light target;
    [SerializeField] float peakIntensity = 200.0f;
    [SerializeField] float duration = 0.6f;
    [SerializeField] AnimationCurve intensityOverLife = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);
    [SerializeField] Gradient colorOverLife;
    [SerializeField] bool disableWhenDone = true;

    float startTime;

    void Reset()
    {
        TryGetComponent(out target);
    }

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (!target) return;

        float life = (Time.time - startTime) / Mathf.Max(0.0001f, duration);
        if (life >= 1.0f)
        {
            target.intensity = 0.0f;
            if (disableWhenDone) target.enabled = false;
            enabled = false;
            return;
        }

        target.intensity = peakIntensity * intensityOverLife.Evaluate(life);
        if (colorOverLife != null) target.color = colorOverLife.Evaluate(life);
    }
}
