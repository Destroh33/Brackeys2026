using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Transform cameraAnchor;

    public float sensitivity = 0.12f;
    public float minPitch = -89f;
    public float maxPitch = 89f;

    public float recoilReturnSpeed = 15f;
    public float recoilReturnMultiplier = 3f;

    private InputAction lookAction;
    private float yaw;
    private float pitch;
    private float pitchRecoil;
    private float yawRecoil;

    private void Awake()
    {
        var map = inputActions.FindActionMap("Player", true);
        lookAction = map.FindAction("Look", true);
        map.Enable();

        yaw = transform.eulerAngles.y;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AddRecoil(float pitchAmount, float yawAmount)
    {
        pitchRecoil += pitchAmount;
        yawRecoil += yawAmount;
    }

    private void LateUpdate()
    {
        Vector2 look = lookAction.ReadValue<Vector2>() * sensitivity;

        yaw += look.x;
        pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);

        float pitchSpeed = Mathf.Max(recoilReturnSpeed, Mathf.Abs(pitchRecoil) * recoilReturnMultiplier);
        float yawSpeed = Mathf.Max(recoilReturnSpeed, Mathf.Abs(yawRecoil) * recoilReturnMultiplier);
        pitchRecoil = Mathf.MoveTowards(pitchRecoil, 0f, pitchSpeed * Time.deltaTime);
        yawRecoil = Mathf.MoveTowards(yawRecoil, 0f, yawSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, yaw + yawRecoil, 0f);
        cameraAnchor.localRotation = Quaternion.Euler(Mathf.Clamp(pitch + pitchRecoil, minPitch, maxPitch), 0f, 0f);
    }
}
