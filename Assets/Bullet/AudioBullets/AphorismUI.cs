using TMPro;
using UnityEngine;

public class AphorismUI : MonoBehaviour
{
    [SerializeField] private TMP_Text aphorismText;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        Destroy(gameObject, displayDuration);
    }

    public void Show(string text)
    {
        aphorismText.text = text;
    }
}