using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ResponsiveCanvasScaler
{
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
    private const float TargetAspectRatio = 16f / 9f;
    private const string ViewportName = "SixteenNineViewport";
    private const string BackgroundCameraName = "SixteenNineBackgroundCamera";
    private static ResolutionWatcher watcher;
    private static Camera backgroundCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureWatcher();
        ApplyToActiveScene();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToActiveScene();
    }

    private static void ApplyToActiveScene()
    {
        CanvasScaler[] scalers = Object.FindObjectsByType<CanvasScaler>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (CanvasScaler scaler in scalers)
        {
            if (scaler == null)
                continue;

            Canvas canvas = scaler.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
                continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = GetScreenMatchMode();
        }

        Canvas.ForceUpdateCanvases();

        foreach (CanvasScaler scaler in scalers)
        {
            if (scaler == null)
                continue;

            Canvas canvas = scaler.GetComponent<Canvas>();
            FitCanvasToTargetAspect(canvas);
        }

        ApplyTargetAspectRatioToCameras();
    }

    private static float GetScreenMatchMode()
    {
        if (Screen.height <= 0)
            return 0.5f;

        float screenAspectRatio = (float)Screen.width / Screen.height;
        return screenAspectRatio >= TargetAspectRatio ? 1f : 0f;
    }

    private static void FitCanvasToTargetAspect(Canvas canvas)
    {
        if (canvas == null || canvas.renderMode == RenderMode.WorldSpace || !canvas.isRootCanvas)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        RectTransform viewport = GetOrCreateViewport(canvasRect);
        MoveCanvasChildrenIntoViewport(canvasRect, viewport);
        ResizeViewport(canvasRect, viewport);
    }

    private static RectTransform GetOrCreateViewport(RectTransform canvasRect)
    {
        Transform existingViewport = canvasRect.Find(ViewportName);
        if (existingViewport != null && existingViewport.TryGetComponent(out RectTransform viewport))
        {
            viewport.SetAsFirstSibling();
            return viewport;
        }

        GameObject viewportObject = new GameObject(ViewportName, typeof(RectTransform));
        viewportObject.transform.SetParent(canvasRect, false);
        viewportObject.transform.SetAsFirstSibling();
        return viewportObject.GetComponent<RectTransform>();
    }

    private static void MoveCanvasChildrenIntoViewport(RectTransform canvasRect, RectTransform viewport)
    {
        while (canvasRect.childCount > 1)
        {
            Transform child = canvasRect.GetChild(1);
            child.SetParent(viewport, false);
        }
    }

    private static void ResizeViewport(RectTransform canvasRect, RectTransform viewport)
    {
        Rect rect = canvasRect.rect;
        float width = rect.width;
        float height = rect.height;

        if (width <= 0f || height <= 0f)
            return;

        float canvasAspectRatio = width / height;

        if (canvasAspectRatio > TargetAspectRatio)
            width = height * TargetAspectRatio;
        else
            height = width / TargetAspectRatio;

        viewport.anchorMin = new Vector2(0.5f, 0.5f);
        viewport.anchorMax = new Vector2(0.5f, 0.5f);
        viewport.pivot = new Vector2(0.5f, 0.5f);
        viewport.anchoredPosition = Vector2.zero;
        viewport.sizeDelta = new Vector2(width, height);
        viewport.localScale = Vector3.one;
        viewport.localRotation = Quaternion.identity;
    }

    private static void ApplyTargetAspectRatioToCameras()
    {
        Rect viewportRect = GetTargetViewportRect();
        EnsureBackgroundCamera();

        Camera[] cameras = Object.FindObjectsByType<Camera>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Camera sceneCamera in cameras)
        {
            if (sceneCamera == null || sceneCamera == backgroundCamera || sceneCamera.targetTexture != null)
                continue;

            sceneCamera.rect = viewportRect;
        }
    }

    private static Rect GetTargetViewportRect()
    {
        if (Screen.height <= 0)
            return new Rect(0f, 0f, 1f, 1f);

        float screenAspectRatio = (float)Screen.width / Screen.height;
        Rect viewportRect = new Rect(0f, 0f, 1f, 1f);

        if (screenAspectRatio > TargetAspectRatio)
        {
            float width = TargetAspectRatio / screenAspectRatio;
            viewportRect.x = (1f - width) * 0.5f;
            viewportRect.width = width;
        }
        else if (screenAspectRatio < TargetAspectRatio)
        {
            float height = screenAspectRatio / TargetAspectRatio;
            viewportRect.y = (1f - height) * 0.5f;
            viewportRect.height = height;
        }

        return viewportRect;
    }

    private static void EnsureBackgroundCamera()
    {
        if (backgroundCamera != null)
            return;

        GameObject cameraObject = new GameObject(BackgroundCameraName);
        Object.DontDestroyOnLoad(cameraObject);
        backgroundCamera = cameraObject.AddComponent<Camera>();
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = Color.black;
        backgroundCamera.cullingMask = 0;
        backgroundCamera.depth = -1000f;
        backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f);
        backgroundCamera.allowHDR = false;
        backgroundCamera.allowMSAA = false;
    }

    private static void EnsureWatcher()
    {
        if (watcher != null)
            return;

        GameObject watcherObject = new GameObject(nameof(ResponsiveCanvasScaler));
        Object.DontDestroyOnLoad(watcherObject);
        watcher = watcherObject.AddComponent<ResolutionWatcher>();
    }

    private sealed class ResolutionWatcher : MonoBehaviour
    {
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        private void Update()
        {
            if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
                return;

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyToActiveScene();
        }
    }
}
