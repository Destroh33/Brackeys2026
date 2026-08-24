using UnityEngine;

public class LevelStartTrigger : MonoBehaviour
{
    [SerializeField] Level level;

    void Reset()
    {
        level = GetComponentInParent<Level>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (level.IsActiveLevel)
        {
            return;
        }
        if (other.gameObject != GameManager.Instance.Player.gameObject)
        {
            return;
        }
        level.StartLevel();
    }
}
