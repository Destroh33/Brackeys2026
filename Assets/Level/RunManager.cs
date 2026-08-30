using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-900)]
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [SerializeField] Checkpoint startCheckpoint;
    [SerializeField] GameHud hudPrefab;

    [SerializeField] int fixedSeed;
    [SerializeField] bool useFixedSeed;
    [SerializeField] float minimumDeathHold = 1.4f;
    [SerializeField] bool logLoadout = true;
    [SerializeField] bool loadEndScene = true;
    [SerializeField] float endSceneDelay = 1.2f;

    public int RunSeed { get; private set; }
    public string SeedText { get; private set; }
    public bool Seeded { get; private set; }
    public Checkpoint Active { get; private set; }

    public float RunTime { get; private set; }
    public int Deaths { get; private set; }
    public bool RunActive { get; private set; }
    public bool RunComplete { get; private set; }

    bool sawEnemies;

    public event System.Action<float, int> RunFinished;

    InputSystem_Actions actions;
    GameHud hud;
    DeathScreen deathScreen;
    Player player;
    PlayerDeath playerDeath;
    LevelSegment[] segments;
    bool dying;
    float deathElapsed;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple RunManagers");
            return;
        }
        Instance = this;

        ResolveSeed();
        actions = new InputSystem_Actions();

        EnsureHud();
    }

    void ResolveSeed()
    {
        bool typed = RunSeedEntry.WasEntered();
        string entered = RunSeedEntry.Consume();

        if (!string.IsNullOrWhiteSpace(entered))
        {
            SeedText = entered.Trim();
            RunSeed = SeedText.GetHashCode();
            Seeded = typed;
        }
        else if (useFixedSeed)
        {
            RunSeed = fixedSeed;
            SeedText = fixedSeed.ToString();
            Seeded = true;
        }
        else
        {
            RunSeed = Random.Range(int.MinValue, int.MaxValue);
            SeedText = RunSeed.ToString();
            Seeded = false;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        actions?.Dispose();
    }

    void OnEnable()
    {
        actions.Player.Enable();
    }

    void OnDisable()
    {
        actions.Player.Disable();
    }

    void Start()
    {
        player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (!player)
        {
            Debug.LogError("RunManager found no Player");
            return;
        }

        player.TryGetComponent(out playerDeath);
        if (!playerDeath) playerDeath = player.gameObject.AddComponent<PlayerDeath>();

        player.Health.DeathEvent.AddListener(OnPlayerDied);

        segments = FindObjectsByType<LevelSegment>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (startCheckpoint) Activate(startCheckpoint);

        RunActive = true;
    }

    void Update()
    {
        TickRun();

        if (dying)
        {
            deathElapsed += Time.unscaledDeltaTime;
            bool ready = deathElapsed >= minimumDeathHold && (!deathScreen || deathScreen.FadedOut);
            if (ready) Restart();
            return;
        }

        if (actions.Player.Restart.WasPressedThisFrame()) Restart();
    }

    void TickRun()
    {
        if (!RunActive || RunComplete) return;

        RunTime += Time.deltaTime;

        int remaining = EnemiesRemaining();
        if (remaining > 0)
        {
            sawEnemies = true;
            return;
        }

        if (!sawEnemies) return;

        RunComplete = true;
        RunActive = false;

        RunResult.Record(RunTime, Deaths, SeedText, Seeded);
        RunFinished?.Invoke(RunTime, Deaths);

        if (loadEndScene) StartCoroutine(GoToEnd());
    }

    System.Collections.IEnumerator GoToEnd()
    {
        yield return new WaitForSecondsRealtime(endSceneDelay);
        ScreenFlow.GoEnd();
    }

    public int EnemiesRemaining()
    {
        if (segments == null) return 1;

        int alive = 0;
        foreach (var segment in segments)
        {
            if (!segment) continue;

            foreach (var enemy in segment.Live)
            {
                if (!enemy) continue;

                var health = enemy.GetComponentInChildren<EntityHealth>(true);
                if (health && !health.Dead) ++alive;
            }
        }
        return alive;
    }

    public void Activate(Checkpoint checkpoint)
    {
        if (!checkpoint) return;
        if (Active && checkpoint.Order <= Active.Order) return;

        Active = checkpoint;
        EngageSegment();
        LoadRounds();
    }

    void EngageSegment()
    {
        if (segments == null) return;

        foreach (var segment in segments)
        {
            if (segment) segment.SetEngaged(Active && segment == Active.Segment);
        }
    }

    public void Restart()
    {
        dying = false;
        deathElapsed = 0f;

        ClearTransients();

        if (Active && Active.Segment) Active.Segment.Respawn();
        EngageSegment();

        if (player)
        {
            if (playerDeath) playerDeath.Restore();
            player.Health.Revive();
            PlacePlayer();
        }

        LoadRounds();

        if (hud) hud.SetAlive(true);
        if (deathScreen) deathScreen.PlayRevive();
    }

    void OnPlayerDied()
    {
        ++Deaths;
        dying = true;
        deathElapsed = 0f;

        if (playerDeath) playerDeath.Play();
        if (deathScreen) deathScreen.PlayDeath();
        if (hud) hud.SetAlive(false);
    }

    void LoadRounds()
    {
        if (!Active || !player || !player.Gun) return;

        var rounds = Active.Rounds(RunSeed);
        player.Gun.SetBullets(rounds);
        AudioManager.PlayEventOn(SfxEvent.Reload, player.transform);
        LogLoadout(rounds);
    }

    void LogLoadout(IReadOnlyList<BulletData> rounds)
    {
        if (!logLoadout) return;

        var text = new StringBuilder();
        text.Append("Checkpoint ").Append(Active.Order)
            .Append(" | seed ").Append(RunSeed)
            .Append(" | ").Append(rounds.Count).Append(" rounds");

        for (int i = 0; i < rounds.Count; ++i)
        {
            var round = rounds[i];
            text.AppendLine().Append("  ").Append(i).Append("  ");

            if (!round)
            {
                text.Append("<empty>");
                continue;
            }

            text.Append(string.IsNullOrWhiteSpace(round.Name) ? round.name : round.Name)
                .Append("  [").Append(round.Category).Append(", rarity ").Append(round.Rarity).Append("]");
        }

        Debug.Log(text.ToString(), this);
    }

    void PlacePlayer()
    {
        if (!Active) return;

        if (player.TryGetComponent(out Rigidbody body))
        {
            body.position = Active.SpawnPosition;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        player.transform.SetPositionAndRotation(Active.SpawnPosition, Quaternion.identity);

        var look = player.GetComponentInChildren<PlayerLook>(true);
        if (look) look.SetYaw(Active.SpawnRotation.eulerAngles.y);
    }

    static void ClearTransients()
    {
        DestroyAll<Bullet>();
        DestroyAll<BlockPiece>();
        DestroyAll<SelfDestruct>();
        DestroyAll<PoisonGasCloud>();
    }

    static void DestroyAll<T>() where T : Component
    {
        foreach (var component in FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (component) Destroy(component.gameObject);
        }
    }

    void EnsureHud()
    {
        hud = FindFirstObjectByType<GameHud>(FindObjectsInactive.Include);
        if (!hud)
        {
            var prefab = hudPrefab ? hudPrefab : Resources.Load<GameHud>("GameHud");
            if (prefab) hud = Instantiate(prefab);
        }

        if (hud) hud.name = "Game Hud";
        deathScreen = hud ? hud.Death : null;
    }
}
