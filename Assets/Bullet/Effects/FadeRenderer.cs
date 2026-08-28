using UnityEngine;

public class FadeRenderer : MonoBehaviour
{
    [SerializeField] Renderer target;
    [SerializeField] string colorProperty = "_BaseColor";
    [SerializeField] Gradient colorOverLife;
    [SerializeField] AnimationCurve intensityOverLife = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);
    [SerializeField] float duration = 1.0f;
    [SerializeField] bool disableRendererWhenDone = true;

    MaterialPropertyBlock block;
    float startTime;

    void Reset()
    {
        TryGetComponent(out target);
    }

    void Start()
    {
        block = new MaterialPropertyBlock();
        startTime = Time.time;
        Apply(0.0f);
    }

    void Update()
    {
        float life = (Time.time - startTime) / Mathf.Max(0.0001f, duration);
        if (life >= 1.0f)
        {
            if (disableRendererWhenDone && target) target.enabled = false;
            enabled = false;
            return;
        }
        Apply(life);
    }

    void Apply(float life)
    {
        if (!target) return;
        Color color = colorOverLife != null ? colorOverLife.Evaluate(life) : Color.white;
        float intensity = intensityOverLife.Evaluate(life);
        target.GetPropertyBlock(block);
        block.SetColor(colorProperty, new Color(color.r * intensity, color.g * intensity, color.b * intensity, color.a));
        target.SetPropertyBlock(block);
    }
}
