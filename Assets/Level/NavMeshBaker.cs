using Unity.AI.Navigation;
using UnityEngine;

[DefaultExecutionOrder(-2000)]
[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshBaker : MonoBehaviour
{
    [SerializeField] NavMeshSurface surface;

    void Reset()
    {
        TryGetComponent(out surface);
    }

    void Awake()
    {
        if (!surface) TryGetComponent(out surface);
        if (surface) surface.BuildNavMesh();
    }
}
