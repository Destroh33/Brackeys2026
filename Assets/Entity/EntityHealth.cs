using UnityEngine;
using UnityEngine.Events;

public class EntityHealth : MonoBehaviour
{
    [System.NonSerialized] public bool Dead = false;
    public UnityEvent DeathEvent = new();

    public void Kill()
    {
        if (Dead)
        {
            return;
        }
        Dead = true;
        DeathEvent?.Invoke();
    }
}
