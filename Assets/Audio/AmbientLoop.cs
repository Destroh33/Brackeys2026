using UnityEngine;

public class AmbientLoop : MonoBehaviour
{
    [SerializeField] string cue = "world_room_tone";
    [SerializeField, Range(0f, 1f)] float volume = 0.6f;
    [SerializeField] bool spatial = true;
    [SerializeField] float fadeIn = 1.2f;

    AudioHandle handle;

    void OnEnable()
    {
        handle = spatial
            ? AudioManager.Loop(cue, transform, volume)
            : AudioManager.LoopAt(cue, transform.position, volume);
    }

    void OnDisable()
    {
        AudioManager.Stop(handle, fadeIn);
    }
}
