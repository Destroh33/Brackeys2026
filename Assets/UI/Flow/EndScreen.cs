using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text detailText;
    [SerializeField] Button leaderboardButton;
    [SerializeField] Button playAgainButton;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timeText) timeText.text = RunResult.HasRun ? LeaderboardManager.Format(RunResult.Time) : "--:--";

        if (detailText)
        {
            string mode = RunResult.Seeded ? "SEEDED" : "RANDOM";
            detailText.text = RunResult.HasRun
                ? $"{mode}  ·  SEED {RunResult.Seed}  ·  {RunResult.Deaths} DEATHS"
                : "";
        }

        if (leaderboardButton) leaderboardButton.onClick.AddListener(() => ScreenFlow.GoLeaderboard(ScreenFlow.End));
        if (playAgainButton) playAgainButton.onClick.AddListener(ScreenFlow.GoOptions);
    }
}
