using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-800)]
public class LeaderboardManager : MonoBehaviour
{
    [Header("Supabase")]
    [SerializeField] string supabaseReference = "lkmlbpiemvtopfmlyozs";
    [SerializeField] string supabasePublishableKey = "sb_publishable_ZtAXrmIeuXmwHy8fsPDsxQ_AKLvdV7s";

    [Header("Run")]
    [SerializeField] bool submitSeededRuns = true;
    [SerializeField] string fallbackUsername = "anonymous";

    const string PlayerIdKey = "lb_player_id";
    const string UsernameKey = "lb_username";

    string playerId;

    public int LastRank { get; private set; } = -1;
    public bool Submitted { get; private set; }

    public static string Username
    {
        get => PlayerPrefs.GetString(UsernameKey, "");
        set { PlayerPrefs.SetString(UsernameKey, value ?? ""); PlayerPrefs.Save(); }
    }

    public static LeaderboardManager Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LeaderboardService.Configure(supabaseReference, supabasePublishableKey);

        if (!PlayerPrefs.HasKey(PlayerIdKey))
        {
            PlayerPrefs.SetString(PlayerIdKey, System.Guid.NewGuid().ToString("N"));
            PlayerPrefs.Save();
        }
        playerId = PlayerPrefs.GetString(PlayerIdKey);
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start() => Subscribe();

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) => Subscribe();

    void Subscribe()
    {
        if (!RunManager.Instance) return;

        RunManager.Instance.RunFinished -= OnRunFinished;
        RunManager.Instance.RunFinished += OnRunFinished;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (RunManager.Instance) RunManager.Instance.RunFinished -= OnRunFinished;
    }

    void OnRunFinished(float seconds, int deaths)
    {
        StartCoroutine(SubmitRun(seconds, deaths));
    }

    IEnumerator SubmitRun(float seconds, int deaths)
    {
        var run = RunManager.Instance;
        if (!run) yield break;

        if (run.Seeded && !submitSeededRuns)
        {
            Debug.Log("[Leaderboard] Seeded run not submitted");
            yield break;
        }

        int timeMs = Mathf.RoundToInt(seconds * 1000f);
        string username = string.IsNullOrWhiteSpace(Username) ? fallbackUsername : Username;

        Debug.Log($"[Leaderboard] {username} cleared in {Format(seconds)} | seed {run.SeedText} | seeded {run.Seeded} | deaths {deaths}");

        yield return LeaderboardService.Submit(playerId, username, timeMs, run.SeedText, run.Seeded, deaths,
            ok => Submitted = ok);

        if (!Submitted) yield break;

        yield return LeaderboardService.FetchRank(timeMs, rank => LastRank = rank);
        Debug.Log($"[Leaderboard] Global rank #{LastRank}");
    }

    public static string Format(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float rest = seconds - minutes * 60f;
        return $"{minutes}:{rest:00.00}";
    }
}
