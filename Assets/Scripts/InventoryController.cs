using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    static InventoryController instance;

    GameObject inventoryRoot;
    RenderTexture previewTexture;
    Camera previewCamera;
    GameObject previewModel;
    int previewLayer = 31;
    static readonly Vector3 previewOrigin = new Vector3(50000f, 50000f, 50000f);

    PlayerLevel playerLevel;
    GameObject player;

    [Header("Window Template")]
    public GameObject sceneWindowTemplate;
    public bool keepTemplateTransform = true;

    [Header("Window Reference")]
    public RectTransform windowSizeReference;
    public bool useWelcomePopupAsReference = true;
    public string welcomeWindowTitle = "Welcome";

    [Header("Window")]
    public string windowTitle = "Stats";
    public Vector2 windowSize = new Vector2(760f, 440f);
    public bool autoResizeWindow = true;
    public Vector2 minWindowSize = new Vector2(700f, 420f);
    public Vector2 maxWindowSize = new Vector2(1250f, 780f);
    [Range(0.45f, 0.7f)] public float previewPanelWidth = 0.58f;
    public bool dimBackground = false;
    public Color overlayColor = new Color(0f, 0f, 0f, 0.45f);
    public Color windowBodyColor = new Color(0.06f, 0.08f, 0.12f, 0.96f);
    public Color windowBorderColor = new Color(0.20f, 0.30f, 0.40f, 1f);
    public Color titleBarColor = new Color(0.10f, 0.14f, 0.20f, 0.96f);
    public Color accentColor = new Color(0.35f, 0.85f, 1f, 1f);

    [Header("Panel Backgrounds")]
    public bool showInnerPanelBackgrounds = false;
    public Color previewPanelColor = new Color(0.05f, 0.07f, 0.10f, 0.98f);
    public Color previewPanelBorderColor = new Color(0.18f, 0.26f, 0.34f, 1f);
    public Color optionsPanelColor = new Color(0.03f, 0.04f, 0.06f, 0.96f);
    public Color optionsPanelBorderColor = new Color(0.12f, 0.15f, 0.20f, 1f);

    [Header("Text Style")]
    public TMP_FontAsset menuFont;
    public FontStyles menuFontStyle = FontStyles.Normal;
    public Color headerTextColor = new Color(0.35f, 0.85f, 1f, 1f);
    public Color bodyTextColor = new Color(0.90f, 0.95f, 1f, 1f);
    public Color secondaryTextColor = new Color(0.78f, 0.86f, 0.95f, 1f);
    public Color tertiaryTextColor = new Color(0.86f, 0.90f, 0.95f, 1f);
    public float headerTextSize = 18f;
    public float sectionTextSize = 16f;
    public float statsTextSize = 20f;
    public float optionTitleTextSize = 21f;
    public float optionDescriptionTextSize = 14f;
    public float progressTextSize = 15f;

    TaskbarManager taskbarManager;
    GameObject taskbarTab;
    bool isMinimized = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            bool thisConfigured = sceneWindowTemplate != null || windowSizeReference != null;
            bool existingConfigured = instance.sceneWindowTemplate != null || instance.windowSizeReference != null;

            if (thisConfigured && !existingConfigured)
            {
                if (instance != null)
                    Destroy(instance.gameObject);
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        instance = this;
        // ensure a single manager exists at runtime
        DontDestroyOnLoad(gameObject);

        if (sceneWindowTemplate != null && sceneWindowTemplate.scene.IsValid() && sceneWindowTemplate.activeSelf)
            sceneWindowTemplate.SetActive(false);

        player = GameObject.FindWithTag("Player");
        if (player) playerLevel = player.GetComponent<PlayerLevel>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitOnLoad()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureManagerForScene(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureManagerForScene(scene);
    }

    static void EnsureManagerForScene(Scene scene)
    {
        string sceneName = scene.name.ToLowerInvariant();
        if (sceneName == "main_menu" || sceneName == "mainmenu" || sceneName.Contains("menu"))
            return;

        var managers = FindObjectsByType<InventoryController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers.Length > 0)
        {
            InventoryController preferred = null;
            for (int i = 0; i < managers.Length; i++)
            {
                var candidate = managers[i];
                if (candidate == null)
                    continue;

                if (preferred == null)
                    preferred = candidate;

                bool hasTemplate = candidate.sceneWindowTemplate != null || candidate.windowSizeReference != null;
                bool preferredHasTemplate = preferred.sceneWindowTemplate != null || preferred.windowSizeReference != null;
                if (hasTemplate && !preferredHasTemplate)
                    preferred = candidate;
            }

            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] != null && managers[i] != preferred)
                    Destroy(managers[i].gameObject);
            }

            instance = preferred;
            return;
        }

        var go = new GameObject("InventoryManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<InventoryController>();
    }

    void Update()
    {
        bool tabPressed = false;
        if (Keyboard.current != null)
            tabPressed = Keyboard.current.tabKey.wasPressedThisFrame;
        else
            tabPressed = Input.GetKeyDown(KeyCode.Tab);

        if (tabPressed)
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        if (inventoryRoot == null)
        {
            Open();
            return;
        }

        if (isMinimized || !inventoryRoot.activeSelf)
        {
            Restore();
            return;
        }

        Close();
    }

    void Open()
    {
        if (inventoryRoot == null)
            CreateUI();

        if (taskbarManager == null)
            taskbarManager = FindFirstObjectByType<TaskbarManager>();

        RemoveTaskbarTab();

        inventoryRoot.SetActive(true);
        isMinimized = false;
        CreatePreview();
        PopulateOptions();
        FitWindowToContent();
    }

    void Close()
    {
        RemoveTaskbarTab();

        if (inventoryRoot) Destroy(inventoryRoot);
        if (previewCamera)
        {
            previewCamera.targetTexture = null;
            Destroy(previewCamera.gameObject);
        }
        if (previewTexture) Destroy(previewTexture);
        if (previewModel) Destroy(previewModel);
        inventoryRoot = null;
        previewCamera = null;
        previewTexture = null;
        previewModel = null;
        isMinimized = false;
    }

    void Minimise()
    {
        if (inventoryRoot == null || !inventoryRoot.activeSelf)
            return;

        if (taskbarManager == null)
            taskbarManager = FindFirstObjectByType<TaskbarManager>();

        if (taskbarManager != null && taskbarTab == null)
            taskbarTab = taskbarManager.CreateTab(windowTitle, Restore);

        inventoryRoot.SetActive(false);
        isMinimized = true;
    }

    void Restore()
    {
        if (inventoryRoot == null)
        {
            Open();
            return;
        }

        inventoryRoot.SetActive(true);
        isMinimized = false;

        RemoveTaskbarTab();

        CreatePreview();
        PopulateOptions();
        FitWindowToContent();
    }

    void CreateUI()
    {
        if (inventoryRoot) return;
        // Canvas
        var canvasGO = new GameObject("InventoryCanvas");
        inventoryRoot = canvasGO;
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        if (dimBackground)
        {
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasGO.transform, false);
            var img = overlay.AddComponent<Image>();
            img.color = overlayColor;
            var rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        var popupBox = CreateWindowRoot(canvasGO.transform);
        if (popupBox == null)
            return;

        var contentRoot = new GameObject("Content");
        contentRoot.transform.SetParent(popupBox.transform, false);
        var contentRect = contentRoot.AddComponent<RectTransform>();
        float titleBarHeight = GetTitleBarHeight(popupBox);
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.offsetMin = new Vector2(16f, 16f);
        contentRect.offsetMax = new Vector2(-16f, -(titleBarHeight + 8f));

        float previewMaxX = Mathf.Clamp(previewPanelWidth, 0.45f, 0.7f);
        float optionsMinX = Mathf.Clamp(previewMaxX + 0.02f, 0.5f, 0.9f);

        var previewPanel = CreateWindowPanel(contentRoot.transform, "PreviewPanel", new Vector2(0f, 0f), new Vector2(previewMaxX, 1f), previewPanelColor, previewPanelBorderColor);
        var optionsPanel = CreateWindowPanel(contentRoot.transform, "OptionsPanel", new Vector2(optionsMinX, 0f), new Vector2(1f, 1f), optionsPanelColor, optionsPanelBorderColor);

        var previewHeader = CreateText(previewPanel.transform, "PreviewHeader", "Live Preview", headerTextSize, TextAlignmentOptions.Left, headerTextColor);
        var previewHeaderRect = previewHeader.rectTransform;
        previewHeaderRect.anchorMin = new Vector2(0f, 1f);
        previewHeaderRect.anchorMax = new Vector2(1f, 1f);
        previewHeaderRect.offsetMin = new Vector2(12f, -30f);
        previewHeaderRect.offsetMax = new Vector2(-12f, -4f);

        var statsGO = new GameObject("StatsText");
        statsGO.transform.SetParent(previewPanel.transform, false);
        var stats = statsGO.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(stats, "", statsTextSize, TextAlignmentOptions.BottomLeft, bodyTextColor);
        var sRt = statsGO.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0f);
        sRt.anchorMax = new Vector2(1f, 0.22f);
        sRt.offsetMin = new Vector2(12f, 10f);
        sRt.offsetMax = new Vector2(-12f, -10f);

        var rawGO = new GameObject("PlayerPreview");
        rawGO.transform.SetParent(previewPanel.transform, false);
        rawGO.AddComponent<CanvasRenderer>();
        var raw = rawGO.AddComponent<RawImage>();
        raw.color = Color.white;
        var rawRt = rawGO.GetComponent<RectTransform>();
        rawRt.anchorMin = new Vector2(0.06f, 0.24f);
        rawRt.anchorMax = new Vector2(0.94f, 0.94f);
        rawRt.offsetMin = Vector2.zero;
        rawRt.offsetMax = Vector2.zero;
        var rawOutline = rawGO.AddComponent<Outline>();
        rawOutline.effectColor = new Color(0.07f, 0.12f, 0.18f, 0.9f);
        rawOutline.effectDistance = new Vector2(1f, -1f);

        var optionsHeader = CreateText(optionsPanel.transform, "OptionsHeader", "Choices", headerTextSize, TextAlignmentOptions.Left, headerTextColor);
        var optionsHeaderRect = optionsHeader.rectTransform;
        optionsHeaderRect.anchorMin = new Vector2(0f, 1f);
        optionsHeaderRect.anchorMax = new Vector2(1f, 1f);
        optionsHeaderRect.offsetMin = new Vector2(12f, -30f);
        optionsHeaderRect.offsetMax = new Vector2(-12f, -4f);

        var listGO = new GameObject("OptionsList");
        listGO.transform.SetParent(optionsPanel.transform, false);
        var listRt = listGO.AddComponent<RectTransform>();
        listRt.anchorMin = new Vector2(0f, 0.42f);
        listRt.anchorMax = new Vector2(1f, 0.90f);
        listRt.offsetMin = new Vector2(12f, 4f);
        listRt.offsetMax = new Vector2(-12f, -4f);
        var layout = listGO.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.spacing = 8f;

        var prevHeader = CreateText(optionsPanel.transform, "UpgradesHeader", "Progress", sectionTextSize, TextAlignmentOptions.Left, tertiaryTextColor);
        var phRt = prevHeader.rectTransform;
        phRt.anchorMin = new Vector2(0f, 0.34f);
        phRt.anchorMax = new Vector2(1f, 0.40f);
        phRt.offsetMin = new Vector2(12f, 0f);
        phRt.offsetMax = new Vector2(-12f, 0f);

        var prevList = new GameObject("PrevUpgrades");
        prevList.transform.SetParent(optionsPanel.transform, false);
        var prevRt = prevList.AddComponent<RectTransform>();
        prevRt.anchorMin = new Vector2(0f, 0.06f);
        prevRt.anchorMax = new Vector2(1f, 0.33f);
        prevRt.offsetMin = new Vector2(12f, 8f);
        prevRt.offsetMax = new Vector2(-12f, -4f);
        var prevLayout = prevList.AddComponent<VerticalLayoutGroup>();
        prevLayout.childForceExpandHeight = false;
        prevLayout.childControlHeight = true;
        prevLayout.spacing = 4f;

        inventoryRoot = canvasGO;
        inventoryRoot.AddComponent<InventoryRootMarker>();
        inventoryRoot.name = "InventoryRoot";
    }

    void CreatePreview()
    {
        if (!player) player = GameObject.FindWithTag("Player");
        if (!player) return;

        // create render texture
        previewTexture = new RenderTexture(512, 512, 16);

        // camera
        var camGO = new GameObject("InventoryPreviewCamera");
        previewCamera = camGO.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0,0,0,0);
        previewCamera.targetTexture = previewTexture;

        // instantiate a lightweight copy of the player for preview
        previewModel = Instantiate(player);
        previewModel.name = "_InventoryPreviewPlayer";

        // disable runtime behaviour on copy (safety)
        var behaviours = previewModel.GetComponentsInChildren<Behaviour>();
        foreach (var b in behaviours)
        {
            b.enabled = false;
        }

        // remove physics and colliders so the copy stays static
        var rbs = previewModel.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs) Destroy(rb);
        var cols = previewModel.GetComponentsInChildren<Collider>();
        foreach (var c in cols) Destroy(c);
        var joints = previewModel.GetComponentsInChildren<Joint>();
        foreach (var j in joints) Destroy(j);
        var cloths = previewModel.GetComponentsInChildren<Cloth>();
        foreach (var cl in cloths) Destroy(cl);

        // stop particle systems so they don't follow or emit continuously
        var pss = previewModel.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in pss) { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }

        // disable any animators explicitly
        var anims = previewModel.GetComponentsInChildren<Animator>();
        foreach (var a in anims) a.enabled = false;

        // place preview model far away so it never appears in the gameplay camera
        previewModel.transform.SetParent(null);
        previewModel.transform.position = previewOrigin;
        previewModel.transform.rotation = Quaternion.identity;

        // set preview layer recursively and camera culling
        SetLayerRecursively(previewModel, previewLayer);
        previewCamera.cullingMask = 1 << previewLayer;

        // position camera in front of model
        var bounds = BoundsOfRenderers(previewModel);
        float distance = Mathf.Max(bounds.size.magnitude * 1.2f, 1.5f);
        previewCamera.transform.position = previewOrigin + Vector3.back * distance + Vector3.up * (bounds.extents.y * 0.5f + 0.2f);
        previewCamera.transform.LookAt(previewModel.transform);

        // add a small directional light for nicer shading
        var lightGO = new GameObject("InventoryPreviewLight");
        lightGO.transform.SetParent(previewCamera.transform, false);
        lightGO.transform.localPosition = Vector3.zero;
        lightGO.transform.localRotation = Quaternion.Euler(50f, -30f, 0f);
        var l = lightGO.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.1f;
        l.shadowStrength = 0.6f;

        // assign texture to RawImage if present
        var raw = inventoryRoot.GetComponentInChildren<RawImage>();
        if (raw) raw.texture = previewTexture;
    }

    void PopulateOptions()
    {
        if (!playerLevel) playerLevel = player ? player.GetComponent<PlayerLevel>() : null;
        var list = inventoryRoot.GetComponentInChildren<VerticalLayoutGroup>()?.transform;
        if (list == null) return;

        // clear existing
        foreach (Transform c in list) Destroy(c.gameObject);

        // Example: show level options; if player has reached level 5 show dash & shield
        if (playerLevel != null && playerLevel.level >= 5)
        {
            AddOption(list, "Dash Protocol", "Press Space to perform a short burst dash.", () => { playerLevel.ChooseCombatMastery(); Close(); });
            AddOption(list, "Shield Mastery", "Increase your shield charges and duration.", () => { playerLevel.ChooseShieldMastery(); Close(); });
        }
        else
        {
            AddOption(list, "No advanced options yet", "Reach level 5 to unlock powerful adaptations.", null);
        }

        // Update stats text under preview
        var statsText = inventoryRoot.transform.Find("PopupBox/Content/PreviewPanel/StatsText")?.GetComponent<TextMeshProUGUI>();
        if (statsText != null && playerLevel != null)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Level: {playerLevel.level}");
            if (playerLevel.playerHealth != null)
                sb.AppendLine($"Health: {playerLevel.playerHealth.currentHealth}/{playerLevel.playerHealth.maxHealth}");
            if (playerLevel.playerEnergy != null)
                sb.AppendLine($"Energy: {Mathf.RoundToInt(playerLevel.playerEnergy.currentEnergy)}/{Mathf.RoundToInt(playerLevel.playerEnergy.maxEnergy)}");
            if (playerLevel.playerShield != null)
                sb.AppendLine($"Shield: {playerLevel.playerShield.currentShieldCharges}/{playerLevel.playerShield.maxShieldCharges}");
            var pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                sb.AppendLine($"Dash: {(pm.dashUnlocked ? "Unlocked" : "Locked")}");

            statsText.text = sb.ToString();
        }

        // Populate previous upgrades list
        var prevList = inventoryRoot.transform.Find("PopupBox/Content/OptionsPanel/PrevUpgrades");
        if (prevList != null && playerLevel != null)
        {
            // clear existing
            foreach (Transform c in prevList) Destroy(c.gameObject);

            AddPrevEntry(prevList, $"Collected food: {playerLevel.foodCollected}");
            AddPrevEntry(prevList, $"Next level in: {playerLevel.foodToLevelUp} food");
            if (playerLevel.playerEnergy != null)
                AddPrevEntry(prevList, $"Max Energy: {Mathf.RoundToInt(playerLevel.playerEnergy.maxEnergy)}");
            if (playerLevel.playerShield != null)
                AddPrevEntry(prevList, $"Shield Charges: {playerLevel.playerShield.maxShieldCharges}");
            if (playerLevel.playerCombat != null)
                AddPrevEntry(prevList, $"Combat: {(playerLevel.playerCombat.combatUnlocked ? "Unlocked" : "Locked")}");
            var pm2 = player.GetComponent<PlayerMovement>();
            if (pm2 != null && pm2.dashUnlocked)
                AddPrevEntry(prevList, "Dash: Unlocked");
        }
    }

    void AddPrevEntry(Transform parent, string text)
    {
        var item = new GameObject("PrevItem");
        item.transform.SetParent(parent, false);
        var t = item.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(t, text, progressTextSize, TextAlignmentOptions.Left, tertiaryTextColor);
        var rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 20);
    }

    void AddOption(Transform parent, string title, string desc, System.Action onClick)
    {
        var item = new GameObject("Option");
        item.transform.SetParent(parent, false);
        var button = item.AddComponent<Button>();
        var img = item.AddComponent<Image>();
        img.color = new Color(1f,1f,1f,0.04f);
        var outline = item.AddComponent<Outline>();
        outline.effectColor = new Color(0.18f, 0.24f, 0.32f, 1f);
        outline.effectDistance = new Vector2(1f, -1f);
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock
        {
            normalColor = new Color(1f,1f,1f,0.04f),
            highlightedColor = new Color(0.20f,0.32f,0.42f,0.95f),
            pressedColor = new Color(0.14f,0.22f,0.30f,1f),
            selectedColor = new Color(0.20f,0.32f,0.42f,0.95f),
            disabledColor = new Color(1f,1f,1f,0.02f),
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };
        var rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 60);

        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(item.transform, false);
        var t = titleGO.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(t, title, optionTitleTextSize, TextAlignmentOptions.Left, bodyTextColor);
        var trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0.5f);
        trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = new Vector2(8, -8);
        trt.offsetMax = new Vector2(-8, 8);

        var descGO = new GameObject("Desc");
        descGO.transform.SetParent(item.transform, false);
        var d = descGO.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(d, desc, optionDescriptionTextSize, TextAlignmentOptions.Left, secondaryTextColor);
        var drt = descGO.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(1, 0.5f);
        drt.offsetMin = new Vector2(8, 8);
        drt.offsetMax = new Vector2(-8, -8);

        if (onClick != null)
            button.onClick.AddListener(() => onClick());
    }

    Image CreateWindowPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, Color borderColor)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var image = panel.AddComponent<Image>();
        image.color = showInnerPanelBackgrounds ? fillColor : Color.clear;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = showInnerPanelBackgrounds ? borderColor : Color.clear;
        outline.effectDistance = showInnerPanelBackgrounds ? new Vector2(1.5f, -1.5f) : Vector2.zero;
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    GameObject CreateWindowRoot(Transform parent)
    {
        GameObject popupBox = null;
        GameObject templateRoot = null;
        bool usingTemplate = false;

        if (sceneWindowTemplate != null)
        {
            templateRoot = Instantiate(sceneWindowTemplate, parent, false);
            usingTemplate = true;
            popupBox = ResolveTemplatePopupBox(templateRoot);

            if (popupBox != null && popupBox != templateRoot)
            {
                popupBox.transform.SetParent(parent, false);
                Destroy(templateRoot);
            }

            StripLegacyPopupComponents(popupBox);
        }

        if (popupBox == null)
        {
            popupBox = new GameObject("PopupBox");
            popupBox.transform.SetParent(parent, false);
            var popupImage = popupBox.AddComponent<Image>();
            popupImage.color = windowBodyColor;
            var popupOutline = popupBox.AddComponent<Outline>();
            popupOutline.effectColor = windowBorderColor;
            popupOutline.effectDistance = new Vector2(2f, -2f);

            var titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(popupBox.transform, false);
            var titleBarImage = titleBar.AddComponent<Image>();
            titleBarImage.color = titleBarColor;
            var titleBarRect = titleBar.GetComponent<RectTransform>();
            titleBarRect.anchorMin = new Vector2(0f, 1f);
            titleBarRect.anchorMax = new Vector2(1f, 1f);
            titleBarRect.pivot = new Vector2(0.5f, 1f);
            titleBarRect.sizeDelta = new Vector2(0f, 40f);
            titleBarRect.anchoredPosition = Vector2.zero;
            titleBar.AddComponent<WindowDrag>();

            var titleText = CreateText(titleBar.transform, "Title", windowTitle, 26f, TextAlignmentOptions.Left, new Color(0.95f, 0.97f, 1f, 1f));
            var titleTextRect = titleText.rectTransform;
            titleTextRect.anchorMin = new Vector2(0f, 0f);
            titleTextRect.anchorMax = new Vector2(1f, 1f);
            titleTextRect.offsetMin = new Vector2(16f, 0f);
            titleTextRect.offsetMax = new Vector2(-96f, 0f);

            CreateWindowButton(titleBar.transform, "_", new Vector2(-72f, 0f), Minimise);
            CreateWindowButton(titleBar.transform, "X", new Vector2(-32f, 0f), Close);
        }

        popupBox.name = "PopupBox";
        popupBox.SetActive(true);

        var popupRect = popupBox.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            popupRect.localScale = Vector3.one;
            popupRect.localRotation = Quaternion.identity;

            if (!usingTemplate || !keepTemplateTransform)
            {
                var baseSize = GetReferenceWindowSize();
                popupRect.anchorMin = new Vector2(0.5f, 0.5f);
                popupRect.anchorMax = new Vector2(0.5f, 0.5f);
                popupRect.pivot = new Vector2(0.5f, 0.5f);
                popupRect.sizeDelta = baseSize;
                popupRect.anchoredPosition = Vector2.zero;
            }
        }

        var controls = popupBox.GetComponent<WindowControls>();
        if (controls != null)
            Destroy(controls);

        WireTemplateButtons(popupBox);
        DisableTemplateBodyContent(popupBox);
        EnsureTitleBarCanDrag(popupBox);

        return popupBox;
    }

    GameObject ResolveTemplatePopupBox(GameObject templateRoot)
    {
        if (templateRoot == null)
            return null;

        if (templateRoot.name == "PopupBox")
            return templateRoot;

        var transforms = templateRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (t != null && t.name == "PopupBox")
                return t.gameObject;
        }

        return templateRoot;
    }

    void WireTemplateButtons(GameObject popupBox)
    {
        var buttons = popupBox.GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
                continue;

            string txt = label.text.Trim();
            if (txt == "X")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Close);
            }
            else if (txt == "_" || txt == "-")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Minimise);
            }
        }
    }

    void DisableTemplateBodyContent(GameObject popupBox)
    {
        for (int i = 0; i < popupBox.transform.childCount; i++)
        {
            var child = popupBox.transform.GetChild(i);
            if (child.GetComponent<WindowDrag>() != null)
                continue;

            child.gameObject.SetActive(false);
        }
    }

    void EnsureTitleBarCanDrag(GameObject popupBox)
    {
        for (int i = 0; i < popupBox.transform.childCount; i++)
        {
            var child = popupBox.transform.GetChild(i);
            var rect = child as RectTransform;
            if (rect == null)
                continue;

            if (Mathf.Abs(rect.anchorMin.y - 1f) < 0.001f && Mathf.Abs(rect.anchorMax.y - 1f) < 0.001f)
            {
                if (child.GetComponent<WindowDrag>() == null)
                    child.gameObject.AddComponent<WindowDrag>();
                return;
            }
        }
    }

    void StripLegacyPopupComponents(GameObject popupBox)
    {
        if (popupBox == null)
            return;

        var legacyPopups = popupBox.GetComponentsInChildren<TutorialPopup>(true);
        foreach (var legacyPopup in legacyPopups)
            Destroy(legacyPopup);

        var legacyWindowControls = popupBox.GetComponentsInChildren<WindowControls>(true);
        foreach (var legacyWindowControl in legacyWindowControls)
            Destroy(legacyWindowControl);
    }

    TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(label, text, fontSize, alignment, color);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return label;
    }

    void ApplyTextStyle(TextMeshProUGUI label, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        if (label == null)
            return;

        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        label.fontStyle = menuFontStyle;
        if (menuFont != null)
            label.font = menuFont;
    }

    Button CreateWindowButton(Transform parent, string labelText, Vector2 anchoredPosition, System.Action onClick)
    {
        var buttonGO = new GameObject($"Button_{labelText}");
        buttonGO.transform.SetParent(parent, false);

        var button = buttonGO.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock
        {
            normalColor = new Color(1f, 1f, 1f, 0.08f),
            highlightedColor = new Color(0.30f, 0.42f, 0.52f, 1f),
            pressedColor = new Color(0.15f, 0.22f, 0.28f, 1f),
            selectedColor = new Color(0.30f, 0.42f, 0.52f, 1f),
            disabledColor = new Color(1f, 1f, 1f, 0.04f),
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };

        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.08f);
        button.targetGraphic = image;

        var rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(26f, 20f);
        rect.anchoredPosition = anchoredPosition;

        var text = CreateText(buttonGO.transform, "Label", labelText, 16f, TextAlignmentOptions.Center, Color.white);
        var textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        if (onClick != null)
            button.onClick.AddListener(() => onClick());

        return button;
    }

    void RemoveTaskbarTab()
    {
        if (taskbarTab != null && taskbarManager != null)
        {
            taskbarManager.RemoveTab(taskbarTab);
            taskbarTab = null;
        }
    }

    float GetTitleBarHeight(GameObject popupBox)
    {
        if (popupBox == null)
            return 40f;

        for (int i = 0; i < popupBox.transform.childCount; i++)
        {
            var child = popupBox.transform.GetChild(i);
            if (child.GetComponent<WindowDrag>() == null)
                continue;

            var rect = child as RectTransform;
            if (rect != null)
                return Mathf.Max(28f, rect.rect.height);
        }

        return 40f;
    }

    Vector2 GetReferenceWindowSize()
    {
        var explicitReference = windowSizeReference != null ? windowSizeReference : GetRectTransform(sceneWindowTemplate);
        if (explicitReference != null)
        {
            var explicitSize = explicitReference.rect.size;
            if (explicitSize.x > 0f && explicitSize.y > 0f)
                return explicitSize;
        }

        if (useWelcomePopupAsReference)
        {
            var controls = FindObjectsByType<WindowControls>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var control in controls)
            {
                if (control == null)
                    continue;

                if (!string.Equals(control.windowTitle, welcomeWindowTitle, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var welcomeRect = GetRectTransform(control.window) ?? control.GetComponent<RectTransform>();
                if (welcomeRect != null)
                {
                    var welcomeSize = welcomeRect.rect.size;
                    if (welcomeSize.x > 0f && welcomeSize.y > 0f)
                        return welcomeSize;
                }
            }
        }

        return windowSize;
    }

    static RectTransform GetRectTransform(GameObject target)
    {
        return target != null ? target.GetComponent<RectTransform>() : null;
    }

    void FitWindowToContent()
    {
        if (!autoResizeWindow || inventoryRoot == null)
            return;

        var popupRect = inventoryRoot.transform.Find("PopupBox") as RectTransform;
        if (popupRect == null)
            return;

        if (popupRect.anchorMin != popupRect.anchorMax)
            return;

        var optionsList = inventoryRoot.transform.Find("PopupBox/Content/OptionsPanel/OptionsList");
        var prevList = inventoryRoot.transform.Find("PopupBox/Content/OptionsPanel/PrevUpgrades");

        int optionCount = optionsList != null ? optionsList.childCount : 1;
        int progressCount = prevList != null ? prevList.childCount : 4;

        Vector2 baseSize = GetReferenceWindowSize();
        float baseWidth = Mathf.Max(1f, baseSize.x);
        float baseHeight = Mathf.Max(1f, baseSize.y);
        float baseAspect = baseWidth / baseHeight;

        float titleBarHeight = GetTitleBarHeight(popupRect.gameObject);
        float optionsHeight = 84f + optionCount * 68f;
        float progressHeight = 74f + progressCount * 22f;
        float contentHeight = Mathf.Max(baseHeight - titleBarHeight - 24f, optionsHeight + progressHeight);
        float targetHeight = Mathf.Clamp(contentHeight + titleBarHeight + 24f, minWindowSize.y, maxWindowSize.y);

        float targetWidth = Mathf.Clamp(Mathf.Max(baseWidth, targetHeight * baseAspect), minWindowSize.x, maxWindowSize.x);
        popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

    static Bounds BoundsOfRenderers(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        var b = new Bounds(go.transform.position, Vector3.zero);
        if (rends.Length == 0) return b;
        b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }
}

// marker so we can find the root easily if needed
public class InventoryRootMarker : MonoBehaviour { }
