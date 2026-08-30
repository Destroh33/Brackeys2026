using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

/// <summary>
/// Popup that walks the player through several key prompts in order. Each prompt fires its own
/// callback when answered, and a final callback fires once every prompt has been answered.
/// </summary>
public class KeyPromptSequencePopup : MonoBehaviour
{
    [System.Serializable]
    public class KeyResponse
    {
        public Key Key = Key.None;
        public string Label;
        public UnityEvent Response = new();
    }

    [System.Serializable]
    public class PromptStage
    {
        public string Title;
        public List<KeyResponse> Responses = new();
        public float Duration = 3.0f;
        public UnityEvent AnsweredEvent = new();
    }

    public static KeyPromptSequencePopup Active { get; private set; }

    [SerializeField] TMP_Text titleLabel;
    [SerializeField] TMP_Text promptLabel;
    [SerializeField] Image timerFill;
    [SerializeField] string separator = "    ";
    [SerializeField] List<PromptStage> stages = new();
    [SerializeField] UnityEvent completedEvent = new();
    [SerializeField] UnityEvent timeoutEvent = new();
    [SerializeField] UnityEvent closedEvent = new();
    [SerializeField] string stageCounterFormat = "  {0}/{1}";
    [SerializeField] float closeDelay = 0.12f;
    [SerializeField] bool closeOnComplete = true;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] bool replaceActivePopup = true;

    int stageIndex;
    float stageStartTime;
    float destroyTime;
    bool closing;

    public int StageIndex => stageIndex;
    public bool Completed { get; private set; }

    float Now => useUnscaledTime ? Time.unscaledTime : Time.time;
    PromptStage CurrentStage => stageIndex < stages.Count ? stages[stageIndex] : null;

    void Start()
    {
        if (replaceActivePopup && Active && Active != this) Active.Close();
        Active = this;

        if (stages.Count == 0)
        {
            Complete();
            return;
        }
        ShowStage();
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }

    void Update()
    {
        if (closing)
        {
            if (Now >= destroyTime) Destroy(gameObject);
            return;
        }

        var stage = CurrentStage;
        float duration = stage != null ? stage.Duration : 0.0f;
        float remaining = duration - (Now - stageStartTime);
        if (timerFill) timerFill.fillAmount = duration > 0.0f ? Mathf.Clamp01(remaining / duration) : 1.0f;

        if (stage == null || TryReadResponse(stage)) return;

        if (duration > 0.0f && remaining <= 0.0f)
        {
            timeoutEvent.Invoke();
            Close();
        }
    }

    bool TryReadResponse(PromptStage stage)
    {
        foreach (var response in stage.Responses)
        {
            var control = GetControl(response.Key);
            if (control == null || !control.wasPressedThisFrame) continue;

            response.Response.Invoke();
            stage.AnsweredEvent.Invoke();

            // A callback is allowed to end the sequence early.
            if (closing) return true;

            ++stageIndex;
            if (stageIndex >= stages.Count) Complete();
            else ShowStage();
            return true;
        }
        return false;
    }

    public void Close()
    {
        if (closing) return;
        closing = true;
        destroyTime = Now + closeDelay;
        closedEvent.Invoke();
        if (Active == this) Active = null;
    }

    void ShowStage()
    {
        var stage = CurrentStage;
        stageStartTime = Now;

        string title = stage.Title;
        if (!string.IsNullOrEmpty(stageCounterFormat) && stages.Count > 1)
        {
            title += string.Format(stageCounterFormat, stageIndex + 1, stages.Count);
        }

        if (titleLabel) titleLabel.text = title;
        if (promptLabel) promptLabel.text = BuildPromptText(stage.Responses);
    }

    void Complete()
    {
        Completed = true;
        completedEvent.Invoke();
        if (closeOnComplete) Close();
    }

    string BuildPromptText(List<KeyResponse> responses)
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
