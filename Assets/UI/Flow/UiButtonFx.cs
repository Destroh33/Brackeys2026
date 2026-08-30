using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiButtonFx : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] float hoverScale = 1.06f;
    [SerializeField] float pressScale = 0.94f;
    [SerializeField] float punchScale = 1.14f;
    [SerializeField] float smoothing = 16f;
    [SerializeField] float tiltDegrees = 1.5f;
    [SerializeField] bool useStartSound;

    RectTransform rect;
    Button button;
    float target = 1f;
    float current = 1f;
    float punch;
    float tilt;

    void Awake()
    {
        rect = (RectTransform)transform;
        button = GetComponent<Button>();
    }

    void OnDisable()
    {
        target = current = 1f;
        punch = 0f;
        tilt = 0f;
        if (rect) rect.localScale = Vector3.one;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-smoothing * dt));
        punch = Mathf.MoveTowards(punch, 0f, dt * 3.2f);
        tilt = Mathf.Lerp(tilt, target > 1f ? tiltDegrees : 0f, 1f - Mathf.Exp(-10f * dt));

        float scale = current + Mathf.Sin(punch * Mathf.PI) * (punchScale - 1f);
        rect.localScale = new Vector3(scale, scale, 1f);
        rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    bool Interactable => !button || button.interactable;

    public void OnPointerEnter(PointerEventData e)
    {
        if (!Interactable) return;

        target = hoverScale;
        AudioManager.PlayEvent(SfxEvent.UiHover, AudioBus.Ui);
    }

    public void OnPointerExit(PointerEventData e) => target = 1f;

    public void OnPointerDown(PointerEventData e)
    {
        if (!Interactable) return;
        target = pressScale;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!Interactable) return;

        target = hoverScale;
        punch = 1f;
        AudioManager.PlayEvent(useStartSound ? SfxEvent.UiStart : SfxEvent.UiClick, AudioBus.Ui);
    }
}
