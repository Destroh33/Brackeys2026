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

    public int RunSeed { get; private set; }
    public Checkpoint Active { get; private set; }

    InputSystem_Actions actions;
    GameHud hud;
    DeathScreen deathScreen;
    Player player;
    PlayerDeath playerDeath;
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

        RunSeed = useFixedSeed ? fixedSeed : Random.Range(int.MinValue, int.MaxValue);
        actions = new InputSystem_Actions();

        EnsureHud();
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

        if (startCheckpoint) Activate(startCheckpoint);
    }

    void Update()
    {
        if (dying)
        {
            deathElapsed += Time.unscaledDeltaTime;
            bool ready = deathElapsed >= minimumDeathHold && (!deathScreen || deathScreen.FadedOut);
            if (ready) Restart();
            return;
        }

        if (actions.Player.Restart.WasPressedThisFrame()) Restart();
    }

    public void Activate(Checkpoint checkpoint)
    {
        if (!checkpoint) return;
        if (Active && checkpoint.Order <= Active.Order) return;

        Active = checkpoint;
        LoadRounds();
    }

    public void Restart()
    {
        dying = false;
        deathElapsed = 0f;

        ClearTransients();

        if (Active && Active.Segment) Active.Segment.Respawn();

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
