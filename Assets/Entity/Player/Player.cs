using UnityEngine;

[RequireComponent(typeof(EntityHealth))]
public class Player : MonoBehaviour
{
    public PlayerGun Gun;
    public Transform Center;

    EntityHealth health;

    public EntityHealth Health
    {
        get
        {
            if (!health && !TryGetComponent(out health))
            {
                health = gameObject.AddComponent<EntityHealth>();
            }
            return health;
        }
    }

    void Awake()
    {
        _ = Health;

        if (GameManager.Instance) GameManager.Instance.Player = this;
        else Debug.LogError("No GameManager in the scene");
    }
}
