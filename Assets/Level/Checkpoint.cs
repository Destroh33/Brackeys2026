using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] int order;
    [SerializeField] Transform spawnPoint;
    [SerializeField] LevelSegment segment;
    [SerializeField] CheckpointLoadout loadout;

    List<BulletData> rounds;

    public int Order => order;
    public LevelSegment Segment => segment;
    public Vector3 SpawnPosition => spawnPoint ? spawnPoint.position : transform.position;
    public Quaternion SpawnRotation => spawnPoint ? spawnPoint.rotation : transform.rotation;

    void Reset()
    {
        segment = GetComponentInParent<LevelSegment>();
        spawnPoint = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        var run = RunManager.Instance;
        if (!run || !GameManager.Instance || !GameManager.Instance.Player) return;
        if (other.GetComponentInParent<Player>() != GameManager.Instance.Player) return;

        run.Activate(this);
    }

    public IReadOnlyList<BulletData> Rounds(int runSeed)
    {
        rounds ??= loadout ? loadout.Build(Util.CombineSeed(runSeed, order)) : new List<BulletData>();
        return rounds;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 0.5f, 0.6f);
        Gizmos.DrawWireSphere(SpawnPosition, 0.5f);
        Gizmos.DrawLine(SpawnPosition, SpawnPosition + SpawnRotation * Vector3.forward * 2f);
    }
}
