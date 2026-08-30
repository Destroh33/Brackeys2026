using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1400)]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    public static bool Silenced;

    [SerializeField] string bgmCue = Sfx.MusicBgm;
    [SerializeField, Range(0f, 1f)] float bgmVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] float takeoverVolume = 0.85f;
    [SerializeField] float fadeDuration = 0.9f;

    readonly List<Object> owners = new();
    readonly List<string> cues = new();

    AudioHandle bgm;
    AudioHandle takeover;
    string takeoverCue;
    float bgmLevel = 1f;
    float takeoverLevel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance) return;

        var go = new GameObject("Music Manager");
        DontDestroyOnLoad(go);
        go.AddComponent<MusicManager>();
    }

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (Silenced) bgmLevel = 0f;
        bgm = AudioManager.Loop(bgmCue, null, bgmLevel * bgmVolume, 0f);
    }

    public static void Push(Object owner, string cue)
    {
        if (!Instance || !owner || string.IsNullOrEmpty(cue)) return;

        Instance.owners.Add(owner);
        Instance.cues.Add(cue);
    }

    public static void Pop(Object owner)
    {
        if (!Instance || !owner) return;

        int index = Instance.owners.LastIndexOf(owner);
        if (index < 0) return;

        Instance.owners.RemoveAt(index);
        Instance.cues.RemoveAt(index);
    }

    void Update()
    {
        Prune();

        string wanted = cues.Count > 0 ? cues[^1] : null;
        if (wanted != takeoverCue) Swap(wanted);

        float step = Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration);
        bgmLevel = Mathf.MoveTowards(bgmLevel, Silenced || wanted != null ? 0f : 1f, step);
        takeoverLevel = Mathf.MoveTowards(takeoverLevel, wanted == null ? 0f : 1f, step);

        if (!AudioManager.Playing(bgm)) bgm = AudioManager.Loop(bgmCue, null, 0f, 0f);
        AudioManager.SetVolume(bgm, bgmLevel * bgmVolume);

        if (!takeover.Valid) return;

        AudioManager.SetVolume(takeover, takeoverLevel * takeoverVolume);

        if (wanted == null && takeoverLevel <= 0f)
        {
            AudioManager.Stop(takeover, 0.05f);
            takeover = AudioHandle.None;
            takeoverCue = null;
        }
    }

    void Prune()
    {
        for (int i = owners.Count - 1; i >= 0; --i)
        {
            if (!owners[i])
            {
                owners.RemoveAt(i);
                cues.RemoveAt(i);
            }
        }
    }

    void Swap(string wanted)
    {
        if (takeover.Valid) AudioManager.Stop(takeover, 0.2f);

        takeoverCue = wanted;
        takeoverLevel = 0f;

        takeover = string.IsNullOrEmpty(wanted)
            ? AudioHandle.None
            : AudioManager.Loop(wanted, null, 0f, 0f);
    }

    public static void PlayAphorism(AudioHandle voiceHandle)
    {
        if (!Instance || !voiceHandle.Valid)
        {
            return;
        }

        Instance.StartCoroutine(
            Instance.AphorismMusicRoutine(voiceHandle)
        );
    }

    System.Collections.IEnumerator AphorismMusicRoutine(AudioHandle voiceHandle)
    {
        Push(this, Sfx.MusicAphorism);

        while (AudioManager.Playing(voiceHandle))
        {
            yield return null;
        }

        Pop(this);
    }
}
