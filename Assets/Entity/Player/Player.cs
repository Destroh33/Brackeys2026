using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerGun Gun;
    public Transform Center;

    void Awake()
    {
        GameManager.Instance.Player = this;
    }
}
