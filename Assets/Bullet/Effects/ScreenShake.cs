using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] float amplitude = 5.0f;
    [SerializeField] float duration = 0.7f;
    [SerializeField] float frequency = 55.0f;
    [SerializeField] float falloffRadius = 35.0f;
    [SerializeField] AnimationCurve envelope = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);

    PlayerLook look;
    float startTime;
    float distanceScale = 1.0f;

    void Start()
    {
        startTime = Time.time;

        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (!player)
        {
            enabled = false;
            return;
        }

        look = player.GetComponentInChildren<PlayerLook>(true);
        float distance = Vector3.Distance(player.transform.position, transform.position);
        distanceScale = Mathf.Clamp01(1.0f - distance / Mathf.Max(0.001f, falloffRadius));
        if (!look || distanceScale <= 0.0f) enabled = false;
    }

    void Update()
    {
        float life = (Time.time - startTime) / Mathf.Max(0.0001f, duration);
        if (life >= 1.0f)
        {
            enabled = false;
            return;
        }

        float kick = amplitude * distanceScale * envelope.Evaluate(life) * frequency * Time.deltaTime;
        look.AddRecoil(Random.Range(-kick, kick), Random.Range(-kick, kick));
    }
}
