using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] TMP_Text rankText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text runText;
    [SerializeField] TMP_Text seedText;
    [SerializeField] Button seedButton;

    [SerializeField] Color rowEven = new(0f, 0f, 0f, 0f);
    [SerializeField] Color rowOdd = new(0f, 0f, 0f, 0.08f);
    [SerializeField] Color rowYou = new(1f, 0.831f, 0.047f, 0.16f);

    [SerializeField] Color textMain = new(1f, 1f, 1f);
    [SerializeField] Color textDim = new(0.86f, 0.95f, 1f);
    [SerializeField] Color textYou = new(1f, 0.831f, 0.047f);

    [SerializeField] Color seededColor = new(1f, 0.831f, 0.047f);
    [SerializeField] Color randomColor = new(1f, 1f, 1f);
    [SerializeField] Color copiedColor = new(1f, 0.831f, 0.047f);

    public LeaderboardService.Entry Entry { get; private set; }

    Coroutine feedback;

    void Awake()
    {
        if (seedButton) seedButton.onClick.AddListener(CopySeed);
    }

    public void Bind(int rank, LeaderboardService.Entry entry, bool you)
    {
        Entry = entry;

        Color main = you ? textYou : textMain;
        Color dim = you ? textYou : textDim;

        if (rankText)
        {
            rankText.text = rank.ToString();
            rankText.color = rank <= 3 ? textYou : dim;
            rankText.fontStyle = rank <= 3 ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;
        }
        if (nameText) { nameText.text = Truncate(entry.username); nameText.color = main; }
        if (timeText) { timeText.text = LeaderboardManager.Format(entry.Seconds); timeText.color = main; }

        if (runText)
        {
            runText.text = entry.seeded ? "SEEDED" : "RANDOM";
            runText.color = entry.seeded ? seededColor : randomColor;
        }

        if (seedText)
        {
            seedText.text = Truncate(entry.seed, 18);
            seedText.color = dim;
        }

        if (background)
        {
            background.color = you ? rowYou : (transform.GetSiblingIndex() % 2 == 0 ? rowEven : rowOdd);
        }

        gameObject.SetActive(true);
    }

    void CopySeed()
    {
        if (Entry == null || string.IsNullOrEmpty(Entry.seed)) return;

        Clipboard.Copy(Entry.seed);
        AudioManager.PlayEvent(SfxEvent.UiCopy);

        if (feedback != null) StopCoroutine(feedback);
        feedback = StartCoroutine(ShowCopied());
    }

    IEnumerator ShowCopied()
    {
        if (!seedText) yield break;

        string original = seedText.text;
        Color originalColor = seedText.color;

        seedText.text = "COPIED";
        seedText.color = copiedColor;

        yield return new WaitForSecondsRealtime(0.9f);

        seedText.text = original;
        seedText.color = originalColor;
        feedback = null;
    }

    static string Truncate(string value, int limit = 14)
    {
        if (string.IsNullOrEmpty(value)) return "-";
        return value.Length > limit ? value[..(limit - 1)] + "…" : value;
    }
}
