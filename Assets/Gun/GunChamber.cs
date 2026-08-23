using UnityEngine;

public class GunChamber : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.forward;
    public float degreesPerShot = 60f;
    public float smoothTime = 0.07f;

    private Quaternion baseRotation;
    private float currentAngle;
    private float targetAngle;
    private float angleVelocity;

    private void Awake()
    {
        baseRotation = transform.localRotation;
    }

    public void Advance()
    {
        targetAngle += degreesPerShot;
    }

    private void Update()
    {
        if (Mathf.Abs(targetAngle - currentAngle) < 0.01f) return;

        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref angleVelocity, smoothTime);
        transform.localRotation = baseRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
    }
}
