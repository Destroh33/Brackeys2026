using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    enum Phase { Idle, Dying, Reviving }

    [SerializeField] Image vignette;
    [SerializeField] Image fade;

    [SerializeField] Color vignetteColor = new(0.62f, 0.03f, 0.05f, 1f);
    [SerializeField] float vignetteRiseDuration = 0.18f;
    [SerializeField] float vignettePulseSpeed = 7f;
    [SerializeField] float vignettePulseDepth = 0.16f;
    [SerializeField] float fadeDelay = 0.55f;
    [SerializeField] float fadeDuration = 1.1f;
    [SerializeField] float reviveDuration = 0.5f;

    Phase phase = Phase.Idle;
    float elapsed;

    public bool FadedOut => phase == Phase.Dying && elapsed >= fadeDelay + fadeDuration;

    void Awake()
    {
        Clear();
    }

    public void PlayDeath()
    {
        phase = Phase.Dying;
        elapsed = 0f;
    }

    public void PlayRevive()
    {
        phase = Phase.Reviving;
        elapsed = 0f;
    }

    void Clear()
    {
        if (vignette) vignette.color = WithAlpha(vignetteColor, 0f);
        if (fade) fade.color = new Color(0f, 0f, 0f, 0f);
    }

    void Update()
    {
        if (phase == Phase.Idle) return;

        elapsed += Time.unscaledDeltaTime;

        if (phase == Phase.Dying) TickDying();
        else TickReviving();
    }

    void TickDying()
    {
        float rise = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, vignetteRiseDuration));
        float pulse = 1f - vignettePulseDepth * (0.5f - 0.5f * Mathf.Cos(elapsed * vignettePulseSpeed));
        if (vignette) vignette.color = WithAlpha(vignetteColor, rise * pulse);

        float fadeTime = Mathf.Clamp01((elapsed - fadeDelay) / Mathf.Max(0.0001f, fadeDuration));
        if (fade) fade.color = new Color(0f, 0f, 0f, fadeTime * fadeTime);
    }

    void TickReviving()
    {
        float time = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, reviveDuration));

        if (vignette) vignette.color = WithAlpha(vignetteColor, 0f);
        if (fade) fade.color = new Color(0f, 0f, 0f, 1f - time);

        if (time >= 1f) phase = Phase.Idle;
    }

    static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
}
