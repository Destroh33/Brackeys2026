using System.Collections.Generic;
using UnityEngine;

public struct AudioHandle
{
    internal int Index;
    internal int Generation;

    public bool Valid => Generation > 0;
    public static readonly AudioHandle None = default;
}

[DefaultExecutionOrder(-1500)]
public class AudioManager : MonoBehaviour
{
    class Voice
    {
        public AudioSource Source;
        public int Generation;
        public AudioBus Bus;
        public Transform Follow;
        public string Cue;
        public float Gain;
        public bool Active;
        public bool Looping;
        public bool HadFollow;
        public float FadeRate;
        public float Fade;
    }

    struct Scheduled
    {
        public string Cue;
        public float Due;
        public float Volume;
        public Vector3 Position;
        public Transform Follow;
        public bool Spatial;
        public float Jitter;
        public AudioBus? Bus;
    }

    class Duck
    {
        public float Level = 1f;
        public float Target = 1f;
        public float Hold;
        public float Release = 0.4f;
    }

    public const string Root = "Audio/";
    const string ConfigPath = "AudioConfig";

    public static AudioManager Instance { get; private set; }

    AudioConfig config;
    Voice[] voices;
    int generation = 1;

    readonly Dictionary<string, AudioClip[]> library = new();
    readonly Dictionary<string, int> lastVariant = new();
    readonly Dictionary<string, int> livePerCue = new();
    readonly HashSet<string> missing = new();
    readonly Dictionary<AudioBus, Duck> ducks = new();
    readonly List<Scheduled> scheduled = new();
    AudioListener ownListener;

    const string SfxKey = "vol_sfx";
    const string MusicKey = "vol_music";

    static float sfxScale = -1f;
    static float musicScale = -1f;

    public static float SfxVolume
    {
        get
        {
            if (sfxScale < 0f) sfxScale = PlayerPrefs.GetFloat(SfxKey, 1f);
            return sfxScale;
        }
        set
        {
            sfxScale = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, sfxScale);
        }
    }

    public static float MusicVolume
    {
        get
        {
            if (musicScale < 0f) musicScale = PlayerPrefs.GetFloat(MusicKey, 1f);
            return musicScale;
        }
        set
        {
            musicScale = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, musicScale);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance) return;

        var go = new GameObject("Audio Manager");
        DontDestroyOnLoad(go);
        go.AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        config = Resources.Load<AudioConfig>(ConfigPath);
        if (!config) config = ScriptableObject.CreateInstance<AudioConfig>();

        foreach (AudioBus bus in System.Enum.GetValues(typeof(AudioBus)))
        {
            ducks[bus] = new Duck();
        }

        BuildPool();

        ownListener = gameObject.AddComponent<AudioListener>();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshListener();
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        RefreshListener();
    }

    void RefreshListener()
    {
        if (!ownListener) return;

        ownListener.enabled = false;

        foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (listener != ownListener && listener.isActiveAndEnabled) return;
        }

        ownListener.enabled = true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void BuildPool()
    {
        voices = new Voice[config.Voices];

        for (int i = 0; i < voices.Length; ++i)
        {
            var go = new GameObject($"Voice {i}");
            go.transform.SetParent(transform, false);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.rolloffMode = config.Rolloff;
            source.minDistance = config.MinDistance;
            source.maxDistance = config.MaxDistance;
            source.dopplerLevel = 0f;

            voices[i] = new Voice { Source = source };
        }
    }

    public static AudioHandle Play(string cue, float volume = 1f)
    {
        return Instance ? Instance.Spawn(cue, null, Vector3.zero, false, volume, false) : AudioHandle.None;
    }

    public static AudioHandle PlayAt(string cue, Vector3 position, float volume = 1f)
    {
        return Instance ? Instance.Spawn(cue, null, position, true, volume, false) : AudioHandle.None;
    }

    public static AudioHandle PlayOn(string cue, Transform follow, float volume = 1f)
    {
        if (!Instance || !follow) return AudioHandle.None;
        return Instance.Spawn(cue, follow, follow.position, true, volume, false);
    }

    public static AudioHandle Loop(string cue, Transform follow, float volume = 1f, float jitter = -1f)
    {
        if (!Instance) return AudioHandle.None;
        return Instance.Spawn(cue, follow, follow ? follow.position : Vector3.zero, follow != null, volume, true, jitter);
    }

    public static AudioHandle LoopAt(string cue, Vector3 position, float volume = 1f)
    {
        return Instance ? Instance.Spawn(cue, null, position, true, volume, true) : AudioHandle.None;
    }

    public static AudioHandle PlayClip(AudioClip clip, Vector3 position, AudioBus bus = AudioBus.Weapon, float volume = 1f, bool spatial = true)
    {
        return Instance ? Instance.SpawnClip(clip, bus, null, position, spatial, volume, false) : AudioHandle.None;
    }

    public static void SetVolume(AudioHandle handle, float volume)
    {
        var voice = Instance ? Instance.Resolve(handle) : null;
        if (voice != null) voice.Gain = Mathf.Max(0f, volume);
    }

    public static void Stop(AudioHandle handle, float fade = 0.15f)
    {
        Instance?.Release(handle, fade);
    }

    public static bool Playing(AudioHandle handle)
    {
        var voice = Instance ? Instance.Resolve(handle) : null;
        return voice != null && voice.Active;
    }

    public static void PlayEvent(SfxLayer[] layers)
    {
        Instance?.Schedule(layers, null, Vector3.zero, false, 1f);
    }

    public static void PlayEvent(SfxLayer[] layers, AudioBus bus)
    {
        Instance?.Schedule(layers, null, Vector3.zero, false, 1f, bus);
    }

    public static void PlayEventAt(SfxLayer[] layers, Vector3 position, float volume = 1f)
    {
        Instance?.Schedule(layers, null, position, true, volume);
    }

    public static void PlayEventOn(SfxLayer[] layers, Transform follow, float volume = 1f)
    {
        if (Instance && follow) Instance.Schedule(layers, follow, follow.position, true, volume);
    }

    void Schedule(SfxLayer[] layers, Transform follow, Vector3 position, bool spatial, float volume, AudioBus? bus = null)
    {
        if (layers == null) return;

        foreach (var layer in layers)
        {
            if (string.IsNullOrEmpty(layer.Cue)) continue;

            if (layer.Delay <= 0f)
            {
                Spawn(layer.Cue, follow, position, spatial, volume * layer.Volume, false, layer.Jitter, bus);
                continue;
            }

            scheduled.Add(new Scheduled
            {
                Cue = layer.Cue,
                Due = Time.unscaledTime + layer.Delay,
                Volume = volume * layer.Volume,
                Position = position,
                Follow = follow,
                Spatial = spatial,
                Jitter = layer.Jitter,
                Bus = bus,
            });
        }
    }

    void TickScheduled()
    {
        for (int i = scheduled.Count - 1; i >= 0; --i)
        {
            var entry = scheduled[i];
            if (Time.unscaledTime < entry.Due) continue;

            scheduled.RemoveAt(i);
            Vector3 position = entry.Follow ? entry.Follow.position : entry.Position;
            Spawn(entry.Cue, entry.Follow, position, entry.Spatial, entry.Volume, false, entry.Jitter, entry.Bus);
        }
    }

    public static void SetMaster(float volume)
    {
        if (Instance && Instance.config) Instance.config.Master = Mathf.Clamp01(volume);
    }

    public static void DuckBus(AudioBus bus, float level, float hold, float release = 0.5f)
    {
        if (!Instance) return;

        var duck = Instance.ducks[bus];
        duck.Level = Mathf.Min(duck.Level, Mathf.Clamp01(level));
        duck.Target = Mathf.Clamp01(level);
        duck.Hold = hold;
        duck.Release = Mathf.Max(0.01f, release);
    }

    public static void StopBus(AudioBus bus, float fade = 0.2f)
    {
        if (!Instance) return;

        foreach (var voice in Instance.voices)
        {
            if (voice.Active && voice.Bus == bus) Instance.Fade(voice, fade);
        }
    }

    public static void StopAll(float fade = 0.2f)
    {
        if (!Instance) return;

        foreach (var voice in Instance.voices)
        {
            if (voice.Active) Instance.Fade(voice, fade);
        }
    }

    AudioHandle Spawn(string cue, Transform follow, Vector3 position, bool spatial, float volume, bool looping, float jitter = -1f, AudioBus? bus = null)
    {
        var clips = Resolve(cue);
        if (clips.Length == 0) return AudioHandle.None;

        if (!looping && Live(cue) >= config.VoicesPerCue) return AudioHandle.None;

        var clip = Pick(cue, clips);
        var handle = SpawnClip(clip, bus ?? BusFor(cue), follow, position, spatial, volume, looping, jitter);

        var voice = Resolve(handle);
        if (voice != null)
        {
            voice.Cue = cue;
            livePerCue[cue] = Live(cue) + 1;
        }
        return handle;
    }

    AudioHandle SpawnClip(AudioClip clip, AudioBus bus, Transform follow, Vector3 position, bool spatial, float volume, bool looping, float jitter = -1f)
    {
        if (!clip) return AudioHandle.None;

        var voice = Claim();
        if (voice == null) return AudioHandle.None;

        voice.Generation = ++generation;
        voice.Bus = bus;
        voice.Follow = follow;
        voice.HadFollow = follow != null;
        voice.Cue = null;
        voice.Active = true;
        voice.Looping = looping;
        voice.Fade = 1f;
        voice.FadeRate = 0f;
        voice.Gain = Mathf.Max(0f, volume) * (1f + Random.Range(-config.VolumeJitter, config.VolumeJitter));

        var source = voice.Source;
        source.transform.position = follow ? follow.position : position;
        source.clip = clip;
        source.loop = looping;
        source.spatialBlend = spatial ? 1f : 0f;
        float spread = jitter < 0f ? config.PitchJitter : jitter;
        source.pitch = 1f + Random.Range(-spread, spread);
        source.volume = voice.Gain * BusGain(bus);
        source.Play();

        return new AudioHandle { Index = System.Array.IndexOf(voices, voice), Generation = voice.Generation };
    }

    Voice Claim()
    {
        foreach (var voice in voices)
        {
            if (!voice.Active) return voice;
        }

        Voice oldest = null;
        foreach (var voice in voices)
        {
            if (voice.Looping) continue;
            if (oldest == null || voice.Source.time > oldest.Source.time) oldest = voice;
        }

        if (oldest != null) Retire(oldest);
        return oldest;
    }

    Voice Resolve(AudioHandle handle)
    {
        if (!handle.Valid || voices == null) return null;
        if (handle.Index < 0 || handle.Index >= voices.Length) return null;

        var voice = voices[handle.Index];
        return voice.Generation == handle.Generation && voice.Active ? voice : null;
    }

    void Release(AudioHandle handle, float fade)
    {
        var voice = Resolve(handle);
        if (voice != null) Fade(voice, fade);
    }

    void Fade(Voice voice, float fade)
    {
        if (fade <= 0.01f)
        {
            Retire(voice);
            return;
        }
        voice.FadeRate = 1f / fade;
    }

    void Retire(Voice voice)
    {
        if (!voice.Active) return;

        voice.Source.Stop();
        voice.Source.clip = null;
        voice.Active = false;
        voice.Looping = false;
        voice.Follow = null;
        voice.HadFollow = false;
        voice.Generation = 0;

        if (!string.IsNullOrEmpty(voice.Cue))
        {
            livePerCue[voice.Cue] = Mathf.Max(0, Live(voice.Cue) - 1);
            voice.Cue = null;
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        TickDucks(dt);
        TickScheduled();

        foreach (var voice in voices)
        {
            if (!voice.Active) continue;

            if (voice.FadeRate > 0f)
            {
                voice.Fade -= voice.FadeRate * dt;
                if (voice.Fade <= 0f)
                {
                    Retire(voice);
                    continue;
                }
            }

            if (voice.Follow)
            {
                voice.Source.transform.position = voice.Follow.position;
            }
            else if (voice.HadFollow)
            {
                if (voice.Looping)
                {
                    Retire(voice);
                    continue;
                }

                voice.HadFollow = false;
            }

            if (!voice.Looping && !voice.Source.isPlaying)
            {
                Retire(voice);
                continue;
            }

            voice.Source.volume = voice.Gain * voice.Fade * BusGain(voice.Bus);
        }
    }


    void TickDucks(float dt)
    {
        foreach (var duck in ducks.Values)
        {
            if (duck.Hold > 0f)
            {
                duck.Hold -= dt;
                duck.Level = duck.Target;
                continue;
            }

            duck.Level = Mathf.MoveTowards(duck.Level, 1f, dt / duck.Release);
        }
    }

    float BusGain(AudioBus bus)
    {
        float user = bus == AudioBus.Music ? MusicVolume : SfxVolume;
        return config.Master * config.Volume(bus) * ducks[bus].Level * user;
    }

    int Live(string cue) => livePerCue.TryGetValue(cue, out int count) ? count : 0;

    AudioClip Pick(string cue, AudioClip[] clips)
    {
        if (clips.Length == 1) return clips[0];

        int previous = lastVariant.TryGetValue(cue, out int last) ? last : -1;
        int index = Random.Range(0, clips.Length);
        if (index == previous) index = (index + 1) % clips.Length;

        lastVariant[cue] = index;
        return clips[index];
    }

    AudioClip[] Resolve(string cue)
    {
        if (library.TryGetValue(cue, out var cached)) return cached;

        var clips = Resources.LoadAll<AudioClip>(Root + cue);
        if (clips == null || clips.Length == 0)
        {
            var single = Resources.Load<AudioClip>(Root + cue);
            clips = single ? new[] { single } : System.Array.Empty<AudioClip>();
        }

        library[cue] = clips;

        if (clips.Length == 0 && config.WarnOnMissingCue && missing.Add(cue))
        {
            Debug.LogWarning($"Audio cue not found: Resources/{Root}{cue}");
        }

        return clips;
    }

    static AudioBus BusFor(string cue)
    {
        int split = cue.IndexOf('_');
        string head = split > 0 ? cue[..split] : cue;

        return head.ToLowerInvariant() switch
        {
            "weapon" => AudioBus.Weapon,
            "bullet" => AudioBus.Weapon,
            "enemy" => AudioBus.Enemy,
            "player" => AudioBus.Player,
            "ui" => AudioBus.Ui,
            "music" => AudioBus.Music,
            _ => AudioBus.World,
        };
    }
}
