using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Checkpoint Loadout", menuName = "Checkpoint Loadout")]
public class CheckpointLoadout : ScriptableObject
{
    public BulletPool Pool;

    [Min(0)] public int Good = 2;
    [Min(0)] public int Neutral = 3;
    [Min(0)] public int Bad = 1;

    public bool Shuffle = true;

    public List<BulletData> Build(int seed)
    {
        List<BulletData> rounds = new();
        if (!Pool) return rounds;

        var random = new System.Random(seed);
        Take(rounds, random, BulletCategory.Neutral, Neutral);
        Take(rounds, random, BulletCategory.Good, Good);
        Take(rounds, random, BulletCategory.Bad, Bad);

        if (Shuffle) rounds.Shuffle(random);
        return rounds;
    }

    void Take(List<BulletData> rounds, System.Random random, BulletCategory category, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            var bullet = Pool.Draw(random, category);
            if (bullet) rounds.Add(bullet);
        }
    }
}
