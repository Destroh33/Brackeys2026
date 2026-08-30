using System.Collections;
using UnityEngine;

public class FlareBullet : Bullet
{
    [SerializeField] Transform glow;
    [SerializeField] Light burnLight;
    [SerializeField] TrailRenderer smoke;
    [SerializeField] GameObject strikePrefab;
    [SerializeField] GameObject warningPrefab;
    [SerializeField] float speed = 20.0f;
    [SerializeField] float armDelay = 1.6f;
    [SerializeField] int strikeCount = 12;
    [SerializeField] float strikeInterval = 0.4f;
    [SerializeField] float strikeRadius = 8.0f;
    [SerializeField] float maxTiltDegrees = 22.0f;
    [SerializeField] float spawnDistance = 50.0f;
    [SerializeField] float warningLead = 0.55f;
    [SerializeField] float burnOut = 1.5f;
    [SerializeField] float flickerSpeed = 11.0f;
    [SerializeField] float flickerAmount = 0.3f;
    [SerializeField] float bounceInterval = 0.25f;

    float lightIntensity;
    Vector3 glowScale;
    float flickerSeed;
    float nextBounceTime;

    void Start()
    {
        RB.linearVelocity = transform.forward * speed;

        if (burnLight) lightIntensity = burnLight.intensity;
        if (glow) glowScale = glow.localScale;
        flickerSeed = Random.value * 100.0f;

        AudioManager.PlayEventOn(SfxEvent.FlareIgnite, transform);
        StartCoroutine(BurnCoro());
    }

    void Update()
    {
        float flicker = 1.0f + (Mathf.PerlinNoise(flickerSeed, Time.time * flickerSpeed) - 0.5f) * 2.0f * flickerAmount;
        if (burnLight) burnLight.intensity = lightIntensity * flicker;
        if (glow) glow.localScale = glowScale * flicker;
    }

    // the flare itself is inert: it bounces off whatever it lands on and never damages anything
    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsBullet(collision) || Time.time < nextBounceTime) return;

        nextBounceTime = Time.time + bounceInterval;
        AudioManager.PlayEventAt(SfxEvent.BangBounce, collision.GetContact(0).point);
    }

    IEnumerator BurnCoro()
    {
        yield return new WaitForSeconds(armDelay);

        for (int i = 0; i < strikeCount; ++i)
        {
            StartCoroutine(StrikeCoro());
            yield return new WaitForSeconds(strikeInterval);
        }

        yield return new WaitForSeconds(warningLead + burnOut);
        Extinguish();
    }

    IEnumerator StrikeCoro()
    {
        if (!strikePrefab) yield break;

        Vector3 target = RandomPointAroundFlare();
        Vector3 skyward = RandomSkywardDirection();
        Vector3 spawn = target + skyward * spawnDistance;
        Quaternion rotation = Quaternion.LookRotation(-skyward);

        if (warningPrefab)
        {
            var warning = Instantiate(warningPrefab, target, rotation);
            if (warning.TryGetComponent(out FlareStrikeWarning guide)) guide.Draw(spawn, target, warningLead);
        }

        yield return new WaitForSeconds(warningLead);

        var strike = Instantiate(strikePrefab, spawn, rotation);
        if (strike.TryGetComponent(out FlareStrike projectile)) projectile.Launch(target);
    }

    Vector3 RandomPointAroundFlare()
    {
        Vector2 offset = Random.insideUnitCircle * strikeRadius;
        Vector3 position = transform.position;
        return new Vector3(position.x + offset.x, position.y, position.z + offset.y);
    }

    // cones from Util point along +Z, so tilt one onto the world vertical
    Vector3 RandomSkywardDirection()
    {
        return Quaternion.Euler(-90.0f, 0.0f, 0.0f) * Util.GetRandomDirectionInCone(maxTiltDegrees * Mathf.Deg2Rad);
    }

    void Extinguish()
    {
        if (smoke)
        {
            smoke.transform.SetParent(null, true);
            smoke.emitting = false;
            Destroy(smoke.gameObject, smoke.time + 0.1f);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.45f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, strikeRadius);
    }
}
