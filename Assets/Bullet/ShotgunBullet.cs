using UnityEngine;

public class ShotgunBullet : MonoBehaviour
{
    [SerializeField] GameObject pelletPrefab;
    [SerializeField] float spreadDegrees = 10.0f;
    [SerializeField] int pelletCount = 8;

    void Start()
    {
        for (int i = 0; i < pelletCount; ++i)
        {
            var dir = Util.GetRandomDirectionInCone(spreadDegrees * Mathf.Deg2Rad);
            Instantiate(pelletPrefab, transform.position, transform.rotation * Quaternion.LookRotation(dir));
        }
    }
}
