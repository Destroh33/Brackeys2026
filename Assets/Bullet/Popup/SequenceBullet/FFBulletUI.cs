using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class FFBulletUI : MonoBehaviour
{
    [System.Serializable]
    public class Prompt
    {
        public TMP_Text PromptText;
        public TMP_Text CharacterText;
        public AudioClip VoiceClip;
        public List<GameObject> Bullets = new();
    }

    [SerializeField] List<Prompt> prompts = new();
    [SerializeField] Color activeColor = Color.yellow;
    [SerializeField] Color inactiveColor = Color.white;

    int[] choices;
    int promptIndex;
    PlayerGun gun;

    public int PromptIndex => promptIndex;

    void Awake()
    {
        choices = new int[prompts.Count];
        for (int i = 0; i < choices.Length; ++i) choices[i] = -1;
    }

    void Start()
    {
        MusicManager.Push(this, Sfx.MusicFF);
        gun = GameManager.Instance.Player.Gun;
        ShowCurrentPrompt();
    }

    public void Choose(int index)
    {
        if (promptIndex < 0 || promptIndex >= choices.Length) return;
        choices[promptIndex] = index;
    }

    public void AdvancePrompt()
    {
        ++promptIndex;
        ShowCurrentPrompt();
    }

    public void FireBullets()
    {
        for (int i = 0; i < prompts.Count; ++i)
        {
            int choice = choices[i];
            var bullets = prompts[i].Bullets;
            if (choice < 0 || choice >= bullets.Count) continue;

            gun.SpawnAimedBullet(bullets[choice]);
        }
    }

    void ShowCurrentPrompt()
    {
        for (int i = 0; i < prompts.Count; ++i)
        {
            var prompt = prompts[i];
            bool active = i == promptIndex;

            if (active) AudioManager.PlayClip(prompt.VoiceClip, GameManager.Instance.Player.transform.position);

            if (prompt.PromptText) prompt.PromptText.gameObject.SetActive(active);
            if (prompt.CharacterText) prompt.CharacterText.color = active ? activeColor : inactiveColor;
        }

        //switch (promptIndex)
        //{
        //    case 0:
        //        MusicManager.Push(this, Sfx.FFVoiceline1);
        //        break;
        //    case 1:
        //        MusicManager.Push(this, Sfx.FFVoiceline2);
        //        break;
        //    case 2:
        //        MusicManager.Push(this, Sfx.FFVoiceline3);
        //        break;
        //}

        
    }
}
