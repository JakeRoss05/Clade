using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    GameObject inventoryRoot;
    RenderTexture previewTexture;
    Camera previewCamera;
    GameObject previewModel;
    int previewLayer = 31;
    bool open = false;

    PlayerLevel playerLevel;
    GameObject player;

    void Awake()
    {
        // ensure a single manager exists at runtime
        DontDestroyOnLoad(gameObject);
        player = GameObject.FindWithTag("Player");
        if (player) playerLevel = player.GetComponent<PlayerLevel>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitOnLoad()
    {
        var go = new GameObject("InventoryManager");
        DontDestroyOnLoad(go);
        go.AddComponent<InventoryController>();
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
        open = !open;
        if (open) Open(); else Close();
    }

    void Open()
    {
        CreateUI();
        CreatePreview();
        PopulateOptions();
    }

    void Close()
    {
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
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Dark overlay
        var overlay = new GameObject("Overlay");
        overlay.transform.SetParent(canvasGO.transform, false);
        var img = overlay.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.45f);
        var rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Center panel for preview
        var previewPanel = new GameObject("PreviewPanel");
        previewPanel.transform.SetParent(canvasGO.transform, false);
        var pImg = previewPanel.AddComponent<Image>();
        pImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
        // subtle border
        var pOutline = previewPanel.AddComponent<Outline>();
        pOutline.effectColor = new Color(0.2f, 0.28f, 0.36f, 1f);
        pOutline.effectDistance = new Vector2(2, -2);
        var prt = previewPanel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.1f, 0.2f);
        prt.anchorMax = new Vector2(0.6f, 0.85f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;

        // Right-side options panel
        var optionsPanel = new GameObject("OptionsPanel");
        optionsPanel.transform.SetParent(canvasGO.transform, false);
        var oImg = optionsPanel.AddComponent<Image>();
        oImg.color = new Color(0.03f, 0.04f, 0.06f, 0.9f);
        var oOutline = optionsPanel.AddComponent<Outline>();
        oOutline.effectColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        oOutline.effectDistance = new Vector2(2, -2);
        var ort = optionsPanel.GetComponent<RectTransform>();
        ort.anchorMin = new Vector2(0.62f, 0.2f);
        ort.anchorMax = new Vector2(0.95f, 0.85f);
        ort.offsetMin = Vector2.zero;
        ort.offsetMax = Vector2.zero;

        // Title
        var titleGO = new GameObject("InventoryTitle");
        titleGO.transform.SetParent(optionsPanel.transform, false);
        var titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Inventory";
        titleText.color = new Color(0.95f,0.95f,0.98f,1f);
        titleText.fontSize = 36;
        var tRt = titleGO.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0.9f);
        tRt.anchorMax = new Vector2(1, 1);
        tRt.offsetMin = new Vector2(8, -8);
        tRt.offsetMax = new Vector2(-8, 8);

        // Options container
        var listGO = new GameObject("OptionsList");
        listGO.transform.SetParent(optionsPanel.transform, false);
        var listRt = listGO.AddComponent<RectTransform>();
        listRt.anchorMin = new Vector2(0, 0);
        listRt.anchorMax = new Vector2(1, 0.9f);
        listRt.offsetMin = new Vector2(8, 8);
        listRt.offsetMax = new Vector2(-8, -40);
        var layout = listGO.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;

        // Previous upgrades header and list (above options)
        var prevHeader = new GameObject("UpgradesHeader");
        prevHeader.transform.SetParent(optionsPanel.transform, false);
        var prevHeaderText = prevHeader.AddComponent<TextMeshProUGUI>();
        prevHeaderText.text = "Upgrades";
        prevHeaderText.fontSize = 20;
        prevHeaderText.color = new Color(0.85f,0.85f,0.9f,1f);
        var phRt = prevHeader.GetComponent<RectTransform>();
        phRt.anchorMin = new Vector2(0, 0.82f);
        phRt.anchorMax = new Vector2(1, 0.9f);
        phRt.offsetMin = new Vector2(8, -4);
        phRt.offsetMax = new Vector2(-8, 4);

        var prevList = new GameObject("PrevUpgrades");
        prevList.transform.SetParent(optionsPanel.transform, false);
        var prevRt = prevList.AddComponent<RectTransform>();
        prevRt.anchorMin = new Vector2(0, 0.6f);
        prevRt.anchorMax = new Vector2(1, 0.82f);
        prevRt.offsetMin = new Vector2(8, 4);
        prevRt.offsetMax = new Vector2(-8, -4);
        var prevLayout = prevList.AddComponent<VerticalLayoutGroup>();
        prevLayout.childForceExpandHeight = false;
        prevLayout.childControlHeight = true;

        // Stats text under preview
        var statsGO = new GameObject("StatsText");
        statsGO.transform.SetParent(previewPanel.transform, false);
        var stats = statsGO.AddComponent<TextMeshProUGUI>();
        stats.text = "";
        stats.fontSize = 20;
        stats.color = new Color(0.9f,0.95f,1f,1f);
        var sRt = statsGO.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 0);
        sRt.anchorMax = new Vector2(1, 0.25f);
        sRt.offsetMin = new Vector2(8, 8);
        sRt.offsetMax = new Vector2(-8, -8);

        // Player preview RawImage
        var rawGO = new GameObject("PlayerPreview");
        rawGO.transform.SetParent(previewPanel.transform, false);
        var raw = rawGO.AddComponent<RawImage>();
        var rawRt = rawGO.GetComponent<RectTransform>();
        rawRt.anchorMin = new Vector2(0.1f, 0.25f);
        rawRt.anchorMax = new Vector2(0.9f, 0.95f);
        rawRt.offsetMin = rawRt.offsetMax = Vector2.zero;

        // give the raw image a soft inner margin and subtle frame
        var rawOutline = rawGO.AddComponent<Outline>();
        rawOutline.effectColor = new Color(0.06f,0.12f,0.18f,0.9f);
        rawOutline.effectDistance = new Vector2(1, -1);

        // store references via child names
        inventoryRoot = canvasGO;
        // attach helper components
        inventoryRoot.AddComponent<InventoryRootMarker>();
        // cache commonly used child names
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

        // place preview model at a fixed preview origin so it doesn't track the live player
        previewModel.transform.SetParent(null);
        previewModel.transform.position = Vector3.zero;
        previewModel.transform.rotation = Quaternion.identity;

        // set preview layer recursively and camera culling
        SetLayerRecursively(previewModel, previewLayer);
        previewCamera.cullingMask = 1 << previewLayer;

        // position camera in front of model
        var bounds = BoundsOfRenderers(previewModel);
        float distance = Mathf.Max(bounds.size.magnitude * 1.2f, 1.5f);
        previewCamera.transform.position = previewModel.transform.position + Vector3.back * distance + Vector3.up * (bounds.extents.y * 0.5f + 0.2f);
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
        var statsText = inventoryRoot.transform.Find("PreviewPanel/StatsText")?.GetComponent<TextMeshProUGUI>();
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
        var prevList = inventoryRoot.transform.Find("OptionsPanel/PrevUpgrades");
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
        t.text = text;
        t.fontSize = 14;
        t.color = new Color(0.86f,0.9f,0.95f,1f);
        var rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 20);
    }

    void AddOption(Transform parent, string title, string desc, System.Action onClick)
    {
        var item = new GameObject("Option");
        item.transform.SetParent(parent, false);
        var button = item.AddComponent<Button>();
        var img = item.AddComponent<Image>();
        img.color = new Color(1,1,1,0.02f);
        var rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 60);

        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(item.transform, false);
        var t = titleGO.AddComponent<TextMeshProUGUI>();
        t.text = title;
        t.fontSize = 22;
        var trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0.5f);
        trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = new Vector2(8, -8);
        trt.offsetMax = new Vector2(-8, 8);

        var descGO = new GameObject("Desc");
        descGO.transform.SetParent(item.transform, false);
        var d = descGO.AddComponent<TextMeshProUGUI>();
        d.text = desc;
        d.fontSize = 14;
        var drt = descGO.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(1, 0.5f);
        drt.offsetMin = new Vector2(8, 8);
        drt.offsetMax = new Vector2(-8, -8);

        if (onClick != null) button.onClick.AddListener(() => onClick());
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
