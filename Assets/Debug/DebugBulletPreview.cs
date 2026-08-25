using System.Linq;
using UnityEngine;

public class DebugChamberPreview : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text text;

    void Update()
    {
        if (!GameManager.Instance || !GameManager.Instance.Player) return;
        var gun = GameManager.Instance.Player.Gun;
        text.text = string.Join(
            '\n',
            gun.Chambers.Select(
                (x, i) => (
                    x
                    ? (string.IsNullOrWhiteSpace(x.Name) ? "NO NAME :(" : x.Name)
                    : "<empty>"
                ) + (
                    gun.ChamberIndex == i
                    ? "<-"
                    : ""
                )
            )
        );
    }
}
