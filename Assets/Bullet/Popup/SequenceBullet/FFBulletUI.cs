using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Drives an FF-style menu for a <see cref="KeyPromptSequencePopup"/>: one prompt is on screen at a
/// time, its character is highlighted, and the picks are fired as bullets once the sequence ends.
/// </summary>
public class FFBulletUI : MonoBehaviour
{
    [System.Serializable]
    public class Prompt
    {
        public TMP_Text PromptText;
        public TMP_Text CharacterText;
        public List<GameObject> Bullets = new();
    }

    [SerializeField] List<Prompt> prompts = new();
    [SerializeField] Color activeColor = Color.yellow;
    [SerializeField] Color inactiveColor = Color.white;

    int[] choices;
    int promptIndex;
    Transform muzzle;

    public int PromptIndex => promptIndex;

    void Awake()
    {
        choices = new int[prompts.Count];
        for (int i = 0; i < choices.Length; ++i) choices[i] = -1;
    }

    void Start()
    {
        MusicManager.Push(this, Sfx.MusicFF);
        muzzle = GameManager.Instance.Player.Gun.muzzle;
        ShowCurrentPrompt();
    }

    /// <summary>Records which option the player picked for the prompt currently on screen.</summary>
    public void Choose(int index)
    {
        if (promptIndex < 0 || promptIndex >= choices.Length) return;
        choices[promptIndex] = index;
    }

    /// <summary>Shows the next prompt and highlights its character; the rest go dark and white.</summary>
    public void AdvancePrompt()
    {
        ++promptIndex;
        ShowCurrentPrompt();
    }

    /// <summary>Fires the bullet each answered prompt picked.</summary>
    public void FireBullets()
    {
        for (int i = 0; i < prompts.Count; ++i)
        {
            int choice = choices[i];
            var bullets = prompts[i].Bullets;
            if (choice < 0 || choice >= bullets.Count) continue;

            var prefab = bullets[choice];
            if (prefab) Instantiate(prefab, muzzle.position, muzzle.rotation);
        }
    }

    void ShowCurrentPrompt()
    {
        for (int i = 0; i < prompts.Count; ++i)
        {
            var prompt = prompts[i];
            bool active = i == promptIndex;

            if (prompt.PromptText) prompt.PromptText.gameObject.SetActive(active);
            if (prompt.CharacterText) prompt.CharacterText.color = active ? activeColor : inactiveColor;
        }
    }
}
