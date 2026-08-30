using UnityEngine;

public class ShotgunBullet : MonoBehaviour
{
    [SerializeField] GameObject pelletPrefab;
    [SerializeField] float spreadDegrees = 10.0f;
    [SerializeField] int pelletCount = 8;
    [SerializeField] float spawnOffset = 0.09f;

    void Start()
    {
        TryGetComponent(out BulletOwnerIgnore inherited);

        for (int i = 0; i < pelletCount; ++i)
        {
            var cone = Util.GetRandomDirectionInCone(spreadDegrees * Mathf.Deg2Rad);
            Vector3 direction = transform.rotation * cone;

            var pellet = Instantiate(pelletPrefab, transform.position + direction * spawnOffset, Quaternion.LookRotation(direction));

            pellet.AddComponent<BulletWhoosh>().Configure(0.12f, 0.4f);

            if (inherited && inherited.Owner)
            {
                pellet.AddComponent<BulletOwnerIgnore>().Apply(inherited.Owner, inherited.OwnerColliders);
            }
        }

        Destroy(gameObject);
    }
}
