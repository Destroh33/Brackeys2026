using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Player Player;
    public Level Level;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameManagers");
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
