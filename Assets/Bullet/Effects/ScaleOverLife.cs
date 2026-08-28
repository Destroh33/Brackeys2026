using UnityEngine;

public class ScaleOverLife : MonoBehaviour
{
    [SerializeField] Vector3 axisScale = Vector3.one;
    [SerializeField] float duration = 1.0f;
    [SerializeField] AnimationCurve overLife = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField] bool holdAtEnd = true;

    float startTime;

    void Start()
    {
        startTime = Time.time;
        Apply(0.0f);
    }

    void Update()
    {
        float life = (Time.time - startTime) / Mathf.Max(0.0001f, duration);
        if (life >= 1.0f)
        {
            Apply(1.0f);
            if (holdAtEnd) enabled = false;
            return;
        }
        Apply(life);
    }

    void Apply(float life)
    {
        transform.localScale = axisScale * overLife.Evaluate(life);
    }
}
