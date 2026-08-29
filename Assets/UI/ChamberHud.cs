using UnityEngine;
using UnityEngine.UI;

public class ChamberHud : MonoBehaviour
{
    [SerializeField] CanvasGroup group;
    [SerializeField] RectTransform cylinder;
    [SerializeField] Image[] dots;

    [SerializeField] Sprite liveSprite;
    [SerializeField] Sprite spentSprite;

    [SerializeField] Color liveColor = new(0.93f, 0.76f, 0.36f, 1f);
    [SerializeField] Color spentColor = new(0.24f, 0.24f, 0.26f, 1f);
    [SerializeField] float currentDotScale = 1.35f;
    [SerializeField] float rotationSmoothing = 14f;
    [SerializeField] float fadeSpeed = 6f;

    PlayerGun gun;
    float angle;
    float targetAlpha = 1f;

    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (!ResolveGun())
        {
            if (group) group.alpha = Mathf.MoveTowards(group.alpha, 0f, fadeSpeed * dt);
            return;
        }

        int count = Mathf.Min(dots.Length, gun.Chambers.Count);
        float step = count > 0 ? 360f / count : 0f;

        angle = Mathf.LerpAngle(angle, -gun.ChamberIndex * step, 1f - Mathf.Exp(-rotationSmoothing * dt));
        if (cylinder) cylinder.localRotation = Quaternion.Euler(0f, 0f, angle);

        for (int i = 0; i < count; ++i)
        {
            var dot = dots[i];
            if (!dot) continue;

            bool live = gun.Chambers[i] != null;
            dot.sprite = live ? liveSprite : spentSprite;
            dot.color = live ? liveColor : spentColor;
            dot.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            dot.rectTransform.localScale = Vector3.one * (i == gun.ChamberIndex ? currentDotScale : 1f);
        }

        if (group) group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * dt);
    }

    bool ResolveGun()
    {
        if (gun) return true;

        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        gun = player ? player.Gun : null;
        return gun != null && gun.Chambers != null;
    }
}
