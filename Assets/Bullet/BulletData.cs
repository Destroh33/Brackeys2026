using UnityEngine;

public enum BulletCategory
{
    Neutral,
    Good,
    Bad,
}

[CreateAssetMenu(fileName = "BulletData", menuName = "Bullet Data")]
public class BulletData : ScriptableObject
{
    public string Name;
    public GameObject Prefab;

    public BulletCategory Category = BulletCategory.Neutral;
    [Min(0.01f)] public float Rarity = 1f;
}
