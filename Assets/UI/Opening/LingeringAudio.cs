using UnityEngine;

public class LingeringAudio : MonoBehaviour
{
    public static bool Active { get; private set; }

    AudioSource carried;

    public static bool Carry(AudioSource source)
    {
        if (!source || !source.isPlaying) return false;

        source.transform.SetParent(null, true);
        DontDestroyOnLoad(source.gameObject);

        source.gameObject.AddComponent<LingeringAudio>().carried = source;
        Active = true;

        return true;
    }

    void Update()
    {
        if (carried && carried.isPlaying) return;

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Active = false;
        MusicManager.Silenced = false;
    }
}
