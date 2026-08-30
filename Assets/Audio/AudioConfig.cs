using UnityEngine;

public enum AudioBus
{
    World,
    Weapon,
    Enemy,
    Player,
    Ui,
    Music,
    VoiceLine,
}

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio Config")]
public class AudioConfig : ScriptableObject
{
    [Range(0f, 1f)] public float Master = 1f;

    [Range(0f, 1f)] public float World = 0.6f;
    [Range(0f, 1f)] public float Weapon = 0.8f;
    [Range(0f, 1f)] public float Enemy = 0.7f;
    [Range(0f, 1f)] public float Player = 0.7f;
    [Range(0f, 1f)] public float Ui = 0.5f;
    [Range(0f, 1f)] public float Music = 0.4f;
    [Range(0f, 1f)] public float VoiceLine = 1f;

    [Min(8)] public int Voices = 48;
    [Min(1)] public int VoicesPerCue = 4;

    [Range(0f, 0.5f)] public float PitchJitter = 0.06f;
    [Range(0f, 0.5f)] public float VolumeJitter = 0.08f;

    public float MinDistance = 3f;
    public float MaxDistance = 45f;
    public AudioRolloffMode Rolloff = AudioRolloffMode.Logarithmic;

    public bool WarnOnMissingCue = true;

    public float Volume(AudioBus bus)
    {
        return bus switch
        {
            AudioBus.Weapon => Weapon,
            AudioBus.Enemy => Enemy,
            AudioBus.Player => Player,
            AudioBus.Ui => Ui,
            AudioBus.Music => Music,
            AudioBus.VoiceLine => VoiceLine,
            _ => World,
        };
    }
}
