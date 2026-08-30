using UnityEngine;

public class AphorismBullet : MonoBehaviour
{
    [SerializeField] private BulletVoiceLine[] aphorisms;
    [SerializeField] private Transform playerTransform;

    public void PlayRandomAphorism()
    {
        if (aphorisms == null || aphorisms.Length == 0)
        {
            return;
        }

        BulletVoiceLine aphorism =
            aphorisms[Random.Range(0, aphorisms.Length)];

        if (aphorism.voiceClip)
        {
            AudioHandle voiceHandle = AudioManager.PlayClip(
                aphorism.voiceClip,
                playerTransform.position,
                AudioBus.VoiceLine
            );

            MusicManager.PlayAphorism(voiceHandle);
        }

        Debug.Log(aphorism.voiceText);
    }
}
