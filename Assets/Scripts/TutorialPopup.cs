using UnityEngine;
using TMPro;

public class TutorialPopup : MonoBehaviour
{
    public GameObject popup;
    public TextMeshProUGUI popupText;
    public bool pauseGameWhileOpen = false;

    [TextArea(3, 10)]
    public string[] messages;

    private int index = 0;

    void Start()
    {
        if (popup == null || popupText == null)
        {
            Debug.LogWarning("TutorialPopup is missing popup references.");
            SetPaused(false);
            return;
        }

        index = 0;
        popup.SetActive(true);

        // If messages are provided, they drive the popup pages.
        // If not, keep whatever text is already authored in PopupText.
        if (messages != null && messages.Length > 0)
        {
            popupText.text = messages[index];
        }

        SetPaused(true);
    }

    public void Next()
    {
        if (messages == null || messages.Length == 0)
        {
            // No paging configured, treat Next as Close.
            Close();
            return;
        }

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
        SetPaused(false);
    }

    void SetPaused(bool shouldPause)
    {
        if (pauseGameWhileOpen)
            Time.timeScale = shouldPause ? 0f : 1f;
    }
}