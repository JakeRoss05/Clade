using UnityEngine;
using TMPro;

public class TutorialPopup : MonoBehaviour
{
    public GameObject popup;
    public TextMeshProUGUI popupText;

    [TextArea(3, 10)]
    public string[] messages;

    private int index = 0;

    void Start()
    {
        popup.SetActive(true);
        popupText.text = messages[index];

        Time.timeScale = 0f;
    }

    public void Next()
    {
        index++;

        if (index < messages.Length)
        {
            popupText.text = messages[index];
        }
        else
        {
            Close();
        }
    }

    public void Close()
    {
        popup.SetActive(false);
        Time.timeScale = 1f;
    }
}