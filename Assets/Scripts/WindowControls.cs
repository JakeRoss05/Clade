using UnityEngine;

public class WindowControls : MonoBehaviour
{
    public GameObject window;
    public string windowTitle = "Window";

    private GameObject tab;
    private TaskbarManager taskbar;

    void Start()
    {
        taskbar = FindObjectOfType<TaskbarManager>();
    }

    public void Close()
    {
        if (tab != null)
            taskbar.RemoveTab(tab);

        transform.parent.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Minimise()
    {
        window.SetActive(false);

        if (tab == null)
        {
            tab = taskbar.CreateTab(windowTitle, Restore);
        }
    }

    public void Restore()
    {
        window.SetActive(true);

        if (tab != null)
        {
            taskbar.RemoveTab(tab);
            tab = null;
        }
    }

    public void ToggleFullscreen()
    {
        RectTransform rect = window.GetComponent<RectTransform>();

        if (rect.localScale == Vector3.one)
            rect.localScale = new Vector3(1.5f, 1.5f, 1f);
        else
            rect.localScale = Vector3.one;
    }
}