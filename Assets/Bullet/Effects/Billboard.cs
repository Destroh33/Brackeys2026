using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] bool lockUpright = true;

    Camera cam;

    void LateUpdate()
    {
        if (!cam)
        {
            cam = Camera.main;
            if (!cam) return;
        }

        Vector3 forward = transform.position - cam.transform.position;
        if (lockUpright) forward.y = 0.0f;
        if (forward.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
    }
}
