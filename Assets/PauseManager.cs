using UnityEngine;
using UnityEngine.Events;

public class PauseManager : MonoBehaviour
{
    bool _paused = false;
    public bool Paused
    {
        get => _paused;
        set {
            if (value)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }

    public System.Action PausedEvent;
    public System.Action UnpausedEvent;

    public void Pause()
    {
        if (_paused) return;
        _paused = true;
        Time.timeScale = 0.0f;
        PausedEvent?.Invoke();
    }

    public void Unpause()
    {
        if (!_paused) return;
        _paused = false;
        Time.timeScale = 1.0f;
        UnpausedEvent?.Invoke();
    }
}
