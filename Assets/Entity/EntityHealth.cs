using UnityEngine;
using UnityEngine.Events;

public class EntityHealth : MonoBehaviour
{
    [System.NonSerialized] public bool Dead = false;
    [System.NonSerialized] public bool Invulnerable = false;
    public UnityEvent DeathEvent = new();
    public UnityEvent ReviveEvent = new();

    public void Kill()
    {
        if (Dead || Invulnerable)
        {
            return;
        }
        Dead = true;
        DeathEvent?.Invoke();
    }

    public void Revive()
    {
        if (!Dead)
        {
            return;
        }
        Dead = false;
        ReviveEvent?.Invoke();
    }
}
