using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField] CanvasGroup group;
    [SerializeField] RectTransform[] arms;
    [SerializeField] Image centerDot;

    [SerializeField] float restGap = 9f;
    [SerializeField] float moveGap = 16f;
    [SerializeField] float fireGap = 30f;
    [SerializeField] float skipGap = 18f;
    [SerializeField] float recovery = 11f;
    [SerializeField] float fadeSpeed = 8f;

    float gap;
    float kick;
    float targetAlpha = 1f;

    PlayerMovement movement;

    void OnEnable()
    {
        PlayerGun.Fired += OnFired;
        PlayerGun.Skipped += OnSkipped;
    }

    void OnDisable()
    {
        PlayerGun.Fired -= OnFired;
        PlayerGun.Skipped -= OnSkipped;
    }

    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;
    }

    void OnFired()
    {
        kick = Mathf.Max(kick, fireGap - restGap);
    }

    void OnSkipped()
    {
        kick = Mathf.Max(kick, skipGap - restGap);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        kick = Mathf.MoveTowards(kick, 0f, recovery * dt * Mathf.Max(1f, kick));

        float target = restGap + kick + Mathf.Lerp(0f, moveGap - restGap, MoveWeight());
        gap = Mathf.Lerp(gap, target, 1f - Mathf.Exp(-18f * dt));

        for (int i = 0; i < arms.Length; ++i)
        {
            var arm = arms[i];
            if (!arm) continue;

            Vector2 direction = arm.anchoredPosition.sqrMagnitude > 0.0001f
                ? arm.anchoredPosition.normalized
                : Direction(i);
            arm.anchoredPosition = direction * gap;
        }

        if (group) group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * dt);
        if (centerDot) centerDot.enabled = targetAlpha > 0.5f;
    }

    float MoveWeight()
    {
        if (!ResolveMovement()) return 0f;

        Vector3 velocity = movement.Velocity;
        velocity.y = 0f;
        return Mathf.Clamp01(velocity.magnitude / Mathf.Max(0.01f, movement.maxSpeed));
    }

    bool ResolveMovement()
    {
        if (movement) return true;

        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (player) player.TryGetComponent(out movement);
        return movement;
    }

    static Vector2 Direction(int index)
    {
        return index switch
        {
            0 => Vector2.up,
            1 => Vector2.down,
            2 => Vector2.left,
            _ => Vector2.right,
        };
    }
}
