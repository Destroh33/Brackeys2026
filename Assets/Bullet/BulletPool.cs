using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet Pool", menuName = "Bullet Pool")]
public class BulletPool : ScriptableObject
{
    public List<BulletData> Bullets;

    public BulletData Draw(System.Random random, BulletCategory category)
    {
        float total = 0f;
        foreach (var bullet in Bullets)
        {
            total += Chance(bullet, category);
        }
        if (total <= 0f) return null;

        float roll = (float)random.NextDouble() * total;
        foreach (var bullet in Bullets)
        {
            roll -= Chance(bullet, category);
            if (roll <= 0f) return bullet;
        }
        return null;
    }

    static float Chance(BulletData bullet, BulletCategory category)
    {
        if (!bullet || bullet.Category != category) return 0f;
        return 1f / Mathf.Max(0.01f, bullet.Rarity);
    }
}
