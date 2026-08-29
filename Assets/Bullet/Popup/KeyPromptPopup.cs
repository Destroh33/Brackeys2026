using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class KeyPromptPopup : MonoBehaviour
{
    [System.Serializable]
    public class KeyResponse
    {
        public Key Key = Key.None;
        public string Label;
        public UnityEvent Response = new();
    }

    public static KeyPromptPopup Active { get; private set; }

    [SerializeField] TMP_Text titleLabel;
    [SerializeField] TMP_Text promptLabel;
    [SerializeField] Image timerFill;
    [SerializeField] string title = "";
    [SerializeField] string separator = "    ";
    [SerializeField] List<KeyResponse> responses = new();
    [SerializeField] UnityEvent timeoutEvent = new();
    [SerializeField] UnityEvent closedEvent = new();
    [SerializeField] float duration = 3.0f;
    [SerializeField] float closeDelay = 0.12f;
    [SerializeField] bool closeOnResponse = true;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] bool replaceActivePopup = true;

    float openTime;
    float destroyTime;
    bool closing;

    public bool Answered { get; private set; }

    float Now => useUnscaledTime ? Time.unscaledTime : Time.time;

    void Start()
    {
        if (replaceActivePopup && Active && Active != this) Active.Close();
        Active = this;

        openTime = Now;
        if (titleLabel) titleLabel.text = title;
        if (promptLabel) promptLabel.text = BuildPromptText();
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }

    void Update()
    {
        float remaining = duration - (Now - openTime);
        if (timerFill) timerFill.fillAmount = duration > 0.0f ? Mathf.Clamp01(remaining / duration) : 1.0f;

        if (closing)
        {
            if (Now >= destroyTime) Destroy(gameObject);
            return;
        }

        if (TryReadResponse()) return;

        if (duration > 0.0f && remaining <= 0.0f)
        {
            timeoutEvent.Invoke();
            Close();
        }
    }

    bool TryReadResponse()
    {
        foreach (var response in responses)
        {
            var control = GetControl(response.Key);
            if (control == null || !control.wasPressedThisFrame) continue;

            Answered = true;
            response.Response.Invoke();
            if (closeOnResponse) Close();
            return true;
        }
        return false;
    }

    public void SetTitle(string value)
    {
        title = value;
        if (titleLabel) titleLabel.text = value;
    }

    public void Close()
    {
        if (closing) return;
        closing = true;
        destroyTime = Now + closeDelay;
        closedEvent.Invoke();
        if (Active == this) Active = null;
    }

    string BuildPromptText()
    {
        var builder = new StringBuilder();
        foreach (var response in responses)
        {
            if (!IsKey(response.Key)) continue;
            if (builder.Length > 0) builder.Append(separator);
            builder.Append('[').Append(KeyName(response.Key)).Append("] ").Append(response.Label);
        }
        return builder.ToString();
    }

    static string KeyName(Key key)
    {
        string name = GetControl(key)?.displayName;
        return string.IsNullOrWhiteSpace(name) ? key.ToString().ToUpperInvariant() : name.ToUpperInvariant();
    }

    // Key.None and the trailing dummy keys have no control, and the keyboard indexer throws on them.
    static bool IsKey(Key key) => key >= Key.Space && key <= Key.OEM5;

    static KeyControl GetControl(Key key)
    {
        if (!IsKey(key)) return null;
        var keyboard = Keyboard.current;
        return keyboard != null ? keyboard[key] : null;
    }
}
