using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] ChamberHud chamber;
    [SerializeField] DeathScreen death;
    [SerializeField] Crosshair crosshair;

    public ChamberHud Chamber => chamber;
    public DeathScreen Death => death;
    public Crosshair Crosshair => crosshair;

    public void SetAlive(bool alive)
    {
        if (chamber) chamber.SetVisible(alive);
        if (crosshair) crosshair.SetVisible(alive);
    }
}
