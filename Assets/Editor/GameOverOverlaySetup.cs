// ─────────────────────────────────────────────────────────────
// GameOverOverlaySetup.cs  (Editor-only)
// Tools ▶ Deserted Echoes ▶ Create Game Over Overlay Prefab
// Tools ▶ Deserted Echoes ▶ Add Game Over Overlay To Scene
// ─────────────────────────────────────────────────────────────

using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class GameOverOverlaySetup
{
    private const string PrefabPath = "Assets/Prefabs/GameOverCanvas.prefab";

    [MenuItem("Tools/Deserted Echoes/Create Game Over Overlay Prefab")]
    public static void CreatePrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
        {
            if (!EditorUtility.DisplayDialog(
                    "Replace prefab?",
                    "GameOverCanvas.prefab already exists. Replace it?",
                    "Replace",
                    "Cancel"))
                return;
        }

        GameObject root = BuildOverlayHierarchy();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[GameOverOverlaySetup] Created {PrefabPath}");
    }

    [MenuItem("Tools/Deserted Echoes/Add Game Over Overlay To Scene")]
    public static void AddToScene()
    {
        if (Object.FindObjectOfType<GameOverUI>() != null)
        {
            Debug.Log("[GameOverOverlaySetup] GameOverUI already exists in this scene.");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[GameOverOverlaySetup] Run 'Create Game Over Overlay Prefab' first.");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(instance, "Add Game Over Overlay");
        Debug.Log("[GameOverOverlaySetup] Added GameOverCanvas to scene.");
    }

    private static GameObject BuildOverlayHierarchy()
    {
        GameObject canvasObject = new GameObject("GameOverCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(GameOverUI));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = CreateStretchChild(canvasObject.transform, "GameOverPanel");
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);
        panelImage.raycastTarget = true;

        CreateTitle(panel.transform, "Game Over");
        GameOverUI gameOver = canvasObject.GetComponent<GameOverUI>();
        CreateButton(panel.transform, "RetryButton", "Try Again", new Vector2(0f, -40f), gameOver.OnRetry);
        CreateButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -120f), gameOver.OnMainMenu);

        SerializedObject serialized = new SerializedObject(gameOver);
        serialized.FindProperty("overlayPanel").objectReferenceValue = panel;
        serialized.FindProperty("showDelay").floatValue = 1.5f;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
        return canvasObject;
    }

    private static GameObject CreateStretchChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);

        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return child;
    }

    private static void CreateTitle(Transform parent, string text)
    {
        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(parent, false);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 80f);
        rect.sizeDelta = new Vector2(800f, 120f);

        TextMeshProUGUI label = titleObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 72;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 position,
        UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(320f, 64f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.2f, 0.55f, 0.25f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        UnityEventTools.AddPersistentListener(button.onClick, onClick);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }
}
