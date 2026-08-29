using UnityEngine;

public class BulletOwnerIgnore : MonoBehaviour
{
    [SerializeField] float clearDistance = 1.4f;
    [SerializeField] float maxDuration = 4f;

    Transform owner;
    Collider[] ownerColliders;

    public Transform Owner => owner;
    public Collider[] OwnerColliders => ownerColliders;

    Collider[] bulletColliders;
    float elapsed;

    public void Apply(Transform target, Collider[] colliders)
    {
        owner = target;
        ownerColliders = colliders;
        bulletColliders = GetComponentsInChildren<Collider>();

        Toggle(true);
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        bool cleared = !owner || Vector3.Distance(transform.position, owner.position) > clearDistance;
        if (!cleared && elapsed < maxDuration) return;

        Toggle(false);
        Destroy(this);
    }

    void Toggle(bool ignore)
    {
        if (bulletColliders == null || ownerColliders == null) return;

        foreach (var bulletCollider in bulletColliders)
        {
            if (!bulletCollider) continue;

            foreach (var ownerCollider in ownerColliders)
            {
                if (ownerCollider) Physics.IgnoreCollision(bulletCollider, ownerCollider, ignore);
            }
        }
    }
}
