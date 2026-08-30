using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OpeningCutscene : MonoBehaviour
{
    [SerializeField] CrtScreen screen;
    [SerializeField] AudioSource speaker;
    [SerializeField] Camera view;
    [SerializeField] Image curtain;
    [SerializeField] CanvasGroup prompt;
    [SerializeField] string voiceFallback = "Audio/opening_voice";
    [SerializeField] float cutAt = 16f;
    [SerializeField] float fadeIn = 1.2f;
    [SerializeField] float warmUp = 1.1f;
    [SerializeField] float powerOff = 0.75f;
    [SerializeField] float fadeOut = 0.9f;
    [SerializeField] float promptDelay = 3f;
    [SerializeField] float minimumHold = 0.6f;
    [SerializeField] float silentHold = 4f;
    [SerializeField] float drift = 0.035f;

    Vector3 anchor;
    float elapsed;
    bool leaving;

    void Awake()
    {
        MusicManager.Silenced = true;

        if (!view) view = Camera.main;
        if (view) anchor = view.transform.position;

        if (speaker && !speaker.clip && !string.IsNullOrEmpty(voiceFallback))
        {
            speaker.clip = Resources.Load<AudioClip>(voiceFallback);
        }

        SetCurtain(1f);
        if (prompt) prompt.alpha = 0f;
    }

    void OnDestroy()
    {
        if (!LingeringAudio.Active) MusicManager.Silenced = false;
    }

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        AudioManager.Loop(Sfx.RoomTone, null, 0.35f, 0f);

        yield return Curtain(1f, 0f, fadeIn);

        AudioManager.Play(Sfx.LightBuzz, 0.5f);
        if (screen) screen.TurnOn();

        yield return Hold(warmUp);
        yield return speaker && speaker.clip ? Voice() : Silent();

        Leave();
    }

    IEnumerator Voice()
    {
        double start = AudioSettings.dspTime;
        speaker.Play();

        float cut = cutAt > 0f ? cutAt : speaker.clip.length;

        yield return Until(start, cut - powerOff - fadeOut);
        if (screen) StartCoroutine(screen.TurnOff(powerOff));

        yield return Until(start, cut - fadeOut);
        StartCoroutine(Curtain(0f, 1f, fadeOut));

        yield return Until(start, cut);
    }

    IEnumerator Silent()
    {
        yield return Hold(silentHold);

        if (screen) yield return screen.TurnOff(powerOff);

        yield return Hold(0.35f);
        yield return Curtain(0f, 1f, fadeOut);
    }

    static IEnumerator Until(double start, float seconds)
    {
        while (AudioSettings.dspTime - start < seconds) yield return null;
    }

    IEnumerator Skip()
    {
        leaving = true;
        if (speaker) speaker.Stop();
        if (prompt) prompt.alpha = 0f;

        yield return Curtain(curtain ? curtain.color.a : 0f, 1f, 0.3f);

        Leave();
    }

    void Leave()
    {
        leaving = true;
        LingeringAudio.Carry(speaker);
        AudioManager.StopBus(AudioBus.World, 0.25f);
        ScreenFlow.GoTitle();
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        Drift();

        if (prompt && !leaving)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * 2.4f);
            prompt.alpha = elapsed < promptDelay ? 0f : Mathf.Lerp(0.15f, 0.5f, pulse);
        }

        if (leaving || elapsed < minimumHold || !Pressed()) return;

        StopAllCoroutines();
        StartCoroutine(Skip());
    }

    void Drift()
    {
        if (!view || drift <= 0f) return;

        float sway = Mathf.Sin(elapsed * 0.32f) * drift;
        float bob = Mathf.Sin(elapsed * 0.21f) * drift * 0.6f;
        float push = Mathf.Min(elapsed, 40f) * drift * 0.17f;

        view.transform.position = anchor + new Vector3(sway, bob, push);
    }

    static bool Pressed()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }

    IEnumerator Curtain(float from, float to, float duration)
    {
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.01f, duration))
        {
            SetCurtain(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }

        SetCurtain(to);
    }

    void SetCurtain(float alpha)
    {
        if (curtain) curtain.color = new Color(0f, 0f, 0f, alpha);
    }

    static WaitForSecondsRealtime Hold(float seconds)
    {
        return new WaitForSecondsRealtime(seconds);
    }
}
