using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelSegment : MonoBehaviour
{
    class SpawnRecord
    {
        public GameObject Template;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [SerializeField] Transform enemyRoot;

    readonly List<SpawnRecord> records = new();
    readonly List<GameObject> live = new();

    Transform templateRoot;

    public int EnemyCount => records.Count;

    public event System.Action Respawned;

    bool engaged;
    public bool Engaged => engaged;

    void Awake()
    {
        if (!enemyRoot) enemyRoot = transform;
        Capture();
        ApplyEngagement();
    }

    public void SetEngaged(bool value)
    {
        engaged = value;
        ApplyEngagement();
    }

    void ApplyEngagement()
    {
        foreach (var enemy in live)
        {
            if (!enemy) continue;

            foreach (var health in enemy.GetComponentsInChildren<EntityHealth>(true))
            {
                health.Invulnerable = !engaged;
            }
            foreach (var brain in enemy.GetComponentsInChildren<Enemy>(true))
            {
                brain.enabled = engaged;
            }
        }
    }

    void Capture()
    {
        var holder = new GameObject("Templates");
        holder.SetActive(false);
        holder.transform.SetParent(transform, false);
        templateRoot = holder.transform;

        for (int i = 0; i < enemyRoot.childCount; ++i)
        {
            var child = enemyRoot.GetChild(i);
            if (child == templateRoot) continue;
            if (!child.GetComponentInChildren<Enemy>(true)) continue;

            var template = Instantiate(child.gameObject, templateRoot);
            template.name = child.name;

            records.Add(new SpawnRecord
            {
                Template = template,
                Position = child.position,
                Rotation = child.rotation,
            });
            live.Add(child.gameObject);
        }
    }

    public void Respawn()
    {
        foreach (var enemy in live)
        {
            if (enemy) Destroy(enemy);
        }
        live.Clear();

        foreach (var record in records)
        {
            var enemy = Instantiate(record.Template, record.Position, record.Rotation, enemyRoot);
            enemy.name = record.Template.name;
            enemy.SetActive(true);
            Warp(enemy, record.Position);
            live.Add(enemy);
        }

        ApplyEngagement();
        Respawned?.Invoke();
    }

    static void Warp(GameObject enemy, Vector3 position)
    {
        if (!enemy.TryGetComponent(out NavMeshAgent agent)) return;
        if (agent.isOnNavMesh) agent.Warp(position);
    }
}
