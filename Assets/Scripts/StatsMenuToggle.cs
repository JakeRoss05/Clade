using UnityEngine;
using UnityEngine.InputSystem;

public class StatsMenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject statsMenu;
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool lockCursorWhenClosed = true;

    private bool isOpen;

    private void Start()
    {
        if (statsMenu != null)
        {
            statsMenu.SetActive(false);
        }

        ApplyOpenState(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ApplyOpenState(!isOpen);
        }

        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ApplyOpenState(false);
        }
    }

    public void ApplyOpenState(bool open)
    {
        isOpen = open;

        if (statsMenu != null)
        {
            statsMenu.SetActive(isOpen);
        }

        if (pauseGameWhileOpen)
        {
            Time.timeScale = isOpen ? 0f : 1f;
        }

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : (lockCursorWhenClosed ? CursorLockMode.Locked : CursorLockMode.None);
    }
}