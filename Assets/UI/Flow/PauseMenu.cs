using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] CanvasGroup group;
    [SerializeField] RectTransform card;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] TMP_Text musicValue;
    [SerializeField] TMP_Text sfxValue;
    [SerializeField] Button closeButton;
    [SerializeField] Button resumeButton;
    [SerializeField] Button quitButton;

    [SerializeField] float openDuration = 0.22f;
    [SerializeField] float scaleFrom = 0.9f;

    public bool Paused { get; private set; }

    float restoreTimeScale = 1f;
    float progress;
    PlayerLook look;

    void Awake()
    {
        if (panel) panel.SetActive(false);

        if (musicSlider)
        {
            musicSlider.value = AudioManager.MusicVolume;
            musicSlider.onValueChanged.AddListener(SetMusic);
        }

        if (sfxSlider)
        {
            sfxSlider.value = AudioManager.SfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSfx);
        }

        if (closeButton) closeButton.onClick.AddListener(Resume);
        if (resumeButton) resumeButton.onClick.AddListener(Resume);
        if (quitButton) quitButton.onClick.AddListener(Quit);

        Label(musicValue, AudioManager.MusicVolume);
        Label(sfxValue, AudioManager.SfxVolume);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame) Toggle();

        if (!panel) return;

        float dt = Time.unscaledDeltaTime / Mathf.Max(0.0001f, openDuration);
        progress = Mathf.MoveTowards(progress, Paused ? 1f : 0f, dt);

        if (group) group.alpha = progress;
        if (card) card.localScale = Vector3.one * Mathf.LerpUnclamped(scaleFrom, 1f, Ease(progress));

        if (!Paused && progress <= 0f && panel.activeSelf) panel.SetActive(false);
    }

    static float Ease(float t) => 1f - Mathf.Pow(1f - t, 3f);

    public void Toggle()
    {
        if (Paused) Resume();
        else Open();
    }

    public void Open()
    {
        if (Paused) return;

        Paused = true;
        restoreTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (panel) panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPlayerInput(false);
        AudioManager.DuckBus(AudioBus.World, 0.25f, 9999f, 0.3f);
        AudioManager.PlayEvent(SfxEvent.UiScreen, AudioBus.Ui);
    }

    public void Resume()
    {
        if (!Paused) return;

        Paused = false;
        Time.timeScale = restoreTimeScale <= 0f ? 1f : restoreTimeScale;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetPlayerInput(true);
        AudioManager.DuckBus(AudioBus.World, 1f, 0f, 0.25f);
        AudioManager.PlayEvent(SfxEvent.UiClick, AudioBus.Ui);
    }

    void Quit()
    {
        Time.timeScale = 1f;
        Paused = false;
        ScreenFlow.GoOptions();
    }

    void SetPlayerInput(bool enabled)
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (!player) return;

        if (player.Gun) player.Gun.InputLocked = !enabled;

        if (!look) look = player.GetComponentInChildren<PlayerLook>(true);
        if (look) look.enabled = enabled;
    }

    void SetMusic(float value)
    {
        AudioManager.MusicVolume = value;
        Label(musicValue, value);
    }

    void SetSfx(float value)
    {
        AudioManager.SfxVolume = value;
        Label(sfxValue, value);
        AudioManager.PlayEvent(SfxEvent.UiHover, AudioBus.Ui);
    }

    static void Label(TMP_Text text, float value)
    {
        if (text) text.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
