using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KahootBullet : MonoBehaviour
{
    PlayerGun gun;

    [SerializeField] private TMP_Text[] answerLabels = new TMP_Text[4];

    [SerializeField] private List<KahootAnswer> answers = new();

    [SerializeField] private GameObject defaultBullet;
    [SerializeField] private GameObject bangBullet;

    [SerializeField] private UnityEngine.Events.UnityEvent correctEvent = new(); 
    [SerializeField] private UnityEngine.Events.UnityEvent incorrectEvent = new();

    private bool answered;
    private List<KahootAnswer> shuffledAnswers = new();
    private KeyPromptPopup popup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicManager.Push(this, Sfx.MusicKahoot);
        gun = GameManager.Instance.Player.Gun;
        popup = GetComponent<KeyPromptPopup>();
        RandomizeAnswers();
        DisplayAnswers();
    }

    private void OnDestroy()
    {
        MusicManager.Pop(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (answered)
        {
            return;
        }
        CheckInput();
    }

    private void RandomizeAnswers()
    {
        shuffledAnswers = new List<KahootAnswer> (answers);
        
        for (int i = shuffledAnswers.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i+1);

            KahootAnswer tmp = shuffledAnswers[i];
            shuffledAnswers[i] = shuffledAnswers[randomIndex];
            shuffledAnswers[randomIndex] = tmp;
        }
    }

    private void DisplayAnswers()
    {
        for (int i = 0; i < answerLabels.Length; i++)
        {
            if (answerLabels[i] == null)
            {
                continue;
            }
            if (i < shuffledAnswers.Count)
            {
                answerLabels[i].text = shuffledAnswers[i].Text;
                answerLabels[i].gameObject.SetActive(true);
            }
            else
            {
                answerLabels[i].gameObject.SetActive(false);
            }
        }
    }

    private void CheckInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            SelectAnswer(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            SelectAnswer(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            SelectAnswer(2);
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            SelectAnswer(3);
        }
    }

    private void SelectAnswer(int index)
    {
        if (index >= shuffledAnswers.Count)
        {
            return;
        }

        answered = true;

        KahootAnswer selectedAnswer = shuffledAnswers[index];

        if (selectedAnswer.IsCorrect)
        {
            correctEvent.Invoke();
        }
        else
        {
            incorrectEvent.Invoke();
        }
        popup.Close();
    }

    public void ShootBullet()
    {
        gun.SpawnAimedBullet(defaultBullet);
    }
    public void ShootBangBullet()
    {
        gun.SpawnAimedBullet(bangBullet);
    }
}
