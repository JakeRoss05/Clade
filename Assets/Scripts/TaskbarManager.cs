using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskbarManager : MonoBehaviour
{
    public GameObject tabTemplate;
    public Transform tabParent;

    public GameObject CreateTab(string title, System.Action onClick)
    {
        GameObject tab = Instantiate(tabTemplate, tabParent);
        tab.SetActive(true);

        TextMeshProUGUI text = tab.GetComponentInChildren<TextMeshProUGUI>();
        text.text = title;

        Button button = tab.GetComponent<Button>();
        button.onClick.AddListener(() => onClick());

        return tab;
    }

    public void RemoveTab(GameObject tab)
    {
        Destroy(tab);
    }
}