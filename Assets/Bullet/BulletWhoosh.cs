using UnityEngine;

public class BulletWhoosh : MonoBehaviour
{
    [SerializeField] float trailVolume = 0.35f;
    [SerializeField] float whizzVolume = 0.8f;
    [SerializeField] float whizzRadius = 4.5f;

    AudioHandle trail;
    Transform listener;
    float closest = float.MaxValue;
    bool whizzed;

    public void Configure(float trail, float whizz)
    {
        trailVolume = trail;
        whizzVolume = whizz;
    }

    void Start()
    {
        trail = AudioManager.PlayOn(Sfx.Whoosh, transform, trailVolume);

        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        listener = player ? (player.Center ? player.Center : player.transform) : null;
    }

    void Update()
    {
        if (whizzed || !listener) return;

        float distance = Vector3.Distance(transform.position, listener.position);

        if (distance < closest)
        {
            closest = distance;
            return;
        }

        whizzed = true;
        if (closest > whizzRadius) return;

        float nearness = 1f - Mathf.Clamp01(closest / whizzRadius);
        AudioManager.PlayEventAt(SfxEvent.Whizz, transform.position, whizzVolume * nearness);
    }

    void OnDestroy()
    {
        AudioManager.Stop(trail, 0.08f);
    }
}
