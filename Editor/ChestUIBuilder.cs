#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility that builds the entire Chest UI hierarchy inside the scene Canvas.
///
/// Usage: Unity menu → Tools → Placement System → Build Chest UI
///
/// What it creates:
///   Canvas
///   ├── ChestNotificationButton  (Button + ChestNotificationButton script)
///   │   ├── ChestIcon            (Image)
///   │   └── BadgeText            (TextMeshProUGUI)
///   └── ChestOpeningPanel        (inactive by default)
///       ├── Backdrop             (full-screen semi-transparent Image)
///       ├── PanelBackground      (centred card Image)
///       │   ├── ChestTitleText   (TextMeshProUGUI)
///       │   ├── RewardGrid       (GridLayoutGroup)
///       │   ├── OpenButton       (Button + TMP)
///       │   ├── CloseButton      (Button + TMP, inactive)
///       │   └── SkipButton       (Button + TMP, inactive)
///       └── Scripts are attached and cross-referenced automatically.
/// </summary>
public static class ChestUIBuilder
{
    [MenuItem("Tools/Placement System/Build Reward Slot Prefab")]
    public static void BuildRewardSlotPrefab()
    {
        const string prefabPath = "Assets/PlacementSystem/Prefabs/UI/RewardSlotPrefab.prefab";

        // Ensure directory exists
        System.IO.Directory.CreateDirectory(
            System.IO.Path.Combine(Application.dataPath,
            "../Assets/PlacementSystem/Prefabs/UI"));
        AssetDatabase.Refresh();

        // Build the slot hierarchy in memory
        GameObject root = new GameObject("RewardSlotPrefab");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(160f, 200f);
        Image rootBg = root.AddComponent<Image>();
        rootBg.color = new Color(0.15f, 0.12f, 0.20f, 1f);

        // Item icon
        GameObject iconGO = new GameObject("ItemIcon");
        iconGO.transform.SetParent(root.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 1f);
        iconRect.anchorMax = new Vector2(0.5f, 1f);
        iconRect.pivot     = new Vector2(0.5f, 1f);
        iconRect.anchoredPosition = new Vector2(0f, -10f);
        iconRect.sizeDelta = new Vector2(90f, 90f);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.6f, 0.6f, 0.6f, 1f);

        // Item name
        GameObject nameGO = CreateTMPText(root.transform, "ItemName", "Item Name");
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.5f);
        nameRect.anchorMax = new Vector2(1f, 0.5f);
        nameRect.pivot     = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = new Vector2(0f, 10f);
        nameRect.sizeDelta = new Vector2(0f, 28f);
        nameGO.GetComponent<TextMeshProUGUI>().fontSize = 14f;

        // Rarity text
        GameObject rarityGO = CreateTMPText(root.transform, "RarityText", "Common");
        RectTransform rarityRect = rarityGO.GetComponent<RectTransform>();
        rarityRect.anchorMin = new Vector2(0f, 0f);
        rarityRect.anchorMax = new Vector2(1f, 0f);
        rarityRect.pivot     = new Vector2(0.5f, 0f);
        rarityRect.anchoredPosition = new Vector2(0f, 36f);
        rarityRect.sizeDelta = new Vector2(0f, 24f);
        TextMeshProUGUI rarityTMP = rarityGO.GetComponent<TextMeshProUGUI>();
        rarityTMP.fontSize = 12f;
        rarityTMP.color = new Color(0.8f, 0.7f, 0.3f, 1f);

        // Quantity text
        GameObject qtyGO = CreateTMPText(root.transform, "QuantityText", "x1");
        RectTransform qtyRect = qtyGO.GetComponent<RectTransform>();
        qtyRect.anchorMin = new Vector2(1f, 1f);
        qtyRect.anchorMax = new Vector2(1f, 1f);
        qtyRect.pivot     = new Vector2(1f, 1f);
        qtyRect.anchoredPosition = new Vector2(-6f, -6f);
        qtyRect.sizeDelta = new Vector2(40f, 24f);
        qtyGO.GetComponent<TextMeshProUGUI>().fontSize = 13f;

        // Rarity border (placeholder)
        GameObject borderGO = new GameObject("RarityBorder");
        borderGO.transform.SetParent(root.transform, false);
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(1f, 0.84f, 0f, 0f); // transparent placeholder

        // Rarity glow (placeholder)
        GameObject glowGO = new GameObject("RarityGlow");
        glowGO.transform.SetParent(root.transform, false);
        RectTransform glowRect = glowGO.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-10f, -10f);
        glowRect.offsetMax = new Vector2(10f, 10f);
        Image glowImg = glowGO.AddComponent<Image>();
        glowImg.color = new Color(1f, 1f, 0f, 0f); // transparent placeholder

        // Attach RewardSlotUI and wire fields
        RewardSlotUI slotScript = root.AddComponent<RewardSlotUI>();
        SerializedObject slotSO = new SerializedObject(slotScript);
        slotSO.FindProperty("itemIcon").objectReferenceValue      = iconImg;
        slotSO.FindProperty("itemNameText").objectReferenceValue  = nameGO.GetComponent<TextMeshProUGUI>();
        slotSO.FindProperty("rarityText").objectReferenceValue    = rarityTMP;
        slotSO.FindProperty("quantityText").objectReferenceValue  = qtyGO.GetComponent<TextMeshProUGUI>();
        slotSO.FindProperty("rarityBorder").objectReferenceValue  = borderImg;
        slotSO.FindProperty("rarityGlow").objectReferenceValue    = glowImg;
        slotSO.ApplyModifiedProperties();

        // Save as prefab asset
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        if (prefab != null)
        {
            Debug.Log($"[ChestUIBuilder] RewardSlotPrefab created at {prefabPath}. " +
                      "Assign it to ChestOpeningPanel → Reward Slot Prefab.");
            Selection.activeObject = prefab;
        }
        else
        {
            Debug.LogError("[ChestUIBuilder] Failed to save RewardSlotPrefab.");
        }
    }

    [MenuItem("Tools/Placement System/Build Chest UI")]
    public static void BuildChestUI()
    {
        // ── Find Canvas ──────────────────────────────────────────────────────
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ChestUIBuilder] No Canvas found in scene. Please add a Canvas first.");
            return;
        }

        Transform canvasT = canvas.transform;

        // Guard: don't duplicate
        if (canvasT.Find("ChestNotificationButton") != null ||
            canvasT.Find("ChestOpeningPanel") != null)
        {
            Debug.LogWarning("[ChestUIBuilder] Chest UI already exists in Canvas. Aborting to avoid duplicates.");
            return;
        }

        // ── Notification Button ───────────────────────────────────────────────
        GameObject notifGO = CreateButton(canvasT, "ChestNotificationButton", "[CHEST]");
        RectTransform notifRect = notifGO.GetComponent<RectTransform>();
        notifRect.anchorMin = new Vector2(0.5f, 0f);
        notifRect.anchorMax = new Vector2(0.5f, 0f);
        notifRect.pivot     = new Vector2(0.5f, 0f);
        notifRect.anchoredPosition = new Vector2(0f, 20f);
        notifRect.sizeDelta = new Vector2(80f, 80f);

        // Badge text (child of button)
        GameObject badgeGO = CreateTMPText(notifGO.transform, "BadgeText", "");
        RectTransform badgeRect = badgeGO.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(1f, 1f);
        badgeRect.anchorMax = new Vector2(1f, 1f);
        badgeRect.pivot     = new Vector2(1f, 1f);
        badgeRect.anchoredPosition = new Vector2(0f, 0f);
        badgeRect.sizeDelta = new Vector2(30f, 30f);
        TextMeshProUGUI badgeTMP = badgeGO.GetComponent<TextMeshProUGUI>();
        badgeTMP.fontSize   = 14f;
        badgeTMP.alignment  = TextAlignmentOptions.Center;
        badgeTMP.color      = Color.white;

        // Attach ChestNotificationButton script
        ChestNotificationButton notifScript = notifGO.AddComponent<ChestNotificationButton>();

        // ── Chest Opening Panel ───────────────────────────────────────────────
        GameObject panelGO = new GameObject("ChestOpeningPanel");
        panelGO.transform.SetParent(canvasT, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        StretchFull(panelRect);
        CanvasGroup panelCG = panelGO.AddComponent<CanvasGroup>();

        // Backdrop (full-screen dark overlay)
        GameObject backdropGO = new GameObject("Backdrop");
        backdropGO.transform.SetParent(panelGO.transform, false);
        RectTransform backdropRect = backdropGO.AddComponent<RectTransform>();
        StretchFull(backdropRect);
        Image backdropImg = backdropGO.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.75f);

        // Panel background card (centred)
        GameObject bgGO = new GameObject("PanelBackground");
        bgGO.transform.SetParent(panelGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax        = new Vector2(0.5f, 0.5f);
        bgRect.pivot            = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta        = new Vector2(620f, 480f);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.10f, 0.18f, 1f);

        // Title text
        GameObject titleGO = CreateTMPText(bgGO.transform, "ChestTitleText", "Basic Reward Chest");
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin        = new Vector2(0f, 1f);
        titleRect.anchorMax        = new Vector2(1f, 1f);
        titleRect.pivot            = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta        = new Vector2(0f, 50f);
        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.fontSize  = 26f;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = Color.white;

        // Reward grid
        GameObject gridGO = new GameObject("RewardGrid");
        gridGO.transform.SetParent(bgGO.transform, false);
        RectTransform gridRect = gridGO.AddComponent<RectTransform>();
        gridRect.anchorMin        = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax        = new Vector2(0.5f, 0.5f);
        gridRect.pivot            = new Vector2(0.5f, 0.5f);
        gridRect.anchoredPosition = new Vector2(0f, 20f);
        gridRect.sizeDelta        = new Vector2(560f, 240f);
        GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.cellSize        = new Vector2(160f, 200f);
        grid.spacing         = new Vector2(20f, 20f);
        grid.childAlignment  = TextAnchor.MiddleCenter;
        gridGO.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.03f);

        // Open button
        GameObject openBtnGO = CreateButton(bgGO.transform, "OpenButton", "Open Chest");
        RectTransform openRect = openBtnGO.GetComponent<RectTransform>();
        openRect.anchorMin        = new Vector2(0.5f, 0f);
        openRect.anchorMax        = new Vector2(0.5f, 0f);
        openRect.pivot            = new Vector2(0.5f, 0f);
        openRect.anchoredPosition = new Vector2(-90f, 20f);
        openRect.sizeDelta        = new Vector2(160f, 50f);

        // Close button (starts inactive)
        GameObject closeBtnGO = CreateButton(bgGO.transform, "CloseButton", "Close");
        RectTransform closeRect = closeBtnGO.GetComponent<RectTransform>();
        closeRect.anchorMin        = new Vector2(0.5f, 0f);
        closeRect.anchorMax        = new Vector2(0.5f, 0f);
        closeRect.pivot            = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(90f, 20f);
        closeRect.sizeDelta        = new Vector2(160f, 50f);
        closeBtnGO.SetActive(false);

        // Skip button (starts inactive, full-screen transparent overlay)
        GameObject skipBtnGO = new GameObject("SkipButton");
        skipBtnGO.transform.SetParent(panelGO.transform, false);
        RectTransform skipRect = skipBtnGO.AddComponent<RectTransform>();
        StretchFull(skipRect);
        Button skipBtn = skipBtnGO.AddComponent<Button>();
        Image skipImg = skipBtnGO.AddComponent<Image>();
        skipImg.color = new Color(0f, 0f, 0f, 0f); // fully transparent — tap anywhere
        skipBtnGO.SetActive(false);

        // Attach ChestOpeningPanel script and wire references
        ChestOpeningPanel panelScript = panelGO.AddComponent<ChestOpeningPanel>();
        SerializedObject so = new SerializedObject(panelScript);
        so.FindProperty("canvasGroup").objectReferenceValue       = panelCG;
        so.FindProperty("backdrop").objectReferenceValue          = backdropGO;
        so.FindProperty("chestTitleText").objectReferenceValue    = titleGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("rewardGrid").objectReferenceValue        = gridGO.transform;
        so.FindProperty("openButton").objectReferenceValue        = openBtnGO.GetComponent<Button>();
        so.FindProperty("closeButton").objectReferenceValue       = closeBtnGO.GetComponent<Button>();
        so.FindProperty("skipButton").objectReferenceValue        = skipBtnGO.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Wire notification button badge text
        SerializedObject notifSO = new SerializedObject(notifScript);
        notifSO.FindProperty("badgeText").objectReferenceValue = badgeTMP;
        notifSO.ApplyModifiedProperties();

        // Deactivate panel by default
        panelGO.SetActive(false);

        // ── ChestUIController on Canvas ───────────────────────────────────────
        ChestUIController controller = canvas.GetComponent<ChestUIController>();
        if (controller == null)
            controller = canvas.gameObject.AddComponent<ChestUIController>();

        SerializedObject ctrlSO = new SerializedObject(controller);
        ctrlSO.FindProperty("notificationButton").objectReferenceValue = notifScript;
        ctrlSO.FindProperty("openingPanel").objectReferenceValue       = panelScript;
        ctrlSO.ApplyModifiedProperties();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[ChestUIBuilder] Chest UI built successfully. " +
                  "Assign a RewardSlot prefab to ChestOpeningPanel.rewardSlotPrefab to complete setup.");
        Selection.activeGameObject = panelGO;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.20f, 0.35f, 1f);
        go.AddComponent<Button>();

        GameObject textGO = CreateTMPText(go.transform, "Label", label);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        StretchFull(textRect);
        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.fontSize  = 18f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;

        return go;
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 16f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }
}
#endif
