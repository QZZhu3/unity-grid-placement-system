#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility that builds the entire Ambient Task Journal UI hierarchy.
///
/// Usage: Unity menu → Tools → Placement System → Build Ambient Task Journal UI
///
/// What it creates:
///   Canvas
///   ├── JournalDarkenOverlay     (full-screen darkening Image, alpha 0)
///   └── AmbientJournalRoot       (anchor: left, off-screen)
///       ├── PeekZone             (invisible trigger area at screen edge)
///       └── JournalPanel         (TaskJournalPanel, CanvasGroup)
///           ├── PanelBackground  (styled card)
///           ├── PeekView         (visible in Peek mode)
///           │   ├── TaskIcon
///           │   ├── TaskTitlePeek
///           │   └── TinyProgress (Image, radial fill)
///           └── PinnedView       (visible in Pinned mode)
///               ├── TaskTitlePinned
///               ├── CategoryLabel
///               ├── RewardPreview
///               ├── HoldButton   (HoldCompleteInteraction)
///               │   ├── HoldLabel
///               │   └── HoldFill (Image, radial fill)
///               └── SwapButton
///
/// All components are attached and cross-referenced automatically.
/// </summary>
public static class AmbientTaskJournalUIBuilder
{
    [MenuItem("Tools/Placement System/Build Ambient Task Journal UI")]
    public static void BuildJournalUI()
    {
        // ── Find Canvas ───────────────────────────────────────────────────────
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Journal UI Builder",
                "No Canvas found in the scene. Please add a Canvas first.", "OK");
            return;
        }
        Transform canvasT = canvas.transform;

        // ── Guard: don't build twice ──────────────────────────────────────────
        if (canvasT.Find("AmbientJournalRoot") != null)
        {
            bool replace = EditorUtility.DisplayDialog("Journal UI Builder",
                "AmbientJournalRoot already exists. Rebuild it?", "Rebuild", "Cancel");
            if (!replace) return;
            Object.DestroyImmediate(canvasT.Find("AmbientJournalRoot").gameObject);
        }
        if (canvasT.Find("JournalDarkenOverlay") != null)
        {
            Object.DestroyImmediate(canvasT.Find("JournalDarkenOverlay").gameObject);
        }

        // ── Darken overlay (behind journal, full screen) ──────────────────────
        GameObject overlayGO = new GameObject("JournalDarkenOverlay");
        overlayGO.transform.SetParent(canvasT, false);
        RectTransform overlayRect = overlayGO.AddComponent<RectTransform>();
        StretchFull(overlayRect);
        Image overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0f);
        overlayImg.raycastTarget = false;

        // ── Journal root ──────────────────────────────────────────────────────
        GameObject rootGO = new GameObject("AmbientJournalRoot");
        rootGO.transform.SetParent(canvasT, false);
        RectTransform rootRect = rootGO.AddComponent<RectTransform>();
        // Anchor to left side, vertically centred
        rootRect.anchorMin        = new Vector2(0f, 0.5f);
        rootRect.anchorMax        = new Vector2(0f, 0.5f);
        rootRect.pivot            = new Vector2(0f, 0.5f);
        rootRect.anchoredPosition = new Vector2(-300f, 0f); // off-screen left
        rootRect.sizeDelta        = new Vector2(320f, 600f);

        // ── Peek zone (invisible trigger strip at left edge) ──────────────────
        GameObject peekZoneGO = new GameObject("PeekZone");
        peekZoneGO.transform.SetParent(canvasT, false);
        RectTransform peekZoneRect = peekZoneGO.AddComponent<RectTransform>();
        peekZoneRect.anchorMin        = new Vector2(0f, 0.1f);
        peekZoneRect.anchorMax        = new Vector2(0f, 0.9f);
        peekZoneRect.pivot            = new Vector2(0f, 0.5f);
        peekZoneRect.anchoredPosition = new Vector2(0f, 0f);
        peekZoneRect.sizeDelta        = new Vector2(40f, 0f);
        Image peekZoneImg = peekZoneGO.AddComponent<Image>();
        peekZoneImg.color = new Color(0f, 0f, 0f, 0f); // invisible but raycasts
        peekZoneGO.transform.SetParent(canvasT, false); // keep at canvas level

        // ── Journal panel ─────────────────────────────────────────────────────
        GameObject panelGO = new GameObject("JournalPanel");
        panelGO.transform.SetParent(rootGO.transform, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        StretchFull(panelRect);
        CanvasGroup panelCG = panelGO.AddComponent<CanvasGroup>();
        panelCG.alpha          = 0f;
        panelCG.interactable   = false;
        panelCG.blocksRaycasts = false;

        // ── Panel background ──────────────────────────────────────────────────
        GameObject bgGO = new GameObject("PanelBackground");
        bgGO.transform.SetParent(panelGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        StretchFull(bgRect);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.14f, 0.11f, 0.20f, 0.95f);

        // ── Peek view ─────────────────────────────────────────────────────────
        GameObject peekViewGO = new GameObject("PeekView");
        peekViewGO.transform.SetParent(panelGO.transform, false);
        RectTransform peekViewRect = peekViewGO.AddComponent<RectTransform>();
        peekViewRect.anchorMin        = new Vector2(0f, 1f);
        peekViewRect.anchorMax        = new Vector2(1f, 1f);
        peekViewRect.pivot            = new Vector2(0.5f, 1f);
        peekViewRect.anchoredPosition = new Vector2(0f, -16f);
        peekViewRect.sizeDelta        = new Vector2(-24f, 80f);

        // Task icon (peek)
        GameObject iconGO = new GameObject("TaskIcon");
        iconGO.transform.SetParent(peekViewGO.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin        = new Vector2(0f, 0.5f);
        iconRect.anchorMax        = new Vector2(0f, 0.5f);
        iconRect.pivot            = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(8f, 0f);
        iconRect.sizeDelta        = new Vector2(40f, 40f);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.color = new Color(0.7f, 0.6f, 0.9f, 1f);

        // Task title (peek)
        GameObject peekTitleGO = CreateTMPText(peekViewGO.transform, "TaskTitlePeek", "Task Name");
        RectTransform peekTitleRect = peekTitleGO.GetComponent<RectTransform>();
        peekTitleRect.anchorMin        = new Vector2(0f, 0.5f);
        peekTitleRect.anchorMax        = new Vector2(1f, 0.5f);
        peekTitleRect.pivot            = new Vector2(0f, 0.5f);
        peekTitleRect.anchoredPosition = new Vector2(60f, 8f);
        peekTitleRect.sizeDelta        = new Vector2(-68f, 28f);
        peekTitleGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // Tiny progress indicator (peek)
        GameObject tinyProgressGO = new GameObject("TinyProgress");
        tinyProgressGO.transform.SetParent(peekViewGO.transform, false);
        RectTransform tinyProgressRect = tinyProgressGO.AddComponent<RectTransform>();
        tinyProgressRect.anchorMin        = new Vector2(0f, 0f);
        tinyProgressRect.anchorMax        = new Vector2(0f, 0f);
        tinyProgressRect.pivot            = new Vector2(0f, 0f);
        tinyProgressRect.anchoredPosition = new Vector2(60f, 4f);
        tinyProgressRect.sizeDelta        = new Vector2(16f, 16f);
        Image tinyProgressImg = tinyProgressGO.AddComponent<Image>();
        tinyProgressImg.color = new Color(0.55f, 0.85f, 0.55f, 1f);
        tinyProgressImg.type = Image.Type.Filled;
        tinyProgressImg.fillMethod = Image.FillMethod.Radial360;
        tinyProgressImg.fillAmount = 0.6f; // placeholder

        // ── Pinned view ───────────────────────────────────────────────────────
        GameObject pinnedViewGO = new GameObject("PinnedView");
        pinnedViewGO.transform.SetParent(panelGO.transform, false);
        RectTransform pinnedViewRect = pinnedViewGO.AddComponent<RectTransform>();
        pinnedViewRect.anchorMin        = new Vector2(0f, 0f);
        pinnedViewRect.anchorMax        = new Vector2(1f, 1f);
        pinnedViewRect.offsetMin        = new Vector2(16f, 16f);
        pinnedViewRect.offsetMax        = new Vector2(-16f, -16f);
        pinnedViewGO.SetActive(false);

        // Task title (pinned)
        GameObject pinnedTitleGO = CreateTMPText(pinnedViewGO.transform, "TaskTitlePinned", "Task Name");
        RectTransform pinnedTitleRect = pinnedTitleGO.GetComponent<RectTransform>();
        pinnedTitleRect.anchorMin        = new Vector2(0f, 1f);
        pinnedTitleRect.anchorMax        = new Vector2(1f, 1f);
        pinnedTitleRect.pivot            = new Vector2(0.5f, 1f);
        pinnedTitleRect.anchoredPosition = new Vector2(0f, -8f);
        pinnedTitleRect.sizeDelta        = new Vector2(0f, 36f);
        TextMeshProUGUI pinnedTitleTMP = pinnedTitleGO.GetComponent<TextMeshProUGUI>();
        pinnedTitleTMP.fontSize  = 20f;
        pinnedTitleTMP.fontStyle = FontStyles.Bold;
        pinnedTitleTMP.alignment = TextAlignmentOptions.Left;

        // Category label
        GameObject categoryGO = CreateTMPText(pinnedViewGO.transform, "CategoryLabel", "Category");
        RectTransform categoryRect = categoryGO.GetComponent<RectTransform>();
        categoryRect.anchorMin        = new Vector2(0f, 1f);
        categoryRect.anchorMax        = new Vector2(1f, 1f);
        categoryRect.pivot            = new Vector2(0.5f, 1f);
        categoryRect.anchoredPosition = new Vector2(0f, -52f);
        categoryRect.sizeDelta        = new Vector2(0f, 24f);
        TextMeshProUGUI categoryTMP = categoryGO.GetComponent<TextMeshProUGUI>();
        categoryTMP.fontSize = 13f;
        categoryTMP.color    = new Color(0.7f, 0.65f, 0.85f, 1f);
        categoryTMP.alignment = TextAlignmentOptions.Left;

        // Reward preview
        GameObject rewardGO = new GameObject("RewardPreview");
        rewardGO.transform.SetParent(pinnedViewGO.transform, false);
        RectTransform rewardRect = rewardGO.AddComponent<RectTransform>();
        rewardRect.anchorMin        = new Vector2(0f, 1f);
        rewardRect.anchorMax        = new Vector2(0f, 1f);
        rewardRect.pivot            = new Vector2(0f, 1f);
        rewardRect.anchoredPosition = new Vector2(0f, -84f);
        rewardRect.sizeDelta        = new Vector2(48f, 48f);
        Image rewardImg = rewardGO.AddComponent<Image>();
        rewardImg.color = new Color(1f, 0.85f, 0.3f, 0.8f);

        // Hold-to-complete button
        GameObject holdBtnGO = new GameObject("HoldButton");
        holdBtnGO.transform.SetParent(pinnedViewGO.transform, false);
        RectTransform holdBtnRect = holdBtnGO.AddComponent<RectTransform>();
        holdBtnRect.anchorMin        = new Vector2(0.5f, 0f);
        holdBtnRect.anchorMax        = new Vector2(0.5f, 0f);
        holdBtnRect.pivot            = new Vector2(0.5f, 0f);
        holdBtnRect.anchoredPosition = new Vector2(0f, 60f);
        holdBtnRect.sizeDelta        = new Vector2(180f, 64f);
        Image holdBtnImg = holdBtnGO.AddComponent<Image>();
        holdBtnImg.color = new Color(0.30f, 0.22f, 0.45f, 1f);

        // Hold fill (radial progress ring)
        GameObject holdFillGO = new GameObject("HoldFill");
        holdFillGO.transform.SetParent(holdBtnGO.transform, false);
        RectTransform holdFillRect = holdFillGO.AddComponent<RectTransform>();
        StretchFull(holdFillRect);
        Image holdFillImg = holdFillGO.AddComponent<Image>();
        holdFillImg.color = new Color(0.55f, 0.40f, 0.80f, 0.7f);
        holdFillImg.type = Image.Type.Filled;
        holdFillImg.fillMethod = Image.FillMethod.Radial360;
        holdFillImg.fillAmount = 0f;

        // Hold label
        GameObject holdLabelGO = CreateTMPText(holdBtnGO.transform, "HoldLabel", "Hold to Complete");
        RectTransform holdLabelRect = holdLabelGO.GetComponent<RectTransform>();
        StretchFull(holdLabelRect);
        holdLabelGO.GetComponent<TextMeshProUGUI>().fontSize = 15f;

        // Attach HoldCompleteInteraction
        HoldCompleteInteraction holdInteraction = holdBtnGO.AddComponent<HoldCompleteInteraction>();
        SerializedObject holdSO = new SerializedObject(holdInteraction);
        holdSO.FindProperty("progressFillImage").objectReferenceValue = holdFillImg;
        holdSO.ApplyModifiedProperties();

        // Swap/replace task button
        GameObject swapBtnGO = CreateButton(pinnedViewGO.transform, "SwapButton", "Replace Task");
        RectTransform swapBtnRect = swapBtnGO.GetComponent<RectTransform>();
        swapBtnRect.anchorMin        = new Vector2(0.5f, 0f);
        swapBtnRect.anchorMax        = new Vector2(0.5f, 0f);
        swapBtnRect.pivot            = new Vector2(0.5f, 0f);
        swapBtnRect.anchoredPosition = new Vector2(0f, 8f);
        swapBtnRect.sizeDelta        = new Vector2(180f, 44f);
        swapBtnGO.GetComponent<Image>().color = new Color(0.20f, 0.16f, 0.30f, 1f);

        // ── Attach TaskJournalPanel ───────────────────────────────────────────
        TaskJournalPanel journalPanel = panelGO.AddComponent<TaskJournalPanel>();

        // ── Attach AmbientTaskJournalController to root ───────────────────────
        AmbientTaskJournalController controller = rootGO.AddComponent<AmbientTaskJournalController>();
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("journalPanel").objectReferenceValue = journalPanel;
        controllerSO.ApplyModifiedProperties();

        // ── Attach JournalBlurController to root ──────────────────────────────
        JournalBlurController blurController = rootGO.AddComponent<JournalBlurController>();
        SerializedObject blurSO = new SerializedObject(blurController);
        blurSO.FindProperty("darkenOverlay").objectReferenceValue = overlayImg;
        blurSO.ApplyModifiedProperties();

        // Wire blur controller back to journal controller
        controllerSO.Update();
        controllerSO.FindProperty("blurController").objectReferenceValue = blurController;
        controllerSO.ApplyModifiedProperties();

        // ── Mark scene dirty ──────────────────────────────────────────────────
        EditorUtility.SetDirty(rootGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[AmbientTaskJournalUIBuilder] Journal UI built successfully.");

        EditorUtility.DisplayDialog("Journal UI Builder",
            "Ambient Task Journal built successfully!\n\n" +
            "Next steps:\n" +
            "1. Add EventTrigger components to PeekZone and JournalPanel to call " +
            "OnPeekZoneEnter/Exit and OnJournalEnter/Exit on the AmbientTaskJournalController.\n" +
            "2. Optionally assign a Global Volume with Depth of Field to JournalBlurController.\n" +
            "3. Wire task data to TaskRowUI at runtime via your task management system.",
            "OK");

        Selection.activeGameObject = rootGO;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin  = Vector2.zero;
        rect.anchorMax  = Vector2.one;
        rect.offsetMin  = Vector2.zero;
        rect.offsetMax  = Vector2.zero;
    }

    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.35f, 0.25f, 0.55f, 1f);
        go.AddComponent<Button>();
        GameObject textGO = CreateTMPText(go.transform, "Label", label);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.fontSize  = 16f;
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
