using UnityEngine;

public class AphorismBullet : MonoBehaviour
{
    [SerializeField] private BulletVoiceLine[] aphorisms;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AphorismUI aphorismUI;

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
                AudioBus.VoiceLine,
                1f,
                false
            );

            MusicManager.PlayAphorism(voiceHandle);
        }
        AphorismUI ui = Instantiate(aphorismUI);
        ui.Show(aphorism.voiceText);
    }
}
