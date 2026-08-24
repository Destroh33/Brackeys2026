using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class LevelStartBullets
{
    public int Count = 0;
    public BulletPool Pool;
}

public class Level : MonoBehaviour
{
    [System.NonSerialized]
    public List<Enemy> Enemies;

    [SerializeField]
    List<LevelStartBullets> StartBullets;

    public bool IsActiveLevel => GameManager.Instance.Level == this;

    public void StartLevel()
    {
        GameManager.Instance.Level = this;
        LoadBullets();
        OnStarted();
    }

    virtual protected void OnStarted() {}

    void LoadBullets()
    {
        List<BulletData> bullets = new();
        foreach (var startBullets in StartBullets)
        {
            var sample = startBullets.Pool.Bullets.Sample(startBullets.Count);
            bullets.AddRange(sample);
        }
        GameManager.Instance.Player.Gun.SetBullets(bullets);
    }
}
