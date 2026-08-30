using UnityEngine;

public class FlareStrikeWarning : MonoBehaviour
{
    [SerializeField] LineRenderer line;
    [SerializeField] float width = 0.09f;
    [SerializeField] float linger = 0.12f;
    [SerializeField] float defaultLifetime = 0.6f;
    [SerializeField] float texturesPerMeter = 0.12f;
    [SerializeField] float textureScrollSpeed = -1.6f;
    [SerializeField] AnimationCurve intensityOverLife = AnimationCurve.EaseInOut(0.0f, 0.15f, 1.0f, 1.0f);

    MaterialPropertyBlock block;
    Color baseColor;
    float startTime;
    float duration;
    float length;
    float scrollOffset;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        baseColor = line.sharedMaterial.GetColor("_BaseColor");

        startTime = Time.time;
        duration = defaultLifetime;
        length = Vector3.Distance(line.GetPosition(0), line.GetPosition(1));
    }

    public void Draw(Vector3 from, Vector3 to, float lead)
    {
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);

        length = Vector3.Distance(from, to);
        duration = Mathf.Max(0.01f, lead + linger);
        startTime = Time.time;
    }

    void Update()
    {
        float life = (Time.time - startTime) / duration;
        if (life >= 1.0f)
        {
            Destroy(gameObject);
            return;
        }

        float intensity = intensityOverLife.Evaluate(life);
        scrollOffset += textureScrollSpeed * Time.deltaTime;

        line.widthMultiplier = width * intensity;
        line.GetPropertyBlock(block);
        block.SetColor("_BaseColor", baseColor * intensity);
        block.SetVector("_BaseMap_ST", new Vector4(length * texturesPerMeter, 1.0f, scrollOffset, 0.0f));
        line.SetPropertyBlock(block);
    }
}
