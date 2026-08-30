using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardPanel : MonoBehaviour
{
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] RectTransform content;
    [SerializeField] LeaderboardRow rowTemplate;
    [SerializeField] TMP_Text statusText;
    [SerializeField] int maxRows = 100;

    readonly List<LeaderboardRow> rows = new();

    string playerId;
    bool loading;

    void Awake()
    {
        playerId = PlayerPrefs.GetString("lb_player_id", "");
        if (rowTemplate) rowTemplate.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (rows.Count == 0 && !loading) Refresh();
    }

    public void Refresh()
    {
        if (!loading) StartCoroutine(FetchRoutine());
    }

    IEnumerator FetchRoutine()
    {
        loading = true;
        SetStatus("Loading times...");

        float waited = 0f;
        while (!LeaderboardService.Configured && waited < 3f)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        List<LeaderboardService.Entry> entries = null;
        yield return LeaderboardService.FetchTop(maxRows, result => entries = result);

        loading = false;

        if (entries == null) { SetStatus("Couldn't reach the leaderboard"); yield break; }
        if (entries.Count == 0) { SetStatus("No runs yet - go set one"); yield break; }

        SetStatus(null);
        Build(entries);
        AudioManager.PlayEvent(SfxEvent.UiScreen, AudioBus.Ui);
    }

    void Build(List<LeaderboardService.Entry> entries)
    {
        EnsureRowCount(entries.Count);

        for (int i = 0; i < rows.Count; ++i)
        {
            if (i >= entries.Count)
            {
                rows[i].gameObject.SetActive(false);
                continue;
            }
            rows[i].Bind(i + 1, entries[i], false);
        }

        if (content) LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
    }

    void EnsureRowCount(int needed)
    {
        if (!rowTemplate || !content) return;

        while (rows.Count < needed)
        {
            var row = Instantiate(rowTemplate, content);
            row.name = "Row " + rows.Count;
            rows.Add(row);
        }
    }

    void SetStatus(string message)
    {
        if (!statusText) return;

        statusText.text = message ?? "";
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }
}
