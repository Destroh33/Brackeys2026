using UnityEngine;
using TMPro;

public class RunTimerHud : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] CanvasGroup group;

    [SerializeField] Color idle = new(1f, 1f, 1f, 0.62f);
    [SerializeField] Color accent = new(1f, 0.831f, 0.047f, 1f);
    [SerializeField] float tickScale = 1.07f;
    [SerializeField] float flashDuration = 0.55f;
    [SerializeField] float settleSpeed = 9f;

    RectTransform rect;
    RunManager run;
    int lastSecond = -1;
    float punch;
    float flash;
    int lastCheckpoint = -1;

    void Awake()
    {
        rect = (RectTransform)transform;
    }

    void Update()
    {
        if (!Resolve())
        {
            if (group) group.alpha = 0f;
            return;
        }

        float time = run.RunTime;
        int second = Mathf.FloorToInt(time);

        if (second != lastSecond)
        {
            lastSecond = second;
            punch = 1f;
        }

        int checkpoint = run.Active ? run.Active.Order : -1;
        if (checkpoint != lastCheckpoint)
        {
            lastCheckpoint = checkpoint;
            flash = 1f;
            punch = 1f;
        }

        float dt = Time.unscaledDeltaTime;
        punch = Mathf.MoveTowards(punch, 0f, dt * settleSpeed);
        flash = Mathf.MoveTowards(flash, 0f, dt / flashDuration);

        if (label)
        {
            label.text = Format(time);
            label.color = Color.Lerp(idle, accent, flash);
        }

        float scale = 1f + Mathf.Sin(punch * Mathf.PI) * (tickScale - 1f);
        rect.localScale = new Vector3(scale, scale, 1f);

        if (group) group.alpha = Mathf.MoveTowards(group.alpha, run.RunComplete ? 0f : 1f, dt * 2f);
    }

    bool Resolve()
    {
        if (run) return true;

        run = RunManager.Instance;
        return run;
    }

    public static string Format(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float rest = seconds - minutes * 60f;
        return $"{minutes}:{rest:00.00}";
    }
}
