using UnityEngine;
using UnityEngine.UI;

public class LeaderboardScreen : MonoBehaviour
{
    [SerializeField] Button backButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button titleButton;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (backButton) backButton.onClick.AddListener(ScreenFlow.GoBack);
        if (optionsButton) optionsButton.onClick.AddListener(ScreenFlow.GoOptions);
        if (titleButton) titleButton.onClick.AddListener(ScreenFlow.GoTitle);
    }
}
