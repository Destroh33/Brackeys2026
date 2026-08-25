using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    [SerializeField] LineRenderer core;
    [SerializeField] LineRenderer glow;
    [SerializeField] Transform impact;
    [SerializeField] Light impactLight;
    [SerializeField] LayerMask hitMask = 129;
    [SerializeField] float maxDistance = 200.0f;
    [SerializeField] int maxTargets = 3;
    [SerializeField] float duration = 0.3f;
    [SerializeField] float coreWidth = 0.06f;
    [SerializeField] float glowWidth = 0.3f;
    [SerializeField] float impactLightIntensity = 12.0f;
    [SerializeField] float texturesPerMeter = 0.35f;
    [SerializeField] float textureScrollSpeed = -2.0f;
    [SerializeField] AnimationCurve widthOverLife = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);
    [SerializeField] AnimationCurve intensityOverLife = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);

    float startTime;
    float beamLength;
    float scrollOffset;
    Color coreColor;
    Color glowColor;
    MaterialPropertyBlock block;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        coreColor = core.sharedMaterial.GetColor("_BaseColor");
        glowColor = glow.sharedMaterial.GetColor("_BaseColor");
    }

    void Start()
    {
        startTime = Time.time;
        Fire();
    }

    void Fire()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 end = origin + direction * maxDistance;

        var hits = Physics.RaycastAll(origin, direction, maxDistance, hitMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool blocked = false;
        int hitTargets = 0;
        foreach (var hit in hits)
        {
            var entityHealth = hit.collider.GetComponentInParent<EntityHealth>();
            if (entityHealth)
            {
                if (!entityHealth.Dead)
                {
                    entityHealth.Kill();
                    ++hitTargets;
                }
                if (hitTargets < maxTargets)
                {
                    continue;
                }
            }
            end = hit.point;
            impact.SetPositionAndRotation(hit.point + hit.normal * 0.05f, Quaternion.LookRotation(hit.normal));
            blocked = true;
            break;
        }

        impact.gameObject.SetActive(blocked);
        beamLength = Vector3.Distance(origin, end);

        core.SetPosition(0, origin);
        core.SetPosition(1, end);
        glow.SetPosition(0, origin);
        glow.SetPosition(1, end);
    }

    void Update()
    {
        float life = (Time.time - startTime) / duration;
        if (life >= 1.0f)
        {
            Destroy(gameObject);
            return;
        }

        float width = widthOverLife.Evaluate(life);
        float intensity = intensityOverLife.Evaluate(life);
        scrollOffset += textureScrollSpeed * Time.deltaTime;

        core.widthMultiplier = coreWidth * width;
        glow.widthMultiplier = glowWidth * width;
        ApplyBeamMaterial(core, coreColor * intensity);
        ApplyBeamMaterial(glow, glowColor * intensity);

        if (impactLight)
        {
            impactLight.intensity = impactLightIntensity * intensity;
        }
    }

    void ApplyBeamMaterial(LineRenderer line, Color color)
    {
        line.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetVector("_BaseMap_ST", new Vector4(beamLength * texturesPerMeter, 1.0f, scrollOffset, 0.0f));
        line.SetPropertyBlock(block);
    }
}
