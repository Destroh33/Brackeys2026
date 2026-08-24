using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerGun Gun;

    void Awake()
    {
        GameManager.Instance.Player = this;
    }
}
