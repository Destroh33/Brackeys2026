using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsScreen : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] TMP_InputField seedField;
    [SerializeField] Button randomButton;
    [SerializeField] Button enterButton;
    [SerializeField] Button startButton;
    [SerializeField] Button leaderboardButton;

    [SerializeField] Color activeMode = new(1f, 0.83f, 0.05f);
    [SerializeField] Color inactiveMode = new(0.09f, 0.18f, 0.35f);
    [SerializeField] Color activeLabel = new(0.07f, 0.15f, 0.31f);
    [SerializeField] Color inactiveLabel = new(1f, 1f, 1f);

    bool userEntered;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (usernameField)
        {
            usernameField.text = LeaderboardManager.Username;
            usernameField.onValueChanged.AddListener(value => LeaderboardManager.Username = value);
        }

        if (randomButton) randomButton.onClick.AddListener(() => SetMode(false));
        if (enterButton) enterButton.onClick.AddListener(() => SetMode(true));
        if (startButton) startButton.onClick.AddListener(StartRun);
        if (leaderboardButton) leaderboardButton.onClick.AddListener(() => ScreenFlow.GoLeaderboard(ScreenFlow.Options));

        SetMode(RunSeedEntry.WasEntered());
    }

    void SetMode(bool entered)
    {
        userEntered = entered;

        if (seedField)
        {
            seedField.interactable = entered;
            seedField.text = entered ? RunSeedEntry.Peek() : RunSeedEntry.Generate();
        }

        Paint(randomButton, !entered);
        Paint(enterButton, entered);
    }

    void Paint(Button button, bool active)
    {
        if (!button) return;

        var image = button.GetComponent<Image>();
        if (image) image.color = active ? activeMode : inactiveMode;

        var label = button.GetComponentInChildren<TMP_Text>();
        if (label) label.color = active ? activeLabel : inactiveLabel;
    }

    void StartRun()
    {
        string seed = seedField ? seedField.text.Trim() : "";
        if (string.IsNullOrEmpty(seed)) seed = RunSeedEntry.Generate();

        RunSeedEntry.Set(seed, userEntered);
        RunResult.Clear();
        ScreenFlow.GoLevel();
    }
}
