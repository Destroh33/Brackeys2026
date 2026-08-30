using System.Collections.Generic;
using UnityEngine;

public class PopSign : MonoBehaviour
{
    [SerializeField] Transform popRoot;
    [SerializeField] SpriteRenderer art;
    [SerializeField] List<Sprite> sprites = new();

    [SerializeField] float popDuration = 0.12f;
    [SerializeField] AnimationCurve popCurve = new(new Keyframe(0f, 0f), new Keyframe(0.55f, 1.3f), new Keyframe(1f, 1f));
    [SerializeField] float lifetime = 0.9f;
    [SerializeField] float shrinkDuration = 0.16f;

    [SerializeField] float wiggleAngle = 12f;
    [SerializeField] float wiggleSpeed = 9f;
    [SerializeField] float bobHeight = 0.04f;
    [SerializeField] float bobSpeed = 6f;

    Transform follow;
    Vector3 anchor;
    float height;

    Camera cam;
    Vector3 baseScale = Vector3.one;
    float startTime;

    public void Attach(Transform target, float aboveTarget)
    {
        follow = target;
        height = aboveTarget;
        anchor = target ? target.position : transform.position;
    }

    void Start()
    {
        PickSprite();

        startTime = Time.time;

        if (popRoot)
        {
            baseScale = popRoot.localScale;
            popRoot.localScale = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        float elapsed = Time.time - startTime;

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (follow) anchor = follow.position;
        transform.position = anchor + Vector3.up * (height + Mathf.Sin(elapsed * bobSpeed) * bobHeight);

        if (!popRoot) return;

        popRoot.localScale = baseScale * Scale(elapsed);
        Face(elapsed);
    }

    void PickSprite()
    {
        if (!art && popRoot) art = popRoot.GetComponentInChildren<SpriteRenderer>(true);
        if (!art || sprites == null || sprites.Count == 0) return;

        var pick = sprites[Random.Range(0, sprites.Count)];
        if (pick) art.sprite = pick;
    }

    float Scale(float elapsed)
    {
        float shrinkStart = lifetime - shrinkDuration;

        if (elapsed < popDuration) return popCurve.Evaluate(elapsed / popDuration);
        if (elapsed > shrinkStart) return 1f - Mathf.SmoothStep(0f, 1f, (elapsed - shrinkStart) / shrinkDuration);
        return 1f;
    }

    void Face(float elapsed)
    {
        if (!cam)
        {
            cam = Camera.main;
            if (!cam) return;
        }

        Vector3 forward = popRoot.position - cam.transform.position;
        if (forward.sqrMagnitude < 0.0001f) return;

        float wiggle = Mathf.Sin(elapsed * wiggleSpeed) * wiggleAngle;
        popRoot.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up) * Quaternion.Euler(0f, 0f, wiggle);
    }
}
