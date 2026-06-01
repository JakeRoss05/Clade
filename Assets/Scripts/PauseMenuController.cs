using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseMenuController : MonoBehaviour
{
    GameObject pauseRoot;
    bool paused = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitOnLoad()
    {
        // register to create the pause manager only when a non-menu scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
        // also run once for the currently active scene
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // do not create the pause manager in menu scenes
        if (scene.name == "Main_Menu" || scene.name == "MainMenu" || scene.name.ToLower().Contains("menu"))
            return;

        // create manager if it doesn't exist
        if (GameObject.Find("PauseMenuManager") == null)
        {
            var go = new GameObject("PauseMenuManager");
            DontDestroyOnLoad(go);
            go.AddComponent<PauseMenuController>();
        }
    }

    void Update()
    {
        bool escPressed = false;
        if (Keyboard.current != null)
            escPressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        else
            escPressed = Input.GetKeyDown(KeyCode.Escape);

        if (escPressed)
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        if (paused) Resume(); else Pause();
    }

    void Pause()
    {
        if (pauseRoot != null) return;
        CreateUI();
        paused = true;
        Time.timeScale = 0f;

        // close inventory if open
        var inv = GameObject.Find("InventoryRoot");
        if (inv) Destroy(inv);
    }

    void Resume()
    {
        if (pauseRoot) Destroy(pauseRoot);
        Time.timeScale = 1f;
        paused = false;
    }

    void CreateUI()
    {
        // Canvas
        var canvasGO = new GameObject("PauseCanvas");
        pauseRoot = canvasGO;
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dark overlay
        var overlay = new GameObject("Overlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        var img = overlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.6f);
        var rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Center panel
        var panel = new GameObject("PausePanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0.05f, 0.06f, 0.08f, 0.98f);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.3f, 0.35f);
        prt.anchorMax = new Vector2(0.7f, 0.65f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        outline.effectDistance = new Vector2(2, -2);

        // Title
        var titleGO = new GameObject("PauseTitle");
        titleGO.transform.SetParent(panel.transform, false);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "Paused";
        title.fontSize = 34;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.95f,0.95f,0.98f,1f);
        var titRt = titleGO.GetComponent<RectTransform>();
        titRt.anchorMin = new Vector2(0.1f, 0.7f);
        titRt.anchorMax = new Vector2(0.9f, 0.95f);
        titRt.offsetMin = titRt.offsetMax = Vector2.zero;

        // Buttons container
        var btnContainer = new GameObject("Buttons");
        btnContainer.transform.SetParent(panel.transform, false);
        var brt = btnContainer.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.1f, 0.1f);
        brt.anchorMax = new Vector2(0.9f, 0.6f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        var vlayout = btnContainer.AddComponent<VerticalLayoutGroup>();
        vlayout.spacing = 10;
        vlayout.childControlHeight = true;
        vlayout.childForceExpandHeight = false;
        vlayout.childAlignment = TextAnchor.MiddleCenter;

        // Resume button
        CreateButton(btnContainer.transform, "Resume", () => { Resume(); });

        // Quit to Menu button
        CreateButton(btnContainer.transform, "Quit to Menu", () => { Time.timeScale = 1f; SceneManager.LoadScene("Main_Menu"); });

        // Quit to Desktop button
        CreateButton(btnContainer.transform, "Quit to Desktop", () => {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        });
    }

    void CreateButton(Transform parent, string label, System.Action onClick)
    {
        var btnGO = new GameObject(label + "Button");
        btnGO.transform.SetParent(parent, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.14f,0.16f,0.2f,1f);
        var btn = btnGO.AddComponent<Button>();
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(0, 44);

        // ensure the layout system gives this button a stable height
        var layoutEl = btnGO.AddComponent<LayoutElement>();
        layoutEl.preferredHeight = 44f;
        layoutEl.flexibleWidth = 1f;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 20;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = new Color(0.95f,0.95f,0.98f,1f);
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        if (onClick != null) btn.onClick.AddListener(() => onClick());
    }
}
