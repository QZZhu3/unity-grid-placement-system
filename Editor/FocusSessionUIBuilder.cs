#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility that builds the entire FocusSessionPanel UI hierarchy inside the scene Canvas.
///
/// Usage: Unity menu -> Tools -> Placement System -> Build Focus Session UI
///
/// What it creates:
///   Canvas
///   +-- FocusSessionPanel        (CanvasGroup, FocusSessionUI script, inactive by default)
///       +-- Backdrop             (full-screen semi-transparent Image, closes panel on click)
///       +-- PanelBackground      (centred card)
///       |   +-- SessionNameText  (TextMeshProUGUI)
///       |   +-- TimerText        (TextMeshProUGUI, large)
///       |   +-- ProgressBar      (Slider)
///       |   +-- StartButton      (Button + TMP "Start")
///       |   +-- PauseResumeButton(Button + TMP "Pause", inactive by default)
///       |   +-- CancelButton     (Button + TMP "Cancel", inactive by default)
///       |   +-- CompletionPopup  (inactive by default)
///       |       +-- PopupBackground
///       |       +-- CompletionText
///       |       +-- DismissButton
///
/// All FocusSessionUI fields are wired automatically.
/// </summary>
public static class FocusSessionUIBuilder
{
    [MenuItem("Tools/Placement System/Build Focus Session UI")]
    public static void BuildFocusSessionUI()
    {
        // -- Find Canvas -------------------------------------------------------
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Focus Session UI Builder",
                "No Canvas found in the scene. Please add a Canvas first.", "OK");
            return;
        }
        Transform canvasT = canvas.transform;

        // -- Guard: don't build twice ------------------------------------------
        if (canvasT.Find("FocusSessionPanel") != null)
        {
            bool replace = EditorUtility.DisplayDialog("Focus Session UI Builder",
                "FocusSessionPanel already exists. Rebuild it?", "Rebuild", "Cancel");
            if (!replace) return;
            Object.DestroyImmediate(canvasT.Find("FocusSessionPanel").gameObject);
        }

        // -- Root panel --------------------------------------------------------
        GameObject panelGO = new GameObject("FocusSessionPanel");
        panelGO.transform.SetParent(canvasT, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        StretchFull(panelRect);
        CanvasGroup panelCG = panelGO.AddComponent<CanvasGroup>();
        panelCG.alpha          = 0f;
        panelCG.interactable   = false;
        panelCG.blocksRaycasts = false;
        panelGO.SetActive(true); // stays active; hidden via CanvasGroup

        // -- Backdrop ----------------------------------------------------------
        GameObject backdropGO = new GameObject("Backdrop");
        backdropGO.transform.SetParent(panelGO.transform, false);
        RectTransform backdropRect = backdropGO.AddComponent<RectTransform>();
        StretchFull(backdropRect);
        Image backdropImg = backdropGO.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.6f);
        backdropGO.AddComponent<Button>(); // clicking backdrop can close panel later

        // -- Panel card --------------------------------------------------------
        GameObject bgGO = new GameObject("PanelBackground");
        bgGO.transform.SetParent(panelGO.transform, false);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax        = new Vector2(0.5f, 0.5f);
        bgRect.pivot            = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta        = new Vector2(480f, 520f);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.10f, 0.18f, 1f);

        // -- Session name text -------------------------------------------------
        GameObject nameGO = CreateTMPText(bgGO.transform, "SessionNameText", "Standard Focus Session");
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin        = new Vector2(0f, 1f);
        nameRect.anchorMax        = new Vector2(1f, 1f);
        nameRect.pivot            = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0f, -24f);
        nameRect.sizeDelta        = new Vector2(-32f, 36f);
        TextMeshProUGUI nameTMP = nameGO.GetComponent<TextMeshProUGUI>();
        nameTMP.fontSize  = 20f;
        nameTMP.fontStyle = FontStyles.Bold;

        // -- Timer text --------------------------------------------------------
        GameObject timerGO = CreateTMPText(bgGO.transform, "TimerText", "25:00");
        RectTransform timerRect = timerGO.GetComponent<RectTransform>();
        timerRect.anchorMin        = new Vector2(0f, 1f);
        timerRect.anchorMax        = new Vector2(1f, 1f);
        timerRect.pivot            = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0f, -80f);
        timerRect.sizeDelta        = new Vector2(-32f, 90f);
        TextMeshProUGUI timerTMP = timerGO.GetComponent<TextMeshProUGUI>();
        timerTMP.fontSize  = 64f;
        timerTMP.fontStyle = FontStyles.Bold;

        // -- Progress bar ------------------------------------------------------
        GameObject sliderGO = new GameObject("ProgressBar");
        sliderGO.transform.SetParent(bgGO.transform, false);
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin        = new Vector2(0f, 1f);
        sliderRect.anchorMax        = new Vector2(1f, 1f);
        sliderRect.pivot            = new Vector2(0.5f, 1f);
        sliderRect.anchoredPosition = new Vector2(0f, -190f);
        sliderRect.sizeDelta        = new Vector2(-48f, 20f);
        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 0f;
        // Background
        GameObject sliderBgGO = new GameObject("Background");
        sliderBgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform sliderBgRect = sliderBgGO.AddComponent<RectTransform>();
        StretchFull(sliderBgRect);
        Image sliderBgImg = sliderBgGO.AddComponent<Image>();
        sliderBgImg.color = new Color(0.2f, 0.17f, 0.28f, 1f);
        // Fill area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin        = new Vector2(0f, 0f);
        fillAreaRect.anchorMax        = new Vector2(1f, 1f);
        fillAreaRect.offsetMin        = new Vector2(5f, 0f);
        fillAreaRect.offsetMax        = new Vector2(-5f, 0f);
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        StretchFull(fillRect);
        Image fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.55f, 0.40f, 0.80f, 1f);
        slider.fillRect = fillRect;

        // -- Buttons -----------------------------------------------------------
        float btnY     = -240f;
        float btnW     = 180f;
        float btnH     = 56f;

        // Start button (centred)
        GameObject startGO = CreateButton(bgGO.transform, "StartButton", "Start Session");
        RectTransform startRect = startGO.GetComponent<RectTransform>();
        startRect.anchorMin        = new Vector2(0.5f, 1f);
        startRect.anchorMax        = new Vector2(0.5f, 1f);
        startRect.pivot            = new Vector2(0.5f, 1f);
        startRect.anchoredPosition = new Vector2(0f, btnY);
        startRect.sizeDelta        = new Vector2(btnW + 40f, btnH);

        // Pause/Resume button (left, inactive by default)
        GameObject pauseGO = CreateButton(bgGO.transform, "PauseResumeButton", "Pause");
        RectTransform pauseRect = pauseGO.GetComponent<RectTransform>();
        pauseRect.anchorMin        = new Vector2(0.5f, 1f);
        pauseRect.anchorMax        = new Vector2(0.5f, 1f);
        pauseRect.pivot            = new Vector2(1f, 1f);
        pauseRect.anchoredPosition = new Vector2(-8f, btnY);
        pauseRect.sizeDelta        = new Vector2(btnW, btnH);
        pauseGO.SetActive(false);

        // Cancel button (right, inactive by default)
        GameObject cancelGO = CreateButton(bgGO.transform, "CancelButton", "Cancel");
        RectTransform cancelRect = cancelGO.GetComponent<RectTransform>();
        cancelRect.anchorMin        = new Vector2(0.5f, 1f);
        cancelRect.anchorMax        = new Vector2(0.5f, 1f);
        cancelRect.pivot            = new Vector2(0f, 1f);
        cancelRect.anchoredPosition = new Vector2(8f, btnY);
        cancelRect.sizeDelta        = new Vector2(btnW, btnH);
        cancelGO.SetActive(false);

        // -- Completion popup --------------------------------------------------
        GameObject popupGO = new GameObject("CompletionPopup");
        popupGO.transform.SetParent(bgGO.transform, false);
        RectTransform popupRect = popupGO.AddComponent<RectTransform>();
        StretchFull(popupRect);
        popupGO.SetActive(false);

        GameObject popupBgGO = new GameObject("PopupBackground");
        popupBgGO.transform.SetParent(popupGO.transform, false);
        RectTransform popupBgRect = popupBgGO.AddComponent<RectTransform>();
        StretchFull(popupBgRect);
        Image popupBgImg = popupBgGO.AddComponent<Image>();
        popupBgImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);

        GameObject completionTextGO = CreateTMPText(popupGO.transform, "CompletionText",
            "Session complete!\nGreat work!");
        RectTransform completionTextRect = completionTextGO.GetComponent<RectTransform>();
        completionTextRect.anchorMin        = new Vector2(0f, 0.5f);
        completionTextRect.anchorMax        = new Vector2(1f, 0.5f);
        completionTextRect.pivot            = new Vector2(0.5f, 0.5f);
        completionTextRect.anchoredPosition = new Vector2(0f, 40f);
        completionTextRect.sizeDelta        = new Vector2(-32f, 80f);
        completionTextGO.GetComponent<TextMeshProUGUI>().fontSize = 22f;

        GameObject dismissGO = CreateButton(popupGO.transform, "DismissButton", "OK");
        RectTransform dismissRect = dismissGO.GetComponent<RectTransform>();
        dismissRect.anchorMin        = new Vector2(0.5f, 0.5f);
        dismissRect.anchorMax        = new Vector2(0.5f, 0.5f);
        dismissRect.pivot            = new Vector2(0.5f, 0.5f);
        dismissRect.anchoredPosition = new Vector2(0f, -60f);
        dismissRect.sizeDelta        = new Vector2(140f, 52f);

        // -- Attach and wire FocusSessionUI ------------------------------------
        FocusSessionUI ui = panelGO.AddComponent<FocusSessionUI>();

        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("timerText")        .objectReferenceValue = timerGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("sessionNameText")  .objectReferenceValue = nameGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("progressBar")      .objectReferenceValue = slider;
        so.FindProperty("startButton")      .objectReferenceValue = startGO.GetComponent<Button>();
        so.FindProperty("pauseResumeButton").objectReferenceValue = pauseGO.GetComponent<Button>();
        so.FindProperty("pauseResumeLabel") .objectReferenceValue = pauseGO.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("cancelButton")     .objectReferenceValue = cancelGO.GetComponent<Button>();
        so.FindProperty("completionPopup")  .objectReferenceValue = popupGO;
        so.FindProperty("completionText")   .objectReferenceValue = completionTextGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("dismissButton")    .objectReferenceValue = dismissGO.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // -- Mark scene dirty --------------------------------------------------
        EditorUtility.SetDirty(panelGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[FocusSessionUIBuilder] FocusSessionPanel built successfully. " +
                  "Assign a FocusSessionDefinition to the 'Default Definition' field on FocusSessionUI.");

        EditorUtility.DisplayDialog("Focus Session UI Builder",
            "FocusSessionPanel built successfully!\n\n" +
            "Next: Select FocusSessionPanel in the Hierarchy and assign a " +
            "FocusSessionDefinition to the 'Default Definition' field on FocusSessionUI.",
            "OK");

        Selection.activeGameObject = panelGO;
    }

    // -- Helpers ---------------------------------------------------------------

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin        = Vector2.zero;
        rect.anchorMax        = Vector2.one;
        rect.offsetMin        = Vector2.zero;
        rect.offsetMax        = Vector2.zero;
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
