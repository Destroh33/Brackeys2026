using System.Collections;
using UnityEngine;

/// <summary>
/// Swaps the popup from its opening display to the prompt display, then shows the display for the
/// player's pick and fires that pick's bullet.
/// </summary>
public class RomanceBulletUI : MonoBehaviour
{
    [SerializeField] GameObject firstDisplay;
    [SerializeField] GameObject secondDisplay;
    [SerializeField] float swapDelay = 2.0f;

    [SerializeField] GameObject choiceZeroDisplay;
    [SerializeField] GameObject choiceOneDisplay;
    [SerializeField] GameObject choiceZeroBullet;
    [SerializeField] GameObject choiceOneBullet;

    PlayerGun gun;
    Coroutine swap;

    void Start()
    {
        gun = GameManager.Instance.Player.Gun;
        MusicManager.Push(this, Sfx.MusicRomance);
        Show(firstDisplay, true);
        Show(secondDisplay, false);
        Show(choiceZeroDisplay, false);
        Show(choiceOneDisplay, false);

        swap = StartCoroutine(SwapDisplays());
    }

    IEnumerator SwapDisplays()
    {
        // Realtime, so the swap still runs while the popup holds the game paused.
        yield return new WaitForSecondsRealtime(swapDelay);

        Show(firstDisplay, false);
        Show(secondDisplay, true);
        swap = null;
    }

    /// <summary>Shows the display for the picked option and fires its bullet.</summary>
    public void Choose(int index)
    {
        if (swap != null)
        {
            StopCoroutine(swap);
            swap = null;
        }

        Show(firstDisplay, false);
        Show(secondDisplay, false);
        Show(choiceZeroDisplay, index == 0);
        Show(choiceOneDisplay, index == 1);

        GameObject prefab = index switch
        {
            0 => choiceZeroBullet,
            1 => choiceOneBullet,
            _ => null,
        };
        gun.SpawnAimedBullet(prefab);
    }

    static void Show(GameObject display, bool visible)
    {
        if (display) display.SetActive(visible);
    }
}
