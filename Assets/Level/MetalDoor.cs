using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MetalDoor : MonoBehaviour
{
    [SerializeField] List<EntityHealth> enemies = new();
    [SerializeField] LevelSegment segment;

    [SerializeField] Vector3 openDirection = Vector3.up;
    [SerializeField] float openTravel = 1.04f;

    [SerializeField] float rattleDuration = 0.6f;
    [SerializeField] float rattleAmplitude = 0.035f;
    [SerializeField] float rattleFrequency = 34f;

    [SerializeField] float slideDuration = 1.25f;
    [SerializeField] AnimationCurve slideCurve = new(new Keyframe(0f, 0f), new Keyframe(0.75f, 1.05f), new Keyframe(1f, 1f));
    [SerializeField] float settleDuration = 0.15f;
    [SerializeField] float settleDrop = 0.06f;

    [SerializeField] float shakeAmplitude = 0.9f;
    [SerializeField] float clunkShake = 2.6f;
    [SerializeField] float shakeRadius = 26f;

    [SerializeField] ParticleSystem dust;
    [SerializeField] Renderer doorRenderer;

    public UnityEvent OpeningEvent = new();
    public UnityEvent OpenedEvent = new();

    readonly List<int> gated = new();
    bool warned;

    Vector3 closedPosition;
    Coroutine routine;
    bool opened;

    public bool Opened => opened;

    void Reset()
    {
        TryGetComponent(out doorRenderer);
    }

    void Awake()
    {
        if (!doorRenderer) TryGetComponent(out doorRenderer);
        closedPosition = transform.localPosition;
    }

    void Start()
    {
        CaptureGated();
    }

    void OnEnable()
    {
        if (segment) segment.Respawned += Rebind;
    }

    void CaptureGated()
    {
        gated.Clear();
        if (!segment) return;

        foreach (var health in enemies)
        {
            if (!health) continue;

            int index = IndexInSegment(health);
            if (index >= 0) gated.Add(index);
            else Debug.LogWarning($"{name}: gated enemy '{health.name}' is not inside {segment.name}, it will never respawn correctly", this);
        }
    }

    int IndexInSegment(EntityHealth health)
    {
        var live = segment.Live;
        for (int i = 0; i < live.Count; ++i)
        {
            if (live[i] && health.transform.IsChildOf(live[i].transform)) return i;
        }
        return -1;
    }

    void OnDisable()
    {
        if (segment) segment.Respawned -= Rebind;
    }

    void Update()
    {
        if (opened || !AllDead()) return;
        Open();
    }

    bool AllDead()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy || enemy.Dead) continue;

            if (!warned && enemy.Invulnerable)
            {
                warned = true;
                Debug.LogWarning($"{name} is waiting on '{enemy.name}', which is invulnerable because its segment is not engaged", this);
            }
            return false;
        }
        return true;
    }

    [ContextMenu("Log Blockers")]
    void LogBlockers()
    {
        foreach (var enemy in enemies)
        {
            if (enemy && !enemy.Dead) Debug.Log($"{name} blocked by '{enemy.name}' (invulnerable: {enemy.Invulnerable})", enemy);
        }
    }

    public void Open()
    {
        if (opened) return;
        opened = true;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(OpenRoutine());
    }

    public void Rebind()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        opened = false;
        transform.localPosition = closedPosition;

        if (!segment) return;

        enemies.Clear();
        foreach (int index in gated)
        {
            if (index < 0 || index >= segment.Live.Count) continue;

            var spawned = segment.Live[index];
            if (!spawned) continue;

            var health = spawned.GetComponentInChildren<EntityHealth>(true);
            if (health) enemies.Add(health);
        }

        warned = false;
    }

    IEnumerator OpenRoutine()
    {
        OpeningEvent?.Invoke();
        if (dust) dust.Play();

        AudioManager.PlayEventAt(SfxEvent.DoorOpen, transform.position);
        yield return Rattle();

        Vector3 travel = transform.InverseTransformDirection(WorldOpenDirection()) * OpenDistance();
        Vector3 target = closedPosition + travel;

        yield return Slide(closedPosition, target);
        yield return Settle(target, travel.normalized);

        transform.localPosition = target;
        Shake(clunkShake);
        OpenedEvent?.Invoke();
        routine = null;
    }

    IEnumerator Rattle()
    {
        float elapsed = 0f;
        Vector3 axis = transform.InverseTransformDirection(WorldOpenDirection());

        while (elapsed < rattleDuration)
        {
            elapsed += Time.deltaTime;

            float ramp = elapsed / Mathf.Max(0.0001f, rattleDuration);
            float offset = Mathf.Sin(elapsed * rattleFrequency) * rattleAmplitude * ramp;
            transform.localPosition = closedPosition + axis * offset;

            Shake(shakeAmplitude * ramp * Time.deltaTime * 60f);
            yield return null;
        }
    }

    IEnumerator Slide(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
            transform.localPosition = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }
    }

    IEnumerator Settle(Vector3 target, Vector3 axis)
    {
        float elapsed = 0f;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            float drop = Mathf.Sin(t * Mathf.PI) * settleDrop * (1f - t);
            transform.localPosition = target - axis * drop;
            yield return null;
        }
    }

    Vector3 WorldOpenDirection()
    {
        Vector3 direction = openDirection.sqrMagnitude < 0.0001f ? Vector3.up : openDirection.normalized;
        return transform.TransformDirection(direction).normalized;
    }

    float OpenDistance()
    {
        Vector3 world = WorldOpenDirection();
        Vector3 size = doorRenderer ? doorRenderer.bounds.size : transform.lossyScale;

        float span = Mathf.Abs(size.x * world.x) + Mathf.Abs(size.y * world.y) + Mathf.Abs(size.z * world.z);
        return span * openTravel;
    }

    void Shake(float amount)
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (!player) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        float falloff = Mathf.Clamp01(1f - distance / Mathf.Max(0.001f, shakeRadius));
        if (falloff <= 0f) return;

        var look = player.GetComponentInChildren<PlayerLook>(true);
        if (!look) return;

        float kick = amount * falloff;
        look.AddRecoil(Random.Range(-kick, kick), Random.Range(-kick, kick));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.7f, 0.2f, 0.9f);
        Vector3 origin = transform.position;
        Gizmos.DrawLine(origin, origin + WorldOpenDirection() * OpenDistance());

        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.7f);
        foreach (var enemy in enemies)
        {
            if (enemy) Gizmos.DrawLine(origin, enemy.transform.position);
        }
    }
}
