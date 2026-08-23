using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public Vector3 positionKick = new Vector3(0f, 0.01f, -0.09f);
    public Vector3 rotationKick = new Vector3(-7f, 0f, 2f);
    [Range(0f, 1f)] public float randomness = 0.25f;
    public float recoveryTime = 0.14f;
    public float settleSharpness = 6.6f;

    private const float Euler = 2.71828182845905f;

    private Vector3 posOffset;
    private Vector3 posVel;
    private Vector3 rotOffset;
    private Vector3 rotVel;
    private float omega = 40f;
    private Vector3 basePosition;
    private Quaternion baseRotation;

    private void Awake()
    {
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
    }

    public void Kick()
    {
        omega = SpringUtil.OmegaForSettleTime(recoveryTime, settleSharpness);
        float impulse = omega * Euler;

        posVel += positionKick * impulse;

        float random = Random.Range(-1f, 1f);
        Vector3 rotation = new Vector3(
            rotationKick.x,
            rotationKick.y + rotationKick.y * random * randomness,
            rotationKick.z + rotationKick.z * random * randomness);
        rotVel += rotation * impulse;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        SpringUtil.Damp(ref posOffset, ref posVel, omega, 1f, dt);
        SpringUtil.Damp(ref rotOffset, ref rotVel, omega, 1f, dt);

        transform.localPosition = basePosition + posOffset;
        transform.localRotation = baseRotation * Quaternion.Euler(rotOffset);
    }
}
