using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [SerializeField] Transform recoilRoot;
    [SerializeField] ParticleSystem flash;
    [SerializeField] Light flashLight;
    [SerializeField] float recoilKick = 0.05f;
    [SerializeField] float recoilPitch = 16.0f;
    [SerializeField] float recoilRecovery = 11.0f;
    [SerializeField] float flashDuration = 0.06f;
    [SerializeField] float flashIntensity = 14.0f;

    Vector3 restPosition;
    Quaternion restRotation;
    float kick;
    float pitch;
    float flashTime;

    void Awake()
    {
        if (recoilRoot)
        {
            restPosition = recoilRoot.localPosition;
            restRotation = recoilRoot.localRotation;
        }
        if (flashLight) flashLight.intensity = 0.0f;
    }

    public void Fire()
    {
        kick = recoilKick;
        pitch = recoilPitch;
        flashTime = flashDuration;

        if (flash) flash.Play();
    }

    void LateUpdate()
    {
        float decay = recoilRecovery * Time.deltaTime;
        kick = Mathf.MoveTowards(kick, 0.0f, recoilKick * decay);
        pitch = Mathf.MoveTowards(pitch, 0.0f, recoilPitch * decay);

        if (recoilRoot)
        {
            recoilRoot.localPosition = restPosition - restRotation * Vector3.forward * kick;
            recoilRoot.localRotation = restRotation * Quaternion.Euler(-pitch, 0.0f, 0.0f);
        }

        if (!flashLight) return;

        flashTime = Mathf.Max(0.0f, flashTime - Time.deltaTime);
        flashLight.intensity = flashIntensity * (flashDuration > 0.0f ? flashTime / flashDuration : 0.0f);
    }
}
